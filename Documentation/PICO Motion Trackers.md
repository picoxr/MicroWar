# PICO Motion Tracker 
> [!NOTE]
> **Body tracking** is a motion capture technology that collects a user's body positions, converting them to positions and actions within a virtual environment. Body tracking enables a user to run, kick, walk, and perform other actions in a VR scene, enriching the user's interaction with your app.
>
> PICO's body tracking capability requires **PICO Motion Tracker** - an accessory that captures a user's body positions. Body tracking APIs convert body positions into pose data for body joints, which serves as the input for your app. You can also use body tracking APIs to retrieve PICO Motion Tracker's connection status, battery level, and calibration status.

---
> [!IMPORTANT]
> Before proceeding, we suggest taking a look at the **[PICO Body Tracking SDK Reference](https://developer-global.pico-interactive.com/document/unity/body-tracking/)**. This resource will walk you through the steps of enabling body tracking, providing valuable information about prerequisites and fundamental concepts.

# MicroWar PICO Motion Tracker Integration
In the context of MicroWar's mechanics, we introduce power-up crates containing health and shield bonuses, designed to enhance gameplay subtly.
As players gather crates from the map, these crates dynamically emerge before them at an augmented scale.
>
With a swift tap of your hands or a spirited **kick** of your feet, the crates shatter open, revealing and initiating the concealed power-up within.
For the part that involves feet we use the data we collect from **PICO Motion Trackers**.

## Related Scripts
- **`SwiftTrackerManager.cs`**<br>
  SwiftTrackerManager is responsible for reading the body tracking data and updating the `Transforms` of the `GameObjects` which represent body parts in the game. In our case; those are left and right feet.
  ``` c#
        //Read body tracking data each frame
        private void Update()
        {
            GetBodyTrackingData();
        }

        private void GetBodyTrackingData()
        {
            if (!bothTrackersConnected) return;

            PXR_Input.GetBodyTrackingPose(PXR_System.GetPredictedDisplayTime(), ref bodyTrackerResult);

            UpdateBodyTrackingForObjects();
        }
  ```
  **Debug Mode:** Simply add **`SWIFT_DEBUG`** compiler directive to the project to enable the skeleton that visualizes the movement data received. (Project Settings->Player->Scripting Define Symbols)
  
- **`TrackedBodyPart.cs`** <br>
  We created this component to represent feet objects in the game. `TrackedBodyPart` derives from `MotionTrackedObject`.
- **`MotionTrackedObject.cs`** <br>
  This component lets us record the position/rotation history of a GameObject. We utilize this class to calculate the hit speed of feet and hands when they collide with a Crate (aka. `HittableObject`).
  - We ignore the hits that are under a certain threshold. `HittableObject.displacementThreshold`
- **`HittableObject.cs`** <br>
  We simply attach this component to the objects we want hands and feet to interact with.

## Related GameObjects in the Scene
![microwar_img.png](/Documentation/Files/SwiftDocumentation.png)

## Related Prefabs
![PowerUpCrate.png](/Documentation/Files/PowerUpCrate.png)
