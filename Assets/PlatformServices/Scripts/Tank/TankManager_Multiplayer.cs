using UnityEngine;
using System;
using Unity.Netcode;
using MicroWar.Multiplayer;

namespace MicroWar
{
    [Serializable]
    public class TankManager_Multiplayer : VehicleManagerBase
    {
        private TankMovement_Multiplayer m_Movement;
        private TankShooting_Multiplayer m_Shooting;
       //private TankHealth_Multiplayer m_TankHealth;
        private TankHealth m_TankHealth;
        public override void Setup(VehicleSettings vehicleSettings)
        {
            base.Setup(vehicleSettings);

            // Get references to the components.
            m_Movement = m_Instance.GetComponent<TankMovement_Multiplayer>();
            m_Shooting = m_Instance.GetComponent<TankShooting_Multiplayer>();
            m_TankHealth = m_Instance.GetComponent<TankHealth>();
            m_NetworkObject = m_Instance.GetComponent<NetworkObject>();
            m_NetworkPlayer = m_Instance.GetComponent<NetPlayer>();
            m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas>().gameObject;
            m_TankHealth.VehicleManagerBase = this; //TODO: for turret missile, optimize this.
            switch (VehicleType)
            {
                case VehicleType.Tiger:
                    m_Instance.transform.localScale = Vector3.one;
                    m_TankHealth.m_StartingHealth = 100;
                    m_Movement.m_Speed = 12;
                    m_Movement.m_TurnSpeed = 180;
                    break;
                case VehicleType.Bear:
                    m_Instance.transform.localScale = Vector3.one * 1.5f;
                    m_TankHealth.m_StartingHealth = 100 * 1.5f;
                    m_Movement.m_Speed = 12 / 1.5f;
                    m_Movement.m_TurnSpeed = 180 / 1.5f;
                    break;
                case VehicleType.Cheetah:
                    m_Instance.transform.localScale = Vector3.one / 1.5f;
                    m_TankHealth.m_StartingHealth = 100 / 1.5f;
                    m_Movement.m_Speed = 12 * 1.5f;
                    m_Movement.m_TurnSpeed = 180 * 1.5f;
                    break;
            }
            // Set the player numbers to be consistent across the scripts.
            m_Movement.m_PlayerNumber = m_PlayerNumber;
            m_Shooting.m_PlayerNumber = m_PlayerNumber;
            m_TankHealth.SetShieldPrefab(MultiplayerManager.Instance.shieldPrefab);
            // Create a string using the correct color that says 'PLAYER 1' etc based on the tank's color and the player's number.
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + $">{m_userName} " + "</color>";

            // Get all of the renderers of the tank.
            MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer>();

            //Set Scale
            //m_Instance.transform.localScale *= GameManager.Instance.m_boardGameOrigin.localScale.x; //TODO:Fix me.


            // Go through all the renderers...
            for (int i = 0; i < renderers.Length; i++)
            {
                // ... set their material color to the color specific to this tank.
                renderers[i].material.color = m_PlayerColor;
            }

            //Tell client the current settings.
            //m_NetworkPlayer.SetupClientRpc();
        }

        public override void Spawn()
        {
            IsSpawned = true;
            m_NetworkObject.SpawnAsPlayerObject(m_clientId);
            m_NetworkObject.TrySetParent(MicroWar.GameManager.Instance.BoardGameOrigin);
            m_NetworkPlayer.SetupPlayerInfoClientRpc(m_userName, m_PlayerColor);
        }


        // Used during the phases of the game where the player shouldn't be able to control their tank.
        public override void DisableControl()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive(false);
        }


        // Used during the phases of the game where the player should be able to control their tank.
        public override void EnableControl()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive(true);
        }


        // Used at the start of each round to put the tank into it's default state.
        public override void Reset()
        {
            if (m_NetworkObject.IsSpawned)
                m_NetworkObject.Despawn(false);
            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;
            //Debug.LogWarning($"ClientId:{m_clientId}");
            m_Instance.SetActive(false);
            m_Instance.SetActive(true);
            Spawn();
        }

        public override void DespawnTank()
        {
            if (m_NetworkObject.IsSpawned)
                m_NetworkObject.Despawn(true);
        }

        public override ITankHealth GetVehicleHealth()
        {
            return m_TankHealth;
        }

        public override void RespawnTank()
        {
            if (m_NetworkObject.IsSpawned)
                m_NetworkObject.Despawn(false);
            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;
            //Debug.LogWarning($"ClientId:{m_clientId}");
            m_Instance.SetActive(false);
            m_Instance.SetActive(true);
            Spawn();
        }
    }
}


