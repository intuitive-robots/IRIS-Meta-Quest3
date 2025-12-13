using IRIS.Node;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace IRIS.MetaQuest3.UI
{
    public class MQ3NamePopupManager : Singleton<MQ3NamePopupManager>
    {
        [SerializeField] private GameObject nameChangePopup;
        [SerializeField] private TMP_InputField appNameInput;
        [SerializeField] private Transform headTransform;
        [SerializeField] private GameObject _spawnPoint;
        // Start is called once before the first execution of Update after the MonoBehaviour is created

        void Start()
        {
            if (!PlayerPrefs.HasKey("HostName"))
            {   
                string name = IRISXRNode.Instance.localInfo.name;
                OpenNameChangePopup(name);
            }
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        public void OpenNameChangePopup(string currentName)
        {
            if (nameChangePopup != null)
            {
                // Position the popup in front of the user
                if (headTransform != null)
                {
                    nameChangePopup.transform.position = _spawnPoint.transform.position;
                    // look at user head
                    Vector3 lookPos = headTransform.position - nameChangePopup.transform.position;
                    // lookPos.y = 0; // keep the menu upright
                    Quaternion rotation = Quaternion.LookRotation(-lookPos);
                    nameChangePopup.transform.rotation = rotation;
                    nameChangePopup.SetActive(true);
                }
                if(appNameInput != null)
                {
                    appNameInput.text = currentName;
                }
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
            if (nameChangePopup != null)
            {
                nameChangePopup.SetActive(false);
            }
        }
    }
}
