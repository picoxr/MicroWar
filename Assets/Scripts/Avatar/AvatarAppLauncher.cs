using MicroWar.Platform;
using Pico.Avatar;
using Pico.Platform;
using Pico.Platform.Models;
using System;
using System.Collections;
using UnityEngine;

namespace MicroWar.Avatar
{
    public class AvatarAppLauncher : MonoBehaviour
    {

        private string platformAppID;
        private string userServiceAccessToken = "act.pico.avatar.demo"; //Needed for running in the Editor
        private string userServiceStoreRegion;
        private string userID;
        private string userNationType;

        public virtual string PlatformAppID { get => platformAppID; set => platformAppID = value; }
        public virtual string UserServiceAccessToken { get => userServiceAccessToken; set => userServiceAccessToken = value; }
        public virtual string UserServiceStoreRegion { get => userServiceStoreRegion; set => userServiceStoreRegion = value; }
        public virtual string UserID { get => userID; set => userID = value; }

        private bool isInitializing = false;
        private bool isLaunchFailed = false;


        private void Awake()
        {
            platformAppID = Pico.Platform.CoreService.GetAppID();
        }

        public IEnumerator LaunchPicoAvatarApp() 
        {
            if (!Pico.Platform.CoreService.IsInitialized())
            {
                Debug.LogError("Pico.Platform.CoreService is not initialized. Initialize CoreService first.");
                yield break;
            }

            Initialize();
            while (isInitializing) yield return null;
            if (isLaunchFailed) yield break;

            SetUserID();
            SetStoreRegion();
            SetUserNationType();

            PicoAvatarApp avatarApp = PicoAvatarApp.instance;
            avatarApp.loginSettings.accessToken = UserServiceAccessToken;

#if UNITY_EDITOR
            avatarApp.appSettings.localMode = true; //Work with local SDK 
#else
            avatarApp.appSettings.localMode = false; //Download up-to-date SDK 
#endif

            avatarApp.StartAvatarManager();
        }

        private void Initialize()
        {
            isInitializing = true;

#if UNITY_EDITOR
            InitSuccess();
            return;
#endif
            Pico.Platform.UserService.RequestUserPermissions(new string[2] { "user_info", "avatar" }).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    Debug.LogError($"Unable to get user permissions \"user_info\", \"avatar\" - error: {msg.Error.Code}:{msg.Error.Message}");
                    InitFail();
                    return;
                }

                GetAccessToken();
            });

        }

        private void GetAccessToken()
        {
            
            Pico.Platform.UserService.GetAccessToken().OnComplete(delegate (Pico.Platform.Message<string> message)
            {
                if (message.IsError)
                {
                    Error err = message.Error;
                    Debug.LogError($"GetAccessToken Error - message:{err.Message} - code:{err.Code}");
                    InitFail();
                    return;
                }

                userServiceAccessToken = (message.Data);
                InitSuccess();
            });
        }

        private void SetUserID()
        {
            userID = PlatformServiceManager.Instance.Me.ID;
        }
        
        private void SetStoreRegion()
        {
            if ((Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor))
            {
                userServiceStoreRegion = "ooidc-pico-cn";
                return;
            }

            userServiceStoreRegion = ApplicationService.GetSystemInfo().IsCnDevice ? "oidc-pico-cn" : "oidc-pico-global";

            if (string.IsNullOrEmpty(userServiceStoreRegion))
            { 
                userServiceStoreRegion = "ooidc-pico-cn"; 
            }  
        }

        private void SetUserNationType()
        {
            userNationType = UserServiceStoreRegion.Equals("oidc-pico-global") ? "sg" : "cn";

#if UNITY_EDITOR
            userNationType = "cn-test";
#endif
        }

        private void InitFail()
        {
            isInitializing = false;
            isLaunchFailed = true;
        }

        private void InitSuccess()
        {
            isInitializing = false;
        }


    }
}

