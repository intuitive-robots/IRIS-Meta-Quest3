using UnityEngine;
using System;
using TMPro;
using UnityEngine.Events;

public class MQ3UIManager : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private TMP_Text debugText;
    public UnityEvent onQRTrackingStarted;
    public UnityEvent onQRTrackingStopped;
    public UnityEvent onAlignmentStarted;
    public UnityEvent onAlignmentStopped;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        onQRTrackingStarted.AddListener(() => debugText.text = "QR Tracking Started");
        onQRTrackingStopped.AddListener(() => debugText.text = "QR Tracking Stopped");
        onAlignmentStarted.AddListener(() => debugText.text = "Alignment Started");
        onAlignmentStopped.AddListener(() => debugText.text = "Alignment Stopped");

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
}
