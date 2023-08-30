![microwar-image](https://github.com/picoxr/MicroWar/blob/74c764006aad1b8d2b3008aa9552217a59e32b49/microwar_img.png)
# MicroWar
## About MicroWar
Welcome to MicroWar, an VR game project implemented using the PICO Integration SDK. Our primary objective is to present an immersive experience that spotlights the breadth and depth of capabilities offered by the PICO Integration SDK. This single project encapsulates all the key features, demonstrating their integration potential for fellow developers.

## Get Involved
We value your questions and ideas. If you're curious about specific aspects of MicroWar or have innovative suggestions, don't hesitate to get in touch. Reach out to us at pico_devsupport@bytedance.com, and let's collaborate to elevate the world of VR development together.

Thank you for your interest in MicroWar and the PICO Integration SDK. Let's embark on a journey of immersive possibilities!
## Table of Contents
- [Demo](#demo)
- [Getting Started](#getting-started)
- [Dependency](#dependency)
- [Features](#features)
- [Game Instruction](#game-instruction)
- [Download APK](#download-apk)
- [Known Issues](#known-issues)
## Demo
[![Demo](https://img.youtube.com/vi/MtnugBt0IuQ/0.jpg)](https://www.youtube.com/watch?v=MtnugBt0IuQ)

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
  - Haptic feedback
  - Hand Tracking
  - Tracking Origin
  - Eye Tracking
  - [PICO Motion Tracker Integration](https://github.com/picoxr/MicroWar/blob/bdc2762d5d03262e497725e5183bca76c384ac47/Documentation/PICO%20Motion%20Trackers.md)
- [Platform Service](
https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/MicroWar%20Platform%20Service%20Architecture.md)
  - [Initialization And Login](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Initialization%20And%20Login.md)
  - [Real Time Communication](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/RTC%20(Real-Time%20communication).md)
  - [Rooms](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Rooms.md)
  - [Multiplay](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Multiplay.md)
  - Achievements
  - Leaderboard
## Game Instruction
- Game UI
  - Pick the game mode and check leaderboard and achiement
  - ![MainMenu-Image](https://github.com/picoxr/MicroWar/blob/e0c1b5d000392a340ea6d71b6f86e9a1f07c41d3/Documentation/Files/mainMenu.png)
- Gameplay
  - Check Vehicle status and Pick Vehicle
  - ![PickVehicle-image](https://github.com/picoxr/MicroWar/blob/342d25e3b8f80e2728ce4598e86e8c20a3c63de2/Documentation/Files/Check%26PickVechicle.jpeg)
  - Battle with instruction
  - ![Battle-image](https://github.com/picoxr/MicroWar/blob/342d25e3b8f80e2728ce4598e86e8c20a3c63de2/Documentation/Files/GamePlay.jpeg)

## Assets used in this project
- Assets from [Kenny.nl](https://www.kenney.nl/)
- SFX from [OpenGameArt](https://opengameart.org/), [Sonniss - Game Audio GDC](https://sonniss.com/gameaudiogdc)
- Prefab Lightmap Data Script: https://github.com/Ayfel/PrefabLightmapping/blob/master/PrefabLightmapData.cs

## Download APK
- [Download APK - China Version](link-to-apk)
- [Download APK - Global Version](link-to-apk)

## Known Issues

