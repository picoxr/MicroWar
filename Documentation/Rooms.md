# Rooms

The rooms feature is designed to unite numerous online players. In the PICO platform services, room functionality stands as the infrastructure for multiplayer gaming. The matchmaking service and multiplayer data transmission are both contingent on the room feature. The subsequent section will elaborate on the integration of room functionality in the MicroWar project.

## Create And Join Room
In the MicroWar project, we encapsulate the room functionality within a single script. Through the foundational structure of platform services, this enables the transmission of room data changes to the external environment and facilitates easy access to control script instances. To achieve room listing functionality, we have opted for the PICO named room type. The following code snippet demonstrates how to create a NamedRoom within MicroWar.
### Create Room

- **`PlatformController_Rooms.cs`**<br>

   ```csharp
    public void CreateRoom(uint maxUser)
        {
            ...
            string userID = platformServiceManager.Me.ID;
            string roomName = userID + DateTime.Now.ToString();
            RoomOptions op = new RoomOptions();
            op.GetHashCode();
            op.SetRoomName(roomName);
            RoomService.JoinOrCreateNamedRoom(RoomJoinPolicy.Everyone, true, maxUser, op).OnComplete(msg => {
                if (msg.IsError)
                {
                    ...
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
            ...
            RoomOptions op = new RoomOptions();
            RoomService.Join2(roomID, op).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    ...
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Join Room Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                }
                //Join Room Success
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Join Room Success! Room ID: {msg.Data.RoomId}");
                UpdateRoomServiceStatus(RoomServiceStatus.InRoom, msg.Data); //Success in room
            });
        }
   ```
### Retrieve Room List

- **`PlatformController_Rooms.cs`**<br>

   ```csharp
  public void RetrieveRoomList()
        {
            ...
            RoomService.GetNamedRooms(0, 20).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    ...
                    platformServiceManager.HandlePlatformErrors(PlatformErrors.RetrieveRoomListFailed, msg.Error.Code.ToString());//Notify error handler
                    DebugUtils.LogError(nameof(PlatformController_Rooms), $"Retrieve Room List Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    return;
                }
                DebugUtils.Log(nameof(PlatformController_Rooms), $"Retrieve Room List Success!");
                var roomList = msg.Data;
                UpdateRoomListRetrieveStatus(RoomListRetrieveStatus.Idle, roomList);
            });
   ```
## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [MicroWar Platform Service Architecture](/Documentation/MicroWar%20Platform%20Service%20Architecture.md)
- [Initialization And Login](/Documentation/Initialization%20And%20Login.md)
- [Rooms](/Documentation/Rooms.md)
- [RTC](/Documentation/RTC%20(Real-Time%20Communication).md)
- [Multiplay](/Documentation/Multiplay.md)


