using System.Collections.Generic;
using UnityEngine;
using System;
using MessagePack;
using IRIS.Node;
using IRIS.Utilities;


namespace IRIS.MetaQuest3.MotionController
{

    [MessagePackObject]
    public class MetaQuest3HeadData
    {
        [Key("pos")]
        public List<float> pos;      // [x, y, z]
        [Key("rot")]
        public List<float> rot;      // [x, y, z, w]
    }


    public class MetaQuest3HeadTracking : MonoBehaviour
    {
        [SerializeField] private Transform headAnchor;
        [SerializeField] private Transform rootTrans;

        private Publisher<MetaQuest3HeadData> _HeadPublisher;

        void Start()
        {
            _HeadPublisher = new Publisher<MetaQuest3HeadData>("HeadTracking");
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