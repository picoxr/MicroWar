using MicroWar.AI;
using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;
using MicroWar.Multiplayer;
namespace MicroWar
{
    [Serializable]
    public class SinglePlayerVehicleManager : VehicleManagerBase
    {
        //Tank controllers
        private TankMovement m_Movement;
        private TankShooting m_Shooting; 
        private TankHealth m_TankHealth; 
        public override void Setup (VehicleSettings vehicleSettings)
        {
            base.Setup(vehicleSettings);

            // Get references to the components.
            m_Movement = m_Instance.GetComponent<TankMovement> ();
            m_Shooting = m_Instance.GetComponent<TankShooting> ();
            m_TankHealth = m_Instance.GetComponent<TankHealth> ();
            m_CanvasGameObject = m_Instance.GetComponentInChildren<Canvas> ().gameObject;
            m_XRGrabInteractable = m_Instance.GetComponent <XRGrabInteractable> ();

            //m_TankHealth.VehicleManager = this;
            m_TankHealth.VehicleManagerBase = this;

            if (m_PlayerType == VehiclePlayerType.AI) 
            {
                vehicleAIController = m_Instance.GetComponent<VehicleAIStateController> ();
                //Get components for multiplayer
                m_NetworkObject = m_Instance.GetComponent<NetworkObject>();
                m_NetworkPlayer = m_Instance.GetComponent<NetPlayer>();
            }

            ApplyVehicleSettings();

            // Set the player numbers to be consistent across the scripts.
            m_Movement.m_PlayerNumber = m_PlayerNumber;
            m_Shooting.m_PlayerNumber = m_PlayerNumber;

            // Create a string using the correct color that says 'PLAYER 1' etc based on the vehicle's color and the player's number.
            m_ColoredPlayerText = "<color=#" + ColorUtility.ToHtmlStringRGB(m_PlayerColor) + ">PLAYER " + m_PlayerNumber + "</color>";

            // Get all of the renderers of the tank.
            MeshRenderer[] renderers = m_Instance.GetComponentsInChildren<MeshRenderer> ();

            // Go through all the renderers
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                if (renderer.CompareTag("FakeShadow")) continue;

                // set material color to the color specific to this vehicle.
                renderer.material.color = m_PlayerColor;
            }

            m_XRGrabInteractable.enabled = false;
        }

        private void ApplyVehicleSettings()
        {
            m_Instance.transform.localScale = vehicleSettings.localScale;
            m_TankHealth.m_StartingHealth = vehicleSettings.StartingHealth;
            m_Movement.m_Speed = vehicleSettings.Speed;
            m_Movement.m_TurnSpeed = vehicleSettings.TurnSpeed;
            m_Shooting.SetShootingConfig(vehicleSettings.MinLaunchForce, vehicleSettings.MaxLaunchForce, vehicleSettings.MaxChargeTime, vehicleSettings.GunTurnSpeed);
        }

        // Used during the phases of the game where the player shouldn't be able to control their vehicle.
        public override void DisableControl ()
        {
            m_Movement.enabled = false;
            m_Shooting.enabled = false;

            m_CanvasGameObject.SetActive (false);

            if(vehicleAIController != null)
            {
                vehicleAIController.enabled = false;
            }
        }


        // Used during the phases of the game where the player should be able to control their tank.
        public override void EnableControl ()
        {
            m_Movement.enabled = true;
            m_Shooting.enabled = true;

            m_CanvasGameObject.SetActive (true);

            if (vehicleAIController != null)
            {
                vehicleAIController.enabled = true;
            }
        }


        // Used at the start of each round to put the tank into it's default state.
        public override void Reset ()
        {
            m_Instance.transform.position = m_SpawnPoint.position;
            m_Instance.transform.rotation = m_SpawnPoint.rotation;

            m_Instance.SetActive (false);
            m_Instance.SetActive (true);
        }

        public override ITankHealth GetVehicleHealth() 
        {
            return m_TankHealth;
        }

        //Multiplayer AI Mode:
        public override void Spawn()
        {
            IsSpawned = true;
            m_NetworkObject.Spawn();
            m_NetworkObject.TrySetParent(MicroWar.GameManager.Instance.BoardGameOrigin);
            m_NetworkPlayer.SetupPlayerInfoClientRpc(m_userName, m_PlayerColor);
        }

        public override void DespawnTank()
        {
            if (m_NetworkObject.IsSpawned)
                m_NetworkObject.Despawn(true);
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