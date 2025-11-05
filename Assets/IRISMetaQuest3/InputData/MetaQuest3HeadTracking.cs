using System.Collections.Generic;
using UnityEngine;
using System;
using IRIS.Node;
using IRIS.Utilities;


namespace IRIS.MetaQuest3.MotionController
{

    [Serializable]
    public class MetaQuest3HeadData
    {
        public List<float> pos;      // [x, y, z]
        public List<float> rot;      // [x, y, z, w]
    }


    public class MetaQuest3HeadTracking : MonoBehaviour
    {
        [SerializeField] private Transform trackingSpace;
        [SerializeField] private Transform rootTrans;

        private Publisher<MetaQuest3HeadData> _HeadPublisher;
        private Transform headAnchor;

        void Start()
        {
            _HeadPublisher = new Publisher<MetaQuest3HeadData>("HeadTracking");
            headAnchor = OVRManager.instance.GetComponent<OVRCameraRig>().centerEyeAnchor;
        }

        void Update()
        {
            PublishHeadData();
        }

        void PublishHeadData()
        {
            MetaQuest3HeadData head = new();

            // Get the head position and rotation in world space
            // Convert to rootTrans-relative coordinates (ROS frame)
            Vector3 pos = rootTrans.InverseTransformPoint(headAnchor.position);
            Quaternion rot = Quaternion.Inverse(rootTrans.rotation) * headAnchor.rotation;

            head.pos = TransformationUtils.Unity2ROS(pos);
            head.rot = TransformationUtils.Unity2ROS(rot);

            _HeadPublisher.Publish(head);
        }
    }


}