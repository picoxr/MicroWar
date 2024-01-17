using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR;

namespace MicroWar
{
    public abstract class TankShooting : MonoBehaviour
    {
        public int m_PlayerNumber = 1;              // Used to identify the different players.
        public Rigidbody m_Shell;                   // Prefab of the shell.
        public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
        public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
        public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
        public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
        public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
        public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
        public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
        public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.
        public float m_TurnSpeed = 180f;
        public Transform m_Turret;

        private string m_FireButton;                // The input axis that is used for launching shells.
        protected float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
        protected float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
        protected bool m_Fired;                       // Whether or not the shell has been launched with this button press.



        private void OnEnable()
        {
            // When the tank is turned on, reset the launch force and the UI
            m_CurrentLaunchForce = m_MinLaunchForce;
            if (m_AimSlider != null)
            {
                m_AimSlider.value = m_MinLaunchForce;
            }
        }

        protected void Start ()
        {
            // The fire axis is based on the player number.
            m_FireButton = "Fire" + m_PlayerNumber;
            CalculateChargeSpeed();
            OverrideSFX();
        }

        public void SetShootingConfig(float minLaunchForce, float maxLaunchForce, float maxChargeTime, float turnSpeed)
        {
            m_MinLaunchForce = minLaunchForce;        
            m_MaxLaunchForce = maxLaunchForce;        
            m_MaxChargeTime = maxChargeTime;      
            m_TurnSpeed = turnSpeed;
            CalculateChargeSpeed();
        }

        private void CalculateChargeSpeed() 
        {
            // The rate that the launch force charges up is the range of possible forces by the max charge time.
            m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
        }

        private void OverrideSFX()
        {
            if (GameManager.Instance.VehicleChargeSFXOverride != null)
            {
                m_ChargingClip = GameManager.Instance.VehicleChargeSFXOverride;
            }
        }

        protected virtual void Fire ()
        {
            // Set the fired flag so only Fire is only called once.
            m_Fired = true;

            // Create an instance of the shell and store a reference to it's rigidbody.
            Rigidbody shellInstance =
                Instantiate (m_Shell, m_FireTransform.position, m_FireTransform.rotation, GameManager.Instance.BoardGameOrigin) as Rigidbody;

            // Set the shell's velocity to the launch force in the fire position's forward direction.
            shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward; 

            // Change the clip to the firing clip and play it.
            m_ShootingAudio.clip = m_FireClip;
            m_ShootingAudio.Play ();

            // Reset the launch force.  This is a precaution in case of missing button events.
            m_CurrentLaunchForce = m_MinLaunchForce;
        }
    }
}