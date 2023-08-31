# Hand Tracking

Experience the next level of immersion in the VR realm by embracing Hand Tracking, a alternative to traditional controllers.

## MicroWar Hand Tracking Integration

In MicroWar, we harness the power of PICO Integration 2.3.0, XR Interaction Toolkit 2.4.3, and XR Hands 1.2.1 to integrate the prowess of hand tracking. Remember to check the hand tracking on PXR_Manager.
![https://github.com/picoxr/MicroWar/blob/4acfc63d30751bad8755fd81411f7340ad3e269b/Documentation/Files/PXR_Manager_HandTracking.png](https://github.com/picoxr/MicroWar/blob/4acfc63d30751bad8755fd81411f7340ad3e269b/Documentation/Files/PXR_Manager_HandTracking.png)
## Scene Setup

To delve into the realm of Hand Tracking, initiate the process by adding the hands prefabs located at "Packages/com.unity.xr.picoxr/Assets/Resources/Prefabs" to your primary scene. This pivotal step empowers you to capture the precise position and pinch state of each hand.

For the interaction with Unity UI elements, simply affix the Poke Interactor prefab beneath your hands.

Moreover, we created the MicroHandInteractor prefab—a creation dedicated to grab vehicles via a pinch gesture within the game. Attach this prefab to the hand wrist and assign the appropriate hand reference on the component.\
![https://github.com/picoxr/MicroWar/blob/4acfc63d30751bad8755fd81411f7340ad3e269b/Documentation/Files/HandTracking.png](https://github.com/picoxr/MicroWar/blob/4acfc63d30751bad8755fd81411f7340ad3e269b/Documentation/Files/HandTracking.png)
## Related Prefabs
![https://github.com/picoxr/MicroWar/blob/4acfc63d30751bad8755fd81411f7340ad3e269b/Documentation/Files/TankHandTracking.png](https://github.com/picoxr/MicroWar/blob/4acfc63d30751bad8755fd81411f7340ad3e269b/Documentation/Files/TankHandTracking.png)
## Key Scripts and Logic

### TankMovement_Player.cs

This script serves as an embodiment of how we orchestrate the tank's movement via the left hand. We commence by extracting the pinch state from PXR_Hand. Upon confirming a pinch action, we procure the hand's position and calculate the ensuing movement:
   
   ```csharp
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
   ```
### TankShooting_Player.cs
This script shows how to charge and shoot with the right hand. By evaluating the pinch state, we emulate the trigger state to execute charge and shooting actions:
```csharp
 if (PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.HandTrackingActive)
            {
                m_triggerCurState = GameManager.Instance.RightHandInteractor.m_Hand.PinchStrength > 0;
            }
```
### MicroWarHand.cs
For game objects with a collider and the TankSelectorBehaviour.cs script, the MicroWarHand script outlines a method to grasp them upon collision:
```csharp
  private void OnTriggerStay(Collider other)
    {
        //Debug.LogWarning(other.name + "OnTriggerStay");
        if (m_Hand != null && m_Tank)
        {
            if(m_Hand.PinchStrength > 0 && !m_IsTankInHand)
            {
                Debug.LogWarning("Grab Tank!");
                m_Tank.transform.parent = transform;
                m_Tank.transform.localPosition = Vector3.zero;
                m_Tank.transform.localRotation = Quaternion.identity;
                m_IsTankInHand = true;
                m_Tank.SelectEntered();
            }
            if(m_Hand.PinchStrength == 0 && m_IsTankInHand)
            {
                m_IsTankInHand = false;
                Debug.LogWarning("Release Tank!");
                m_Tank.SelectExited();
                
            }
        }
```



