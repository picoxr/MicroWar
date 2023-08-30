using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using MicroWar;
namespace MicroWar
{
    public class TankSelectorBehaviour : MonoBehaviour
    {
        public Transform holder;
        public GameObject infoCanvas;
        public VehicleType m_Type;
        public void SelectEntered()
        {
            Debug.Log(gameObject.name + ": SelectEntered");
            GameManager.Instance.SetVehicleType(m_Type);
            //infoCanvas.SetActive(true);
        }
        public void SelectExited()
        {
            Debug.Log(gameObject.name + ": SelectExited");
            ResetTank();
        }

        public void ResetTank()
        {
            //infoCanvas.SetActive(false);
            if (holder != null)
                transform.parent = holder;
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        
    }
}
