# Initialization And Login

Before utilizing any platform services, it's essential to **initialize the platform services** by making SDK initialization calls. Also, you can utilize the SDK's GetLoggedInUser interface to retrieve data concerning the current local player. 
In the MicroWar project, we made the initiation and login operations within the **PlatformServiceManager.cs** script. In order to provide a universal access point for platform service initialization and the retrieval of login-related data.
Platform initialization consists of two main components. Find the detailed description below.

## Platform Service Initialization

This part involves fundamental SDK initialization, encompassing services such as ***account***, ***friend relationship***, ***achievements***, and ***leaderboards***.See the sample below:
- **`PlatformServiceManager.cs`**<br>

   ```csharp
   using UnityEngine;
   using Pico.Platform;
   using Pico.Platform.Models;
    public void InitPlatformServices()
        {
            UpdateGameInitializeStatus(ServiceInitializeStatus.Initializing);
            CoreService.AsyncInitialize(APPID).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    DebugUtils.LogError(nameof(PlatformServiceManager), $"[Core Service] Aysnc Initialize Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    UpdateGameInitializeStatus(ServiceInitializeStatus.Failed); //Core Initialize Failed
                    return;
                }

                if (msg.Data != PlatformInitializeResult.Success && msg.Data != PlatformInitializeResult.AlreadyInitialized)
                {
                    DebugUtils.LogError(nameof(PlatformServiceManager), $"[Core Service] Aysnc Initialize Failed! Initialize Result: {msg.Data.ToString()}");
                    UpdateGameInitializeStatus(ServiceInitializeStatus.Failed); //Core Initialize Failed
                    return;
                }
                DebugUtils.Log(nameof(PlatformServiceManager), $"[Core Service] Aysnc Initialize Success!");
                //Try get logged in user data.
                GetLoginUserData();
                InitializeGameService();
            });
   ```
## Get Logged-in User Data
In the context of multiplayer gaming, a common requirement is to access data related to the currently logged-in player ***After the platform initialization is completed***, we can call an interface to retrieve all the relevant user data. Please refer to the example provided below.
   ```csharp
   private void GetLoginUserData()
           {
               UserService.GetLoggedInUser().OnComplete(msg =>
               {
                   if (msg.IsError)
                   {
                       DebugUtils.LogError(nameof(PlatformServiceManager), $"[User] Get Login Data Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                       return;
                   }
                   DebugUtils.Log(nameof(PlatformServiceManager), $"[User] Login Success! User: {msg.Data.DisplayName} ID: {msg.Data.ID}");
                   //Login success
                   me = msg.Data;
               });
           }
   ```

## Game Service Initialization
Game service initialization pertains to room management, matchmaking, and multiplayer gameplay. If the project involves these features, it's essential to initialize the game services before calling any of the API. 
   ```csharp
       private void InitializeGameService()
        {
            //Init game service after core service is initialized.
            CoreService.GameUninitialize();
            CoreService.GameInitialize().OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    DebugUtils.LogError(nameof(PlatformServiceManager), $"[Game Service] Aysnc Initialize Failed! Error Code: {msg.Error.Code} Message: {msg.Error.Message}");
                    UpdateGameInitializeStatus(ServiceInitializeStatus.Failed);
                    return;
                }
                //Initialize success
                if (msg.Data == GameInitializeResult.Success)
                {
                    DebugUtils.Log(nameof(PlatformServiceManager), $"[Game Service] Aysnc Initialize Success!");
                    //Bind connection notifications
                    NetworkService.SetNotification_Game_ConnectionEventCallback(HandleGameConnectionEvent);
                    //Try initialize other modules
                    UpdateGameInitializeStatus(ServiceInitializeStatus.initialized); //Core Initialize Failed
                    DelayInit();
                }
                else
                {
                    //Initialize failed
                    DebugUtils.LogError(nameof(PlatformServiceManager), $"[Game Service] Aysnc Initialize Failed! Error{msg.Data}");
                    UpdateGameInitializeStatus(ServiceInitializeStatus.Failed);
                }
            });
        }
   ```