using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MicroWar.Platform;
using Unity.XR.PXR;
using UnityEngine.XR;
using MicroWar;
/// <summary>
/// Control the network player movement. Copied from TankMovement_Player.cs
/// TODO: Optimize this since it's not a good idea to handle mocement in differnt scripts.
/// NOTE: This scripts runs in client auth mode. But physics should run in server auth mode
/// </summary>
public class TankMovement_Multiplayer : MicroWar.TankMovement
{
    //Test Param Start
    //Movement 
    public bool serverAuthoritative = false;
    //Test Params End

    private Vector2 axis2D_L;
    private Vector2 axis2D_R;
    private float leftStickVertical;
    private float rightStickVertical;
    private float threshold = 0.01f;
    private int m_drivingHapticsID;
    private bool wasMoving = false;
    private bool isMoving = false;

    private InputDevice leftController;
    private InputDevice rightController;
    private float maximumMovement = 0.1f;
    private Vector3 leftHandStartPosition;
    private Vector3 leftHandCurrentPosition;
    private Vector3 leftHandMovement;

    private NetworkObject networkObject;
    private void Awake()
    {
        base.Awake();
        PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.LeftController, m_EngineDriving, PXR_Input.ChannelFlip.No, ref m_drivingHapticsID, PXR_Input.CacheType.CacheNoVibrate);
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
        networkObject = transform.GetComponent<NetworkObject>();
    }
    private void Update()
    {
        if (!networkObject.IsOwner) // Disable control if this client is not the owner of this tank.
            return;

        base.Update();
#if !UNITY_EDITOR
            if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.HandTrackingActive)
            {
                if (GameManager.Instance.LeftHandInteractor.m_Hand.PinchStrength > 0 || GameManager.Instance.LeftHandInteractor.m_Hand.Pinch)
                {
                    GameManager.Instance.InGameUIHandler.ShowHandTrackingUICanvas();
                    leftHandCurrentPosition = GameManager.Instance.LeftHandInteractor.m_Hand.handJoints[(int)HandJoint.JointThumbProximal].position;
                    leftHandMovement = leftHandCurrentPosition - leftHandStartPosition;
                    leftHandMovement = Vector3.ProjectOnPlane(leftHandMovement, Vector3.up);
                    if (leftHandMovement.magnitude > threshold)
                    {
                        axis2D_L.x = Mathf.Min(1, Mathf.Max(-1, leftHandMovement.x / maximumMovement));
                        axis2D_L.y = Mathf.Min(1, Mathf.Max(-1, leftHandMovement.z / maximumMovement));
                    }
                    else
                    {
                        axis2D_L = Vector2.zero;
                    }

                }
                else
                {
                    axis2D_L = Vector2.zero;
                    GameManager.Instance.InGameUIHandler.HideHandTrackingUICanvas();
                    leftHandStartPosition = GameManager.Instance.LeftHandInteractor.m_Hand.handJoints[(int)HandJoint.JointThumbProximal].position;
                    GameManager.Instance.InGameUIHandler.HandTrackingUICanvas.transform.position = leftHandStartPosition;
                }
            }
            else if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
#endif
        if (leftController != null)
        {
            leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis2D_L);
        }
        if (rightController != null)
        {
            rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis2D_R);
        }
        switch (GameManager.Instance.Settings.PlayerTankControlType)
        {
            case VehicleControlType.RemoteControlToyLeftControllerOnly:
                //Remote control toy left controller only
                m_MovementInputValue = axis2D_L.y;
                m_TurnInputValue = axis2D_L.x;
                break;
            case VehicleControlType.RemoteControlToyBothController:
                //Remote control toy left controller and right controller
                m_MovementInputValue = axis2D_L.y;
                m_TurnInputValue = axis2D_R.x;
                break;
            case VehicleControlType.RealisticControl:
                //Realistic control
                leftStickVertical = axis2D_L.y;
                rightStickVertical = axis2D_R.y;
                m_MovementInputValue = (leftStickVertical + rightStickVertical) * .5f;
                m_TurnInputValue = leftStickVertical - rightStickVertical;
                break;
            case VehicleControlType.VideoGameControl:
                //2.5D Video Game control
                if (axis2D_L != Vector2.zero)
                {
                    Vector3 lookDirection = new Vector3(axis2D_L.x, 0, axis2D_L.y);
                    lookDirection = Quaternion.Euler(MicroWar.GameManager.Instance.XROrigin.eulerAngles) * lookDirection;
                    Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
                    float step = m_TurnSpeed * Time.deltaTime;
                    transform.rotation = Quaternion.RotateTowards(lookRotation, transform.rotation, step);
                    m_MovementInputValue = axis2D_L.magnitude;
                }
                m_MovementInputValue = axis2D_L.magnitude;
                break;
            default:
                m_MovementInputValue = axis2D_L.y;
                m_TurnInputValue = axis2D_L.x;
                break;
        }
        if (m_MovementInputValue == 0 && m_TurnInputValue == 0)
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }

        if (!wasMoving && isMoving && PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
        {
            StartCoroutine(LoopDrivingHaptics());
        }

        if (wasMoving && !isMoving && PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
        {
            StopAllCoroutines();
            PXR_Input.StopHapticBuffer(m_drivingHapticsID);
        }

        wasMoving = isMoving;
    }
    IEnumerator LoopDrivingHaptics()
    {
        while (true)
        {
            PXR_Input.StartHapticBuffer(m_drivingHapticsID);
            yield return new WaitForSeconds(m_EngineDriving.length);
        }
    }

}
