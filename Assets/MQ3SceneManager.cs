using IRIS.MetaQuest3.QRCodeDetection;
using IRIS.Node;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Meta.XR.Samples;
using Meta.XR.MRUtilityKit;
using IRIS.SceneLoader; 
using System.Linq;
using Newtonsoft.Json;

public class MQ3SceneManager : MonoBehaviour
{
    [SerializeField] private QRCodeManager qrCodeManager;

    private IRISService<string, string> ToggleQRTrackingService;

    private Dictionary<string, SceneData> _sceneConfig = new Dictionary<string, SceneData>();
    public event Action<Dictionary<string, SceneData>> NewSceneConfig;


    // --- 1. Raw JSON Classes (Internal use only) ---
    // These match the JSON structure exactly: List of objects, Robotics Coords
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
    }

    // --- 2. Runtime Classes (What you want to use) ---
    // Dictionary Value Structure, Unity Coords
    public class SceneData
    {
        public string QrCode { get; set; }
        
        // Using Unity's native types makes life easier
        public Vector3 Position { get; set; } 
        public Vector3 Rotation { get; set; } // Or use Quaternion if preferred
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
        // sync tracked QR codes to corresponding scene objects
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

    private void SetPosAndRot(MRUKTrackable trackable, string sceneName)
    {
        var sceneObj = SimSceneSpawner.Instance.GetSceneObject(sceneName);
        // Update position & rotation to match the QR code

        // 1. Get the current forward direction
        Vector3 currentForward = trackable.transform.forward;

        // 2. Project this vector onto the horizontal plane (XZ)
        // This removes the Y component so the vector is "flat"
        Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentForward, Vector3.up);

        // Handle the edge case where the object was looking straight up or down
        if (forwardOnPlane.sqrMagnitude < 0.0001f)
        {
            // If the projected vector is too small, default to some horizontal direction
            // as rotation around Y axis won't matter in this case
            Vector3 currentUp = trackable.transform.up;
            forwardOnPlane = Vector3.ProjectOnPlane(currentUp, Vector3.up);
        }

        // 3. Create a new rotation
        // "Look in the direction of forwardOnPlane, but keep the head up (Vector3.up)"
        Quaternion qua = Quaternion.LookRotation(forwardOnPlane, Vector3.up);

        sceneObj.transform.SetPositionAndRotation(trackable.transform.position, qua);
        SceneData sceneData = _sceneConfig[sceneName];

        // Apply offset from config
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
            // Step A: Deserialize JSON into the "Raw" List structure
            var rawList = JsonConvert.DeserializeObject<List<RawJsonSceneItem>>(json);

            if (rawList == null) return resultDict;

            // Step B: Iterate, Convert Coordinates, and Fill Dictionary
            foreach (var item in rawList)
            {
                if (string.IsNullOrEmpty(item.name)) continue;

                // --- Coordinate Transformation Logic ---
                
                // 1. Position Mapping
                // User Req: Robotics (Z=Up, X=Fwd) -> Unity (Y=Up, Z=Fwd)
                // Robotics X (Fwd) -> Unity Z (Fwd)
                // Robotics Z (Up)  -> Unity Y (Up)
                // Robotics Y (Side)-> Unity X (Side)
                
                // Note: Robotics is usually Right-Handed, Unity is Left-Handed.
                // To preserve the world shape, we usually negate the Side axis (Unity X).
                float unityX = -item.offset.y; 
                float unityY = item.offset.z;
                float unityZ = item.offset.x;
                
                Vector3 finalPos = new Vector3(unityX, unityY, unityZ);

                // 2. Rotation Mapping
                // Rotations are complex (order matters: Roll/Pitch/Yaw). 
                // A simple axis swap for Euler often looks like this:
                float unityRotX = item.offset.rotY; // Pitch usually maps to X rotation in Unity
                float unityRotY = -item.offset.rotZ; // Yaw maps to Y (negated for handedness)
                float unityRotZ = -item.offset.rotX; // Roll maps to Z
                
                Vector3 finalRot = new Vector3(unityRotX, unityRotY, unityRotZ);

                // Create the runtime object
                var sceneData = new SceneData
                {
                    QrCode = item.qrCode,
                    Position = finalPos,
                    Rotation = finalRot
                };

                // Add to Dictionary (using "name" as the Key)
                resultDict[item.name] = sceneData;
                Debug.Log($"[MQ3SceneManager] Parsed Scene: {item.name}, QR: {item.qrCode}, Pos: {finalPos}, Rot: {finalRot}");
                Debug.Log($"[MQ3SceneManager] Parsed Scene: {item.name}, exists: {SimSceneSpawner.Instance.GetSceneObject(item.name) != null}");
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
