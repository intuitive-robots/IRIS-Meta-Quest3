using UnityEngine;
using IRIS.MetaQuest3.QRCodeDetection;
using Meta.XR.MRUtilityKit;
using MessagePack;
using IRIS.Node;
using System.Collections.Generic;


[MessagePackObject]
public class QRCodeOffsetData
{
    [Key("qrText")] public string QRText;
    [Key("pos")] public float[] Pos;
    [Key("rot")] public float[] Rot;
    [Key("fixZAxis")] public bool FixZAxis;
}

public class QRCodeAnchor : MonoBehaviour
{
    [SerializeField] private string trackedID;
    private Vector3 offsetPosition = Vector3.zero;
    private Quaternion offsetRotation = Quaternion.identity;
    [SerializeField] private bool fixZAxis = false;

    void Start()
    {
        IRISXRNode.Instance.ServiceManager.RegisterServiceCallback<QRCodeOffsetData, string>($"{gameObject.name}/SetQRCodeOffset", SetOffset, true);
        QRCodeManager.Instance.QRCodeUpdated += ApplyScenePose;
    }

    public string SetOffset(QRCodeOffsetData offsetData)
    {
        trackedID = offsetData.QRText;
        offsetPosition = new Vector3(offsetData.Pos[0], offsetData.Pos[1], offsetData.Pos[2]);
        offsetRotation = new Quaternion(offsetData.Rot[0], offsetData.Rot[1], offsetData.Rot[2], offsetData.Rot[3]);
        fixZAxis = offsetData.FixZAxis;
        if (fixZAxis)
        {
            Vector3 currentForward = offsetRotation * Vector3.forward;
            Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentForward, Vector3.up);
            if (forwardOnPlane.sqrMagnitude < 0.0001f)
            {
                Vector3 currentUp = offsetRotation * Vector3.up;
                forwardOnPlane = Vector3.ProjectOnPlane(currentUp, Vector3.up);
            }
            offsetRotation = Quaternion.LookRotation(forwardOnPlane, Vector3.up);
        }
        ApplyOffset();
        return "Offset applied";
    }


    private void ApplyOffset()
    {
        transform.position += offsetPosition;
        transform.rotation *= offsetRotation;
    }


    private void ApplyScenePose(Dictionary<string, QRCodeData> qrPoseDataDict)
    {
        if (trackedID == null || trackedID == "")
        {
            return;
        }
        if (!qrPoseDataDict.ContainsKey(trackedID))
        {
            return;
        }
        QRCodeData qrPoseData = qrPoseDataDict[trackedID];
        // 1. Set base position to QR code
        transform.SetPositionAndRotation(qrPoseData.worldPose.position, qrPoseData.worldPose.rotation);
        // 2. Add Offsets (Local to the QR's orientation)
        ApplyOffset();
    }

    private void OnDestroy() {
        IRISXRNode.Instance.ServiceManager.UnregisterServiceCallback($"{gameObject.name}/SetQRCodeOffset", true);
        QRCodeManager.Instance.QRCodeUpdated -= ApplyScenePose;
    }


}