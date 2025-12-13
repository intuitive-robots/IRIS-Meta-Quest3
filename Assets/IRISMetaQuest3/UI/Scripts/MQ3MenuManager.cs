using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;
using IRIS.Node;
using NUnit.Framework.Internal;

public class MQ3MenuManager : Singleton<MQ3MenuManager>
{
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private TMP_InputField appNameInput;
    [SerializeField] private GameObject nameChangePopup;
    public UnityEvent onQRTrackingStarted;
    public UnityEvent onQRTrackingStopped;
    public UnityEvent onAlignmentStarted;
    public UnityEvent onAlignmentStopped;
    public UnityEvent<string> onChangeName;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onQRTrackingStarted.AddListener(() => debugText.text = "QR Tracking Started");
        onQRTrackingStopped.AddListener(() => debugText.text = "QR Tracking Stopped");
        onAlignmentStarted.AddListener(() => debugText.text = "Alignment Started");
        onAlignmentStopped.AddListener(() => debugText.text = "Alignment Stopped");
        updateDisplayName();


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

    private void updateDisplayName()
    {
        debugText.text = "updateDisplayName called. ";
        name = IRISXRNode.Instance.localInfo.name;
        if(appNameInput != null)
        {
            debugText.text = "App Name: " + name;
            appNameInput.text = name;
        } else
        {
            debugText.text = "App Name Input is null, " + "App Name: " + name;
        }
    }
}
