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

public class MQ3QRAlignmentManager : Singleton<MQ3QRAlignmentManager>
{
    [SerializeField] private QRCodeManager qrCodeManager;

    // CHANGED: Now holds just a single config object, not a dictionary
    // private SceneData _sceneConfig;
    [SerializeField] private string qrText = "IRIS";

    // Cache to store the last known stable pose of a QR Code
    // Key: QR Code Payload (e.g., "IRIS"), Value: World Pose
    private Dictionary<string, Pose> _cachedQRPoses = new Dictionary<string, Pose>();

    void Update()
    {
        if (qrCodeManager == null) return;
        if (qrCodeManager.TrackingEnabled)
        {
            UseLivePose();
        }
    }

    public void StartQRAlignment()
    {
        // Additional logic to start alignment can be added here
        if (qrCodeManager != null && !qrCodeManager.TrackingEnabled)
        {
            qrCodeManager.TrackingEnabled = true;
        }
        Debug.Log("[MQ3QRAlignmentManager] Starting QR Alignment...");
    }

    public void StopQRAlignment()
    {
        if (qrCodeManager != null && qrCodeManager.TrackingEnabled)
        {
            qrCodeManager.TrackingEnabled = false;
        }
        // Additional logic to stop alignment can be added here
        Debug.Log("[MQ3QRAlignmentManager] Stopping QR Alignment...");
    }


    private void UseLivePose()
    {
        // Get all currently visible QRs
        Dictionary<string, MRUKTrackable> tracked = qrCodeManager.GetTrackedQRCodes();

        // We only care about the QR code specified in our single _sceneConfig
        if (tracked.TryGetValue(qrText, out MRUKTrackable trackable))
        {
            // 1. Calculate stable pose
            // Pose stablePose = CalculateStablePose(trackable.transform);
            Pose stablePose = new Pose(trackable.transform.position, trackable.transform.rotation * Quaternion.Euler(90f, 0f, 0f));
            // 3. Apply to Scene
            ApplyQRPose(stablePose);
        }
    }

    private void UseCachedPose()
    {
        // If we aren't tracking, look for the data in our cache
        if (_cachedQRPoses.TryGetValue(qrText, out Pose cachedPose))
        {
            ApplyQRPose(cachedPose);
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

    private void ApplyQRPose(Pose qrPose)
    {
        transform.SetPositionAndRotation(qrPose.position, qrPose.rotation);
    }

    public void ToggleQRTracking()
    {
        if (qrCodeManager != null)
        {
            qrCodeManager.TrackingEnabled = !qrCodeManager.TrackingEnabled;
        }
    }
}