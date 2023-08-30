using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using System;

namespace MicroWar.Multiplayer
{
    public class CrateContainerQueueNetwork : NetworkBehaviour
    {
        private CrateContainerQueue crateContainerQueue;
        private PowerUpCrate powerUpCrate;
        private void Start()
        {
            crateContainerQueue = GetComponent<CrateContainerQueue>();
            crateContainerQueue.OnInstantiateCrate += OnInstantiateCrate;
            crateContainerQueue.OnSetCrateType += OnsetCrateType;
        }
        public override void OnDestroy()
        {
            base.OnDestroy();
            crateContainerQueue.OnSetCrateType -= OnsetCrateType;
            crateContainerQueue.OnInstantiateCrate -= OnInstantiateCrate;
        }

        private void OnsetCrateType(CrateType crateType)
        {
            SetCrateTypeClientRpc((int)crateType);
        }

        private void OnInstantiateCrate(PowerUpCrate activePowerUpCrate, CrateType crateType)//Host
        {
            Debug.Log("On instanciate crates");
            powerUpCrate = activePowerUpCrate;
            powerUpCrate.OnHitSuccessfully = OnHitSuccessfully;
            SpawnCrateContainerClientRpc((int)crateType);
        }

        #region Client Call

        [ClientRpc]
        public void SimulateBreakCrateContainerClientRpc(ulong senderClientId)
        {
            if (powerUpCrate == null)
            {
               
            }
            if(powerUpCrate!=null && senderClientId != NetworkManager.Singleton.LocalClientId)//TODO: Clients Need to know if it's self
            {
                Debug.LogWarning("Client receive Break Crate Container Rpc inner");
                powerUpCrate.SimulateHit();
            }
        }

        [ClientRpc]
        public void SpawnCrateContainerClientRpc( int crateType)
        {
            if (!IsHost)
            {
                Debug.LogWarning($"Create Powerup Crate! Cratecontainer: {gameObject.transform.parent.name}");
                PowerUpCrate breakableCrate = Instantiate(crateContainerQueue.BreakableCratePrefab, crateContainerQueue.Container).GetComponent<PowerUpCrate>();
                breakableCrate.SetPowerUpType((CrateType)crateType);
                powerUpCrate = breakableCrate;
            } 
            powerUpCrate.OnHitSuccessfully = OnHitSuccessfully;
        }

        private void OnHitSuccessfully()
        {
            Debug.LogWarning("Server Send Break Crate Container ServerRpc");
            MultiplayerBehaviour.Instance.OnCrateHitServerRpc();
        }

        [ClientRpc]
        public void SetCrateTypeClientRpc(int crateType)
        {
            if (IsHost) return;
            powerUpCrate.gameObject.SetActive(true);
            powerUpCrate.SetPowerUpType((CrateType)crateType);
        }
        #endregion
    }
}