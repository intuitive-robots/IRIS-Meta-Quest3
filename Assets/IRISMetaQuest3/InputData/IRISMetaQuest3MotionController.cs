using System.Collections.Generic;
using UnityEngine;
using System;
using MessagePack;
using IRIS.Node;
using IRIS.Utilities;

namespace IRIS.MetaQuest3.MotionController
{

    [MessagePackObject]
    public class MetaQuest3MotionControllerHand
    {
        [Key("pos")]
        public List<float> pos;          // Position [x, y, z]
        [Key("rot")]
        public List<float> rot;          // Rotation [x, y, z, w]
        [Key("vel")]
        public List<float> vel;          // Linear velocity [vx, vy, vz]
        [Key("ang_vel")]
        public List<float> ang_vel;      // Angular velocity [wx, wy, wz]
        [Key("index_trigger")]
        public bool index_trigger;       // Index trigger (front trigger)
        [Key("hand_trigger")]
        public bool hand_trigger;        // Hand trigger (grip)
    }


    [MessagePackObject]
    public class MetaQuest3MotionControllerData
    {
        [Key("left")]
        public MetaQuest3MotionControllerHand left;
        [Key("right")]
        public MetaQuest3MotionControllerHand right;
        [Key("A")]
        public bool A;
        [Key("B")]
        public bool B;
        [Key("X")]
        public bool X;
        [Key("Y")]
        public bool Y;
    }

    public class IRISMetaQuest3MotionController : MonoBehaviour
    {

        [SerializeField] private Transform trackingSpace;
        [SerializeField] private Transform rootTrans;
        private Publisher<MetaQuest3MotionControllerData> _MotionControllerPublisher;
        // private bool isMotionControllerEnabled = true;

        void Start()
        {
            _MotionControllerPublisher = new Publisher<MetaQuest3MotionControllerData>("MotionController");
        }

        void Update()
        {
            PublishMotionControllerData();
        }


        // string ToggleMotionController(string message)
        // {
        //     if (isMotionControllerEnabled)
        //     {
        //         isMotionControllerEnabled = false;
        //         Debug.Log("Motion Controller tracking stopped.");
        //     }
        //     else
        //     {
        //         isMotionControllerEnabled = true;
        //         Debug.Log("Motion Controller tracking started.");
        //     }
        //     return IRISMSG.SUCCESS;
        // }


        static MetaQuest3MotionControllerHand CreateHandData(
            OVRInput.Controller controller,
            Transform trackingSpace,
            Transform rootTrans)
        {
            MetaQuest3MotionControllerHand hand = new();

            // Get the local controller position relative to the tracking space
            Vector3 pos = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(controller));
            hand.pos = TransformationUtils.Unity2ROS(rootTrans.InverseTransformPoint(pos));

            // Get the local controller rotation relative to the tracking space
            Quaternion rot = trackingSpace.rotation * OVRInput.GetLocalControllerRotation(controller);
            hand.rot = TransformationUtils.Unity2ROS(Quaternion.Inverse(rootTrans.rotation) * rot);

            // Get the local linear velocity of the controller
            Vector3 vel = trackingSpace.TransformVector(OVRInput.GetLocalControllerVelocity(controller));
            hand.vel = TransformationUtils.Unity2ROS(rootTrans.InverseTransformVector(vel));

            // Get the local angular velocity of the controller
            Vector3 angVel = trackingSpace.TransformVector(OVRInput.GetLocalControllerAngularVelocity(controller));
            hand.ang_vel = TransformationUtils.Unity2ROS(rootTrans.InverseTransformVector(angVel));

            return hand;
        }


        void PublishMotionControllerData()
        {
            MetaQuest3MotionControllerData motionControllerInputData = new();
            // left hand
            MetaQuest3MotionControllerHand leftHand = CreateHandData(OVRInput.Controller.LTouch, trackingSpace, rootTrans);
            leftHand.index_trigger = OVRInput.Get(OVRInput.RawButton.LIndexTrigger);
            leftHand.hand_trigger = OVRInput.Get(OVRInput.RawButton.LHandTrigger);
            motionControllerInputData.left = leftHand;
            // right hand
            MetaQuest3MotionControllerHand rightHand = CreateHandData(OVRInput.Controller.RTouch, trackingSpace, rootTrans);
            rightHand.index_trigger = OVRInput.Get(OVRInput.RawButton.RIndexTrigger);
            rightHand.hand_trigger = OVRInput.Get(OVRInput.RawButton.RHandTrigger);
            motionControllerInputData.right = rightHand;
            // other buttons
            motionControllerInputData.A = OVRInput.Get(OVRInput.RawButton.A);
            motionControllerInputData.B = OVRInput.Get(OVRInput.RawButton.B);
            motionControllerInputData.X = OVRInput.Get(OVRInput.RawButton.X);
            motionControllerInputData.Y = OVRInput.Get(OVRInput.RawButton.Y);
            _MotionControllerPublisher.Publish(motionControllerInputData);
        }

        // private void OnDestroy()
        // {
        //     toggleMotionControllerService?.Unregister();
        // }
    }
}