using System.Collections.Generic;
using UnityEngine;
using Pico.Platform;
using Pico.Platform.Models;
using Unity.Netcode;
using System;
using System.Text;
namespace MicroWar.Platform
{
    public class PlatformController_Network : ControllerBase
    {
        public bool IsHost { get; private set; }
        public event ReceivePICOPackage OnReceivePICOPacket;
        public delegate void ReceivePICOPackage(NetworkEvent eventType, ulong clientId, ArraySegment<byte> payload = default);
        private const string CONNECTION_REQUEST = "MWCR";
        private NetcodeRoomData netcodeRoomData;
        private bool isInitialized = false;
        internal override void DelayInit()
        {
            if (!PlatformServiceManager.Instance.EnableNetwork || isInitialized)
                return;
            IsHost = false;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectedCallback;
            PlatformServiceManager.Instance.RegisterNotification<RoomUpdateEvent>(RoomUpdateEventHandler);
            PlatformServiceManager.Instance.RegisterNotification<RoomPlayerEvent>(RoomPlayerEventHandler);
            Debug.Log("Network controller Init");
            netcodeRoomData = new NetcodeRoomData();
            isInitialized = true;
        }

        private void RoomUpdateEventHandler(EventWrapper<RoomUpdateEvent> msg)
        {
            var roomEvent = msg.Data;
            //Room status update
            if (msg.NotificationType == NotificationType.RoomServiceStatus)
            {
                if (roomEvent.RoomServiceStatus == RoomServiceStatus.InRoom) 
                {
                    //Data
                    netcodeRoomData.UpdateRoomData(roomEvent.CurrentRoom);
                    //We are in a room now, startup Netcode.
                    StartupNetcode(netcodeRoomData.currentRoom);
                }
                else //Not In a room Shutdown Netcode
                {
                    ShutDownNetcode();
                }
            }
            //TODO: OPTIMIZE THIS, should use the same PICORoomData class that room controller use.
            //Room properties update
            if (msg.NotificationType == NotificationType.RoomProperties)
            {
                if (roomEvent.RoomServiceStatus == RoomServiceStatus.InRoom) 
                {
                    netcodeRoomData.UpdateRoomData(roomEvent.CurrentRoom);
                    Debug.Log($"Local client id:  {NetworkManager.Singleton?.LocalClientId}");
                }
                else //Not In a room Shutdown Netcode
                {
                    ShutDownNetcode();
                }
            }

        }
        private void RoomPlayerEventHandler(EventWrapper<RoomPlayerEvent> msg)
        {
            var roomPlayerEvent = msg.Data;
            switch (roomPlayerEvent.RoomUserActionType)
            {
                case RoomUserActionType.Join:
                    OnUserJoinRoom(roomPlayerEvent.Player.ID);
                    break;
                case RoomUserActionType.Leave:
                    OnUserLeaveRoom(roomPlayerEvent.Player.ID);
                    break;
                case RoomUserActionType.Kicked:
                    break;
                case RoomUserActionType.HostChange:
                    break;
                default:
                    break;
            }
        }

        private void OnUserJoinRoom(string userPID)
        {
            var userUID = (ulong)userPID.GetHashCode(); //TODO: optimize this. player uid should be in the map
            var myUID = (ulong)PlatformServiceManager.Instance.Me.ID.GetHashCode();
            if(userUID!=myUID && NetworkManager.Singleton.IsServer) //Server ready to receive connection request.
            {
                //OnReceivePICOPacket?.Invoke(NetworkEvent.Connect, userUID); 
            }
        }

        private void OnUserLeaveRoom(string userPID)
        {
            Debug.Log($"user leave room! {userPID}");
            //TODO: Need to manage the roomdata here?
            var userUID = (ulong)userPID.GetHashCode(); //TODO: optimize this. player uid should be in the map
            OnReceivePICOPacket?.Invoke(NetworkEvent.Disconnect, userUID);
        }

        private void StartupNetcode(Room currentRoom)
        {
            if (currentRoom != null)
            {
                var me = PlatformServiceManager.Instance.Me;
                NetworkManager.Singleton.NetworkConfig.ConnectionApproval = true;

                if (currentRoom.OwnerOptional.ID == me.ID)//TODO: Null check
                {
                    NetworkManager.Singleton.ConnectionApprovalCallback = ConnectionApprovalCallback;
                    NetworkManager.Singleton.StartHost();
                    IsHost = true;
                }
                else
                {
                    NetworkManager.Singleton.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(me.ID);
                    NetworkManager.Singleton.StartClient();
                    SendConnectRequest(currentRoom.OwnerOptional.ID);
                    IsHost = false;
                }
            }
        }

        private void SendConnectRequest(string hostId)
        {
            byte[] rawData =Encoding.ASCII.GetBytes(CONNECTION_REQUEST);
            NetworkService.SendPacket(hostId, rawData);
        }


        private void ShutDownNetcode()
        {
            Debug.Log($"[{nameof(PlatformController_Network)}]: Shutdown Netcode");
            IsHost = false;
            netcodeRoomData.Clear(); //Clear cached room data.
            NetworkManager.Singleton.Shutdown(true);
        }

        private void OnClientDisconnectedCallback(ulong clientId)
        {
            Debug.LogWarning($"[{nameof(PlatformController_Network)}]: Client {clientId} Disconnected");
            if (NetworkManager.Singleton.IsServer)
            {
                netcodeRoomData.ClientId2PIdMap.Remove(clientId); // Remove client.
            }
            //TODO: Optimize logics to handle player offline
            /*
            if (MultiplayerManager.Instance.isPlaying)
            {
                if (NetworkManager.Singleton.IsHost || netcodeRoomData.ClientId2PIdMap.Count<2)
                {
                    ShutDownNetcode();
                    MultiplayerManager.Instance.OnClientDisconnected();
                }
                MultiplayerManager.Instance.OnClientDisconnected();
            }*/
        }

        private void OnClientConnectedCallback(ulong obj)
        {
            Debug.Log($"[{nameof(PlatformController_Network)}]: Client {obj} Connected");
        }

        private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            DebugUtils.Log(nameof(PlatformController_Network), $"Connection approval! ID: {request.ClientNetworkId}");
            var playerOpenID = Encoding.ASCII.GetString(request.Payload);
            if (NetworkManager.ServerClientId == request.ClientNetworkId) //If the host require connection approval, the id will be 0;
            {
                playerOpenID = PlatformServiceManager.Instance.Me.ID;
            }
            netcodeRoomData.ClientId2PIdMap.Add(request.ClientNetworkId, playerOpenID); //TODO: Optimize this. This might be included in the room data.
            response.Approved = true;
            response.CreatePlayerObject = NetworkManager.Singleton.NetworkConfig.PlayerPrefab != null;
        }

        public string GetOpenID(ulong clientId)
        {
            if (netcodeRoomData.ClientId2PIdMap.ContainsKey(clientId))
            {
                return netcodeRoomData.ClientId2PIdMap[clientId];
            }
            return null;
        }

        public Dictionary<ulong, string> GetConnectedClients()
        {
            return netcodeRoomData.ClientId2PIdMap;
        }

        internal override void RegisterController()
        {
            PlatformServiceManager.Instance.AddController(this);
        }

        public bool SendPacket2UID(ulong clientUID, byte[] data)
        {
            string targetPID = string.Empty;

            if (clientUID == 0)//Client send data to server
            {
                return NetworkService.SendPacket(netcodeRoomData.HostPId, data, true);
            }

            if (netcodeRoomData.ParseUID2PID(clientUID,out targetPID)) // Server send data to clients
            {
                return NetworkService.SendPacket(targetPID, data, true);
            }
            return false;
        }

        /// <summary>
        /// Pull event every frame.
        /// </summary>
        public void PullEvent()
        {
            var packet = NetworkService.ReadPacket();
            while (packet != null)
            {
                HandleInComingPacket(packet);
                packet.Dispose();
                packet = NetworkService.ReadPacket();
            }
        }

        private void HandleInComingPacket(Packet packet)
        {
            byte[] data = new byte[packet.Size];
            ulong packetSize = packet.GetBytes(data);

            if (packetSize <= 0)
            {
                Debug.LogError("Error Packet Size!");
                return;
            }
            //Parse PID to UID
            string senderPID = packet.SenderId;
            ulong senderUID = default;
            if (senderPID == netcodeRoomData.HostPId)//Clients get packet, clients can only receive packet from server.
            {
                var payload = new ArraySegment<byte>(data, 0, data.Length);
                OnReceivePICOPacket?.Invoke(NetworkEvent.Data, 0, payload);
                return;
            }

            if (netcodeRoomData.ParsePID2UID(senderPID,out senderUID )) //Server get packet
            {
                if (packetSize < 5) //Connect request check
                {
                    if (data[0] == 'M' && data[1] == 'W' && data[2] == 'C' && data[3] == 'R')
                    {
                        Debug.Log($"[PlatformController_Network]Receive Connect Request!");
                        OnReceivePICOPacket?.Invoke(NetworkEvent.Connect, senderUID);
                        return;
                    }
                }
                var payload = new ArraySegment<byte>(data, 0, data.Length);
                OnReceivePICOPacket?.Invoke(NetworkEvent.Data, senderUID, payload);
            }
        }
        #region Unity Messages
        // Start is called before the first frame update
        void Start()
        {

        }
        #endregion
    }
}

