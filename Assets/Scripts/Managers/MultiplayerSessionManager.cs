using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MicroWar.Platform;
using MicroWar.AI;
using System;
using UnityEngine.SceneManagement;
namespace MicroWar.Multiplayer
{
    /// <summary>
    /// Server side game logic.
    /// </summary>
    public class MultiplayerSessionManager: SessionManagerBase
    {
        
        private MultiplayerBehaviour multiplayerBehaviour;
        private PlatformController_Rooms roomController;
        private PlatformController_Network networkController;
        private PlatformController_RTC rtcController;
        private Dictionary<ulong, VehicleType> readyPlayers; //TODO: Combine these dictionaries.

        public override void Initialize(GameManager gameManager)
        {
            base.Initialize(gameManager);

            SessionType = SessionType.Multiplayer;


            readyPlayers = new Dictionary<ulong, VehicleType>();
            multiplayerBehaviour = MultiplayerBehaviour.Instance;
            roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>(); //Get room controller instance to get room user's info.
            networkController = PlatformServiceManager.Instance.GetController<PlatformController_Network>();
            rtcController = PlatformServiceManager.Instance.GetController<PlatformController_RTC>();
            //multiplayerBehaviour.Startup(gameManager);
            multiplayerBehaviour.SetupCurrentSession(this);
        }
        #region GameLoop
        public override void StartSession()
        {
            rtcController.CheckRTCEnable(multiplayerBehaviour.IsUseRTC);
            StartCoroutine(SessionStarting());
        }

        protected override IEnumerator SessionStarting()
        {
            SetupVehicles();
            OnSessionStateChanged?.Invoke(SessionState.SessionStarting);
            SessionState = SessionState.SessionStarting;
            multiplayerBehaviour.StartTankConfigrationClientRpc();
            var Timeout = gameSettings.VehicleSelectDelay;
            while (Timeout > 0 && !CheckPlayerReady())
            {
                Timeout = Timeout - Time.deltaTime;
                yield return null;
            }
            inGameUIHandler.UpdateMessage(string.Empty);
            SpawnAllVehicles();
            StartCoroutine(SessionLoop());
        }

        protected override IEnumerator SessionLoop()
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());
            if (sessionWinner != null) //End this session
            {
                RoundNumber = 0;
                sessionWinner = null;
                readyPlayers.Clear();
                inGameUIHandler.UpdateMessage(string.Empty);
                for (int i = 0; i < vehicles.Count; i++)
                {
                    vehicles[i].DespawnTank();
                }
                vehicles.Clear();
                multiplayerBehaviour.ResetGameClientRpc(); // Rest Game logic instead reload the scene,
            }
            else
            {
                StartCoroutine(SessionLoop());
            }
        }

        protected override IEnumerator RoundStarting()
        {
            //DisableTankControl();
            ResetAllVehicles();
            RoundNumber++;
            multiplayerBehaviour.RoundStartClientRpc(RoundNumber);;
            OnSessionStateChanged?.Invoke(SessionState.RoundStarting);
            SessionState = SessionState.RoundStarting;
            yield return startWait; //TODO: move this out of session manager
        }

        protected override IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            //EnableTankControl();
            multiplayerBehaviour.RoundPlayingClientRpc();
            OnSessionStateChanged?.Invoke(SessionState.RoundPlaying);
            SessionState = SessionState.RoundPlaying;
            while (!OnePlayerLeft())
            {
                yield return null;
            }
        }

        protected override IEnumerator RoundEnding()
        {
            roundWinner = null;
            roundWinner = GetRoundWinner();
            bool hasSessionWinner = false;
            ulong? winnerClientId = null;
            // If there is a winner, increment their score.
            if (roundWinner != null)
            {
                roundWinner.m_Wins++;
                winnerClientId = roundWinner.m_clientId;
            }
            // Now the winner's score has been incremented, see if someone has won the game.
            sessionWinner = GetGameWinner();
            if (sessionWinner != null)
            {
                hasSessionWinner = true;
                winnerClientId = sessionWinner.m_clientId;
            }
            string message = EndMessage();
            if(winnerClientId!=null)
            {
                multiplayerBehaviour.RoundEndingClientRpc(message, hasSessionWinner, (ulong)winnerClientId);
            }
            OnSessionStateChanged?.Invoke(SessionState.RoundEnding);
            SessionState = SessionState.RoundEnding;
            yield return endWait;
        }
        #endregion
        private string EndMessage()
        {
            string message = "DRAW!";
            if (roundWinner != null)
            {
                message = roundWinner.m_ColoredPlayerText + " WINS THE ROUND!";
            }
            message += "\n\n\n\n";
            for (int i = 0; i < vehicles.Count; i++)
            {
                message += vehicles[i].m_ColoredPlayerText + ": " + vehicles[i].m_Wins + " WINS\n";
            }
            if (sessionWinner != null)
                message = sessionWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        protected override void ResetAllVehicles()
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                vehicles[i].RespawnTank();
            }
        }

        private void SetupVehicles()
        {
            //Get all connected players in this room. 
            var playerList = networkController.GetConnectedClients();
            foreach (KeyValuePair<ulong, string> kvp in playerList)
            {
                MultiplayerVehicleManager vehicleManager = new MultiplayerVehicleManager();
                vehicleManager.m_userName = roomController.GetRoomUserDetails(kvp.Value).DisplayName;
                vehicleManager.m_clientId = kvp.Key;
                vehicleManager.VehicleType = VehicleType.Tiger; //Set default tank type;
                vehicleManager.m_PlayerType = VehiclePlayerType.Network;
                if (kvp.Key == NetworkManager.Singleton.LocalClientId)
                {
                    vehicleManager.IsSelf = true;
                }
                vehicles.Add(vehicleManager);
            }

            var botNumber = 4 - playerList.Count; //TODO: store the max player number in gameManager

            //if (gameManager.IsUseBot) // Add bot
            if (multiplayerBehaviour.IsUseBot) // Add bot
            {
                for (int i = 0; i < botNumber; i++)
                {
                    MultiplayerVehicleManager vehicleManager = new MultiplayerVehicleManager();
                    vehicleManager.m_userName = $"Bot {i}";
                    vehicleManager.m_clientId = 999;
                    vehicleManager.VehicleType = VehicleType.Tiger; //Set default tank type;
                    vehicleManager.m_PlayerType = VehiclePlayerType.AI;

                    vehicles.Add(vehicleManager);
                }
            }
            //Set Tank Prefabs and spawn points /colors
            for (int i = 0; i < vehicles.Count; i++)
            {
                var tankManager = vehicles[i];
                tankManager.m_SpawnPoint = environmentManager.GetMapSpawnPoints()[i];
                tankManager.m_PlayerNumber = i;
                tankManager.m_PlayerColor = multiplayerBehaviour.VehicleColors[i];
                tankManager.IsSpawned = false;
                //TODO: tankManager.GetTankHealth().SetShieldPrefab(shieldPrefab);
            }
        }

        private bool CheckPlayerReady()
        {
            return readyPlayers.Count >= NetworkManager.Singleton.ConnectedClients.Count;
        }


        protected override void SpawnAllVehicles ()//Spawn all the remainning tanks TODO: Optimize
        {
            for (int i = 0; i < vehicles.Count; i++)
            {
                var tankManager = vehicles[i];
                if (tankManager.IsSpawned == false)
                {
                    if (tankManager.m_Instance == null)
                        SetTankPrefab(tankManager);
                    //tankManager.Spawn();
                    if (tankManager.m_PlayerType == VehiclePlayerType.AI)
                    {
                        VehicleAIStateController aiController = tankManager.m_Instance.GetComponent<VehicleAIStateController>();
                        aiController.SetGameManager(gameManager);//TODO: refactor this
                        aiController.SetVehicleManager(tankManager); //TODO: [TEMP] convert from TankManagerBase to tankmanager
                    }
                    tankManager.Setup(vehicleConfigManager.GetVehicleSettings(tankManager.VehicleType));
                    //tankManager.GetVehicleHealth().SetShieldPrefab(shieldPrefab);//Setup shieldPrefab for bot
                    tankManager.Spawn();
                    Debug.Log($"Spawn Tank ID: {tankManager.m_clientId}");
                }
            }
        }

        private void SetTankPrefab(VehicleManagerBase vehicleManager)
        {
            var vehicleType = vehicleManager.VehicleType;
            var vehiclePlayerType = vehicleManager.m_PlayerType;
            var vehiclePrefab = vehicleConfigManager.GetVehiclePrefab(vehicleType, vehiclePlayerType);
            vehicleManager.m_Instance = Instantiate(vehiclePrefab, vehicleManager.m_SpawnPoint.position, vehicleManager.m_SpawnPoint.rotation, environmentManager.MapOrigin);
            //TODO: board game origin should moved to the environment manager
        }

        public void NetworkPlayerReady(VehicleType vehicleType, ulong clientId)
        {
            //Return if game status is playing.
            if (SessionState != SessionState.SessionStarting) return; //Fixed: multi-Spawn issue.

            if (!readyPlayers.ContainsKey(clientId))
            {
                readyPlayers.Add(clientId, vehicleType);
            }
            for (int i = 0; i < vehicles.Count; i++)
            {
                var tankManager = vehicles[i];
                if (tankManager.m_clientId == clientId)
                {
                    tankManager.VehicleType = vehicleType;
                    SetTankPrefab(tankManager);
                    tankManager.Setup(GameManager.Instance.VehicleConfigManager.GetVehicleSettings(tankManager.VehicleType));
                    tankManager.Spawn();
                    Debug.Log($"Spawn Tank ID: {tankManager.m_clientId}");
                }
            }
        }

        public override void DisableVehicleControl() //Client call
        {

        }

        public override void EnableVehicleControl()
        {

        }

        public void ResetGame() //Local version reset game. Execute this when leave a room.
        {
            RoundNumber = 0;
            sessionWinner = null;
            readyPlayers.Clear();
            //networkPlayerReference.Clear();
            //isPlaying = false;
            inGameUIHandler.UpdateMessage(string.Empty);
                for (int i = 0; i < vehicles.Count; i++)
                {
                    vehicles[i].DespawnTank();
                }
            vehicles.Clear();
        }
    }
}



