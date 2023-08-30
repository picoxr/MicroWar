# Multiplay
Multiplay is an important part of the game, to implement multiplay features, we introduced Unity's Netcode package in order to connect Netcode and PICO SDK's data exchange interface via a custom Transport. The PICO server is considered as a relay server to enable player play on the wide area network.
## Packet Distribution Process
PICO SDK provides basic packet forwarding APIs, enabling players within a game room to exchange data. In MicroWar, we utilize a customize transport to establish a connection between Netcode and PICO SDK. The picture below elucidates the travel of network data across different components.

![PacketProcess](https://github.com/picoxr/MicroWar/blob/0e9ef5d885c2913c3105061e906994929bfc2478/Documentation/Files/PacketProcess.jpg)
## Sending Packet
## Receiving Packet
## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [MicroWar Platform Service Architecture]([/Documentation/MicroWarPlatformServiceArchitecture.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/MicroWar%20Platform%20Service%20Architecture.md))
- [Initialization And Login]([/Documentation/InitializationAndLogin.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Initialization%20And%20Login.md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Initialization%20And%20Login.md)
- [Rooms]([/Documentation/Rooms.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Rooms.md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Rooms.md)
- [RTC]([/Documentation/RTC.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/RTC%20(Real-Time%20communication).md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/RTC%20(Real-Time%20communication).md)
- [Multiplay]([/Documentation/Multiplays.md](https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Multiplay.md)https://github.com/picoxr/MicroWar/blob/17e79e7bb7d1f3383b1dfeb6457363885e4b4d31/Documentation/Multiplay.md)
