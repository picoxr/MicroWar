using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.XR;

namespace MicroWar
{
    public class TankShooting_Player : TankShooting
    {
        private bool m_triggerValue;
        private bool m_triggerCurState;
        private bool m_triggerPrevState;
        private int m_chargingHapticsID;
        private int m_fireHapticsID;
        private Vector2 axis2D_R;
        private bool axis2DClick_R;
        private InputDevice rightController;

        private Vector3 spawnPosForward;


        private void Start()
        {
            base.Start();
            spawnPosForward = transform.forward;
            PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.RightController, m_ChargingClip, PXR_Input.ChannelFlip.No, ref m_chargingHapticsID, PXR_Input.CacheType.CacheNoVibrate);
            PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.RightController, m_FireClip, PXR_Input.ChannelFlip.No, ref m_fireHapticsID, PXR_Input.CacheType.CacheNoVibrate);
            rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        }
        // Update is called once per frame
        void Update()
        {
#if !UNITY_EDITOR
            if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.HandTrackingActive)
            {
                m_triggerCurState = GameManager.Instance.RightHandInteractor.m_Hand.PinchStrength > 0;
            }
            else if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
            {
#endif
                if (rightController != null)
                {
                    m_triggerCurState = rightController.TryGetFeatureValue(CommonUsages.triggerButton, out m_triggerValue) && m_triggerValue;
                    rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis2D_R);
                    rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axis2DClick_R);
                    //m_TurnInputValue = axis2D_R.x;

                    if (axis2DClick_R)
                    {
                        m_Turret.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                }
#if !UNITY_EDITOR
        }
#endif      
            if (axis2D_R != Vector2.zero)
            {
                Vector3 lookDirection = new Vector3(axis2D_R.x, 0, axis2D_R.y);
       
                Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up) * Quaternion.FromToRotation(Vector3.forward, spawnPosForward); ;

                float step = m_TurnSpeed * Time.deltaTime;
                m_Turret.rotation = Quaternion.RotateTowards(lookRotation, m_Turret.rotation, step);
            }

            // The slider should have a default value of the minimum launch force.
            m_AimSlider.value = m_MinLaunchForce;

            // If the max force has been exceeded and the shell hasn't yet been launched...
            if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
            {
                // ... use the max force and launch the shell.
                m_CurrentLaunchForce = m_MaxLaunchForce;
                Fire();
                if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
                    PXR_Input.StartHapticBuffer(m_fireHapticsID);
            }
            // Otherwise, if the fire button has just started being pressed...
            else if (m_triggerCurState && !m_triggerPrevState)
            {
                // ... reset the fired flag and reset the launch force.
                m_Fired = false;
                m_CurrentLaunchForce = m_MinLaunchForce;

                // Change the clip to the charging clip and start it playing.
                m_ShootingAudio.clip = m_ChargingClip;
                m_ShootingAudio.Play();
                if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
                    PXR_Input.StartHapticBuffer(m_chargingHapticsID);
            }
            // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
            else if (m_triggerCurState && m_triggerPrevState && !m_Fired)
            {
                // Increment the launch force and update the slider.
                m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

                m_AimSlider.value = m_CurrentLaunchForce;
            }
            // Otherwise, if the fire button is released and the shell hasn't been launched yet...
            else if (!m_triggerCurState && m_triggerPrevState && !m_Fired)
            {
                // ... launch the shell.
                Fire();
                if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
                    PXR_Input.StartHapticBuffer(m_fireHapticsID);
            }
            m_triggerPrevState = m_triggerCurState;
        }
        
    }
}
