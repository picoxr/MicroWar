using System;
using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Unity.XR.PXR;
using UnityEngine;
using UnityEngine.Networking;

#if !NO_XR
using Pico.Platform;
#endif



namespace Pico.Avatar.Sample
{
    public class PicoAppLaunchBase: MonoBehaviour
    {
        private const  string ChinaOfficialAppId = "a3e35b37bc4d8394e2912928caded3d8";
        private const  string OverSeaOfficialAppId = "58d14f0d8aae280bd41f993fd808bbe1";
        private System.Action<bool> m_platformLoginFinish = null;

        #region private Platform Developer Info
        /// <summary>
        ///Pico developer AppID
        /// </summary>
        private string m_platformAppID = string.Empty;
        /// <summary>
        /// get platform userID by platformSDK UserService
        /// </summary>
        private string m_userServiceUserID = string.Empty;
        /// <summary>
        /// get platform accessToken by platformSDK UserService
        /// </summary>
        private string m_UserServiceAccessToken = string.Empty;
        #endregion
        #region public  property
        public virtual string PlatformAppID
        {
            get
            {
                return m_platformAppID;
            }
        }
        public virtual string UserServiceUserID
        {
            get
            {
                return m_userServiceUserID;
            }
            set => m_userServiceUserID = value;
        }

        public virtual string UserServiceAccessToken
        {
            get => m_UserServiceAccessToken;
            set => m_UserServiceAccessToken = value;
        }
        #endregion

        #region PicoAvatarApp Start
        protected virtual void OnPicoAvatarAppStartTestModel()
        {
            Log("Platform Failed, RunApp by TestModel!");
        }

        public virtual void PicoAvatarAppStart()
        {
            var avatarApp = PicoAvatarApp.instance;
            // Set PicoAvatarApp Launch parameters
            
            avatarApp.loginSettings.accessToken = this.UserServiceAccessToken;
       
            avatarApp.StartAvatarManager();
        }
        #endregion
     
        #region PlatformSDK Request
        public void SvrPlatformLogin(System.Action<bool> infoCallFinish)
        {
            m_platformLoginFinish = infoCallFinish;
            try
            {
                Log($"Platform GameInitialize Start");

#if UNITY_EDITOR_WIN
                try
                {
                    Pico.Platform.CoreService.Initialize();
                    platform_SvrAccessToken();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    StartCoroutine(Sample_SvrAccessToken()); 
                }
#elif UNITY_EDITOR_OSX
                StartCoroutine(Sample_SvrAccessToken()); // sample token
#else
                Pico.Platform.CoreService.Initialize();
               if(ApplicationService.GetSystemInfo().IsCnDevice)
               {
                   PXR_PlatformSetting.Instance.appID = ChinaOfficialAppId;
               }
               else
               {
                   PXR_PlatformSetting.Instance.appID = OverSeaOfficialAppId;
               }
               // reinit correct appid
               CoreService.GameUninitialize();
               CoreService.Initialized = false;
               Pico.Platform.CoreService.Initialize(); 
               platform_SvrPermissions();
#endif
            }
            catch (System.Exception ex)
            {
                LogError($"The Platform SDK Must run on the android platform！error :{ex.Message}");

                onPlatformResultCall(false);
            }
        }
     
        public virtual void platform_SvrPermissions()
        {
            Log($"Platform RequestUserPermissions Start");
            //, "avatar" 
            Pico.Platform.UserService.RequestUserPermissions(new string[2] { "user_info", "avatar" }).OnComplete(msg =>
            {
                if (msg.IsError)
                {
                    LogError($"Platform RequestUserPermissions failed:code={msg.Error.Code},message={msg.Error.Message}");
                    onPlatformResultCall(false);
                    return;
                }

                Log($"Platform Request permission successfully:{(msg.Data)}");
                Log($"Platform RequestUserPermissions Finish");
#if false
                //If you want to use your own avatar to display in the sample, please open this branch
                platform_SvrAccessToken(); // official user token
#else
                //The official token is obtained here to ensure that everyone has the same experience(same avatar)
                StartCoroutine(Sample_SvrAccessToken()); 
#endif
            });
        }
        protected virtual void platform_SvrUserID()
        {
            if (!CoreService.Initialized)
            {
                onPlatformResultCall(true);
                return;
            }
            
            Log($"Platform GetLoggedInUser Start");
            Pico.Platform.UserService.GetLoggedInUser().OnComplete((user) =>
            {
                if (user == null || user.IsError)
                {
                    LogError($"Got user isError");
                    onPlatformResultCall(false);
                    return;
                }
               
                Log($"Got userID {user.Data.ID},name = {user.Data.DisplayName}");
                //*****************Official Data For User************************************//
                this.m_userServiceUserID = user.Data.ID;
                onPlatformResultCall(true);
            });
            
        }
        
        
            protected IEnumerator Sample_SvrAccessToken()
            {
                yield return null; //wait picoavtarapp init completed
                OnPicoAvatarAppStartTestModel();
                UnityWebRequest webRequest = UnityWebRequest.Get(NetEnvHelper.GetFullRequestUrl(NetEnvHelper.SampleTokenApi));
                webRequest.timeout = 30;
                webRequest.SetRequestHeader("Content-Type", "application/json"); 
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Get Token Failed! Reason:" + webRequest.error);
                    onPlatformResultCall(false);
                    yield break;
                }
                /*****************Special Data For AvatarSDK Sample************************************/
                
                //get official accesstoken for sample, everyone is same
                m_UserServiceAccessToken = JsonConvert.DeserializeObject<JObject>(webRequest.downloadHandler.text)?.Value<string>("data");
                
                /*****************End************************************/
                onPlatformResultCall(true);
            }

        
        protected void platform_SvrAccessToken()
        {
            Log($"Platform GetAccessToken");
            Pico.Platform.UserService.GetAccessToken().OnComplete(delegate (Pico.Platform.Message<string> message)
            {
                if (message.IsError)
                {
                    var err = message.GetError();
                    LogError($"Got access token error {err.Message} code={err.Code}");
                    onPlatformResultCall(false);
                    return;
                }

                string credential = message.Data;
                Log($"Got credential {credential}");

                this.m_UserServiceAccessToken = (credential);
                if (string.IsNullOrEmpty(m_UserServiceAccessToken))
                {
                    StartCoroutine(Sample_SvrAccessToken()); 
                }
                else
                {
                    platform_SvrUserID();
                }
            });
        }
        protected void onPlatformResultCall(bool finish)
        {
            if(m_platformLoginFinish != null)
                m_platformLoginFinish.Invoke(finish);
        }
        #endregion

        #region XR Check
        protected void PXRCheck()
        {
            Unity.XR.CoreUtils.XROrigin XRgo = null;
            if (XRgo == null)
            {
                GameObject Origin = GameObject.Find("XR Origin");
                if (Origin == null)
                {
                    LogError("XR Origin not find");
                    return;
                }
                XRgo = Origin.GetComponent<Unity.XR.CoreUtils.XROrigin>();
            }
            if (XRgo == null)
            {
                LogError("XR Origin not find");
                return;
            }

            // PXR 
            var pxrMgr = XRgo.GetComponent<Unity.XR.PXR.PXR_Manager>();
            if (pxrMgr == null)
                XRgo.gameObject.AddComponent<Unity.XR.PXR.PXR_Manager>();
        }
        #endregion

        void Log(string content)
        {
            Debug.Log($"PicoAvatarExample Msg :{ content}");
        }
        void LogError(string content)
        {
            Debug.LogError($"PicoAvatarExample Msg Error:{ content}");
        }
    }

}

