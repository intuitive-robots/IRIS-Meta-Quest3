using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;
using IRIS.Node;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Collections.Concurrent;

public class MQ3MenuManager : Singleton<MQ3MenuManager>
{
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private TMP_InputField appNameInput;
    [SerializeField] private MQ3QRAlignmentManager qrAlignmentManager;
    [SerializeField] private IRISMetaQuest3Grabbable sceneGrabbable;
    // [SerializeField] OffsetConfigMenuManager offsetConfigMenuManagerPrefab;
    // [SerializeField] GameObject OffsetConfigMenuManagerParent;
    public UnityEvent onQRTrackingStarted;
    public UnityEvent onQRTrackingStopped;
    public UnityEvent onAlignmentStarted;
    public UnityEvent onAlignmentStopped;
    public UnityEvent<string> onChangeName;

    // private Dictionary<string, OffsetConfigMenuManager> offsetConfigMenuManagers = new Dictionary<string, OffsetConfigMenuManager>();
    // private ConcurrentQueue<Dictionary<string, MQ3QRAlignmentManager.SceneData>> _pendingConfigs = new ConcurrentQueue<Dictionary<string, MQ3QRAlignmentManager.SceneData>>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (debugText == null)
        {
            Debug.LogError("Debug Text is not assigned in the inspector.");
        }
        if (appNameInput == null)
        {
            Debug.LogError("App Name Input is not assigned in the inspector.");
        }
        if (qrAlignmentManager == null)
        {
            Debug.LogError("QR Alignment Manager is not assigned in the inspector.");
        }
        if (sceneGrabbable == null)
        {
            Debug.LogError("Scene Grabbable is not assigned in the inspector.");
        }
        onQRTrackingStarted.AddListener(() => debugText.text = "QR Tracking Started");
        onQRTrackingStarted.AddListener(() => qrAlignmentManager.StartQRAlignment());
        onQRTrackingStopped.AddListener(() => debugText.text = "QR Tracking Stopped");
        onQRTrackingStopped.AddListener(() => qrAlignmentManager.StopQRAlignment());
        onAlignmentStarted.AddListener(() => debugText.text = "Alignment Started");
        onAlignmentStarted.AddListener(() => sceneGrabbable.EnableGrab());
        onAlignmentStopped.AddListener(() => debugText.text = "Alignment Stopped");
        onAlignmentStopped.AddListener(() => sceneGrabbable.DisableGrab());
        UpdateDisplayName();
    }

    public void QRTrackingToggled(bool isTracking)
    {
        if (isTracking)
        {
            onQRTrackingStarted?.Invoke();
        }
        else
        {
            onQRTrackingStopped?.Invoke();
        }
    }

    public void AlignmentToggled(bool isAligning)
    {
        if (isAligning)
        {
            onAlignmentStarted?.Invoke();
        }
        else
        {
            onAlignmentStopped?.Invoke();
        }
    }

    private void UpdateDisplayName()
    {
        debugText.text = "UpdateDisplayName called. ";
        name = IRISXRNode.Instance.localInfo.nodeInfo.Name;
        if (appNameInput != null)
        {
            debugText.text = "App Name: " + name;
            appNameInput.text = name;
        }
        else
        {
            debugText.text = "App Name Input is null, " + "App Name: " + name;
        }
    }


}
