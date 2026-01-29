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
using MessagePack;

public class MQ3SceneManager : Singleton<MQ3SceneManager>
{
    [SerializeField] private QRCodeManager qrCodeManager;

    // CHANGED: Now holds just a single config object, not a dictionary
    private SceneData _sceneConfig;

    // Cache to store the last known stable pose of a QR Code
    // Key: QR Code Payload (e.g., "IRIS"), Value: World Pose
    private Dictionary<string, Pose> _cachedQRPoses = new Dictionary<string, Pose>();

    // CHANGED: Event now passes a single SceneData object
    public event Action<SceneData> NewSceneConfig;

    // --- 1. Raw JSON Classes ---
    // [Serializable]
    // public class RawJsonSceneItem
    // {
    //     // REMOVED: public string name; 
    //     public string qrCode;
    //     public RawOffset offset;
    // }


    [Serializable]
    [MessagePackObject(keyAsPropertyName: true)] // This forces serialization as a Map {"key": value}
    public class RawJsonSceneItem
    {
        // REMOVED: public string name; 

        // Because keyAsPropertyName is true, this serialize as "qrCode": "value"
        public string qrCode; 

        // Ensure the RawOffset class ALSO has the [MessagePackObject] attribute!
        public RawOffset offset; 
    }

    // Don't forget to apply the same logic to the nested class
    [Serializable]
    [MessagePackObject(keyAsPropertyName: true)] 
    public class RawOffset
    {
        public float x;
        public float y;
        public float z;
        public float rotX, rotY, rotZ;

        public SceneData ToSceneData(string qrCode)
        {
            // Coordinate conversion (Robotics -> Unity)
            return new SceneData
            {
                QrCode = qrCode,
                Position = new Vector3(-y, z, x),
                Rotation = new Vector3(rotY, -rotZ, -rotX)
            };
        }
        // ... add your actual fields here
    }


    // [Serializable]
    // public class RawOffset
    // {
    //     // Robotics: x=forward, z=up, y=side
    //     public float x, y, z;
    //     public float rotX, rotY, rotZ;

    //     // CHANGED: Removed 'name' parameter
    // }

    // --- 2. Runtime Classes ---
    public class SceneData
    {
        // REMOVED: public string Name { get; set; }
        public string QrCode { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; } 

        public RawJsonSceneItem ToRawJsonItem()
        {
            return new RawJsonSceneItem
            {
                qrCode = QrCode,
                offset = new RawOffset
                {
                    // Inverse Coordinate conversion (Unity -> Robotics)
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
        IRISXRNode.Instance.ServiceManager.RegisterServiceCallback<string, string>("ToggleQRTracking", ToggleQRTracking, true);
        qrCodeManager = QRCodeManager.Instance;
    }

    void Update()
    {
        if (qrCodeManager == null || _sceneConfig == null) return;

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
        // Get all currently visible QRs
        Dictionary<string, MRUKTrackable> tracked = qrCodeManager.GetTrackedQRCodes();

        // We only care about the QR code specified in our single _sceneConfig
        if (tracked.TryGetValue(_sceneConfig.QrCode, out MRUKTrackable trackable))
        {
            // 1. Calculate stable pose
            Pose stablePose = CalculateStablePose(trackable.transform);

            // 2. Save to Cache
            _cachedQRPoses[_sceneConfig.QrCode] = stablePose;

            // 3. Apply to Scene
            ApplyScenePose(stablePose);
        }
    }

    private void UseCachedPose()
    {
        // If we aren't tracking, look for the data in our cache
        if (_cachedQRPoses.TryGetValue(_sceneConfig.QrCode, out Pose cachedPose))
        {
            ApplyScenePose(cachedPose);
        }
    }

    private Pose CalculateStablePose(Transform t)
    {
        Vector3 currentForward = t.forward;
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentForward, Vector3.up);

        if (forwardOnPlane.sqrMagnitude < 0.0001f)
        {
            forwardOnPlane = Vector3.ProjectOnPlane(t.up, Vector3.up);
        }

        Quaternion rotation = Quaternion.LookRotation(forwardOnPlane, Vector3.up);
        return new Pose(t.position, rotation);
    }

    private void ApplyScenePose(Pose qrPose)
    {
        // CHANGED: Use IRISXRNode.Instance.gameObject instead of SimSceneSpawner lookup
        GameObject sceneObj = IRISXRNode.Instance.gameObject;
        
        if (sceneObj == null) return;

        // 1. Set base position to QR code
        sceneObj.transform.SetPositionAndRotation(qrPose.position, qrPose.rotation);

        // 2. Add Offsets (Local to the QR's orientation)
        sceneObj.transform.position += _sceneConfig.Position;
        sceneObj.transform.rotation *= Quaternion.Euler(_sceneConfig.Rotation);
    }

    public void UpdateRawOffset(RawOffset offset)
    {
        if (_sceneConfig == null) return;

        Debug.Log($"[MQ3SceneManager] Updating RawOffset...");
        
        // Re-create SceneData with new offset, preserving the QR code
        _sceneConfig = offset.ToSceneData(_sceneConfig.QrCode);
        
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

    public SceneData ParseAndConvert(string json)
    {
        Debug.Log($"[MQ3SceneManager] Parsing Scene Config JSON: {json}");

        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            // CHANGED: Deserialize a SINGLE object, not a List
            var rawItem = JsonConvert.DeserializeObject<RawJsonSceneItem>(json);

            if (rawItem == null) return null;

            SceneData sceneData = rawItem.offset.ToSceneData(rawItem.qrCode);
            Debug.Log($"[MQ3SceneManager] Parsed Config for QR: {sceneData.QrCode}");
            
            return sceneData;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[MQ3SceneManager] JSON Parsing failed: {ex.Message}");
            return null;
        }
    }
}