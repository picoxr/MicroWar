using MicroWar.Multiplayer;
using MicroWar.Platform;
using Pico.Avatar;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

namespace MicroWar.Avatar
{
    public class AvatarMultiplayerManager : NetworkBehaviour
    {
        private double networkSyncTime = 0;
        private double lastPacketSentAt = double.MinValue;
        private bool canStartSending = false;

        private Dictionary<string, AvatarController> clientAvatarMap;
        private HashSet<string> clientAvatarIdsSet;

        private void Start()
        {
            MultiplayerBehaviour.Instance.OnPlayerSpawnPointsUpdated += OnPlayerSpawnPointsUpdated;

            clientAvatarMap = new Dictionary<string, AvatarController>();
            clientAvatarIdsSet = new HashSet<string>(); 
        }

        private void Update()
        {
            networkSyncTime += Time.unscaledDeltaTime;

            //Play the actions, and the action data (retrieved via entity.ApplyPacket) on the remote avatars 
            //Note: It is recommended to call this API at every frame to ensure continuity of actions.
            PicoAvatarManager.instance.SyncNetSimulation(networkSyncTime); //May need to modify here and utilize a network delay like SyncNetSimulation(networkSyncTime - networkDelay)

            //Sending Avatar Packet Data of the main user avatar
            if (canStartSending && AvatarManager.Instance?.MainUserAvatar != null)
            {
                SendAvatar();
            }
        }

        public override void OnDestroy()
        {
            if (MultiplayerBehaviour.Instance != null)
            {
                MultiplayerBehaviour.Instance.OnPlayerSpawnPointsUpdated -= OnPlayerSpawnPointsUpdated;
            }
            base.OnDestroy();
        }

        private void OnPlayerSpawnPointsUpdated(Dictionary<string, int> playerSpawnPoints)
        {
 
            Debug.Log($"OnPlayerSpawnPointsUpdated - playerSpawnPoints count {playerSpawnPoints?.Count}");

            HashSet<string> newClientAvatarsSet = new HashSet<string>();

            //playerSpawnPoints.Count > 0 means that there is at least a user connected so we can start sending main user avatar data to the server.
            if (playerSpawnPoints != null && playerSpawnPoints.Count > 0)
            {
                canStartSending = true;
            }
            else 
            {
                canStartSending = false;
            }

            foreach(KeyValuePair<string,int> players in playerSpawnPoints)
            {
                if (clientAvatarMap.TryGetValue(players.Key, out AvatarController controller))
                {
                    //We have already created this player's avatar 
                    //TODO: Change Avatar Transform Parent based on the new spawn points
                }
                else 
                {
                    if(players.Key != PlatformServiceManager.Instance.Me.ID) 
                    {
                        AvatarController avatar = CreateRemoteAvatar(players.Key, players.Value);
                        clientAvatarMap.Add(players.Key, avatar);
                        newClientAvatarsSet.Add(players.Key);
                    }
                }
            }

            clientAvatarIdsSet.ExceptWith(newClientAvatarsSet);
            //Remove avatars of the players who are no longer in the room.
            foreach(string avatarId in clientAvatarIdsSet) 
            {
                DestroyAvatar(avatarId);
            }
            clientAvatarIdsSet = newClientAvatarsSet;
        }

        private AvatarController CreateRemoteAvatar(string playerId, int spawnPoint)
        {
            AvatarController avatarController = AvatarManager.Instance.LoadAvatar(playerId, GameManager.Instance.EnvironmentManager.GetPlayerSpawnPoints()[spawnPoint]);

            avatarController.Avatar.AddFirstEntityReadyCallback((picoAvatar, avatarEntity) =>
            {
                if (avatarEntity == null)
                    return;

                Debug.Log($"picoAvatar.entity.deviceInputReader = {picoAvatar.entity.deviceInputReader.GetType().Name}");
                picoAvatar.PlayAnimation("idle");
            });

            return avatarController;
        }

        //Records a network packet of the main user avatar and sends the packet to the server
        private void SendAvatar()
        {
            if (networkSyncTime - lastPacketSentAt < PicoAvatarApp.instance.netBodyPlaybackSettings.recordInterval)
            {
                return;
            }

            AvatarEntity entity = AvatarManager.Instance.MainUserAvatar.Avatar.entity;

            entity.RecordPacket(networkSyncTime);
            lastPacketSentAt = networkSyncTime;

            MemoryView avatarMemoryView = entity.GetFixedPacketMemoryView();
            if (avatarMemoryView != null)
            {
                SendAvatarPacketServerRpc(avatarMemoryView.GetData());
            }
        }

        private void DestroyAvatar(string avatarId)
        { 
            if(string.IsNullOrEmpty(avatarId)) return;

            if (clientAvatarMap.TryGetValue(avatarId, out AvatarController avatar))
            {
                clientAvatarMap.Remove(avatarId);
                AvatarManager.Instance.UnloadAvatar(avatar.Avatar);
                avatar.gameObject.SetActive(false);
                Destroy(avatar, 1f);
            }
        }

        //Server handles the incoming avatarData and distributes it to rest of the clients
        [ServerRpc(RequireOwnership=false)]
        private void SendAvatarPacketServerRpc(byte[] avatarData, ServerRpcParams serverRpcParams = default)
        {   
            ulong clientId = serverRpcParams.Receive.SenderClientId;
 
            ulong[] targetClients = new ulong[NetworkManager.ConnectedClients.Count - 1];
            int index = 0;

            //Target clients are all the connected clients except the one who has sent us this avatar packet.
            foreach (NetworkClient client in NetworkManager.ConnectedClients.Values)
            {
                if(client.ClientId != clientId) 
                {
                    targetClients[index++] = client.ClientId;
                }
            }

            ClientRpcParams clientRpcParams = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = targetClients
                }
            };

            string clientPlatformId = MultiplayerBehaviour.Instance.NetworkController.GetOpenID(clientId);

            //Telling the rest of the clients to update this user's avatar
            ApplyAvatarPacketClientRpc(avatarData, clientPlatformId, clientRpcParams);
        }

        //ClientRPC applying the incoming avatar packet of a specific user to the corresponding remote avatar instance.
        [ClientRpc]
        private void ApplyAvatarPacketClientRpc(byte[] avatarData, string clientPlatformId, ClientRpcParams clientRpcParams = default)
        {
            AvatarController otherAvatar = null;

            if (!clientAvatarMap.TryGetValue(clientPlatformId, out otherAvatar))
            {
                Debug.Log($"Can not find the remote avatar for the clientPlatformId={clientPlatformId}");
            }

            if (otherAvatar != null && otherAvatar.Avatar != null && otherAvatar.Avatar.isAnyEntityReady)
            {
                Debug.Log($"Applying packet avatarData={avatarData.Length} - OwnerClientId={this.OwnerClientId} - LocalClientId={NetworkManager.LocalClientId}");
                MemoryView avatarMemView = new MemoryView(avatarData, false);
                otherAvatar.Avatar.entity.ApplyPacket(avatarMemView);
                avatarMemView.CheckDelete();
            }
        }   

    }
}

