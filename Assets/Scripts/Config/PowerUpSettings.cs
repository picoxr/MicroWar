using UnityEngine;
using MicroWar;

[CreateAssetMenu(menuName = "MicroWar/PowerUpSettings")]
public class PowerUpSettings : ScriptableObject
{
    public PowerUpType powerUpType;
    [Header("Period (Seconds)")]
    public float period = 10.0f;
    [Header("Probability (%)")]
    [Tooltip("Defines the spawning probability at each period.Range (0,1)")]
    [Range(0f,1f)]
    public float probability = 0.9f;
}
