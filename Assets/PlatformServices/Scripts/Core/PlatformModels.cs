using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pico.Platform;
using Pico.Platform.Models;

namespace MicroWar.Platform
{
    public enum ServiceInitializeStatus
    {
        Unknown,
        Initializing,
        initialized,
        Failed,
    }

    public enum RoomServiceStatus
    {
        UnKnown,
        NotAvailable,
        Idle,
        Processing,
        InRoom,
    }

    public enum RoomUserActionType
    {
        Join,
        Leave,
        Kicked,
        HostChange
    }

    public enum RoomUpdateType
    {
        RoomStatus,
        RoomProperties,
    }

    public enum PlatformErrors
    {
        PassivelyLeaveRoom,
        CreateRoomFailed,
        RetrieveRoomListFailed,
        JoinRoomFailed,
        ConnectToHostFailed,
        LeaveRoomFailed,
        ServerError,
    }

    public enum RoomListRetrieveStatus
    {
        Unknown,
        Idle,
        Processing,
    }

    public enum NotificationType
    {
        //Platform Notifications
        GameServiceInitialize,
        Error,
        //Room Notifications
        RoomServiceStatus,
        RoomProperties,
        RoomListRetrieveStatus,
        RoomPlayerJoin,
        RoomPlayerLeave,
        RoomChangeHost,
    }

    public class EventWrapper 
    {
        public readonly NotificationType NotificationType;
        public EventWrapper(NotificationType notificationType)
        {
            NotificationType = notificationType;
        }
    }

    public class EventWrapper<T>: EventWrapper
    {
        public delegate void Handler(EventWrapper<T> EventData);
        public readonly T Data;
        public EventWrapper(NotificationType notificationType, T data):base( notificationType)
        {
            Data = data;
        }
    }

    public abstract class EventBase { } //Placeholder

    public class ErrorEvent : EventBase
    {
        public readonly PlatformErrors PlatformError;
        public readonly string ErrorMessage;
        public readonly string ErrorCode;

        public ErrorEvent(PlatformErrors errorType, string errorMsg, string errorCode = null)
        {
            PlatformError = errorType;
            ErrorMessage = errorMsg;
            ErrorCode = errorCode;
        }
    }

    public class RoomUpdateEvent : EventBase
    {
        /// <summary>
        /// [Deprecated]
        /// </summary>
        public readonly RoomUpdateType RoomUpdateType;
        public readonly RoomServiceStatus RoomServiceStatus;
        public readonly Room CurrentRoom;
        public RoomUpdateEvent(RoomUpdateType roomUpdateType, RoomServiceStatus status, Room currentRoom = null)
        {
            RoomUpdateType = roomUpdateType;
            CurrentRoom = currentRoom;
            RoomServiceStatus = status;
        }
    }

    public class RoomListEvent : EventBase
    {
        public readonly RoomListRetrieveStatus RoomListRetrieveStatus;
        public readonly RoomList RoomList;
        //Room list update
        public RoomListEvent(RoomListRetrieveStatus roomListRetrieveStatus, RoomList roomList = null)
        {
            RoomListRetrieveStatus = roomListRetrieveStatus;
            RoomList = roomList;
        }
    }

    public class RoomPlayerEvent : EventBase
    {
        public readonly RoomUserActionType RoomUserActionType;
        public readonly User Player;
        public RoomPlayerEvent(RoomUserActionType roomUserAction, User user)
        {
            RoomUserActionType = roomUserAction;
            Player = user;
        }
    }

    public class RoomInfo
    {
        public readonly NotificationType NotificationType;
        public readonly RoomServiceStatus RoomStatus;
        public readonly RoomListRetrieveStatus RoomListRetrieveStatus;
        public readonly Room CurrentRoom;
        public readonly RoomList RoomList;
        public readonly User UpdatePlayer;
        //Get Room Data method
        public RoomInfo(RoomServiceStatus status, Room currentRoom, RoomListRetrieveStatus roomListRetrieveStatus, RoomList roomList) 
        {
            CurrentRoom = currentRoom;
            RoomStatus = status;
            RoomListRetrieveStatus = roomListRetrieveStatus;
            RoomList = roomList;
        }
    }
}





