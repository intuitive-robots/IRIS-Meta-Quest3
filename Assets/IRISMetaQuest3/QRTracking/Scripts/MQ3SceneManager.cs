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

    // NEW: Cache to store the last known stable pose of a QR Code
    // Key: QR Code Payload (e.g., "IRIS"), Value: World Pose (Position + Rotation)
    private Dictionary<string, Pose> _cachedQRPoses = new Dictionary<string, Pose>();

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

        public SceneData ToSceneData(string name, string qrCode)
        {
            float unityX = -y;
            float unityY = z;
            float unityZ = x;

            float unityRotX = rotY;    
            float unityRotY = -rotZ;   
            float unityRotZ = -rotX;   

            return new SceneData
            {
                Name = name,
                QrCode = qrCode,
                Position = new Vector3(unityX, unityY, unityZ),
                Rotation = new Vector3(unityRotX, unityRotY, unityRotZ)
            };
        }
    }

    // --- 2. Runtime Classes ---
    public class SceneData
    {
        public string Name { get; set; }
        public string QrCode { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; } 

        public RawJsonSceneItem ToRawJsonItem()
        {
            return new RawJsonSceneItem
            {
                name = Name,
                qrCode = QrCode,
                offset = new RawOffset
                {
                    x = Position.z,
                    y = -Position.x,
                    z = Position.y,
                    rotX = -Rotation.z,
                    rotY = Rotation.x,
                    rotZ = -Rotation.y
                }
            };
        }
    }

    void Start()
    {
        ToggleQRTrackingService = new IRISService<string, string>("ToggleQRTracking", (message) =>
        {
            return ToggleQRTracking(message);
        });
    }

    void Update()
    {
        if (qrCodeManager == null) return;

        bool isTracking = QRCodeManager.TrackingEnabled;

        if (isTracking)
        {
            UseLivePose();
        }
        else
        {
            UseCachedPose();
        }

        
    }

    private void UseLivePose()
    {
        // --- CASE 1: LIVE TRACKING ---
        // Update scenes AND update the cache
        Dictionary<string, MRUKTrackable> tracked = qrCodeManager.GetTrackedQRCodes();

        foreach (var kv in tracked)
        {
            string qrName = kv.Key;
            MRUKTrackable trackable = kv.Value;

            // 1. Calculate the stable pose (projected on floor)
            Pose stablePose = CalculateStablePose(trackable.transform);

            // 2. Save to Cache
            _cachedQRPoses[qrName] = stablePose;

            // 3. Update Scene
            string sceneName = getSceneNameFromQrName(qrName);
            if (sceneName != null)
            {
                ApplyScenePose(sceneName, stablePose);
            }
        }
    }

    private void UseCachedPose()
    {
        // --- CASE 2: CACHED TRACKING ---
        // Use stored poses to allow offset adjustments without looking at QR code
        foreach (var kv in _sceneConfig)
        {
            string sceneName = kv.Key;
            string qrName = kv.Value.QrCode;

            if (_cachedQRPoses.TryGetValue(qrName, out Pose cachedPose))
            {
                ApplyScenePose(sceneName, cachedPose);
            }
        }
    }

    /// <summary>
    /// Calculates a stable pose from a raw transform (projects forward vector to horizontal plane).
    /// </summary>
    private Pose CalculateStablePose(Transform t)
    {
        // 1. Get current forward
        Vector3 currentForward = t.forward;

        // 2. Project onto horizontal plane
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentForward, Vector3.up);

        if (forwardOnPlane.sqrMagnitude < 0.0001f)
        {
            Vector3 currentUp = t.up;
            forwardOnPlane = Vector3.ProjectOnPlane(currentUp, Vector3.up);
        }

        // 3. Create rotation
        Quaternion rotation = Quaternion.LookRotation(forwardOnPlane, Vector3.up);

        return new Pose(t.position, rotation);
    }

    /// <summary>
    /// Moves the Scene Object to the QR Pose + Configured Offset.
    /// </summary>
    private void ApplyScenePose(string sceneName, Pose qrPose)
    {
        var sceneObj = SimSceneSpawner.Instance.GetSceneObject(sceneName);
        if (sceneObj == null) return;

        SceneData sceneData = _sceneConfig[sceneName];

        // 1. Set base position to QR code
        sceneObj.transform.SetPositionAndRotation(qrPose.position, qrPose.rotation);

        // 2. Add Offsets (Local to the QR's orientation)
        sceneObj.transform.position += sceneData.Position;
        sceneObj.transform.rotation *= Quaternion.Euler(sceneData.Rotation);
    }

    public void UpdateRawOffset(string sceneName, RawOffset offset)
    {
        // Debug logs kept from your snippet
        Debug.Log($"[MQ3SceneManager] Updating RawOffset for scene: {sceneName}...");
        
        SceneData currentSceneData = _sceneConfig[sceneName];
        string qrName = (currentSceneData == null) ? "" : currentSceneData.QrCode;
        SceneData sceneData = offset.ToSceneData(sceneName, qrName);
        
        _sceneConfig[sceneName] = sceneData;
        NewSceneConfig?.Invoke(_sceneConfig);
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

        if (string.IsNullOrWhiteSpace(json)) return resultDict;

        try
        {
            var rawList = JsonConvert.DeserializeObject<List<RawJsonSceneItem>>(json);
            if (rawList == null) return resultDict;

            foreach (var item in rawList)
            {
                if (string.IsNullOrEmpty(item.name)) continue;
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