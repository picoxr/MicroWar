using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.XR;
using UnityEngine.XR;
using Unity.XR.PXR;
using UnityEngine.UI;
using System;
using MicroWar;
using MicroWar.Multiplayer;
public class TankHealth_Multiplayer : NetworkBehaviour
{
    // Start is called before the first frame update
    public float m_StartingHealth_Local = 100f;               // The amount of health each tank starts with.
    private NetworkVariable<float> m_StartingHealth = new NetworkVariable<float>(100f);
    private NetworkVariable<float> m_CurrentHealth = new NetworkVariable<float>();

    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_FillImage;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.


    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
    //private float m_CurrentHealth;                      // How much health the tank currently has.
    private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?
    private int m_explosionHapticsID;

    public event Action OnTankDeath;

    private void Awake()
    {
        // Instantiate the explosion prefab and get a reference to the particle system on it.
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab, MicroWar.GameManager.Instance.BoardGameOrigin).GetComponent<ParticleSystem>();

        // Get a reference to the audio source on the instantiated prefab.
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        // Disable the prefab so it can be activated when it's required.
        m_ExplosionParticles.gameObject.SetActive(false);

        PXR_Input.SendHapticBuffer(PXR_Input.VibrateType.BothController, m_ExplosionAudio.clip, PXR_Input.ChannelFlip.No, ref m_explosionHapticsID, PXR_Input.CacheType.CacheNoVibrate);

    }

    public override void OnNetworkSpawn() //TODO: HOST AND CLIENT LOGOC
    {
        base.OnNetworkSpawn();
        Debug.LogWarning("On network spawn");
        if(IsHost)//Host
        {
            m_StartingHealth.Value = m_StartingHealth_Local;
            m_CurrentHealth.Value = m_StartingHealth.Value;
        }
        m_CurrentHealth.OnValueChanged += OnHealthChange;
        SetHealthUI();
    }

    private void OnHealthChange(float previousValue, float newValue)
    {
        if(IsLocalPlayer && PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive) //If it's local player, vibrate controllers.
        {
            PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.BothController, 1f, 2000, 300); //take damage, vibrate!
        }

        // Change the UI elements appropriately.
        SetHealthUI();
    }

    private void OnEnable()
    {

        m_Dead = false;

        // Update the health slider's value and color.
        //SetHealthUI();
    }


    public void TakeDamage(float amount) //Host cauculate damage
    {
        if (!IsHost) // [IMPORTANT] Only Host copy can receive damage.
        {
            Debug.Log("Only Host can take damage, return.");
            return;
        }

        // Reduce current health by the amount of damage done.
        m_CurrentHealth.Value -= amount;
            
        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        if (m_CurrentHealth.Value <= 0f && !m_Dead) //TODO: server side calculation to validate damage number.
        {
            OnTankDeath?.Invoke();
            OnDeath(); //Client death
        }
    }


    private void SetHealthUI()
    {
        // Set the slider's value appropriately.
        m_Slider.value = 100f * m_CurrentHealth.Value / m_StartingHealth.Value;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth.Value / m_StartingHealth.Value);
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

        if(IsLocalPlayer &&PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive)
            PXR_Input.StartHapticBuffer(m_explosionHapticsID);

        // Turn the tank off.
        if (IsServer)
        {
            NetworkObject.Despawn(false);
            gameObject.SetActive(false);
        }
    }

    public bool IsDead()
    {
        return m_Dead;
    }
}
