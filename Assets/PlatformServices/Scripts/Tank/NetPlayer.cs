using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using MicroWar.AI;
using MicroWar.Multiplayer;
using System;
using static Unity.XR.PXR.PXR_Input;
using Unity.XR.PXR;

namespace MicroWar.Multiplayer
{
    public class NetPlayer : NetworkBehaviour
    {
        private string PlayerNameNetwork;
        private int playerNumber;
        private bool isAIBot = false;

        public TMP_Text PlayerNameText;
        private TankHealth_Multiplayer health;
        private GameObject canvasGameObject;

        private ITankHealth m_TankHealth;
        private ITankShooting m_TankShooting;
        private VehicleAIStateController m_AIController;
        private TankMovement_AI tankMovementAI;
        private TankMovement_Multiplayer tankMovementMultiplayer;

        private NetworkVariable<float> m_StartingHealth = new NetworkVariable<float>();
        private NetworkVariable<float> m_CurrentHealth = new NetworkVariable<float>();



        void Start()
        {
            //[Temp] Test code//////////////////////////
            //OnNetworkSpawn();
            //m_TankHealth.OnTakeDamage += OnTakeDamage;
            //[Temp] Test code//////////////////////////
        }
        public override void OnNetworkSpawn()
        {

            m_TankShooting = GetComponent<ITankShooting>();
            m_TankHealth = GetComponent<ITankHealth>();
            canvasGameObject = GetComponentInChildren<Canvas>().gameObject;
            if (IsHost)//Host
            {
                m_StartingHealth.Value = m_TankHealth.GetStartingHealth();
                m_CurrentHealth.Value = m_StartingHealth.Value;
            }
            m_CurrentHealth.OnValueChanged += OnHealthChange;
            m_TankHealth.OnTakeDamage += OnTakeDamage;
            m_TankHealth.OnTankDeath += OnTankDeath;
            m_TankHealth.OnMaxUpHealth += M_TankHealth_OnMaxUpHealth;
            m_TankHealth.OnActivatingShield += M_TankHealth_OnActivatingShield;
            m_TankShooting.OnTankShooting += TankShootingHandler;

            transform.TryGetComponent<VehicleAIStateController>(out m_AIController);

            MultiplayerBehaviour.Instance.AddNetworkPlayerReference(this);


            if (null != m_AIController)
            {
                tankMovementAI = GetComponent<TankMovement_AI>();
                isAIBot = true;
                if (!IsHost)
                    m_AIController.enabled = false;
                tankMovementAI.enabled = false;
            }
            else
            {
                tankMovementMultiplayer = GetComponent<TankMovement_Multiplayer>();
                tankMovementMultiplayer.enabled = false;
            }

        }

        private void M_TankHealth_OnActivatingShield(bool IsActivated)
        {
            if (IsActivated)
            {
                OnActivatingShieldClientRPC(true);
            }
            else
            {
                OnActivatingShieldClientRPC(false);
            }
        }

        private void M_TankHealth_OnMaxUpHealth()
        {
            m_CurrentHealth.Value = m_StartingHealth.Value;
        }

        public override void OnNetworkDespawn()
        {
            isAIBot = false;
            MultiplayerBehaviour.Instance.RemoveNetworkPlayerReference(NetworkObjectId);
            m_CurrentHealth.OnValueChanged -= OnHealthChange;
            m_TankHealth.OnTankDeath -= OnTankDeath;
            m_TankHealth.OnTakeDamage -= OnTakeDamage;
            m_TankShooting.OnTankShooting -= TankShootingHandler;
            m_TankHealth.OnMaxUpHealth -= M_TankHealth_OnMaxUpHealth;
            m_TankHealth.OnActivatingShield -= M_TankHealth_OnActivatingShield;
        }

        private void OnTakeDamage(float amount)
        {
            if (!IsHost) // [IMPORTANT] Only Host copy can receive damage.
            {
                Debug.Log("Only Host can take damage, return.");
                return;
            }
            Debug.Log($"On take damage, amount:  {amount}");
            // Reduce current health by the amount of damage done.
            m_CurrentHealth.Value -= amount;
            m_TankHealth.SetCurrentHealth(m_StartingHealth.Value, m_CurrentHealth.Value);
        }

        private void OnHealthChange(float previousValue, float newValue)
        {
            if (!isAIBot && NetworkObject.IsOwner && PXR_HandTracking.GetActiveInputDevice() == ActiveInputDevice.ControllerActive) //If it's local player, vibrate controllers.
            {
                PXR_Input.SendHapticImpulse(PXR_Input.VibrateType.BothController, 1f, 2000, 300); //take damage, vibrate!
            }
            if (!IsHost)
                m_TankHealth.ClientSetCurrentHealth(m_StartingHealth.Value, m_CurrentHealth.Value);
        }

        private void TankShootingHandler(float launchForce)
        {
            if (IsOwner)
            {
                MultiplayerBehaviour.Instance.PlayerFireServerRpc(launchForce);
                Debug.Log($"Tank shooting {OwnerClientId}");
            }
        }

        private void OnTankDeath()
        {
            if (IsServer)
            {
                OnDeathClientRpc();
                //gameObject.SetActive(false);
            }
        }

        public void DisableVehicleControl()
        {
            if (null != m_AIController)
            {
                tankMovementAI.enabled = false;
            }
            else
            {
                tankMovementMultiplayer.enabled = false;
            }
        }
        public void EnableVehicleControl()
        {
            if (null != m_AIController)
                tankMovementAI.enabled = true;
            else
                tankMovementMultiplayer.enabled = true;
        }

        [ClientRpc]
        private void OnActivatingShieldClientRPC(bool isActivate)
        {
            if (IsHost) return;
            Debug.Log("[Network Player]: Setup Shield");
            m_TankHealth.SetupShield(isActivate);
        }


        [ClientRpc]
        private void OnDeathClientRpc() //Receive death call
        {
            if (IsHost)
            {
                NetworkObject.Despawn(false);
            }
            else
            {
                m_TankHealth.OnDeath();
            }
        }

        [ClientRpc]
        public void BotFireClientRpc(float launchForce)
        {
            if (!IsOwner)
            {
                m_TankShooting.SimulateFire(launchForce);
            }
        }

        /// <summary>
        /// Used to receive fire rpc and simuate the firing locally
        /// </summary>
        [ClientRpc]
        public void PlayerFireClientRpc(float launchForce)
        {
            if (!IsOwner)
            {
                //Simulate shooting locally
                //shooting.SimulateFire(launchForce);
                m_TankShooting.SimulateFire(launchForce);
            }
        }

        [ClientRpc]
        public void SetupPlayerInfoClientRpc(string DisplayName, Color color) //execute locally
        {
            PlayerNameNetwork = DisplayName;
            if (PlayerNameText != null)
            {
                PlayerNameText.text = PlayerNameNetwork;
                PlayerNameText.color = color;
            }

            //Setup player color
            MeshRenderer[] renderers = transform.GetComponentsInChildren<MeshRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                renderers[i].material.color = color;
            }

            //Debug.LogWarning($"Player Name: {PlayerNameNetwork}");
        }

    }

}
