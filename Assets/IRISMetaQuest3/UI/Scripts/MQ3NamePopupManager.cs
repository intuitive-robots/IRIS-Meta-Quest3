using IRIS.Node;
using Oculus.Interaction.Samples;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IRIS.MetaQuest3.UI
{
    [DefaultExecutionOrder(1000)]
    public class MQ3NamePopupManager : Singleton<MQ3NamePopupManager>
    {
        [SerializeField] private GameObject nameChangePopup;
        [SerializeField] private TMP_InputField appNameInput;
        [SerializeField] private Transform headTransform;
        [SerializeField] private GameObject _spawnPoint;
        [SerializeField] private ISDKSceneMenuManager isdkSceneMenuManager;

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void Start()
        {
            if (!PlayerPrefs.HasKey("HostName"))
            {   
                // // Wait unitl tracking is acquired to show the name change popup (necessary for floor level tracking)
                // OVRManager.TrackingAcquired += () => OpenNameChangePopup();
                OpenNameChangePopup();
            }
        }

        // Update is called once per frame
        void Update()
        {
            SetPose();
        }

        public void OpenNameChangePopup(string currentName = null)
        {
            isdkSceneMenuManager.blockMenuToggle = true;
            currentName ??= IRISXRNode.Instance.localInfo.nodeInfo.Name;
            
            if (nameChangePopup != null)
            {
                // Position the popup in front of the user
                SetPose();
                if (appNameInput != null)
                {
                    appNameInput.text = currentName;
                }
                nameChangePopup.SetActive(true);
            }
            // OVRManager.TrackingAcquired -= () => OpenNameChangePopup();
        }

        private void SetPose()
        {
            if (nameChangePopup.activeInHierarchy && headTransform is not null && nameChangePopup is not null && _spawnPoint is not null)
            {
                nameChangePopup.transform.position = _spawnPoint.transform.position;
                // look at user head
                Vector3 lookPos = headTransform.position - nameChangePopup.transform.position;
                // lookPos.y = 0; // keep the menu upright
                Quaternion rotation = Quaternion.LookRotation(-lookPos);
                nameChangePopup.transform.rotation = rotation;
                nameChangePopup.SetActive(true);
            }
        }

        public void SaveNameAndRestartChangePopup()
        {
            if (nameChangePopup != null)
            {
                IRISXRNode.Instance.Rename(appNameInput.text);
                
                // restart the app to apply the name change
                // Get the name of the current scene
                string currentSceneName = SceneManager.GetActiveScene().name;
                // Reload it
                SceneManager.LoadScene(currentSceneName);
            }
        }

        public void CloseNameChangePopup()
        {
            isdkSceneMenuManager.blockMenuToggle = false;
            if (nameChangePopup != null)
            {
                nameChangePopup.SetActive(false);
            }
        }
    }
}
