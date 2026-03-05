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

using System;
using Meta.XR.MRUtilityKit;
using Meta.XR.Samples;

using UnityEngine;


namespace IRIS.MetaQuest3.QRCodeDetection
{
    [MetaCodeSample("MRUKSample-QRCodeDetection")]
    public class Bounded2DVisualizer : MonoBehaviour
    {
        [SerializeField]
        LineRenderer _lineRenderer;
        [SerializeField]
        RectTransform _canvasRect;
        [SerializeField] 
        GameObject _axes;
        [SerializeField, Tooltip("(in Canvas-local units)")]
        Vector3 _canvasOffset = new(0f, -15f, 0f);

        MRUKTrackable _trackable;

        Rect _box;

        public void Initialize(MRUKTrackable trackable)
        {
            _trackable = trackable;

            if (trackable.PlaneBoundary2D == null && trackable.PlaneRect == null)
            {
                Debug.LogWarning($"{trackable} is missing a plane component.");
            }
            else
            {
                UpdateBoundingBox();
            }
        }

        void Update()
        {
            if (!_trackable)
            {
                return;
            }
            UpdateAxesRotation();

            UnityEngine.Assertions.Assert.IsTrue(_trackable.PlaneRect.HasValue);
            _box = _trackable.PlaneRect.Value;

            UpdateBoundingBox();
            if (!_canvasRect)
            {
                return;
            }

            _canvasRect.localPosition = new Vector3(
                x: _box.center.x + _canvasOffset.x * _canvasRect.localScale.x,
                y: _box.yMin + _canvasOffset.y * _canvasRect.localScale.y,
                z: _canvasOffset.z * _canvasRect.localScale.z
            );
        }

        private void UpdateAxesRotation()
        {
            // 1. Get the current forward direction
            Vector3 currentForward = _axes.transform.forward;

            // 2. Project this vector onto the horizontal plane (XZ)
            // This removes the Y component so the vector is "flat"
            Vector3 forwardOnPlane = Vector3.ProjectOnPlane(currentForward, Vector3.up);

            // Handle the edge case where the object was looking straight up or down
            if (forwardOnPlane.sqrMagnitude < 0.0001f)
            {
                // If the projected vector is too small, default to some horizontal direction
                // as rotation around Y axis won't matter in this case
                Vector3 currentUp = _axes.transform.up;
                forwardOnPlane = Vector3.ProjectOnPlane(currentUp, Vector3.up);
            }

            // 3. Create a new rotation
            // "Look in the direction of forwardOnPlane, but keep the head up (Vector3.up)"
            _axes.transform.rotation = Quaternion.LookRotation(forwardOnPlane, Vector3.up);
        }

        void UpdateBoundingBox()
        {
            _lineRenderer.positionCount = 4;
            _lineRenderer.SetPosition(0, new Vector3(_box.x, _box.y, 0));
            _lineRenderer.SetPosition(1, new Vector3(_box.x + _box.width, _box.y, 0));
            _lineRenderer.SetPosition(2, new Vector3(_box.x + _box.width, _box.y + _box.height, 0));
            _lineRenderer.SetPosition(3, new Vector3(_box.x, _box.y + _box.height, 0));
        }
    }
}
