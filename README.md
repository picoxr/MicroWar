![microwar-image](https://github.com/picoxr/MicroWar/blob/74c764006aad1b8d2b3008aa9552217a59e32b49/microwar_img.png)
# MicroWar
Welcome to MicroWar, an VR game project implemented using the PICO Integration SDK. Our primary objective is to present an immersive experience that spotlights the breadth and depth of capabilities offered by the PICO Integration SDK. This single project encapsulates all the key features, demonstrating their integration potential for fellow developers.

**`Trailer`**<br>

<a href="https://bytedance.us.feishu.cn/file/Sw1GbVhgvo51IAxhnCGu2Ercs7b" target="-blank"><img src="/Documentation/Files/VideoThumbnailPlayButton.png" padding="middle" width="1080px"></a>

## Table of Contents
- [Trailer](#MicroWar)
- [Getting Started](#getting-started)
- [Dependency](#dependency)
- [Features](#features)
- [Gameplay Instruction](#gameplay-instruction)
- [Download APK](#download-apk)

## Getting Started

### Prerequisites
Before you begin, ensure you have met the following requirements:
- PICO 4 / PICO 4 PRO
- Unity 2021.3.28f1
- PICO Developer Center (Optional)
### Getting The Code
First, ensure you have Git LFS installed by running this command:

`git lfs install`

Then, clone this repo using the "Code" button above, or this command:

`git clone https://github.com/picoxr/MicroWar`
### First Run
- Clone the project to your local folder.
- Open the project with Unity Editor
- Change the platform to Android in Build Setting
- Please follow the instructions here to [complete project settings](https://developer-global.pico-interactive.com/document/unity/complete-project-settings/).
- If you want to use your own APPID and platform services, please follow the instructions here to [create a developer account](https://developer-global.pico-interactive.com/document/unity/create-a-developer-account-organization-and-app/).
  and configure the application on Pico Developer Console along with the Achievements, Leaderboards, Access Rights for multiplayer etc.
#### Make a build and run on the headset.
In Unity Editor, click "File/Build Settings/Build And Run". Make sure the platform is set to Android.
#### Run in Unity Editor via Live Preview Plug-in (If you have PDC installed)
Please follow the insturctions here to [setup PDC](https://developer-global.pico-interactive.com/document/unity/pdc-basic-info/) and click play button in Boot Scene.
## Dependency
- PICO Integration SDK 2.3.0.
- PUI 5.7.0
- Swift SDK
- Live Preview Plug-in
- XR Hands 1.2.1
- XR Interaction Toolkit 2.4.3
- Netcode for GameObjects 1.5.2
## Features
- Input & tracking
  - [Controller Input](https://github.com/picoxr/MicroWar/blob/319859ca76dba927ba4c94c061d795b6b220cb4a/Documentation/Controller%20Input.md)
  - [Haptic feedback](https://github.com/picoxr/MicroWar/blob/aa5e77b89ef0681c1bc7e4014177213802bcf1b5/Documentation/Haptic%20feedback.md)
  - [Hand Tracking](https://github.com/picoxr/MicroWar/blob/aa5e77b89ef0681c1bc7e4014177213802bcf1b5/Documentation/Hand%20Tracking.md)
  - [Tracking Origin](https://github.com/picoxr/MicroWar/blob/aa5e77b89ef0681c1bc7e4014177213802bcf1b5/Documentation/TrackingOrigin.md)
  - [Eye Tracking](https://github.com/picoxr/MicroWar/blob/a2ce47280b570edcc76636f5365b5b0cba1c4061/Documentation/Eye%20Tracking.md)
  - [PICO Motion Tracker Integration](https://github.com/picoxr/MicroWar/blob/bdc2762d5d03262e497725e5183bca76c384ac47/Documentation/PICO%20Motion%20Trackers.md)
- [Platform Service](/Documentation/MicroWar%20Platform%20Service%20Architecture.md)
  - [Initialization And Login](/Documentation/Initialization%20And%20Login.md)
  - [Real Time Communication](/Documentation/RTC%20(Real-Time%20Communication).md)
  - [Rooms](/Documentation/Rooms.md)
  - [Multiplay](/Documentation/Multiplay.md)
  - [Achievements](https://github.com/picoxr/MicroWar/blob/169bc6d4743c3ff0f71db69e1212646269ead93d/Documentation/Achievement.md)
  - [Leaderboard](https://github.com/picoxr/MicroWar/blob/169bc6d4743c3ff0f71db69e1212646269ead93d/Documentation/Leaderboard.md)
## How To Play
### Game UI
Players have the option to navigate the UI using either controller line interactors or using Hand tracking poke gesture. The player have the ability to select two game modes on the main menu. In the ***single-player mode***, you can promptly initiate a local solo game, fighting with a formidable AI bot. Alternatively, in the ***multiplayer mode***, players can either create their own game room or join an existing one to engage in battles with other players. Of course, the option to include AI bots in multiplayer matches is also available.

Players also have the option to select and view the ***achievements*** they have earned or browse through the ***leaderboard*** from the main menu.

<img src="/Documentation/Files/UI.png" width="800px">
  
### Check Vehicle status and Pick Vehicle

After entering the game, players can choose from three vehicles with distinct attributes. Using either a controller or hand tracking, they can pick up the tank and place it onto the illuminated sphere on the battlefield. This action grants them control of the chosen vehicle for gameplay. Once any player achieves victory in three rounds, the game will conclude.

<img src="/Documentation/Files/TankSelector.png" width="800px">

### Gameplay Instruction

**`Vehicle control`**<br>

The ***Grab button*** on the controller is used to grab vehicles, while the ***Trigger button*** on the right controller is utilized to launch projectiles. The ***joysticks*** on the left and right controllers are employed to control the vehicle's movement direction and the turret's orientation, respectively.

In the gesture recognition mode, ***clenching the left hand into a fist*** and moving it over the suspended cursor will manipulate the vehicle to move in the corresponding direction. ***Pinching with the right hand*** triggers the firing of projectiles.

**`Power-ups`**<br>

Power-ups will randomly spawn on the battlefield. Players can guide their vehicles to collect these power-ups. Currently, the game features two main types of power-ups:

- ***Turret Activator***: This item can be used to activate one of the three turrets placed within the scene. The activated turret will automatically attack all enemies except the player.

- ***Crate***: This power-up generates a crate in front of the player. The player can break the crate using the controller or by tracking foot movements through the body tracker to acquire the bonus inside. There are two types of bonuses available: ***health regeneration*** and ***immunity to a single attack***.

<img src="/Documentation/Files/Battlefield_01_Text.png" width="800px">



## Assets used in this project
- Assets from [Kenny.nl](https://www.kenney.nl/)
- SFX from [OpenGameArt](https://opengameart.org/), [Sonniss - Game Audio GDC](https://sonniss.com/gameaudiogdc)
- Prefab Lightmap Data Script: https://github.com/Ayfel/PrefabLightmapping/blob/master/PrefabLightmapData.cs

## Download APK

- [Download APK](link-to-apk)

## Get Involved
We value your questions and ideas. If you're curious about specific aspects of MicroWar or have innovative suggestions, don't hesitate to get in touch. Reach out to us at pico_devsupport@bytedance.com, and let's collaborate to elevate the world of VR development together.

Thank you for your interest in MicroWar and the PICO Integration SDK. Let's embark on a journey of immersive possibilities!

