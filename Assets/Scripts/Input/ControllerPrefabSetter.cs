using System.Collections;
using System.Collections.Generic;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ControllerPrefabSetter : MonoBehaviour
{
    public ActionBasedController controllerLeft;
    public ActionBasedController controllerRight;

    public GameObject pico4PrefabLeft;
    public GameObject pico4PrefabRight;
    public GameObject pico4UPrefabLeft;
    public GameObject pico4UPrefabRight;

    
    void Start()
    {
        if (PXR_Input.GetControllerDeviceType() == PXR_Input.ControllerDevice.PICO_4)
        {
            controllerLeft.modelPrefab = pico4PrefabLeft.transform;
            controllerRight.modelPrefab = pico4PrefabRight.transform;
        }
        else 
        {
            controllerLeft.modelPrefab = pico4UPrefabLeft.transform;
            controllerRight.modelPrefab = pico4UPrefabRight.transform;
        }
    }

}
