# Rooms

The rooms feature is designed to unite numerous online players. In the PICO platform services, room functionality stands as the infrastructure for multiplayer gaming. The matchmaking service and multiplayer data transmission are both contingent on the room feature. The subsequent section will elaborate on the integration of room functionality in the MicroWar project.

## Create And Join Room
In the MicroWar project, we encapsulate the room functionality within a single script. Through the foundational structure of platform services, this enables the transmission of room data changes to the external environment and facilitates easy access to control script instances. To achieve room listing functionality, we have opted for the PICO named room type. The following code snippet demonstrates how to create a NamedRoom within MicroWar.
### Create Room
- **`PlatformController_Rooms.cs`**<br>

   ```csharp
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
            op.SetRoomName(roomName);
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
   ```
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


