# PICO Controller Input

Controllers serve as the primary means for users to interact with the virtual world, enabling various actions within your applications. PICO controllers utilize the Unity XR Input System's provided keycodes to facilitate these interactions.

## MicroWar PICO Controller Input Integration

In MicroWar, we leverage XR Interaction Toolkit 2.4.3 to seamlessly integrate controller input. Here's how the controller input enhances gameplay:

- **Left Thumbstick**: Controls vehicle movement.
- **Right Thumbstick**: Manages turret rotation.
- **Right Trigger**: Holding and pressing enables charging and firing shells.
- **Grip Buttons**: Holding both allows vehicle pickup during vehicle selection.
## project Setup
Normally, the controllers under XROrgin are good enough. In MicroWar, we need to do two more things to give the players ability to grab the tank with grip button: 
1) Attach Direct Interactor prefab to contollers. 
>![https://github.com/picoxr/MicroWar/blob/f841b9ab8b017b23d525c7c0a72c5a0711ccb87c/Documentation/Files/DirectInteractor.png](https://github.com/picoxr/MicroWar/blob/f841b9ab8b017b23d525c7c0a72c5a0711ccb87c/Documentation/Files/DirectInteractor.png)
2) Attach a collider, XRGrabInteractable.cs, and TankSelectorBehaviour.cs to tank prefabs and set up Select Enter and Select Exit events.
>![https://github.com/picoxr/MicroWar/blob/a824c9f256bc72a19c1f24abaacff388a36a89a7/Documentation/Files/TankControllerInput.png](https://github.com/picoxr/MicroWar/blob/a824c9f256bc72a19c1f24abaacff388a36a89a7/Documentation/Files/TankControllerInput.png)
## Key Scripts and Logic

### TankMovement_Player.cs

This script exemplifies how we manage the tank's movement through the left controller's thumbstick:

1. Fetch the left controller:
   
   ```csharp
   leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
   ```
2. Retrieve primary2DAxis value from the left controller:
   
   ```csharp
    if (leftController != null)
    {
      leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis2D_L);
    }

   ```
3. Apply primary2DAxis value to tank movement and rotation:
   
   ```csharp
    Vector3 lookDirection = new Vector3(axis2D_L.x, 0, axis2D_L.y);
    lookDirection = Quaternion.Euler(GameManager.Instance.XROrigin.eulerAngles) * lookDirection;
    Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);

    float step = m_TurnSpeed * Time.deltaTime;
    transform.rotation = Quaternion.RotateTowards(lookRotation, transform.rotation, step);
    m_MovementInputValue = axis2D_L.magnitude;
   ```
### TankShooting_Player.cs
This script outlines turret rotation and firing mechanics using the right controller:

1. Fetch the right controller:

```csharp
rightController = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
```
2. Retrieve primary2DAxis value from the right controller:

```csharp
rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out axis2D_R);
```
3. Apply primary2DAxis value to turret rotation:

```csharp
if (axis2D_R != Vector2.zero)
{
    Vector3 lookDirection = new Vector3(axis2D_R.x, 0, axis2D_R.y);
    lookDirection = Quaternion.Euler(GameManager.Instance.XROrigin.eulerAngles) * lookDirection;
    Quaternion lookRotation = Quaternion.LookRotation(lookDirection, Vector3.up);
    
    float step = m_TurnSpeed * Time.deltaTime;
    m_Turret.rotation = Quaternion.RotateTowards(lookRotation, m_Turret.rotation, step);
}
```
4. Reset turret rotation using thumbstick click:

```csharp
rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out axis2DClick_R);
if (axis2DClick_R)
{
    m_Turret.localRotation = Quaternion.Euler(0, 0, 0);
}
```
5. Fire shells using trigger button:

```csharp
m_triggerCurState = rightController.TryGetFeatureValue(CommonUsages.triggerButton, out m_triggerValue) && m_triggerValue;
```
Please review the TankShooting_Player.cs script for detailed information on the charge and fire mechanics within the Update() method.

