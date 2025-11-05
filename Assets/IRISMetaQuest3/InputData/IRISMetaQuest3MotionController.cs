using System.Collections.Generic;
using UnityEngine;
using System;
using IRIS.Node;
using IRIS.Utilities;

namespace IRIS.MetaQuest3.MotionController
{

    [SerializeField]
    public class MetaQuest3MotionControllerHand
    {
        public List<float> pos;          // Position [x, y, z]
        public List<float> rot;          // Rotation [x, y, z, w]
        public List<float> vel;          // Linear velocity [vx, vy, vz]
        public List<float> ang_vel;      // Angular velocity [wx, wy, wz]
        public bool index_trigger;       // Index trigger (front trigger)
        public bool hand_trigger;        // Hand trigger (grip)
    }


     [SerializeField]
    public class MetaQuest3MotionControllerData
    {
        public MetaQuest3MotionControllerHand left;
        public MetaQuest3MotionControllerHand right;
        public bool A;
        public bool B;
        public bool X;
        public bool Y;
    }

    public class IRISMetaQuest3MotionController : MonoBehaviour
    {

        [SerializeField] private Transform trackingSpace;
        [SerializeField] private Transform rootTrans;
        private Publisher<MetaQuest3MotionControllerData> _MotionControllerPublisher;
        private IRISService<string, string> toggleMotionControllerService;
        // private bool isMotionControllerEnabled = true;

        void Start()
        {
            _MotionControllerPublisher = new Publisher<MetaQuest3MotionControllerData>("MotionController");
            // toggleMotionControllerService = new IRISService<string, string>("ToggleMotionController", ToggleMotionController);
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
            hand.rot = TransformationUtils.Unity2ROS(rot * Quaternion.Inverse(rootTrans.rotation));

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