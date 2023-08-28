# Initialization And Login

Before utilizing any platform services, it's essential to **initialize the platform services** by making SDK initialization calls. Also, you can utilize the SDK's GetLoggedInUser interface to retrieve data concerning the current local player. In the MicroWar project, we made the initiation and login operations within the **PlatformServiceManager.cs** script. In order to provide a universal access point for platform service initialization and the retrieval of login-related data.
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
