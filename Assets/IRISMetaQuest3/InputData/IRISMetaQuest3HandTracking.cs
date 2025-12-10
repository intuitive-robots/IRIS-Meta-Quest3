using System.Collections.Generic;
using UnityEngine;
using IRIS.Node;
using IRIS.Utilities;
using IRIS.SceneLoader;

namespace IRIS.MetaQuest3.HandTracking
{

    [SerializeField]
    public class MetaQuest3Bone
    {
        public List<float> pos;
        public List<float> rot;
    }

    [SerializeField]
    public class MetaQuest3Hand
    {
        public List<MetaQuest3Bone> bones;
        // TODO: hand gesture data can be added here
    }

    [SerializeField]
    public class MetaQuest3HandTrackingData
    {
        public MetaQuest3Hand leftHand;
        public MetaQuest3Hand rightHand;
    }

    class IRISMetaQuest3HandTracking : MonoBehaviour
    {
        [SerializeField] private OVRSkeleton leftHand;
        [SerializeField] private OVRSkeleton rightHand;
        [SerializeField] private SimSceneSpawner sceneSpawner;
        private Publisher<MetaQuest3HandTrackingData> _handTrackingPublisher;
        // private IRISService<string, string> toggleHandTrackingService;
        private Transform localTF;
        // private bool isHandTrackingEnabled = true;
        void Start()
        {
            // toggleHandTrackingService = new IRISService<string, string>("ToggleHandTracking", (message) =>
            // {
            //     ToggleHandTracking(message);
            //     return "Hand tracking toggled";
            // });
            _handTrackingPublisher = new Publisher<MetaQuest3HandTrackingData>("HandTracking");
            // default local tracking space is the world space
            localTF = transform;
        }

        void Update()
        {
            // if (isHandTrackingEnabled)
            // {
            // }
            PublishHandTrackingData();
        }

        void PublishHandTrackingData()
        {
            if (leftHand != null && rightHand != null)
            {
                MetaQuest3HandTrackingData handTrackingData = new MetaQuest3HandTrackingData
                {
                    leftHand = GetHandData(leftHand),
                    rightHand = GetHandData(rightHand)
                };
                _handTrackingPublisher.Publish(handTrackingData);
            }
        }


        // private void ToggleHandTracking(string rootFrameName)
        // {
        //     if (isHandTrackingEnabled)
        //     {
        //         isHandTrackingEnabled = false;
        //         Debug.Log("Hand tracking stopped.");
        //         return;
        //     }
        //     UnityMainThreadDispatcher.Instance.Enqueue(
        //         () => {
        //             Transform rootTF;
        //             if (rootFrameName == null || rootFrameName == "")
        //             {
        //                 rootTF = transform; // use the current transform if no root frame is specified
        //                 Debug.LogWarning("No root frame specified, using the current transform as the root frame.");
        //             }
        //             else
        //             {
        //                 rootTF = sceneSpawner.GetSceneTransform(rootFrameName);
        //                 if (rootTF == null)
        //                 {
        //                     Debug.LogError("Scene Spawner is not set. Cannot find root frame.");
        //                     return;
        //                 }
        //             }
        //             localTF = rootTF;
        //             isHandTrackingEnabled = true;
        //             Debug.Log($"Hand tracking started with root frame: {rootFrameName}");
        //         }
        //     );
        // }


        private MetaQuest3Hand GetHandData(OVRSkeleton skeleton)
        {
            MetaQuest3Hand handData = new MetaQuest3Hand();
            handData.bones = new List<MetaQuest3Bone>();
            foreach (var bone in skeleton.Bones)
            {
                MetaQuest3Bone boneData = new MetaQuest3Bone
                {
                    pos = TransformationUtils.Unity2ROS(localTF.InverseTransformPoint(bone.Transform.position)),
                    rot = TransformationUtils.Unity2ROS(Quaternion.Inverse(localTF.rotation) * bone.Transform.rotation)
                };
                handData.bones.Add(boneData);
            }
            return handData;
        }


        // void OnDestroy()
        // {
        //     toggleHandTrackingService?.Unregister();
        // }

    }
}
