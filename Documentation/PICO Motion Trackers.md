# PICO Motion Tracker 
> [!NOTE]
> **Body tracking** is a motion capture technology that collects a user's body positions, converting them to positions and actions within a virtual environment. Body tracking enables a user to run, kick, walk, and perform other actions in a VR scene, enriching the user's interaction with your app.
>
> PICO's body tracking capability requires **PICO Motion Tracker** - an accessory that captures a user's body positions. Body tracking APIs convert body positions into pose data for body joints, which serves as the input for your app. You can also use body tracking APIs to retrieve PICO Motion Tracker's connection status, battery level, and calibration status.

> [!IMPORTANT]
> Before proceeding, we suggest taking a look at the **[PICO Body Tracking SDK Reference](https://developer-global.pico-interactive.com/document/unity/body-tracking/)**. This resource will walk you through the steps of enabling body tracking, providing valuable information about prerequisites and fundamental concepts.
__
# MicroWar PICO Motion Tracker Integration
In the context of MicroWar's mechanics, we introduce power-up crates containing health and shield bonuses, designed to enhance gameplay subtly.
As players gather crates from the map, these crates dynamically emerge before them at an augmented scale.
>
With a swift tap of your hands or a spirited **kick** of your feet, the crates shatter open, revealing and initiating the concealed power-up within.
For the part that involves feet we use the data we collect from **PICO Motion Trackers**.

## Related Scripts
- `SwiftTrackerManager.cs`
- `TrackedBodyPart.cs`
- `MotionTrackedObject.cs`
- `HittableObject.cs`

## Related GameObjects in the Scene
![https://github.com/picoxr/MicroWar/blob/74c764006aad1b8d2b3008aa9552217a59e32b49/microwar_img.png}](https://github.com/picoxr/MicroWar/blob/1e1f6a9e7a733df7b38d8af03c253e1fd0347548/Documentation/Files/SwiftDocumentation.png)
