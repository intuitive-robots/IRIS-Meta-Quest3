/*
 * Copyright (c) Meta Platforms, Inc. and affiliates.
 * All rights reserved.
 *
 * Licensed under the Oculus SDK License Agreement (the "License");
 * you may not use the Oculus SDK except in compliance with the License,
 * which is provided at the time of installation or download, or which
 * otherwise accompanies this software in either electronic or hard copy form.
 *
 * You may obtain a copy of the License at
 *
 * https://developer.oculus.com/licenses/oculussdk/
 *
 * Unless required by applicable law or agreed to in writing, the Oculus SDK
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

using System;
using System.Collections.Generic;

using UnityEngine;
using IRIS.Node;

namespace IRIS.MetaQuest3.QRCodeDetection
{


    public class QRCodeData
    {
        public string payload;
        public Pose worldPose;
    }

    public class QRCodeManager : Singleton<QRCodeManager>
    {
        public const string ScenePermission = OVRPermissionsRequester.ScenePermission;

        public static bool IsSupported
            => OVRAnchor.TrackerConfiguration.QRCodeTrackingSupported;

        private Dictionary<string, MRUKTrackable> _trackedQRCodes = new ();
        private Dictionary<string, QRCodeData> _qrCodeData = new ();
        public Action<Dictionary<string, QRCodeData>> QRCodeUpdated;

        [SerializeField]
        QRCode _qrCodePrefab;

        // [SerializeField]
        // QRCodeSampleUI _uiInstance;

        [SerializeField]
        MRUK _mrukInstance;

        // non-serialized fields

        void Start()
        {
        }

        void Update()
        {
            if (!TrackingEnabled)
            {
                return;
            }
            if (QRCodeUpdated != null && _qrCodeData.Count > 0)
            {
                QRCodeUpdated?.Invoke(_qrCodeData);
            }
        } 


        void OnEnable()
        {
            if (!_mrukInstance)
            {
                Debug.LogError($"{nameof(QRCodeManager)} requires an MRUK object in the scene!");
                return;
            }

            _mrukInstance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
            _mrukInstance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
        }

        public void OnTrackableAdded(MRUKTrackable trackable)
        {
            Debug.Log($" {nameof(OnTrackableAdded)} called.");

            if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
            {
                return;
            }

            if (trackable.MarkerPayloadString == null)
            {
                return;
            }

            _trackedQRCodes[trackable.MarkerPayloadString] = trackable;
            Debug.Log($"{nameof(OnTrackableAdded)}: QRCode tracked! Text: {trackable.MarkerPayloadString}");
            QRCode qrCode = Instantiate(_qrCodePrefab, trackable.transform);
            // QRCode qrCode = qrCode.GetComponent<QRCode>();
            qrCode.Initialize(trackable);
            qrCode.GetComponent<Bounded2DVisualizer>().Initialize(trackable);
            _qrCodeData[trackable.MarkerPayloadString] = new QRCodeData
            {
                payload = trackable.MarkerPayloadString,
                worldPose = new Pose(trackable.transform.position, trackable.transform.rotation)
            };
        }

        public void OnTrackableRemoved(MRUKTrackable trackable)
        {
            if (trackable.TrackableType != OVRAnchor.TrackableType.QRCode)
            {
                return;
            }
            _trackedQRCodes.Remove(trackable.MarkerPayloadString);
            Debug.Log($"{nameof(OnTrackableRemoved)}: {trackable.Anchor.Uuid.ToString("N").Remove(8)}[..]");
            Destroy(trackable.gameObject);
            _qrCodeData.Remove(trackable.MarkerPayloadString);
        }

        public static bool TrackingEnabled
        {
            get => Instance && Instance._mrukInstance && Instance._mrukInstance.SceneSettings.TrackerConfiguration.QRCodeTrackingEnabled;
            set
            {
                if (!Instance || !Instance._mrukInstance)
                {
                    return;
                }
                var config = Instance._mrukInstance.SceneSettings.TrackerConfiguration;
                config.QRCodeTrackingEnabled = value;
                Instance._mrukInstance.SceneSettings.TrackerConfiguration = config;
            }
        }

        public string ToggleQRTracking(string message)
        {
            Debug.Log($"[QRCodeManager] ToggleQRTracking called with message: {message}");
            TrackingEnabled = !TrackingEnabled;
            Debug.Log($"[QRCodeManager] QR Tracking enabled: {TrackingEnabled}");
            return $"QR Tracking enabled: {TrackingEnabled}";
        }

        internal Dictionary<string, MRUKTrackable> GetTrackedQRCodes()
        {
            return _trackedQRCodes;
        }

        public static bool HasPermissions
#if UNITY_EDITOR
            => true;
#else
            => UnityEngine.Android.Permission.HasUserAuthorizedPermission(ScenePermission);
#endif

    }
}
