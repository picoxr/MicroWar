using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MicroWar.Platform;
using System;
using UnityEngine.SceneManagement;

namespace MicroWar.Multiplayer
{
    public class MultiplayerBehaviour : NetworkBehaviour
    {
        public Color[] VehicleColors; //Temp codes

        private int localPlayerIndex = 0;
        public bool IsUseBot { get; private set; } = true;
        public bool IsUseRTC { get; private set; } = true;
        public int LocalPlayerIndex { get => localPlayerIndex; }

        public bool CanReloadScene = false;

        private GameManager gameManager;
        private EnvironmentManager environmentManager;
        private MultiplayerSessionManager currentSession;
        private PlatformController_Rooms roomController;
        private Dictionary<ulong, NetPlayer> networkPlayerReference;

        private InGameUIHandler inGameUIHandler;
        #region Singleton
        private static MultiplayerBehaviour instance;
        public static MultiplayerBehaviour Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                if (instance == null)
                {
                    instance = FindObjectOfType<MultiplayerBehaviour>();
                }

                return instance;
            }
        }


        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this;
            }
            else if (instance != this)
            {
                Debug.LogError($"Destroyed Duplicate Singleton: { this.gameObject} In Scene: {this.gameObject.scene}");
                Destroy(this.gameObject);
            }
        }
        #endregion
        #region UnityMessages
        private void Start()
        {
            PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(RoomUpdateHandler);
            roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>();
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene sceneName, LoadSceneMode mode)
        {
            if (sceneName.name != "Main") return;

            gameManager = GameManager.Instance;
            environmentManager = gameManager.EnvironmentManager;
            inGameUIHandler = gameManager.InGameUIHandler;
            networkPlayerReference = new Dictionary<ulong, NetPlayer>();

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            if (PlatformServiceManager.Instance == null) return;
            PlatformServiceManager.Instance.UnregisterNotification<RoomUpdateEvent>(RoomUpdateHandler);
        }
        #endregion

        private void RoomUpdateHandler(EventWrapper<RoomUpdateEvent> EventData)
        {
            if (EventData.NotificationType == NotificationType.RoomServiceStatus)
            {
                if (EventData.Data.RoomServiceStatus == RoomServiceStatus.InRoom) //Player Join Room
                {
                    if (null != EventData.Data.CurrentRoom.UsersOptional && EventData.Data.CurrentRoom.UsersOptional.Count > 0)
                    {
                        localPlayerIndex = EventData.Data.CurrentRoom.UsersOptional.Count - 1;
                        environmentManager.SetupXROriginPos(localPlayerIndex); //Set XR originPos after joinning a room;
                        CanReloadScene = false;
                    }
                }
                else
                {
                    environmentManager.SetupXROriginPos(0);
                    localPlayerIndex = 0;
                }
            }
        }

        public void SetIsUseBot(bool isUseBot)
        {
            IsUseBot = isUseBot;
        }
        public void SetIsUseRTC(bool isUseRTC)
        {
            IsUseRTC = isUseRTC;
        }

        public void SetupCurrentSession(MultiplayerSessionManager multiplayerSession)
        {
            currentSession = multiplayerSession;
        }

        public void PlayerReady(VehicleType type)
        {
            gameManager.VehicleSelector.SetActive(false);
            PlayerReadyServerRpc(type);
        }

        /// <summary>
        /// Tell server player is ready, send tank configration settings.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PlayerReadyServerRpc(VehicleType vehicleType, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId; // Sender Id
                                                                   //SpawnNetworkPlayer()
            Debug.Log($"Server Set Tank Type! Type: {vehicleType} ID: {clientId}");
            currentSession.NetworkPlayerReady(vehicleType, clientId);
        }


        [ServerRpc(RequireOwnership = false)]
        public void PlayerFireServerRpc(float launchForce, ServerRpcParams serverRpcParams = default)
        {
            var senderClientId = serverRpcParams.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(senderClientId)) //ServerSide simulation
            {
                var client = NetworkManager.ConnectedClients[senderClientId];
                NetPlayer player;
                if(client.PlayerObject.TryGetComponent<NetPlayer>(out player))
                {
                    player.PlayerFireClientRpc(launchForce); //Execute client rpc on the client playerobj copy.
                }
            }
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnCrateHitServerRpc(ServerRpcParams serverRpcParams = default)
        {
            
            Debug.Log("OnCrateHitServerRPC!!");
            var senderClientId = serverRpcParams.Receive.SenderClientId;
            CrateContainerQueueNetwork crateContainerQueueNetwork = null;
            var vehicles = currentSession.GetVehicles();
            for (int i = 0; i < vehicles.Count; i++)
            {
                if (vehicles[i].m_clientId == senderClientId)
                {
                    var crateContainer = gameManager.DynamicMapObjectsManagerMultiplayer.GetCrateContainer(vehicles[i].m_PlayerNumber);
                    crateContainerQueueNetwork = crateContainer.GetComponent<CrateContainerQueueNetwork>();
                }
            }

            if (crateContainerQueueNetwork != null)
            {
                crateContainerQueueNetwork.SimulateBreakCrateContainerClientRpc(senderClientId);
            }
            
        }

        [ClientRpc]
        public void StartTankConfigrationClientRpc()
        {
            MicroWar.UIPanelManager.Instance.HideControllerUI(); //Hide UI
            gameManager.LefthandLaser.enabled = false;
            gameManager.RighthandLaser.enabled = false;
            gameManager.VehicleSelector.SetActive(true);
            environmentManager.ShowSphereForceByIndex(localPlayerIndex);
            inGameUIHandler.ShowMessageCanvas();
            float timeOffset = (float)NetworkManager.LocalTime.Time - (float)NetworkManager.ServerTime.Time;
            float countDownTime = gameManager.Settings.VehicleSelectDelay - timeOffset;
            StartCoroutine("SelectTankTimeout", countDownTime);
        }

        public IEnumerator SelectTankTimeout(float countDownTime)
        {
            string message = "Please pick your Vehicle";
            message += "\n\n";

            while (countDownTime > 0)
            {
                var messageText = message + countDownTime.ToString("#");
                inGameUIHandler.UpdateMessage(messageText);
                countDownTime = countDownTime - Time.deltaTime; //This should be fine. the difference between clients should be in few ms which is hard to noitice.
                yield return null;
            }
            inGameUIHandler.HideMessageCanvas();
            gameManager.VehicleSelector.SetActive(false);
            environmentManager.HideAllSphereForce();
        }

        [ClientRpc]
        public void RoundStartClientRpc(int roundNumber)
        {
            StopCoroutine("SelectTankTimeout");
            environmentManager.HideAllSphereForce();
            inGameUIHandler.ShowMessageCanvas();
            var messageText = "ROUND " + roundNumber;
            inGameUIHandler.UpdateMessage(messageText);
            //isPlaying = true;
            gameManager.VehicleSelector.SetActive(false);
            DisableVehicleControl();
        }

        [ClientRpc]
        public void RoundPlayingClientRpc()
        {
            inGameUIHandler.UpdateMessage(string.Empty);
            EnableVehicleControl();
        }

        [ClientRpc]
        public void RoundEndingClientRpc(string endMessage, bool hasSessionWinner, ulong winnerClientId)
        {
            inGameUIHandler.UpdateMessage(endMessage);
            UpdateLeaderboardAchivement(hasSessionWinner, winnerClientId);
        }

        [ClientRpc]
        public void ResetGameClientRpc()
        {
            Debug.LogWarning("Reset Game Client RPC");
            gameManager.LefthandLaser.enabled = true;
            gameManager.RighthandLaser.enabled = true;
            networkPlayerReference.Clear();
            inGameUIHandler.UpdateMessage(string.Empty);
            //MicroWar.UIPanelManager.Instance.ShowPanelUI();
            StartCoroutine(ReloadSceneCoroutine());
        }
        IEnumerator ReloadSceneCoroutine()
        {
            yield return new WaitForSeconds(gameManager.Settings.EndDelay);
            roomController.LeaveRoom();
            yield return new WaitUntil(()=> { return CanReloadScene;});
            SceneManager.LoadScene("Main");
        }
        private void DisableVehicleControl()
        {
            foreach (KeyValuePair<ulong,NetPlayer> kvp in networkPlayerReference)
            {
                if (kvp.Value.IsOwner)
                {
                    kvp.Value.DisableVehicleControl();
                }
            }
        }

        private void EnableVehicleControl()
        {
            foreach (KeyValuePair<ulong, NetPlayer> kvp in networkPlayerReference)
            {
                if (kvp.Value.IsOwner)
                {
                    kvp.Value.EnableVehicleControl();
                }
            }
        }


        public void AddNetworkPlayerReference(NetPlayer networkPlayer)
        {
            if (networkPlayerReference.ContainsKey(networkPlayer.NetworkObjectId))
            {
                networkPlayerReference.Remove(networkPlayer.NetworkObjectId);
            }
            networkPlayerReference.Add(networkPlayer.NetworkObjectId, networkPlayer);
        }

        public void RemoveNetworkPlayerReference(ulong playerNetworkId)
        {
            if (networkPlayerReference.ContainsKey(playerNetworkId))
                networkPlayerReference.Remove(playerNetworkId);
        }

        public List<Transform> GetNetworkPlayerReference(ulong[] playerIds)
        {
            List<Transform> networkPlayerTrans = new List<Transform>();
            for (int i = 0; i < playerIds.Length; i++)
            {
                NetPlayer playerTrans;
                var id = playerIds[i];
                if (networkPlayerReference.TryGetValue(id, out playerTrans))
                    networkPlayerTrans.Add(playerTrans.transform);
            }
            return networkPlayerTrans;
        }

        private void UpdateLeaderboardAchivement(bool hasSessionWinner, ulong WinnerClientId)
        {

            if(NetworkManager.LocalClientId == WinnerClientId)
            {
                Debug.Log($"[Leaderboard] : Update Score: Winner Client Id:{WinnerClientId}");
                if (hasSessionWinner)
                {
                    // Achievement : Win a match
                    AchievementManager.Instance.UpdateWinMatchAchievement();

                    // Achievement : Play a match
                    AchievementManager.Instance.UpdatePlayMatchAchievement();
                }
                else
                {
                    // Leaderboard : Increment winner score
                    LeaderboardManager.Instance.AddScoreEntry();

                    // Achievement : Win a round
                    AchievementManager.Instance.UpdateWinRoundsAchievement();

                    // Achievement : Play a round
                    AchievementManager.Instance.UpdatePlayRoundAchievement();
                }
            }
            
        }
    }

}

