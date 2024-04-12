using Unity.XR.PXR;
using UnityEngine;

public class InputTypeChangeHandler : MonoBehaviour
{
    public GameObject leftHandMeshGO;
    public GameObject rightHandMeshGO;


    private void Awake()
    {
        PXR_Plugin.System.InputDeviceChanged += OnDeviceInputChanged;
        OnDeviceInputChanged((int)PXR_HandTracking.GetActiveInputDevice());
    }

    private void OnDeviceInputChanged(int inputMode)
    {
        switch (inputMode)
        {
            case 0://HMD
            case 1://Controllers
                ShowHideHands(false);
                break;
            case 2://Hands
                ShowHideHands(true);
                break;
        }
    }

    private void ShowHideHands(bool isShow)
    {
        leftHandMeshGO.SetActive(isShow);
        rightHandMeshGO.SetActive(isShow);
    }

    private void OnDestroy()
    {
        PXR_Plugin.System.InputDeviceChanged -= OnDeviceInputChanged;
    }
}
