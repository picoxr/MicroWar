# PICO Motion Tracker 
> [!NOTE]
> **Body tracking** is a motion capture technology that collects a user's body positions, converting them to positions and actions within a virtual environment. Body tracking enables a user to run, kick, walk, and perform other actions in a VR scene, enriching the user's interaction with your app.
>
> PICO's body tracking capability requires **PICO Motion Tracker** - an accessory that captures a user's body positions. Body tracking APIs convert body positions into pose data for body joints, which serves as the input for your app. You can also use body tracking APIs to retrieve PICO Motion Tracker's connection status, battery level, and calibration status.
>
> Check out the [PICO Body Tracking SDK Reference](https://developer-global.pico-interactive.com/document/unity/body-tracking/) for more details.

# MicroWar PICO Motion Tracker Integration
In the context of MicroWar's mechanics, we introduce power-up crates containing health and shield bonuses, designed to enhance gameplay subtly.
As players gather crates from the map, these crates dynamically emerge before them at an augmented scale.
With a swift tap of your hands or a spirited kick of your feet, the crates shatter open, revealing and initiating the concealed power-up within.
For the part that involves feet we use the data we collect from PICO Motion Trackers. See `SwiftTrackerManager.cs`
