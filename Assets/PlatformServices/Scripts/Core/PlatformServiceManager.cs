using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pico.Platform;
using Pico.Platform.Models;

namespace MicroWar.Platform
{
    public enum AppRegion
    {
        CN,
        Global
    }

    public enum LogLevel
    {
        Development,
        Normal,
        Error,
        Nothing
    }

    public class PlatformServiceManager : MonoSingleton<PlatformServiceManager>
    {

        [Header("Debug Option")]
        [SerializeField] private bool InitOption;
        [SerializeField] internal bool EnableNetwork;
        [SerializeField]public LogLevel LogLevel;
        private string APPID = string.Empty;//"9f7e83c0dacdd38eb9f7167258610888"; // Demo version

        private User me;

        private Dictionary<Type, List<Delegate>> eventMap = new Dictionary<Type, List<Delegate>>();
        private Dictionary<Type, ControllerBase> controllerMap = new Dictionary<Type, ControllerBase>();

        private Room currentRoom;

        public Action<ServiceInitializeStatus> OnPlatformInitializeStatusChange;
        public ServiceInitializeStatus GameServiceInitializeStatus { get; private set; } = ServiceInitializeStatus.Unknown;

        #region Actions
        #endregion
        public User Me
        {
            get { return me; }
        }

        #region Unity Message
        override protected void Awake()
        {
            base.Awake();

            //Test Code block
            if (!InitOption)
            {
                return;
            }
            //Test Code block

        }

        private void Start()
        {
            APPID = Pico.Platform.CoreService.GetAppID();
        }
        #endregion

        #region Initialize
        // Update is called once per frame
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
        }
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
        #endregion

        private void UpdateGameInitializeStatus(ServiceInitializeStatus status)
        {
            GameServiceInitializeStatus = status;
            Debug.Log($"Update status: {status}");
            OnPlatformInitializeStatusChange?.Invoke(status);
        }

        public void HandlePlatformErrors(PlatformErrors error, string errorCode)
        {
            string errorMessage = string.Empty;
            switch (error)
            {
                case PlatformErrors.PassivelyLeaveRoom:
                    errorMessage = $"Server Error. Code: {errorCode}";
                    break;
                case PlatformErrors.CreateRoomFailed:
                    errorMessage = "";
                    break;
                case PlatformErrors.RetrieveRoomListFailed:
                    errorMessage = "";
                    break;
                case PlatformErrors.JoinRoomFailed:
                    errorMessage = "";
                    break;
                case PlatformErrors.ConnectToHostFailed:
                    errorMessage = "";
                    break;
                case PlatformErrors.LeaveRoomFailed:
                    errorMessage = "";
                    break;
                case PlatformErrors.ServerError:
                    errorMessage = "Server Error";
                    break;
            }
            NotifyErrorHandler(error, errorMessage, errorCode);
        }

        private void NotifyErrorHandler(PlatformErrors error, string errorMsg, string errorCode = null)
        {
            ErrorEvent newEvent = new ErrorEvent(error,errorMsg,errorCode);
            EventWrapper<ErrorEvent> errorEventWrapper = new EventWrapper<ErrorEvent>(NotificationType.Error, newEvent);
            NotifyEventHandler(errorEventWrapper); //[Temp] Notify global error panel.
        }

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

        /// <summary>
        /// Register a callback to receive platform event
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handler"></param>
        public void RegisterNotification<T>(EventWrapper<T>.Handler handler)
        {
            if(handler == null)
            {
                DebugUtils.LogError(nameof(PlatformServiceManager),"Can't Register A Handler That Is Null !");
                return;
            }

            if ( ! eventMap.ContainsKey( typeof(T) ))
            {
                eventMap.Add(typeof(T), new List<Delegate>());
            }

            if ( ! eventMap[typeof(T)].Contains(handler))
            {
                eventMap[typeof(T)].Add(handler);
            }
        }

        public void UnregisterNotification<T>(EventWrapper<T>.Handler handler)
        {
            if(eventMap.ContainsKey(typeof(T)))
            {
                try
                {
                    eventMap[typeof(T)].Remove(handler);
                }
                catch(Exception e)
                {
                    throw e;
                }
            }
        }

        public void NotifyEventHandler<T>(EventWrapper<T> data)
        {
            List<Delegate> handlerList = null;
            if( eventMap.ContainsKey(typeof(T)))
            {
                handlerList = eventMap[typeof(T)];
            }
        
            if(handlerList!=null)
            {
                for (int i = 0; i < handlerList.Count; i++)
                {
                    if(handlerList[i]!= null && handlerList[i].Target != null)
                    {
                        handlerList[i].DynamicInvoke(data);
                    }
                }
            }
        }

        public void AddController<T>(T controller) where T : ControllerBase
        {
            if (controller == null)
                return;

            if( !controllerMap.ContainsKey(typeof(T)))
            {
                controllerMap.Add(typeof(T), controller);
            }
            else
            {
                DebugUtils.LogError(nameof(PlatformServiceManager),"Already registered a same controller !");
            }
        }

        /// <summary>
        /// Get a registerd controller instance. Call this function after [ Awake() ];
        /// </summary>
        /// <typeparam name="T">Controller type</typeparam>
        /// <returns></returns>
        public T GetController<T>() where T : ControllerBase
        {
            if (controllerMap.ContainsKey(typeof(T)))
            {
                ControllerBase controller;
                if(controllerMap.TryGetValue(typeof(T),out controller))
                {
                    return controller as T;
                }
            }
            return null;
        }


        void DelayInit()
        {
            foreach (KeyValuePair<Type,ControllerBase> kvp in controllerMap) //Init all the controllers
            {
                var controller = kvp.Value;
                controller.DelayInit();
            }

        }

        private void HandleGameConnectionEvent(Message<GameConnectionEvent> Message)
        {
            if (Message.IsError)
            {
                Debug.LogError("Error");
            }
            var state = Message.Data;
            switch (state)
            {
                case GameConnectionEvent.Connected:
                case GameConnectionEvent.Resumed:
                    UpdateGameInitializeStatus(ServiceInitializeStatus.initialized);
                    break;
                case GameConnectionEvent.Closed:
                case GameConnectionEvent.Lost:
                case GameConnectionEvent.KickedByRelogin:
                case GameConnectionEvent.KickedByGameServer:
                case GameConnectionEvent.GameLogicError:
                case GameConnectionEvent.Unknown:
                    //UpdateGameInitializeStatus(GameServiceInitializeStatus.NotAvailable);
                    UpdateGameInitializeStatus(ServiceInitializeStatus.Failed);
                    HandlePlatformErrors(PlatformErrors.ServerError, "");
                    Debug.LogError($"[PlatformServiceManager]Server error: {state}");
                    break;
            }
        }
    }

}

