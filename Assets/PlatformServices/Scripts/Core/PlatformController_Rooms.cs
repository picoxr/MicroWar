using System;
using System.Collections.Generic;
using UnityEngine;
using Pico.Platform;
using Pico.Platform.Models;

namespace MicroWar.Platform
{
    public class PlatformController_Rooms : ControllerBase
    {
        public bool IsInitialized => isInitialized;
        private bool isInitialized = false;
        private const string poolName = "MicroWar_General";
        //Data
        private PICOInRoomData roomData;
        //private MatchmakingRoomList retrievedRoomList;
        private RoomList retrievedRoomList;
        //Status
        private RoomServiceStatus serviceStatus = RoomServiceStatus.NotAvailable;
        private RoomListRetrieveStatus roomListRetrieveStatus = RoomListRetrieveStatus.Unknown;

        internal override void DelayInit()
        {
            //Init status
            serviceStatus = RoomServiceStatus.Idle;
            roomListRetrieveStatus = RoomListRetrieveStatus.Idle;
            //Room data
            roomData = new PICOInRoomData();
            roomData.OnPlayerJoinRoom = msg => { NotifyHandlers(NotificationType.RoomPlayerJoin, msg); };
            roomData.OnPlayerLeaveRoom = msg => { NotifyHandlers(NotificationType.RoomPlayerLeave, msg); };
            //Set callbacks
            RoomService.SetJoin2NotificationCallback(msg => { });
            RoomService.SetLeaveNotificationCallback(OnLeaveNotification); // Passively leave the room callback
            RoomService.SetUpdateNotificationCallback(OnRoomUpdate);//Room Properties Update
            RoomService.SetKickUserNotificationCallback(msg => { });
            RoomService.SetRoomInviteAcceptedNotificationCallback(msg => { });
            // Room module initialized
            isInitialized = true;
            Debug.Log("Room controller init");
        }
        #region PICO SDK Callbacks
        private void OnLeaveNotification(Message<Room> msg)
        {
            if (msg.IsError)
                return;
            UpdateRoomServiceStatus(RoomServiceStatus.Idle, msg.Data);
            platformServiceManager.HandlePlatformErrors(PlatformErrors.PassivelyLeaveRoom, msg.Error.Code.ToString()); //Notify error handler
            DebugUtils.Log(nameof(PlatformController_Rooms), $"Passively Leave Room! Room ID: {msg.Data.RoomId}");
        }

        private void OnRoomUpdate(Message<Room> msg)
        {
            if (msg.IsError)
                return;
            var data = msg.Data;
            UpdateRoomProperties(data);
        }
        #endregion

        [ContextMenu("Generate Room Name")]
        public void TestRoomName()
        {
            string userID = platformServiceManager.Me.ID;
            string roomName = userID + DateTime.Now.ToString();
            Debug.Log($"Room Name: {roomName}");
        }

        public void CreateRoom(uint maxUser)
        {
            if (!isInitialized || serviceStatus != RoomServiceStatus.Idle)
                return;
            //Named Room
            UpdateRoomServiceStatus(RoomServiceStatus.Processing, null); // Processing
            //Get logged in user ID to set a unique room name.
            string userID = platformServiceManager.Me.ID;
            string roomName = userID + DateTime.Now.ToString();
            RoomOptions op = new RoomOptions();
            op.GetHashCode();
            op.SetRoomName(roomName);//[TEMP]Use user ID and current time to set an Unique room name.
            RoomService.JoinOrCreateNamedRoom(RoomJoinPolicy.Everyone, true, maxUser, op).OnComplete(msg => {
                if (msg.IsError)
                {
                    UpdateRoomServiceStatus(RoomServiceStatus.Idle, null); // Error, back to idle
                    platformServiceManager.HandlePlatformErrors(PlatformErrors.CreateRoomFailed, msg.Error.Code.ToString());  // Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Create Room Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    return;
                }
                //Create Room Success
                UpdateRoomServiceStatus(RoomServiceStatus.InRoom, msg.Data); // Success, in room
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Create Public Room Success! Room ID: {msg.Data.RoomId}");
            });
            /*
            //MatchMacking Room
            UpdateRoomServiceStatus(RoomServiceStatus.Processing, null); // Processing
            MatchmakingOptions op = new MatchmakingOptions();
            op.SetCreateRoomJoinPolicy(RoomJoinPolicy.Everyone);
            op.SetCreateRoomMaxUsers(MaxUser);
            MatchmakingService.CreateAndEnqueueRoom2(poolName, op).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    UpdateRoomServiceStatus(RoomServiceStatus.Idle, null); // Error, back to idle
                    PlatformServiceManager.HandlePlatformErrors(PlatformErrors.CreateRoomFailed, msg.Error.Code.ToString());  // Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Create Room Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    return;
                }
                //Create Room Success
                UpdateRoomServiceStatus(RoomServiceStatus.InRoom, msg.Data.Room); // Success, in room
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Create Public Room Success! Room ID: {msg.Data.Room.RoomId}");
            });*/
        }
        public bool CanCreateRoom() //TODO: temp code here. Remove this. 
        {
            if (!isInitialized || serviceStatus != RoomServiceStatus.Idle)
            {
                return false;
            }
            return true;
        }
        public void JoinToRoom(ulong roomID)
        {
            if (!isInitialized || serviceStatus != RoomServiceStatus.Idle)
                return;

            if (roomID == 0)
                return;

            UpdateRoomServiceStatus(RoomServiceStatus.Processing, null); //Processing
            RoomOptions op = new RoomOptions();
            RoomService.Join2(roomID, op).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    UpdateRoomServiceStatus(RoomServiceStatus.Idle, null);
                    platformServiceManager.HandlePlatformErrors(PlatformErrors.JoinRoomFailed, msg.Error.Code.ToString()); //Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Join Room Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                }
                //Join Room Success
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Join Room Success! Room ID: {msg.Data.RoomId}");
                UpdateRoomServiceStatus(RoomServiceStatus.InRoom, msg.Data); //Success in room
            });
        }

        public void LeaveRoom()
        {
            if (serviceStatus != RoomServiceStatus.InRoom || roomData.currentRoom == null)
                return; //TODO: error handler?

            RoomService.Leave(roomData.currentRoom.RoomId).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    platformServiceManager.HandlePlatformErrors(PlatformErrors.LeaveRoomFailed, msg.Error.Code.ToString());//Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Leave Room Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                }
                UpdateRoomServiceStatus(RoomServiceStatus.Idle);
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Leave Room Success! Room ID: {msg.Data.RoomId}");
            });
        }

        public void RetrieveRoomList()
        {
            if (!isInitialized || roomListRetrieveStatus!= RoomListRetrieveStatus.Idle)
                return;
         
            UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Processing);
            RoomService.GetNamedRooms(0, 20).OnComplete(msg =>  //TODO: We may need design room pages on the UI;
            {
                if (msg.IsError)
                {
                    UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Idle, null);
                    platformServiceManager.HandlePlatformErrors(PlatformErrors.RetrieveRoomListFailed, msg.Error.Code.ToString());//Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Retrieve Room List Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    return;
                }
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Retrieve Room List Success!");
                var roomList = msg.Data;
                UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Idle, roomList);

                //--------------------------TEST---------------------------------
                foreach (Room room in roomList)
                {
                    DebugUtils.Log(nameof(PlatformController_Rooms), $"Room: {room.RoomId}");
                }
            });

            //Matchmacking Room
            /*
            MatchmakingOptions op = new MatchmakingOptions();
            UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Processing);
            MatchmakingService.Browse2(poolName, op).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Idle, null);
                    PlatformServiceManager.HandlePlatformErrors(PlatformErrors.RetrieveRoomListFailed, msg.Error.Code.ToString());//Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Retrieve Room List Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    return;
                }
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Retrieve Room List Success!");
                var roomList = msg.Data.MatchmakingRooms;
                UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Idle, roomList);

                //--------------------------TEST---------------------------------
                foreach (MatchmakingRoom room in roomList)
                {
                    DebugUtils.Log(nameof(PlatformController_Rooms), $"Room: {room.Room.RoomId}");
                }
            });*/
        }

        private void UpdateRoomServiceStatus(RoomServiceStatus status, Room room = null, bool NotifyHandler = true)
        {
            Debug.Log("Update room status");
            serviceStatus = status;
            //First time join to a room
            if (status == RoomServiceStatus.InRoom)
            {
                roomData.InitInRoomData(room);
            }
            else
            {
                roomData.Clear();
            }

            if (NotifyHandler)
            {
                NotifyHandlers(NotificationType.RoomServiceStatus);
            }
        }

        private void UpdateRoomProperties(Room room)
        {
            Debug.Log("Update room Properties");
            //currentRoom = room;
            if (serviceStatus == RoomServiceStatus.InRoom)
            {
                Debug.Log("Parse room status room props");
                //ParsePlayerList(room);
                roomData.UpdateInRoomData(room);
                //HandleRoomPlayerAction(room);
            }
            NotifyHandlers(NotificationType.RoomProperties);
        }

        private void UpdateRoomListRetrieveStatus(RoomListRetrieveStatus status, RoomList roomList = null)
        {
            roomListRetrieveStatus = status;
            retrievedRoomList = roomList;
            NotifyHandlers(NotificationType.RoomListRetrieveStatus);
        }

        private void NotifyHandlers(NotificationType notificationType, User user = null)
        {
            switch (notificationType)
            {
                case NotificationType.RoomServiceStatus:
                    RoomUpdateEvent roomStatusEvent = new RoomUpdateEvent(RoomUpdateType.RoomStatus, serviceStatus, roomData.currentRoom);
                    EventWrapper<RoomUpdateEvent> roomStatusEventWrapper = new EventWrapper<RoomUpdateEvent>(notificationType, roomStatusEvent);
                    platformServiceManager.NotifyEventHandler(roomStatusEventWrapper);
                    break;
                case NotificationType.RoomProperties:
                    RoomUpdateEvent roomPropertiesEvent = new RoomUpdateEvent(RoomUpdateType.RoomProperties, serviceStatus, roomData.currentRoom);
                    EventWrapper<RoomUpdateEvent> roomProperitiesEventWrapper = new EventWrapper<RoomUpdateEvent>(notificationType, roomPropertiesEvent);
                    platformServiceManager.NotifyEventHandler(roomProperitiesEventWrapper);
                    break;
                case NotificationType.RoomListRetrieveStatus:
                    RoomListEvent roomListEvent = new RoomListEvent(roomListRetrieveStatus, retrievedRoomList);
                    EventWrapper<RoomListEvent> roomListEventWrapper = new EventWrapper<RoomListEvent>(notificationType, roomListEvent);
                    platformServiceManager.NotifyEventHandler(roomListEventWrapper);
                    break;
                case NotificationType.RoomPlayerJoin:
                    RoomPlayerEvent playerJoinEvent = new RoomPlayerEvent(RoomUserActionType.Join, user);
                    EventWrapper<RoomPlayerEvent> playerJoinEventWrapper = new EventWrapper<RoomPlayerEvent>(notificationType, playerJoinEvent);
                    platformServiceManager.NotifyEventHandler(playerJoinEventWrapper);
                    break;
                case NotificationType.RoomPlayerLeave:
                    RoomPlayerEvent playerLeaveEvent = new RoomPlayerEvent(RoomUserActionType.Leave, user);
                    EventWrapper<RoomPlayerEvent> playerLeaveEventWrapper = new EventWrapper<RoomPlayerEvent>(notificationType, playerLeaveEvent);
                    platformServiceManager.NotifyEventHandler(playerLeaveEventWrapper);
                    break;
            }
        }


        public RoomInfo GetRoomServiceStatus()
        {
            var data = new RoomInfo(serviceStatus, roomData.currentRoom, roomListRetrieveStatus, retrievedRoomList);
            return data;
        }

        public User GetRoomUserDetails(string PId)
        {
            if (roomData.userDictionary.ContainsKey(PId))
            {
                return roomData.userDictionary[PId];
            }
            Debug.LogWarning("[PlatformController_Rooms]: Invalid User!");
            return null;
        }

        internal override void RegisterController()
        {
            PlatformServiceManager.Instance.AddController(this);
        }

        #region Unity Messages
        // Start is called before the first frame update
        private void OnEnable()
        {
        }
        void Start()
        {
        }

        #endregion
    }


}

