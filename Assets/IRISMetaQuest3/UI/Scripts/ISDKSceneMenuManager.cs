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

using UnityEngine;

namespace Oculus.Interaction.Samples
{
    /// <summary>
    /// The ISDK Scene Manager handles the enabling and disabling
    /// of the ISDK Menu panel and plays audio for each action.
    /// </summary>
    public class ISDKSceneMenuManager : MonoBehaviour
    {
        public bool blockMenuToggle = false;

        /// <summary>
        /// The Parent Object of the Menu
        /// </summary>
        [Tooltip("The parent object of the menu")]
        [Header("Place the grabbable parent object here")]
        [SerializeField]
        private GameObject _menuParent;

        /// <summary>
        /// The audio to play when showing the menu panel
        /// </summary>
        [Tooltip("The audio to play when showing the menu panel")]
        [Header("Place the menu open audio here")]
        [SerializeField]
        private AudioSource _showMenuAudio;

        /// <summary>
        /// The audio to play when hiding the menu panel
        /// </summary>
        [Tooltip("The audio to play when hiding the menu panel")]
        [Header("Place the menu hide audio here")]
        [SerializeField]
        private AudioSource _hideMenuAudio;

        /// <summary>
        /// The location the menu should be spawning at
        /// </summary>
        [Tooltip("The location the menu should be spawning at")]
        [Header("The location the menu should be spawning at")]
        [SerializeField]
        private GameObject _spawnPoint;

        // <summary> The head transform of the user</summary>
        [Tooltip("The head transform of the user")]
        [Header("The head transform of the user")]
        [SerializeField] 
        private Transform headTransform;

        protected bool _started = false;
        protected virtual void Start()
        {
            this.BeginStart(ref _started);
            this.AssertField(_menuParent, nameof(_menuParent));
            this.AssertField(this._showMenuAudio, nameof(_showMenuAudio));
            this.AssertField(this._hideMenuAudio, nameof(_hideMenuAudio));
            this.AssertField(this._spawnPoint, nameof(_spawnPoint));

            this.EndStart(ref _started);
        }

        /// <summary>
        /// Show/hide the menu.
        /// </summary>

        public void ToggleMenu()
        {
            if (blockMenuToggle)
            {
                return;
            }
            if (_menuParent.activeSelf)
            {
                _hideMenuAudio.Play();
                _menuParent.SetActive(false);
            }
            else
            {
                _showMenuAudio.Play();
                _menuParent.transform.position = _spawnPoint.transform.position;
                // look at user head
                Vector3 lookPos = headTransform.position - _menuParent.transform.position;
                // lookPos.y = 0; // keep the menu upright
                Quaternion rotation = Quaternion.LookRotation(-lookPos);
                _menuParent.transform.rotation = rotation;
                _menuParent.SetActive(true);
            }
        }

        #region Injects
        public void InjectAllMenuItems(GameObject parent,
            AudioSource show, AudioSource hide, GameObject spawnpoint)
        {
            InjectMenuParent(parent);
            InjectShowAudio(show);
            InjectHideAudio(hide);
            InjectSpawnPoint(spawnpoint);
        }

        public void InjectMenuParent(GameObject parent)
        {
            _menuParent = parent;
        }

        public void InjectShowAudio(AudioSource show)
        {
            _showMenuAudio = show;
        }

        public void InjectHideAudio(AudioSource hide)
        {
            _showMenuAudio = hide;
        }

        public void InjectSpawnPoint(GameObject spawnpoint)
        {
            _menuParent = spawnpoint;
        }
        #endregion
    }
}
