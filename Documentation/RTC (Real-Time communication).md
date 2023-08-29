# RTC (Real-Time communication)

The RTC service is very similar to the Room service. it gathers all players through the RTC room. In the same room, players can easily communicate with each other, mute, or silence any player. This chapter explains the integration of RTC services in MicroWar.

## Create And Join RTC Room
In MicroWar, we default to joining RTC rooms when a player joins a Named Room
 ```csharp
    public void CreateRoom(uint maxUser)
        {
              private void RoomUpdateEventHandler(EventWrapper<RoomUpdateEvent> msg)
        {
            var roomEvent = msg.Data;
            //Room status update
            if(msg.NotificationType == NotificationType.RoomServiceStatus)
            {
                if(roomEvent.RoomServiceStatus == RoomServiceStatus.InRoom)
                {
                    TryJoinRTCRoom(roomEvent.CurrentRoom.RoomId.ToString());
                }
                else
                {
                    LeaveRTCRoom(rtcRoomID);
                }
            }
        }
   ```
### Create Room
- **`PlatformController_RTC.cs`**<br>

  
 ### Join Room
- **`PlatformController_Rooms.cs`**<br>

   ```csharp
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
   ```
