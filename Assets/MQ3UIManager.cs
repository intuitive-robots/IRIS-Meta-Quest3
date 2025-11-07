using UnityEngine;
using System;

public class MQ3UIManager : MonoBehaviour
{
    [SerializeField] private Transform headTransform;
    [SerializeField] private float distance = 1.5f;
    [SerializeField] private OVRHand rightHand;
    [SerializeField] private OVRHand leftHand;
    [SerializeField] private GameObject uiCanvas;
    public static event Action onQRTrackingStarted;
    public static event Action onQRTrackingStopped;
    public static event Action onAlignmentStarted;
    public static event Action onAlignmentStopped;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (rightHand.IsTracked && leftHand.IsTracked)
        {
            if (rightHand.GetFingerIsPinching(OVRHand.HandFinger.Index) &&
                leftHand.GetFingerIsPinching(OVRHand.HandFinger.Index))
            {
                if (!uiCanvas.activeSelf)
                {
                    PositionCanvas();
                }
                uiCanvas.SetActive(!uiCanvas.activeSelf);
            }
        }
        else
        {
            Debug.Log("Hands not tracked");
        }
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

    public void PositionCanvas()
    {
        if (headTransform == null)
        {
            Debug.LogError("Head Transform is not assigned in PlaceInFrontOfUser script.");
            return;
        }

        // Get the head's position and forward direction
        Vector3 headPosition = headTransform.position;
        Vector3 headForward = headTransform.forward;

        Vector3 targetPosition;
        Quaternion targetRotation;
        bool keepLevel = true; // Set to true to keep the canvas level on the Y axis
        if (keepLevel)
        {
            // Calculate position in front of the user, but flattened on the Y axis
            Vector3 flattenedForward = new Vector3(headForward.x, 0, headForward.z).normalized;

            // Handle user looking straight up or down
            if (flattenedForward == Vector3.zero)
            {
                // Fallback to the rig's forward direction (or another suitable default)
                // For simplicity, we'll just use the head's forward vector as-is
                flattenedForward = headForward;
            }

            targetPosition = headPosition + (flattenedForward * distance);

            // Create a rotation that looks from the canvas to the user, but only on the Y-axis
            Vector3 lookAtPosition = headPosition;
            lookAtPosition.y = targetPosition.y; // Keep the look-at point level with the canvas
            targetRotation = Quaternion.LookRotation(targetPosition - lookAtPosition);
        }
        else
        {
            // Place the canvas directly where the user is looking
            targetPosition = headPosition + (headForward * distance);

            // Make the canvas face the user directly
            targetRotation = Quaternion.LookRotation(targetPosition - headPosition);
        }

        // Set the canvas's position and rotation
        transform.position = targetPosition;
        transform.rotation = targetRotation;
    }
}
