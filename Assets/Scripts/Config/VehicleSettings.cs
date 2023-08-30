using UnityEngine;

namespace MicroWar 
{
    [CreateAssetMenu(menuName = "MicroWar/VehicleSettings")]
    public class VehicleSettings : ScriptableObject
    {
        public VehicleType VehicleType;

        [Header("Health")]
        public float StartingHealth = 100f;
        public Color FullHealthColor = Color.green;
        public Color ZeroHealthColor = Color.red;

        [Header("Movement")]
        public float Speed = 12f;                 // How fast the vehicle moves forward and back.
        public float TurnSpeed = 180f;

        [Header("Shooting")]
        public float MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
        public float GunTurnSpeed = 180f;

        [Header("Scale")]
        public Vector3 localScale = Vector3.one;

        [Header("Effects")]
        public AudioSource ExplosionAudio;
        public ParticleSystem ExplosionParticles;
    }
}
