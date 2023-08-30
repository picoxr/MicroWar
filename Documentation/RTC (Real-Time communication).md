# RTC (Real-Time Communication)

The RTC service is very similar to the Room service. it gathers all players through the RTC room. In the same room, players can easily communicate with each other using their voice, mute, or silence any player. This chapter explains the integration of RTC services in MicroWar.

## Create RTC Room

- **`PlatformController_RTC.cs`**<br>

In MicroWar, we default to joining RTC rooms when a player joins a Named Room. The following example demonstrates how to utilize the event system to automatically notify the RTC module to join a room.

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
## Join RTC Room
- **`PlatformController_RTC.cs`**<br>

  ```csharp
        private void JoinRTCRoom(string roomId, string currentUserId)
        {
           
            RtcService.GetToken(roomId, currentUserId, tokenTTL, privilege).OnComplete(result =>
            {
                if (result.IsError)
                {
                    Debug.LogWarning($"[PVRRTC]: PICO RTC Service get token failed. Result:  {result.Error.Code} | {result.Error.Message} ");
                    return;
                }
                rtcToken = result.Data;
                RtcRoomOptions rtcRoomOp = new RtcRoomOptions();
                rtcRoomOp.SetRoomId(roomId);
                rtcRoomOp.SetUserId(currentUserId);
                rtcRoomOp.SetToken(rtcToken);
                rtcRoomOp.SetRoomProfileType(RtcRoomProfileType.Game);
                rtcRoomOp.SetIsAutoSubscribeAudio(true);
                RtcService.JoinRoom2(rtcRoomOp, true);
            });
        }
  ```

## Leave RTC Room
- **`PlatformController_RTC.cs`**<br>

 ```csharp
    private void LeaveRTCRoom(string rtcRoomId)
        {
            ...
            string strRoomID = rtcRoomId;

            RtcService.StopAudioCapture();
            RtcService.UnPublishRoom(strRoomID);
            RtcService.LeaveRoom(strRoomID);
            rtcRoomID = null;
            rtcToken = null;
        }
  ```

## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [MicroWar Platform Service Architecture]([/Documentation/MicroWarPlatformServiceArchitecture.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/MicroWar%20Platform%20Service%20Architecture.md))
- [Initialization And Login]([/Documentation/InitializationAndLogin.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Initialization%20And%20Login.md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Initialization%20And%20Login.md)
- [Rooms]([/Documentation/Rooms.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Rooms.md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Rooms.md)
- [RTC]([/Documentation/RTC.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/RTC%20(Real-Time%20communication).md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/RTC%20(Real-Time%20communication).md)
- [Multiplay]([/Documentation/Multiplays.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Multiplay.md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Multiplay.md)

