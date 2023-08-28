# PICO Platform Service Infrastructure
Considering the maintainability and scalability of the program, we have encapsulated the PICO Platform APIs. This encapsulation ensures relatively independent logic for each component while providing a unified access approach for all feature's instance. By utilizing an event system, we have interconnected these components to reduce coupling between modules.

In this architecture, specific scripts implement the logic for different services. The PlatformServiceManager serves as an interface for accessing various instances. Here is the platform service structure diagram used in the MicroWar project:

![PICO Platform Service Structure](/Documentation/Files/PlatformServiceStructure.jpg)
### Event Dispatch
 **C# Code Example - Initializing the SDK**:
   
   ```csharp
   using PicoSDK;

   // Initialize the PICO SDK with your API key
   PicoSDK.Initialize("your_api_key");
   ```
## Initialization and Login

The PICO platform offers developers a simplified process for initialization and user login, facilitating easy integration into multiplayer online experiences. The basic steps for initialization and login are as follows:

1. **Obtain API Key**: Register and create an application within the PICO developer portal to acquire a unique API key.

2. **Initialize SDK**: Initialize the PICO SDK using the obtained API key. This ensures that your application can communicate with the PICO platform.

   **C# Code Example - Initializing the SDK**:
   
   ```csharp
   using PicoSDK;

   // Initialize the PICO SDK with your API key
   PicoSDK.Initialize("your_api_key");
   ```

3. **User Authentication**: Implement a user login and authentication mechanism within your application. This could involve username/password, social media logins, or other suitable methods.

   **C# Code Example - User Authentication and Login**:
   
   ```csharp
   using PicoSDK;

   // Authenticate user and log in
   User user = Authentication.Login(username, password);
   ```

4. **Connect to a Room**: Based on user choice or matching mechanisms, connect the user to a room. This could be an existing room or a newly created one.

   **C# Code Example - Connecting to a Room**:
   
   ```csharp
   using PicoSDK;

   // Join an existing room
   Room room = RoomManager.JoinRoom(roomId);
   ```

By following these steps, your application will be able to offer a multiplayer online experience on the PICO platform, enabling players to collaboratively engage in gaming and interactive activities.
## Event Dispatch

The message dispatch system within the PICO platform is pivotal to constructing real-time interactions. As players communicate, move, shoot projectiles, or perform other actions within a room, this information needs to be synchronized with other players. The message dispatch system is responsible for broadcasting these messages to all participants within a room.

Key functionalities of the message dispatch system:

- **Real-time Synchronization**: Ensures players' actions and states remain synchronized within the multiplayer environment, minimizing delays and discrepancies across different clients.

- **Partitioning and Prioritization**: Dispatches messages to specific rooms or players, while prioritizing the timely delivery of critical information.

- **Reliability and Fault Tolerance**: Ensures reliable message delivery, maintaining data integrity and accuracy even in the presence of unstable network conditions.



## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the official PICO SDK documentation:

- [PICO SDK Documentation](https://pico-sdk-docs.example.com)
