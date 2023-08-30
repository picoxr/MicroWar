# Multiplay
Multiplay is an important part of the game, to implement multiplay features, we introduced Unity's Netcode package in order to connect Netcode and PICO SDK's data exchange interface via a custom Transport. The PICO server is considered as a relay server to enable player play on the wide area network.
## Packet Distribution Process
PICO SDK provides basic packet forwarding APIs, enabling players within a game room to exchange data. In MicroWar, we utilize a customize transport to establish a connection between Netcode and PICO SDK. The picture below elucidates the travel of network data across different components.

![PacketProcess](https://github.com/picoxr/MicroWar/blob/0e9ef5d885c2913c3105061e906994929bfc2478/Documentation/Files/PacketProcess.jpg)
## Sending Packet

In the following example, PID represents the ***PICO User ID***, and UID represents the ***Unity Netcode Client ID***. We maintain this mapping relationship in the project, checking in the map to determine the PICO user ID of the target user we need to send to.
> [!NOTE]
> The Client ID of the host is always 0, and clients can only send data to the host. Meanwhile, the host can transmit data to any client, including itself. We store the Host ID for each multiplayer session to facilitate data transmission from clients.

- **`PlatformController_Network.cs`**<br>

```csharp
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
```
## Receiving Packet
After launching Netcode, the program continuously fetches packets from the queue. Upon retrieving a packet, it enters the processing logic ***Handle Incoming Packet***. Within this logic, we internally convert PICO packets into the format compatible with Netcode's packet structure.

- **`PlatformController_Network.cs`**<br>

```csharp
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
            if(packetSize <= 0)
            {
                Debug.LogError("Error Packet Size!");
            }
            else //Receive packet success!
            {
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
                    var payload = new ArraySegment<byte>(data, 0, data.Length);
                    OnReceivePICOPacket?.Invoke(NetworkEvent.Data, senderUID, payload);
                }
            }
        }
```
> [!NOTE]
> For more information about Unity Netcode, please refer to the official documentation at https://unity.com/products/netcode.


## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [MicroWar Platform Service Architecture](/Documentation/MicroWar%20Platform%20Service%20Architecture.md)
- [Initialization And Login](/Documentation/Initialization%20And%20Login.md)
- [Rooms](/Documentation/Rooms.md)
- [RTC](/Documentation/RTC%20(Real-Time%20Communication).md)
- [Multiplay](/Documentation/Multiplay.md)
