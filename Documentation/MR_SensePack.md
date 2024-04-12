# Sense Pack Integration
> [!NOTE]
> **Sense pack** includes mixed reality-related features, including video seethrough, spatial anchors, and space calibration, enabling you to blend the real and virtual environments in a scene. The physical and digital objects in the scene coexist and interact in real time.

---
> [!IMPORTANT]
> The Mixed-Reality features are currently in an **experimental phase and may undergo modifications.**<br>

> Before proceeding, we suggest taking a look at the **[Sense Pack Quickstart](https://developer-global.pico-interactive.com/document/unity/sense-pack-quickstart/)**. This article introduces the development environment for sense pack features and the key steps to integrate them into your app.

# MicroWar Mixed-Reality Features
MicroWar utilizes **[Space Calibration](https://developer-global.pico-interactive.com/document/unity/space-calibration/)** Data and **[Video Seethrough](https://developer-global.pico-interactive.com/document/unity/seethrough/)** features of the Sense Pack.
In MR single-player mode;
- Video Seethrough is enabled.
- A custom LUT Texture is used to modify the video seethrough image to match the color theme of the game.
- The game looks for a table anchor in the space calibration data to place the battleground on.
- Occlusion material is used on the sofa as an example to showcase how to occlude virtual objects in a scene when they position behind a real-world object defined in the space calibration data.

## Related Scripts
- **`MixedRealityManager.cs`**<br>
	Responsible for;
	- Accessing space calibration data which consists of walls, windows, doors, tables, sofas, and other objects.
	- Checking the validity of the spatial tracking and triggering Room Capture App for recalibration if the spatial tracking is in an invalid or limited state.
	- Enabling/Disabling Video Passthrough and applying LUT texture
	- Attaching decorations to the walls. 
	- Handling spatial drift to ensure anchored objects are always in their corresponding physical location during runtime.

## Related GameObjects in the Scene
- Video Seethrough and Spatial Anchors are enabled via PXR_Manager component attached to XR Origin GameObject.
 ![PXRManager_MRSettings](/Documentation/Files/MR/PXRManager_MRSettings.png)
---
- MixedRealityManager component is attached to MR Manager GameObject in the Main scene. By using this component you can assign prefabs for different spatial objects and configure the spatial drift correction delay as well as assigning a LUT texture.
 ![MRManager](/Documentation/Files/MR/MRManager.png)
---
- By modifying the [Neutral LUT Texture](https://developer-global.pico-interactive.com/document/unity/seethrough/#4f80aed6) in one of the image editing tools you can create different effects.
 ![LutTexture](/Documentation/Files/MR/LutTexture.png)
---
- You can find the Passthrough Occluder material under the Materials/MixedReality folder. By creating corresponding volumes with this material for the physical objects in your room, you can make the virtual objects fit more naturally into your physical space through video seethrough.
 ![PassthroughOccluder](/Documentation/Files/MR/PassthroughOccluder.png)
