using MicroWar;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

public class MicroWarHand : MonoBehaviour
{
    public PXR_Hand m_Hand;
    public ActionBasedController controller;
    public XRGrabInteractable interactable;
    private TankSelectorBehaviour m_Tank;
    private bool m_IsTankInHand = false;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.LogWarning(other.name + "Entered");
        m_Tank = other.GetComponent<TankSelectorBehaviour>();
    }
    private void OnTriggerStay(Collider other)
    {
        //Debug.LogWarning(other.name + "OnTriggerStay");
        if (m_Hand != null)
        {
            if(m_Hand.Pinch && !m_IsTankInHand)
            {
                m_Tank = other.GetComponent<TankSelectorBehaviour>();
                if (m_Tank != null)
                {
                    Debug.LogWarning("Grab Tank!");
                    m_Tank.transform.parent = transform;
                    m_Tank.transform.localPosition = Vector3.zero;
                    m_Tank.transform.localRotation = Quaternion.identity;
                    m_IsTankInHand = true;
                    m_Tank.SelectEntered();
                }
            }
            if(!m_Hand.Pinch && m_IsTankInHand)
            {
                m_Tank = other.GetComponent<TankSelectorBehaviour>();
                if (m_Tank != null)
                {
                    m_IsTankInHand = false;
                    Debug.LogWarning("Release Tank!");
                    m_Tank.SelectExited();
                }
                
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //Debug.LogWarning(other.name + "OnTriggerExit");
        m_Tank = null;
    }
}
