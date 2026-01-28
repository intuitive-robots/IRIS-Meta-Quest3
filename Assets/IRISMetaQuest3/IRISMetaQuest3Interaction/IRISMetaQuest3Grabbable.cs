using Oculus.Interaction;
using UnityEngine;
using IRIS.Node;

[RequireComponent(typeof(Grabbable))]
class IRISMetaQuest3Grabbable : MonoBehaviour
{
    public bool isGrabbable = false;
    [SerializeField] private Grabbable grabbable;
    [SerializeField] private GameObject _ISDK_RayGrabInteraction;

    private void Start()
    {
        grabbable = GetComponent<Grabbable>();
        IRISXRNode.Instance.ServiceManager.RegisterServiceCallback<string, string>("ToggleGrab", (message) =>
        {
            if (isGrabbable)
            {
                DisableGrab();
            }
            else
            {
                EnableGrab();
            }
            return "Grab state toggled";
        });
    }

    public void EnableGrab()
    {
        isGrabbable = true;
        // Additional logic for when the object is grabbable
        grabbable.enabled = true;
        if (_ISDK_RayGrabInteraction != null)
        {
            _ISDK_RayGrabInteraction.SetActive(true);
        }
    }

    public void DisableGrab()
    {
        isGrabbable = false;
        grabbable.enabled = false;
        // Additional logic for when the object is released
        if (_ISDK_RayGrabInteraction != null)
        {
            _ISDK_RayGrabInteraction.SetActive(false);
        }
    }
}