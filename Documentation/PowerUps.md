
# Power-Ups
| Script | Function |
| - | - |
| DynamicMapObjectsManager | Manages the dynamic objects such as the crates containing power-ups and turret activators in the game. |
| TurretController | Contains the functionality that handles firing homing missiles from a turret. |
| PowerUpCrate | Represent the big breakable crate that appears in front of the player.  |
| PowerUpSettings | A ScriptableObject storing the configuration of a power-up.    |
| HittableObject | Represents game objects that can hit by hands, controllers or feet(PICO Motion Trackers) |
| CrateContainerQueue | Collected power-up crates are stored in this queue. A player can see and interact with only a crate at a time. |
| EnvironmentManager | Holds references to player spawn points, vehicle spawn points and the map's origin position. Contains methods which help retrieve random points on the map.|


