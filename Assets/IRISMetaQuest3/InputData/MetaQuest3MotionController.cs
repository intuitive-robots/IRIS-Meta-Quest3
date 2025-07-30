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
        public List<float> pos;
        public List<float> rot;
        public bool index_trigger;
        public bool hand_trigger;
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

    public class MetaQuest3MotionControllerPublisher : MonoBehaviour
    {

        [SerializeField] private Transform trackingSpace;
        [SerializeField] private Transform rootTrans;
        private Action updateAction;
        private Publisher<MetaQuest3MotionControllerData> _MotionControllerPublisher;
        private IRISService<string, string> toggleMotionControllerService;
        private bool isMotionControllerEnabled = false;

        void Start()
        {
            _MotionControllerPublisher = new Publisher<MetaQuest3MotionControllerData>("MotionController");
            toggleMotionControllerService = new IRISService<string, string>("ToggleMotionController", ToggleMotionController);
        }

        void Update()
        {
            updateAction?.Invoke();
        }


        string ToggleMotionController(string message)
        {
            if (isMotionControllerEnabled)
            {
                isMotionControllerEnabled = false;
                updateAction -= PublishMotionControllerData;
                Debug.Log("Motion Controller tracking stopped.");
            }
            else
            {
                isMotionControllerEnabled = true;
                updateAction += PublishMotionControllerData;
                Debug.Log("Motion Controller tracking started.");
            }
            return IRISMSG.SUCCESS;
        }


        static MetaQuest3MotionControllerHand CreateHandData(OVRInput.Controller controller, Transform trackingSpace, Transform rootTrans)
        {
            MetaQuest3MotionControllerHand hand = new();
            Vector3 pos = trackingSpace.TransformPoint(OVRInput.GetLocalControllerPosition(controller));
            hand.pos = TransformationUtils.Unity2ROS(rootTrans.InverseTransformPoint(pos));
            Quaternion rot = trackingSpace.rotation * OVRInput.GetLocalControllerRotation(controller);
            hand.rot = TransformationUtils.Unity2ROS(rot * Quaternion.Inverse(rootTrans.rotation));
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

        private void OnDestroy()
        {
            updateAction -= PublishMotionControllerData;
            toggleMotionControllerService?.Unregister();
        }
    }
}