# PICO Platform API Encapsulation
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
### Get Service Instance
## Initialization And Login

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


## More to Explore

For detailed implementation specifics and usage guidelines, please refer to the following documentation:
- [Rooms](/Documentation/Rooms.md)
- [RTC](/Documentation/RTC.md)
- [Multiplay](/Documentation/Multiplays.md)
