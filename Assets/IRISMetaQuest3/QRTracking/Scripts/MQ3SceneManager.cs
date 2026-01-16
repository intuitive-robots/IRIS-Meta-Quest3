using IRIS.MetaQuest3.QRCodeDetection;
using IRIS.Node;
using System;
using System.Collections.Generic;
using UnityEngine;
using Meta.XR.Samples;
using Meta.XR.MRUtilityKit;
using IRIS.SceneLoader;
using Newtonsoft.Json;
using IRIS.Utilities;

public class MQ3SceneManager : Singleton<MQ3SceneManager>
{
    [SerializeField] private QRCodeManager qrCodeManager;

    private IRISService<string, string> ToggleQRTrackingService;

    private Dictionary<string, SceneData> _sceneConfig = new Dictionary<string, SceneData>();

    // Kept as Action because UnityEvent cannot serialize Dictionaries in the Inspector
    public event Action<Dictionary<string, SceneData>> NewSceneConfig;

    // --- 1. Raw JSON Classes (Internal use only) ---
    [Serializable]
    public class RawJsonSceneItem
    {
        public string name;
        public string qrCode;
        public RawOffset offset;
    }

    [Serializable]
    public class RawOffset
    {
        // Robotics: x=forward, z=up, y=side
        public float x;
        public float y;
        public float z;

        // Euler angles in Robotics frame
        public float rotX;
        public float rotY;
        public float rotZ;

        /// <summary>
        /// Converts this RawOffset (Robotics Coords) to a SceneData object (Unity Coords).
        /// </summary>
        public SceneData ToSceneData(string name, string qrCode)
        {
            // Position Mapping:
            // Robotics X (Fwd) -> Unity Z (Fwd)
            // Robotics Z (Up)  -> Unity Y (Up)
            // Robotics Y (Side)-> Unity X (Side) (Negated for LH vs RH system)
            float unityX = -y;
            float unityY = z;
            float unityZ = x;

            // Rotation Mapping:
            // Standard axis swaps for Robotics -> Unity
            float unityRotX = rotY;    // Pitch maps to X
            float unityRotY = -rotZ;   // Yaw maps to Y (negated)
            float unityRotZ = -rotX;   // Roll maps to Z (negated)

            return new SceneData
            {
                Name = name, // Added Name
                QrCode = qrCode,
                Position = new Vector3(unityX, unityY, unityZ),
                Rotation = new Vector3(unityRotX, unityRotY, unityRotZ)
            };
        }
    }

    // --- 2. Runtime Classes (What you want to use) ---
    public class SceneData
    {
        public string Name { get; set; } // Added Name
        public string QrCode { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; } // Euler Angles

        /// <summary>
        /// Converts this SceneData (Unity Coords) back to a RawOffset (Robotics Coords).
        /// Returns a RawJsonSceneItem wrapper containing the name, qr, and offset.
        /// </summary>
        public RawJsonSceneItem ToRawJsonItem()
        {
            return new RawJsonSceneItem
            {
                name = Name,
                qrCode = QrCode,
                offset = new RawOffset
                {
                    // Inverse Position Mapping:
                    x = Position.z,
                    y = -Position.x,
                    z = Position.y,

                    // Inverse Rotation Mapping:
                    rotX = -Rotation.z,
                    rotY = Rotation.x,
                    rotZ = -Rotation.y
                }
            };
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ToggleQRTrackingService = new IRISService<string, string>("ToggleQRTracking", (message) =>
        {
            return ToggleQRTracking(message);
        });
    }

    // Update is called once per frame
    void Update()
    {
        if (qrCodeManager == null || !QRCodeManager.TrackingEnabled)
            return;

        Dictionary<string, MRUKTrackable> tracked = qrCodeManager.GetTrackedQRCodes();
        foreach (var kv in tracked)
        {
            string qrName = kv.Key;
            MRUKTrackable trackable = kv.Value;

            string sceneName = getSceneNameFromQrName(qrName);
            if (sceneName == null)
                continue;
            SetPosAndRot(trackable, sceneName);
        }
    }

    public void UpdateRawOffset(string sceneName, RawOffset offset)
    {
        SceneData currentSceneData = _sceneConfig[sceneName];
        string qrName = (currentSceneData == null) ? "" : currentSceneData.QrCode;
        SceneData sceneData = offset.ToSceneData(sceneName, qrName);
        _sceneConfig[sceneName] = sceneData;
        NewSceneConfig?.Invoke(_sceneConfig);
    }

    private void SetPosAndRot(MRUKTrackable trackable, string sceneName)
    {
        var sceneObj = SimSceneSpawner.Instance.GetSceneObject(sceneName);
        if (sceneObj == null) return;

        // 1. Get current forward
        Vector3 currentForward = trackable.transform.forward;

        // 2. Project onto horizontal plane
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentForward, Vector3.up);

        if (forwardOnPlane.sqrMagnitude < 0.0001f)
        {
            Vector3 currentUp = trackable.transform.up;
            forwardOnPlane = Vector3.ProjectOnPlane(currentUp, Vector3.up);
        }

        // 3. Create rotation
        Quaternion qua = Quaternion.LookRotation(forwardOnPlane, Vector3.up);

        sceneObj.transform.SetPositionAndRotation(trackable.transform.position, qua);
        SceneData sceneData = _sceneConfig[sceneName];

        // Apply offset
        sceneObj.transform.position += sceneData.Position;
        sceneObj.transform.rotation *= Quaternion.Euler(sceneData.Rotation);
    }

    public string ToggleQRTracking(string message)
    {
        _sceneConfig = ParseAndConvert(message);
        NewSceneConfig?.Invoke(_sceneConfig);

        if (qrCodeManager != null)
        {
            return qrCodeManager.ToggleQRTracking(message);
        }

        return "No QRCodeManager available";
    }

    public Dictionary<string, SceneData> ParseAndConvert(string json)
    {
        Debug.Log($"[MQ3SceneManager] Parsing Scene Config JSON: {json}");

        var resultDict = new Dictionary<string, SceneData>();

        if (string.IsNullOrWhiteSpace(json))
            return resultDict;

        try
        {
            var rawList = JsonConvert.DeserializeObject<List<RawJsonSceneItem>>(json);

            if (rawList == null) return resultDict;

            foreach (var item in rawList)
            {
                if (string.IsNullOrEmpty(item.name)) continue;

                // --- Pass Name and QR Code into conversion ---
                SceneData sceneData = item.offset.ToSceneData(item.name, item.qrCode);

                resultDict[item.name] = sceneData;

                Debug.Log($"[MQ3SceneManager] Parsed: {sceneData.Name}, QR: {sceneData.QrCode}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MQ3SceneManager] JSON Parsing failed: {ex.Message}");
        }

        return resultDict;
    }

    private string getSceneNameFromQrName(string qrName)
    {
        foreach (var kv in _sceneConfig)
        {
            if (kv.Value.QrCode == qrName)
                return kv.Key;
        }
        return null;
    }
}