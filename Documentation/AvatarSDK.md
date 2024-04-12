# PICO Avatar SDK Integration
> [!NOTE]
> The PICO Unity Avatar SDK offers versatile functionalities, allowing users to immerse themselves in social interactions and roleplay within virtual environments, with features like customizable avatars, hand pose control, and full-body motion tracking, enhancing the user experience in diverse scenarios.

---
> Before proceeding, we suggest taking a look at the **[PICO Unity Avatar SDK Quickstart](https://developer.picoxr.com/document/unity-avatar/get-started-with-pico-avatar/)**. This article introduces the development environment for PICO Unity Avatar SDK features and the key steps to integrate PICO Avatars into your app.

# MicroWar Avatar SDK Features
In MicroWar, we utilized various functionalities of the SDK such as;
- Full-body representation. Full-Body IK ensures lifelike movements for avatars.
- Custom Hand Pose to demonstrate how to utilize static hand poses on the main user avatar.
- Avatar packet syncronization in the multiplayer mode (including lobby)
- Recalibrating the height of the main user avatar.
- Hiding the arms of the avatars when the hand tracking is active.


## Related Scripts
- **`AvatarAppLauncher.cs`**<br>
	- Base class containing the functionality that initializes and launches the Avatar SDK.
- **`AvatarManager.cs`**<br>
	- Derives from **`AvatarAppLauncher.cs`**. Provides the functionalities to load, unload and manage avatars.
- **`AvatarMultiplayerManager.cs`**<br>
	- Responsible for instantiating, syncronizing and managing the avatar instances in a multiplayer session.
- **`AvatarController.cs`**<br>
	- Gets instantiated for each avatar. Provides the functionalities to manage a single avatar.

## Related GameObjects and Prefabs
- AvatarManager and Custom Hand GameObjects: AvatarManager and AvatarMultiplayerManager components are attachted to AvatarManager GameObject.
 ![AvatarManager](/Documentation/Files/Avatars/AvatarManager.png)
---
- Avatar Prefab
 ![AvatarManager](/Documentation/Files/Avatars/AvatarPrefab.png)

