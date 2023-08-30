# Haptic Feedback

Enhance user immersion with haptic feedback using the PICO 4 controllers' broadband linear motors. Integrated with the SDK, these motors provide dynamic vibrations that simulate real-world haptic sensations. With a vibration frequency range of 50 to 500Hz, users are treated to an exceptional tactile experience.

## MicroWar PICO Controller Input Integration

Within MicroWar, we've seamlessly integrated haptic feedback using both Non-buffered and Buffered approaches to amplify the user experience:

- **Non-buffered Haptics**: Employed for shell collisions, ensuring immediate, impactful responses.
- **Buffered Haptics**: Utilized during movements, charging, shooting, and explosions for a more nuanced and synchronized haptic experience.

## Key Scripts and Logic

### TankHealth.cs

In this script, we harness the power of haptic feedback to accentuate gameplay elements. For instance, when a tank sustains damage, a single haptic impulse is triggered to enhance the feedback loop:
```csharp
PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.BothController, 1f, 2000, 300);
```
To heighten the impact of explosions, we pre-generate a haptic buffer during the Awake() phase and employ it during the tank's destruction sequence:
```csharp
PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.BothController, m_ExplosionAudio.clip, PXR_Input.ChannelFlip.No, ref m_explosionHapticsID, PXR_Input.CacheType.CacheNoVibrate);
```
```csharp
PXR_Input.StartHapticBuffer(m_explosionHapticsID);
```
### TankMovement_Player.cs
Here, we extend the use of buffered haptics to the tank's movement. The steps are similar to those in TankHealth.cs, but tailored to the context of movement feedback:
```csharp
PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.LeftController, m_EngineDriving, PXR_Input.ChannelFlip.No, ref m_drivingHapticsID, PXR_Input.CacheType.CacheNoVibrate);
```
```csharp
PXR_Input.StartHapticBuffer(m_drivingHapticsID);
```
```csharp
PXR_Input.StopHapticBuffer(m_drivingHapticsID);
```
### TankShooting_Player.cs
The excitement of charging and shooting is further heightened through the utilization of buffered haptics. Similar to the above scripts, buffered haptics are integrated to create a synchronized and immersive experience.
