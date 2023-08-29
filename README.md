![microwar-image](https://github.com/picoxr/MicroWar/blob/74c764006aad1b8d2b3008aa9552217a59e32b49/microwar_img.png)
# MicroWar: Showcasing the Power of PICO Integration SDK
Welcome to MicroWar, an exciting game project meticulously crafted using the PICO Integration SDK. Our primary objective is to present an immersive experience that spotlights the breadth and depth of capabilities offered by the PICO Integration SDK. This single, consolidated project encapsulates all the key features, demonstrating their integration potential for fellow developers.

## About MicroWar
MicroWar isn't just a game; it's a testament to the versatility of the PICO Integration SDK. Through this project, we aim to provide developers with a comprehensive demonstration of how various SDK features can seamlessly come together to create captivating VR experiences.

## Key Objectives
- Showcasing Diversity: MicroWar employs different aspects of the PICO Integration SDK to exemplify its diverse functionalities. From audio integration to user input and visual effects, every corner of the SDK is explored.

- Educational Resource: This project is designed as an educational resource for both newcomers and experienced developers. By dissecting the code and mechanics, you'll gain insights into best practices for integrating PICO's capabilities.

- Inspiring Creativity: We believe that seeing these features in action will inspire new and imaginative applications. MicroWar isn't just a showcase; it's a launchpad for your next groundbreaking VR project.

## Get Involved
We value your questions and ideas. If you're curious about specific aspects of MicroWar or have innovative suggestions, don't hesitate to get in touch. Reach out to us at pico_devsupport@bytedance.com, and let's collaborate to elevate the world of VR development together.

Thank you for your interest in MicroWar and the PICO Integration SDK. Let's embark on a journey of immersive possibilities!
## Table of Contents
- [Demo](#demo)
- [Getting Started]
- [Dependency]
- [Project Structure]
- [Features](#features)
- [Game Instruction]
- [Download APK](#download-apk)
## Demo
![Demo-video](https://bytedance.us.feishu.cn/file/WTJ4bLYCUoPPfuxgWFtuAH0ssUd?from=from_copylink)

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
- Please follow the instructions here to [create a developer account](https://developer-global.pico-interactive.com/document/unity/create-a-developer-account-organization-and-app/).
- Open the project with Unity Editor
- Change the platform to Android in Build Setting
- Please follow the instructions here to [complete project settings](https://developer-global.pico-interactive.com/document/unity/complete-project-settings/).
- Configuring the application on Pico Developer Console along with the Achievements, Leaderboards, Access Rights for multiplayer etc.
#### Run in Unity Editor via Live Preview Plug-in (If you have PDC installed)
Launch the PDC tool and connect your headset to computer with a USB cable.
![PDC-image](https://p9-arcosite.byteimg.com/tos-cn-i-goo7wpa0wc/efead1eae68b4830a1655e43ae919fec~tplv-goo7wpa0wc-image.image)
Open the target scene and click the Play button at the top of the scene.
![Play-image](https://p9-arcosite.byteimg.com/tos-cn-i-goo7wpa0wc/b1c8580ed5dd49d1ae842e15c774314e~tplv-goo7wpa0wc-image.image)
In streaming Authorize pop-up window appears on the HMD, click Permit.
![Permit-iamge](https://p9-arcosite.byteimg.com/tos-cn-i-goo7wpa0wc/734027a3322841a2b7c0ab223797d179~tplv-goo7wpa0wc-image.image)
...
#### Get a build and run on the headset.
In Unity Editor, click "File/Build Settings/Build And Run". Make sure the platform is set to Android.
![BuildAndRun-image](https://github.com/picoxr/MicroWar/blob/b00074f5a4166b30d76f22dad78b471a88a43271/BuildAndRun.png)
...
## Dependency
- PICO Integration SDK 2.3.0.
- Live Preview Plug-in
- XR Hands 1.2.1
- XR Interaction Toolkit 2.4.3
- Netcode for GameObjects 1.5.2
## Project Structure
- Boot Scene
  - The scene is used to initialize platform service.
- Main Scene
  - The Scene which the main gameplay happens in.
## Features
- Input & tracking
  - Controller Input
  - Haptic feedback
  - Hand Tracking
  - Tracking Origin
  - Eye Tracking
  - Body Tracking (Swift)
- Platform Service
  - Real Time Communication
  - Multiplayer
  - Achievements
  - Leaderboard
## Game Instruction
- Game UI
  - Pick the game mode and check leaderboard and achiement
  - ![UI-image](https://github.com/picoxr/MicroWar/blob/342d25e3b8f80e2728ce4598e86e8c20a3c63de2/Documentation/Files/UI.png)
- Gameplay
  - Check Vehicle status and Pick Vehicle
  - ![PickVehicle-image](https://github.com/picoxr/MicroWar/blob/342d25e3b8f80e2728ce4598e86e8c20a3c63de2/Documentation/Files/Check%26PickVechicle.jpeg)
  - Battle with instruction
  - ![Battle-image](https://github.com/picoxr/MicroWar/blob/342d25e3b8f80e2728ce4598e86e8c20a3c63de2/Documentation/Files/GamePlay.jpeg)

## Assets used in this project
- Assets from [Kenny.nl](https://www.kenney.nl/)

## Download APK
- [Download APK - China Version](link-to-apk)
- [Download APK - Global Version](link-to-apk)

