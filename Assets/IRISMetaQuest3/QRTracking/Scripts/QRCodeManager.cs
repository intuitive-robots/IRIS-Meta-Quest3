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
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    public class QRCodeManager : MonoBehaviour
    {
        //
        // Static interface

        public const string ScenePermission = OVRPermissionsRequester.ScenePermission;

        public static bool IsSupported
            => OVRAnchor.TrackerConfiguration.QRCodeTrackingSupported;

        private Dictionary<string, MRUKTrackable> _trackedQRCodes = new Dictionary<string, MRUKTrackable>();


        [SerializeField]
        QRCode _qrCodePrefab;

        // [SerializeField]
        // QRCodeSampleUI _uiInstance;

        [SerializeField]
        MRUK _mrukInstance;

        // non-serialized fields


        static QRCodeManager s_instance;

        void Start()
        {
        }

        void OnEnable()
        {
            s_instance = this;

            if (!_mrukInstance)
            {
                Debug.LogError($"{nameof(QRCodeManager)} requires an MRUK object in the scene!");
                return;
            }

            _mrukInstance.SceneSettings.TrackableAdded.AddListener(OnTrackableAdded);
            _mrukInstance.SceneSettings.TrackableRemoved.AddListener(OnTrackableRemoved);
        }

        void Update()
        {
        }


        void OnDestroy()
            => s_instance = null;

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
            var instance = Instantiate(_qrCodePrefab, trackable.transform);
            var qrCode = instance.GetComponent<QRCode>();
            qrCode.Initialize(trackable);
            instance.GetComponent<Bounded2DVisualizer>().Initialize(trackable);

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
        }

        public static bool TrackingEnabled
        {
            get => s_instance && s_instance._mrukInstance && s_instance._mrukInstance.SceneSettings.TrackerConfiguration.QRCodeTrackingEnabled;
            set
            {
                if (!s_instance || !s_instance._mrukInstance)
                {
                    return;
                }
                var config = s_instance._mrukInstance.SceneSettings.TrackerConfiguration;
                config.QRCodeTrackingEnabled = value;
                s_instance._mrukInstance.SceneSettings.TrackerConfiguration = config;
            }
        }

        public string ToggleQRTracking(string message)
        {
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


//         public static void RequestRequiredPermissions(Action<bool> onRequestComplete)
//         {
//             if (!s_instance)
//             {
//                 Debug.LogError($"[QRCodeManager] {nameof(RequestRequiredPermissions)} failed; no QRCodeManager instance.");
//                 return;
//             }

// #if UNITY_EDITOR
//             const string kCantRequestMsg =
//                 "Cannot request Android permission when using Link or XR Sim. " +
//                 "For Link, enable the spatial data permission from the Link app under Settings > Beta > Spatial Data over Meta Quest Link. " +
//                 "For XR Sim, no permission is necessary.";

//             Debug.LogWarning(kCantRequestMsg);

//             onRequestComplete?.Invoke(HasPermissions);
// #else
//             Debug.Log($"Requesting {ScenePermission} ... (currently: {HasPermissions})");

//             var callbacks = new UnityEngine.Android.PermissionCallbacks();
//             callbacks.PermissionGranted += perm => Debug.Log($"{perm} granted");

//             var msgDenied = $"{ScenePermission} denied. Please press the 'Request Permission' button again.";
//             var msgDeniedPermanently = $"{ScenePermission} permanently denied. To enable:\n" +
//                                        $"    1. Uninstall and reinstall the app, OR\n" +
//                                        $"    2. Manually grant permission in device Settings > Privacy & Safety > App Permissions.";

// #if !UNITY_6000_0_OR_NEWER
//             callbacks.PermissionDenied += _ => Debug.LogError(msgDenied);
//             callbacks.PermissionDeniedAndDontAskAgain += _ => Debug.LogError(msgDeniedPermanently);
// #else
//             callbacks.PermissionDenied += perm =>
//             {
//                 // ShouldShowRequestPermissionRationale returns false only if
//                 // the user selected 'Never ask again' or if the user has never
//                 // been asked for the permission (which can't be the case here).
//                 Debug.LogError(
//                     UnityEngine.Android.Permission.ShouldShowRequestPermissionRationale(perm)
//                         ? msgDenied
//                         : msgDeniedPermanently);
//             };
// #endif // UNITY_6000_0_OR_NEWER

//             if (onRequestComplete is not null)
//             {
//                 callbacks.PermissionGranted += _ => onRequestComplete(HasPermissions);
//                 callbacks.PermissionDenied += _ => onRequestComplete(HasPermissions);
// #if !UNITY_6000_0_OR_NEWER
//                 callbacks.PermissionDeniedAndDontAskAgain += _ => onRequestComplete(HasPermissions);
// #endif // UNITY_6000_0_OR_NEWER
//             }

//             UnityEngine.Android.Permission.RequestUserPermission(ScenePermission, callbacks);
// #endif // UNITY_EDITOR
//         }
    }
}
