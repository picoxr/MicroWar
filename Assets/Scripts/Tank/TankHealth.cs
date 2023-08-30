using System;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.UI;
using MicroWar.Multiplayer;

namespace MicroWar
{

    public class TankHealth : MonoBehaviour, ITankHealth
    {
        public float m_StartingHealth = 100f;               // The amount of health each vehicle starts with.
        public Slider m_Slider;                             // The slider to represent how much health the vehicle currently has.
        public Image m_FillImage;                           // The image component of the slider.
        public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
        public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
        public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the vehicle dies.
        [HideInInspector] public GameObject shieldPrefab;
        private GameObject shieldObject;


        private AudioSource m_ExplosionAudio;               // The audio source to play when the vehicle explodes.
        private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the vehicle is destroyed.
        private float m_CurrentHealth;                      // How much health the vehicle currently has.
        private bool m_Dead;                                // Has the vehicle been reduced beyond zero health yet?
        private int m_explosionHapticsID;

        public event Action OnTankDeath;
        public event Action<float> OnTakeDamage;
        public event Action OnMaxUpHealth;
        public event Action<bool> OnActivatingShield;

        private bool hasShield = false;

        //public SinglePlayerVehicleManager VehicleManager { get; internal set; }
        public VehicleManagerBase VehicleManagerBase { get; internal set; }//[Temp] used for multiplayer tankmanager

        private void Awake()
        {
            // Instantiate the explosion prefab and get a reference to the particle system on it.
            m_ExplosionParticles = Instantiate(m_ExplosionPrefab, GameManager.Instance.BoardGameOrigin).GetComponent<ParticleSystem>();

            // Get a reference to the audio source on the instantiated prefab.
            m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

            // Disable the prefab so it can be activated when it's required.
            m_ExplosionParticles.gameObject.SetActive(false);

            PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.BothController, m_ExplosionAudio.clip, PXR_Input.ChannelFlip.No, ref m_explosionHapticsID, PXR_Input.CacheType.CacheNoVibrate);

        }


        private void OnEnable()
        {
            // When the tank is enabled, reset the tank's health and whether or not it's dead.
            m_CurrentHealth = m_StartingHealth;
            m_Dead = false;

            // Update the health slider's value and color.
            SetHealthUI();
        }

        [ContextMenu("TakeDamage")]
        public void UnitTestTakeDamage()
        {
            OnTakeDamage.Invoke(1);
        }

        public void TakeDamage(float amount)
        {
            //If has a shield, don't take damage but remove the shield.
            if (hasShield)
            {
                RemoveShield();
                return;
            }

            if (null != OnTakeDamage) //A hack here, if the tank is spawned by network. this should not be null.
            {
            Debug.Log($"Take damage: netplayer amount {amount}");
                OnTakeDamage?.Invoke(amount);
                return;
            }
            
            // Reduce current health by the amount of damage done.
            m_CurrentHealth -= amount;
            if (VehicleManagerBase.m_PlayerType == VehiclePlayerType.Local && PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.BothController, 1f, 2000, 300);
            // Change the UI elements appropriately.
            SetHealthUI();

            // If the current health is at or below zero and it has not yet been registered, call OnDeath.
            if (m_CurrentHealth <= 0f && !m_Dead)
            {
                OnTankDeath?.Invoke();
                OnDeath();
            }
        }


        private void SetHealthUI()
        {
            // Set the slider's value appropriately.
            m_Slider.value = 100f * m_CurrentHealth / m_StartingHealth;

            // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
            m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);
        }


        public void OnDeath()
        {
            // Set the flag so that this function is only called once.
            m_Dead = true;

            // Move the instantiated explosion prefab to the tank's position and turn it on.
            m_ExplosionParticles.transform.position = transform.position;
            m_ExplosionParticles.gameObject.SetActive(true);

            // Play the particle system of the tank exploding.
            m_ExplosionParticles.Play();

            // Play the tank explosion sound effect.
            m_ExplosionAudio.Play();

            if (VehicleManagerBase.m_PlayerType == VehiclePlayerType.Local && PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
                PXR_Input.StartHapticBuffer(m_explosionHapticsID);

            // Turn the tank off.
            gameObject.SetActive(false);
        }

        public bool IsDead()
        {
            return m_Dead;
        }

        public void AddShield()
        {
            OnActivatingShield?.Invoke(true);
            hasShield = true;
            if (shieldObject != null)
            {
                shieldObject.SetActive(true);
            }
            else
            {
                //shieldObject = Instantiate(shieldPrefab, this.TankManager.m_Instance.transform);
                shieldObject = Instantiate(shieldPrefab, transform);
            }
        }

        public void RemoveShield()
        {
            OnActivatingShield?.Invoke(false);
            hasShield = false;
            if (shieldObject != null)
            {
                shieldObject.SetActive(false);
            }
        }

        public void SetShieldPrefab(GameObject prefab)
        {
            shieldPrefab = prefab;
        }

        public void MaxUpHealth()
        {
            if (!m_Dead)
            {
                m_CurrentHealth = m_StartingHealth;
                OnMaxUpHealth?.Invoke();
                SetHealthUI();
            }
        }

        //Multiplayer
        public void SetupShield(bool IsActivate)
        {
            if (IsActivate)
            {
                hasShield = true;
                if (shieldObject != null)
                {
                    shieldObject.SetActive(true);
                }
                else
                {
                    //shieldObject = Instantiate(shieldPrefab, this.TankManager.m_Instance.transform);
                    shieldObject = Instantiate(shieldPrefab, transform);
                }
            }
            else
            {
                hasShield = false;
                if (shieldObject != null)
                {
                    shieldObject.SetActive(false);
                }
            }
              
        }

        public void SetCurrentHealth( float startingHealth, float currentHealth)
        {
            m_StartingHealth = startingHealth;
            m_CurrentHealth = currentHealth;
            Debug.Log($"CurrentHealth: [tank health]: {m_CurrentHealth}, CurrentTank: {gameObject.name}");
            SetHealthUI();
            // If the current health is at or below zero and it has not yet been registered, call OnDeath.
            if (m_CurrentHealth <= 0f && !m_Dead)
            {
                OnTankDeath?.Invoke();
                OnDeath();
            }
        }

        public void ClientSetCurrentHealth(float startingHealth, float currentHealth)
        {
            m_StartingHealth = startingHealth;
            m_CurrentHealth = currentHealth;
            Debug.Log($"CurrentHealth: [tank health]: {m_CurrentHealth}, CurrentTank: {gameObject.name}");
            SetHealthUI();
        }

        public float GetStartingHealth()
        {
            return m_StartingHealth;
        }

        public float GetCurrentHealth()
        {
            return m_CurrentHealth;
        }
    }
}