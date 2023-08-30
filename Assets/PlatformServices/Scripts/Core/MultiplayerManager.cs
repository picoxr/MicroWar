using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using MicroWar.Platform;
using Pico.Platform.Models;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using MicroWar;
using MicroWar.AI;

namespace MicroWar.Multiplayer
{
    public class MultiplayerManager : NetworkBehaviour, IGameManager
    {
        public static event System.Action<SessionState> OnGameStateChanged;

        [Header("Tank Prefabs")]
        public GameObject TankPrefab_Bear;
        public GameObject TankPrefab_Tiger;
        public GameObject TankPrefab_Cheetah;
        [Header("Tank Prefabs AI")]
        public GameObject TankPrefab_Bear_AI;
        public GameObject TankPrefab_Tiger_AI;
        public GameObject TankPrefab_Cheetah_AI;
        [Header("Spawn Points")]
        public Transform[] SpawnPoints;
        [Header("Board Game Parent")]
        public Transform BoardGameParent;
        public float CountDownSec = 0;
        [Header("XR Origin")]
        public Transform XROrigin;
        public Transform[] PlayerPos;


        public GameObject m_TankSelector;
        public bool m_TankIsDeployed = false;
        public GameObject m_Sphereforce;

        //private int ReadyPlayerCount = 0;
        private float PlayerReadyTimeDelay = 10f;
        private Dictionary<ulong, Transform> NetworkPlayerReference; // TODO: Client only?
        private Dictionary<ulong, VehicleType> readyPlayers;
        private PlatformController_Rooms roomController;
        private PlatformController_Network networkController;

        private List<VehicleManagerBase> Tanks;
        public int m_NumRoundsToWin = 3;            // The number of rounds a single player has to win to win the game.
        public float m_TankSelectDelay = 10f;
        public float m_StartDelay = 3f;             // The delay between the start of RoundStarting and RoundPlaying phases.
        public float m_EndDelay = 3f;               // The delay between the end of RoundPlaying and RoundEnding phases.
                                                    //public CameraControl m_CameraControl;       // Reference to the CameraControl script for control during different phases.
                                                    //public Text m_MessageText;                  // Reference to the overlay Text to display winning text, etc.
        public TMP_Text m_MessageText;
        public GameObject m_MessageCanvas;

        public float m_scaleFactor = .05f;
        public Transform m_boardGameOrigin;
        public Color[] m_TankColors;

        private int m_RoundNumber;                  // Which round the game is currently on.
        private WaitForSeconds m_StartWait;         // Used to have a delay whilst the round starts.
        private WaitForSeconds m_EndWait;           // Used to have a delay whilst the round or game ends.
        private VehicleManagerBase m_RoundWinner;          // Reference to the winner of the current round.  Used to make an announcement of who won.
        private VehicleManagerBase m_GameWinner;           // Reference to the winner of the game.  Used to make an announcement of who won.
        private VehicleType playerTankType = VehicleType.Tiger;
        //[Temp] Indicate if the game is playing.
        public bool isPlaying = false;
        private int m_MaxPlayerNum = 4;
        public bool useBot;
        private int playerIndex; //TODO: put this into room data class
        public DynamicMapObjectsManagerMultiplayer dynamicMapObjectsManager;

        public Transform SelfTransform;

        //Vehicle Shield 
        public GameObject shieldPrefab;

        #region Singleton
        private static MultiplayerManager instance;
        public static MultiplayerManager Instance
        {
            get
            {
                if (instance != null)
                {
                    return instance;
                }

                if (instance == null)
                {
                    instance = FindObjectOfType<MultiplayerManager>();
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
        void Start()
        {
            m_MessageCanvas.SetActive(false);
            Tanks = new List<VehicleManagerBase>();
            readyPlayers = new Dictionary<ulong, VehicleType>();
            NetworkPlayerReference = new Dictionary<ulong, Transform>();
            roomController = PlatformServiceManager.Instance.GetController<PlatformController_Rooms>(); //Get room controller instance to get room user's info.
            networkController = PlatformServiceManager.Instance.GetController<PlatformController_Network>();
            PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(RoomUpdateHandler);
        }

        private void OnDestroy()
        {
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
                        playerIndex = EventData.Data.CurrentRoom.UsersOptional.Count - 1;
                        SetupXROriginPos(playerIndex); //Set XR originPos after joinning a room;//TODO: may put this into room data class.
                    }
                }
                else
                {
                    SetupXROriginPos(0);
                    playerIndex = 0;
                }
            }
        }

        public void StartMultiplayerGame(bool enableBot) //Only host can call this
        {
            if (!IsServer)
                return;
            useBot = enableBot;
            StartCoroutine(GameStarting());
            //SpawnAllTanks();
        }

        private IEnumerator GameStarting()
        {
            OnGameStateChanged?.Invoke(SessionState.SessionStarting);
            SetupTanks();
            StartTankConfigrationClientRpc(NetworkManager.ServerTime.Time);
            var Timeout = m_TankSelectDelay;
            while (Timeout > 0 && !CheckPlayerReady())
            {
                Timeout = Timeout - Time.deltaTime;
                yield return null;
            }
            m_MessageText.text = string.Empty;
            SpawnAllTanks();
            StartCoroutine(GameLoop());
        }

        private void SetupTanks()
        {
            //Get all connected players in this room. 
            var playerList = networkController.GetConnectedClients();
            foreach (KeyValuePair<ulong, string> kvp in playerList)
            {
                TankManager_Multiplayer _tank = new TankManager_Multiplayer();
                _tank.m_userName = roomController.GetRoomUserDetails(kvp.Value).DisplayName;
                _tank.m_clientId = kvp.Key;
                _tank.VehicleType = VehicleType.Tiger; //Set default tank type;
                _tank.m_PlayerType = VehiclePlayerType.Network;
                if (kvp.Key == NetworkManager.Singleton.LocalClientId)
                {
                    _tank.IsSelf = true;
                }
                Tanks.Add(_tank);
            }

            var botNumber = m_MaxPlayerNum - playerList.Count;

            if (useBot) // Add bot
            {
                for (int i = 0; i < botNumber; i++)
                {
                    MicroWar.SinglePlayerVehicleManager _tank = new MicroWar.SinglePlayerVehicleManager();
                    _tank.m_userName = $"Bot {i}";
                    _tank.m_clientId = 999;
                    _tank.VehicleType = VehicleType.Tiger; //Set default tank type;
                    _tank.m_PlayerType = VehiclePlayerType.AI;

                    Tanks.Add(_tank);
                }
            }
            //Set Tank Prefabs and spawn points /colors
            for (int i = 0; i < Tanks.Count; i++)
            {
                var tankManager = Tanks[i];
                tankManager.m_SpawnPoint = SpawnPoints[i];
                tankManager.m_PlayerNumber = i;
                tankManager.m_PlayerColor = m_TankColors[i];
                tankManager.IsSpawned = false;
                //TODO: tankManager.GetTankHealth().SetShieldPrefab(shieldPrefab);
            }
        }

        private void SetTankPrefab(VehicleManagerBase tankManager)
        {
            var tankPlayerType = tankManager.m_PlayerType;
            var tankType = tankManager.VehicleType;
            GameObject tankPrefab;
            switch (tankType)
            {
                case VehicleType.Tiger:
                    tankPrefab = tankPlayerType == VehiclePlayerType.AI ? TankPrefab_Tiger_AI : TankPrefab_Tiger;
                    break;
                case VehicleType.Bear:
                    tankPrefab = tankPlayerType == VehiclePlayerType.AI ? TankPrefab_Bear_AI : TankPrefab_Bear;
                    break;
                case VehicleType.Cheetah:
                    tankPrefab = tankPlayerType == VehiclePlayerType.AI ? TankPrefab_Cheetah_AI : TankPrefab_Cheetah;
                    break;
                default:
                    tankPrefab = tankPlayerType == VehiclePlayerType.AI ? TankPrefab_Tiger_AI : TankPrefab_Tiger;
                    break;
            }
            tankManager.m_Instance = Instantiate(tankPrefab, tankManager.m_SpawnPoint.position, tankManager.m_SpawnPoint.rotation, m_boardGameOrigin);
        }

        private void HideAllSpawnPointForceFieldSphere()
        {
            for (int i = 0; i < SpawnPoints.Length; i++)
            {
                SpawnPoint Sp;
                SpawnPoints[i].TryGetComponent<SpawnPoint>(out Sp);
                if (Sp != null)
                {
                    Sp.DisableForceField();
                }
            }
        }

        private void ShowSpawnPointForceFieldSphere()
        {
            for (int i = 0; i < SpawnPoints.Length; i++)
            {
                SpawnPoint Sp;
                SpawnPoints[i].TryGetComponent<SpawnPoint>(out Sp);
                if (Sp != null && playerIndex == i)
                {
                    Sp.EnableForceField();
                }
            }
        }

        private bool CheckPlayerReady()
        {
            if (readyPlayers.Count >= NetworkManager.Singleton.ConnectedClients.Count)
                return true;
            else
                return false;
        }

    private void SpawnAllTanks()//Spawn all the remainning tanks TODO: Optimize
    {
        Debug.Log("Spawn all tanks!");
        for (int i = 0; i < Tanks.Count; i++)
        {
            var tankManager = Tanks[i];
            if(tankManager.IsSpawned == false)
            {
                if (tankManager.m_Instance == null)
                    SetTankPrefab(tankManager);
                //tankManager.Spawn();
                if (tankManager.m_PlayerType == VehiclePlayerType.AI)
                {
                    VehicleAIStateController aiController = tankManager.m_Instance.GetComponent<VehicleAIStateController>();
                    aiController.SetGameManager(GameManager.Instance);
                    aiController.SetVehicleManager(tankManager); //TODO: [TEMP] convert from TankManagerBase to tankmanager
                }
                tankManager.Setup(GameManager.Instance.VehicleConfigManager.GetVehicleSettings(tankManager.VehicleType));
                tankManager.GetVehicleHealth().SetShieldPrefab(shieldPrefab);//Setup shieldPrefab for bot
                tankManager.Spawn();
                Debug.Log($"Spawn Tank ID: {tankManager.m_clientId}");
            }
        }

            //Start game loop after spawning tanks.
            m_StartWait = new WaitForSeconds(m_StartDelay);
            m_EndWait = new WaitForSeconds(m_EndDelay);
        }

        private IEnumerator GameLoop()
        {
            yield return StartCoroutine(RoundStarting());
            yield return StartCoroutine(RoundPlaying());
            yield return StartCoroutine(RoundEnding());
            if (m_GameWinner != null)
            {
                ResetGameClientRpc(); // Rest Game logic instead reload the scene,
            }
            else
            {
                StartCoroutine(GameLoop());
            }
        }

        private IEnumerator RoundStarting()
        {
            // As soon as the round starts reset the tanks and make sure they can't move.
            //DisableTankControl();
            ResetAllTanks();

            // Snap the camera's zoom and position to something appropriate for the reset tanks.
            //m_CameraControl.SetStartPositionAndSize ();

            // Increment the round number and display text showing the players what round it is.
            m_RoundNumber++;

            RoundStartClientRpc(m_RoundNumber);
            OnGameStateChanged?.Invoke(SessionState.RoundStarting);
            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_StartWait;
        }

        private void SetupXROriginPos(int index)
        {
            XROrigin.position = PlayerPos[index].position;
            XROrigin.rotation = PlayerPos[index].rotation;
        }

        private IEnumerator RoundPlaying()
        {
            // As soon as the round begins playing let the players control the tanks.
            //EnableTankControl();

            // Clear the text from the screen.
            RoundPlayingClientRpc();
            OnGameStateChanged?.Invoke(SessionState.RoundPlaying);
            // While there is not one tank left...
            while (!OneTankLeft())
            {
                // ... return on the next frame.
                yield return null;
            }
        }

        private IEnumerator RoundEnding()
        {
            // Stop tanks from moving.
            //DisableTankControl();

            // Clear the winner from the previous round.
            m_RoundWinner = null;

            // See if there is a winner now the round is over.
            m_RoundWinner = GetRoundWinner();

            // If there is a winner, increment their score.
            if (m_RoundWinner != null)
            {
                m_RoundWinner.m_Wins++;
                LeaderboardManager.Instance.AddScoreEntry(m_RoundWinner.m_userName);
            }

            // Now the winner's score has been incremented, see if someone has one the game.
            m_GameWinner = GetGameWinner();

            // Get a message based on the scores and whether or not there is a game winner and display it.
            string message = EndMessage();
            //m_MessageText.text = message;
            RoundEndingClientRpc(message);
            OnGameStateChanged?.Invoke(SessionState.RoundEnding);
            // Wait for the specified length of time until yielding control back to the game loop.
            yield return m_EndWait;
        }

        private bool OneTankLeft()
        {
            // Start the count of tanks left at zero.
            int numTanksLeft = 0;

            // Go through all the tanks...
            for (int i = 0; i < Tanks.Count; i++)
            {
                // ... and if they are active, increment the counter.
                if (Tanks[i].m_Instance.activeSelf)
                    numTanksLeft++;
            }

            // If there are one or fewer tanks remaining return true, otherwise return false.
            return numTanksLeft <= 1;
        }

        private VehicleManagerBase GetRoundWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < Tanks.Count; i++)
            {
                // ... and if one of them is active, it is the winner so return it.
                if (Tanks[i].m_Instance.activeSelf)
                    return Tanks[i];
            }

            // If none of the tanks are active it is a draw so return null.
            return null;
        }


        // This function is to find out if there is a winner of the game.
        private VehicleManagerBase GetGameWinner()
        {
            // Go through all the tanks...
            for (int i = 0; i < Tanks.Count; i++)
            {
                // ... and if one of them has enough rounds to win the game, return it.
                if (Tanks[i].m_Wins == m_NumRoundsToWin)
                    return Tanks[i];
            }

            // If no tanks have enough rounds to win, return null.
            return null;
        }


        // Returns a string message to display at the end of each round.
        private string EndMessage()
        {
            // By default when a round ends there are no winners so the default end message is a draw.
            string message = "DRAW!";

            // If there is a winner then change the message to reflect that.
            if (m_RoundWinner != null)
                message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

            // Add some line breaks after the initial message.
            message += "\n\n\n\n";

            // Go through all the tanks and add each of their scores to the message.
            for (int i = 0; i < Tanks.Count; i++)
            {
                message += Tanks[i].m_ColoredPlayerText + ": " + Tanks[i].m_Wins + " WINS\n";
            }

            // If there is a game winner, change the entire message to reflect that.
            if (m_GameWinner != null)
                message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

            return message;
        }


        // This function is used to turn all the tanks back on and reset their positions and properties.
        private void ResetAllTanks()
        {
            for (int i = 0; i < Tanks.Count; i++)
            {
                Tanks[i].RespawnTank();
            }
        }

        private float GetClientRPCTimeDifference(double Time)
        {
            return (float)(NetworkManager.LocalTime.Time - Time);
        }

        private NetPlayer GetLocalNetworkPlayer()
        {
            NetPlayer netPlayer = null;
            NetworkObject playerObject = NetworkManager.Singleton.LocalClient.PlayerObject;
            if (null != playerObject)
            {
                netPlayer = playerObject.GetComponent<NetPlayer>();
            }
            return netPlayer;
        }

        private void EnableTankControl()
        {
            foreach (KeyValuePair<ulong, Transform> kvp in NetworkPlayerReference)
            {
                //kvp.Value.GetComponent<NetPlayer>().EnableTankControl();
            }
        }


        private void DisableTankControl() //Client call
        {
            foreach (KeyValuePair<ulong, Transform> kvp in NetworkPlayerReference)
            {
                //kvp.Value.GetComponent<NetPlayer>().DisableTankControl();
            }
        }

        public void AddNetworkPlayerReference(Transform networkPlayer, ulong playerNetworkId)
        {
            if (NetworkPlayerReference.ContainsKey(playerNetworkId))
            {
                NetworkPlayerReference.Remove(playerNetworkId);
            }
            NetworkPlayerReference.Add(playerNetworkId, networkPlayer);
        }

        public void RemoveNetworkPlayerReference(ulong playerNetworkId)
        {
            if (NetworkPlayerReference.ContainsKey(playerNetworkId))
                NetworkPlayerReference.Remove(playerNetworkId);
        }

        public Transform GetNetworkPlayerReference(ulong playerId)
        {
            Transform playerTrans;
            NetworkPlayerReference.TryGetValue(playerId, out playerTrans);
            return playerTrans;
        }

        public List<Transform> GetNetworkPlayerReference(ulong[] playerIds)
        {
            List<Transform> networkPlayerTrans = new List<Transform>();
            for (int i = 0; i < playerIds.Length; i++)
            {
                Transform playerTrans;
                var id = playerIds[i];
                if (NetworkPlayerReference.TryGetValue(id, out playerTrans))
                    networkPlayerTrans.Add(playerTrans);
            }
            return networkPlayerTrans;
        }

        public void NetworkPlayerReady()//TODO: optimize this.
        {
            var tankType = (VehicleType)GameManager.Instance.Settings.PlayerVehicleType;
            Debug.Log("Hide all force field sphere");
            m_TankSelector.SetActive(false);
            HideAllSpawnPointForceFieldSphere();
            PlayerReadyServerRpc(tankType);
        }

        public void ResetGame() //Local version reset game. Execute this when leave a room.
        {
            m_RoundNumber = 0;
            m_GameWinner = null;
            readyPlayers.Clear();
            NetworkPlayerReference.Clear();
            isPlaying = false;
            m_MessageText.text = string.Empty;
            if (IsServer)
            {
                for (int i = 0; i < Tanks.Count; i++)
                {
                    Tanks[i].DespawnTank();
                }
            }
            Tanks.Clear();
        }

        #region ServerRPC
        /// <summary>
        /// Tell server player is ready, send tank configration settings.
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void PlayerReadyServerRpc(VehicleType tankType, ServerRpcParams serverRpcParams = default)
        {
            var clientId = serverRpcParams.Receive.SenderClientId; // Sender Id
                                                                   //SpawnNetworkPlayer()
            Debug.Log($"Server Set Tank Type! Type: {tankType} ID: {clientId}");
            //TODO: Logic to start a session
            if (!readyPlayers.ContainsKey(clientId))
            {
                readyPlayers.Add(clientId, tankType);
            }
            ServerSetTankType(tankType, clientId);
        }

        private void ServerSetTankType(VehicleType tankType, ulong clientId)
        {

        for (int i = 0; i < Tanks.Count; i++)
        {
            var tankManager = Tanks[i];
            if (tankManager.m_clientId == clientId)
            {
                tankManager.VehicleType = tankType;
                SetTankPrefab(tankManager);
                tankManager.Setup(GameManager.Instance.VehicleConfigManager.GetVehicleSettings(tankManager.VehicleType));
                tankManager.Spawn();
                Debug.Log($"Spawn Tank ID: {tankManager.m_clientId}");
            }
        }
    }

        [ServerRpc(RequireOwnership = false)]
        public void PlayerFireServerRpc(float launchForce, ServerRpcParams serverRpcParams = default)
        {
            var senderClientId = serverRpcParams.Receive.SenderClientId;
            if (NetworkManager.ConnectedClients.ContainsKey(senderClientId)) //ServerSide simulation
            {
                var client = NetworkManager.ConnectedClients[senderClientId];
                NetPlayer player = client.PlayerObject.GetComponent<NetPlayer>();
                player.PlayerFireClientRpc(launchForce); //Execute client rpc on the client playerobj copy.
            }
            //TODO: error handler?
        }

        [ServerRpc(RequireOwnership = false)]
        public void OnCrateHitServerRpc(ServerRpcParams serverRpcParams = default)
        {
            Debug.Log("OnCrateHitServerRPC!!");
            var senderClientId = serverRpcParams.Receive.SenderClientId;
            CrateContainerQueueNetwork crateContainerQueueNetwork = null;
            for (int i = 0; i < Tanks.Count; i++)
            {
                if (Tanks[i].m_clientId == senderClientId)
                {
                    var crateContainer = dynamicMapObjectsManager.GetCrateContainer(Tanks[i].m_PlayerNumber);
                    crateContainerQueueNetwork = crateContainer.GetComponent<CrateContainerQueueNetwork>();
                }
            }

            if (crateContainerQueueNetwork != null)
            {
                crateContainerQueueNetwork.SimulateBreakCrateContainerClientRpc(senderClientId);
            }

        }


        #endregion

        #region ClientRPC

        [ClientRpc]
        private void RoundEndingClientRpc(string endMessage)
        {
            m_MessageText.text = endMessage;
            //DisableTankControl();
        }

        [ClientRpc]
        private void StartTankConfigrationClientRpc(double time)
        {
            //m_Sphereforce.SetActive(true);
            m_TankSelector.SetActive(true);
            MicroWar.UIPanelManager.Instance.HideControllerUI(); //Hide UI
            ShowSpawnPointForceFieldSphere();
            m_MessageCanvas.SetActive(true);
            float timeOffset = (float)NetworkManager.LocalTime.Time - (float)time;
            float countDownTime = m_TankSelectDelay - timeOffset;
            Debug.Log($"[TIME] Count down time: {countDownTime}");
            StartCoroutine("SelectTankTimeout", countDownTime);
        }

        private IEnumerator SelectTankTimeout(float countDownTime)
        {
            string message = "Please pick your tank";
            message += "\n\n";

            while (countDownTime > 0)
            {
                m_MessageText.text = message + countDownTime.ToString("#");
                countDownTime = countDownTime - Time.deltaTime; //This should be fine. the difference between clients should be in few ms which is hard to noitice.
                yield return null;
            }
            m_MessageCanvas.SetActive(false);
            m_TankSelector.SetActive(false);
            HideAllSpawnPointForceFieldSphere();
        }

        [ClientRpc]
        private void RoundStartClientRpc(int roundNumber)
        {
            if (IsServer)
                Debug.Log("Client RPC Server Excute");
            else
                Debug.Log("Client RPC");

            StopCoroutine("SelectTankTimeout");
            HideAllSpawnPointForceFieldSphere();//Hide All Force Field
            m_MessageCanvas.SetActive(true);
            m_MessageText.text = "ROUND " + roundNumber;
            isPlaying = true;
            m_TankSelector.SetActive(false);//Hide tank selector in case player didn't choose a tank.
                                            //Disable control for seconds when receiving this.//TODO cauculate the time.
            DisableTankControl();
        }

        [ClientRpc]
        private void RoundPlayingClientRpc()
        {
            m_MessageText.text = string.Empty;
            EnableTankControl();
        }

        [ClientRpc]
        private void ResetGameClientRpc()
        {
            m_RoundNumber = 0;
            m_GameWinner = null;
            readyPlayers.Clear();
            isPlaying = false;
            m_MessageText.text = string.Empty;
            if (IsServer)
            {
                for (int i = 0; i < Tanks.Count; i++)
                {
                    Tanks[i].DespawnTank();
                }
            }
            Tanks.Clear();
            //TODO: temp, show the UI panel
            MicroWar.UIPanelManager.Instance.ShowPanelUI();
        }
        #endregion
        public void OnClientDisconnected()
        {
            ResetGame();
            MicroWar.UIPanelManager.Instance.ResetControllerUI();
            MicroWar.UIPanelManager.Instance.ShowPanelUI();
        }

        public List<VehicleManagerBase> GetAliveEnemiesOf(VehicleManagerBase tank)
        {
            List<VehicleManagerBase> tankList = new List<VehicleManagerBase>();
            for (int i = 0; i < Tanks.Count; i++)
            {
                VehicleManagerBase currentTank = Tanks[i];
                if (currentTank == tank || currentTank.GetVehicleHealth().IsDead()) continue;

                tankList.Add(currentTank);
            }
            return tankList;
        }

        public int GetActivePowerUpCount()
        {
            int count = 0;
            if (dynamicMapObjectsManager != null)
            {
                count = dynamicMapObjectsManager.GetActivePowerUpCount();
            }
            return count;
        }

        public List<Transform> GetAliveEnemyTransformsOf(VehicleManagerBase tank)
        {
            List<Transform> tankList = new List<Transform>();
            for (int i = 0; i < Tanks.Count; i++)
            {
                VehicleManagerBase currentTank = Tanks[i];
                if (currentTank.m_PlayerNumber == tank.m_PlayerNumber || currentTank.GetVehicleHealth().IsDead()) continue;

                tankList.Add(currentTank.m_Instance.transform);
            }
            return tankList;
        }
    }




}

