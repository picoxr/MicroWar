using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Pico.Platform;
using UnityEngine;
using UnityEngine.Networking;

namespace Pico.Avatar.Sample
{
    public class PicoAppLaunchBase: MonoBehaviour
    {
        private System.Action<bool> m_platformLoginFinish = null;

        #region private Platform Developer Info
        /// <summary>
        ///Pico developer AppID
        /// </summary>
        private string m_platformAppID = "";
        /// <summary>
        /// get platform userID by platformSDK UserService
        /// </summary>
        private string m_userServiceUserID = "";
        /// <summary>
        /// get platform accessToken by platformSDK UserService
        /// </summary>
        private string m_UserServiceAccessToken = "";
        /// <summary>
        /// get platform nativeCode by platformSDK UserService
        /// </summary>
        private string m_userServiceStoreRegion = "";
        #endregion
        #region public  property
        public virtual string PlatformAppID
        {
            get
            {
                if (string.IsNullOrEmpty(m_platformAppID))
                {
                    return PicoAvatarAppConfigUtils.PicoPlatformAppID;
                }
                return m_platformAppID;
            }
        }
        public virtual string UserServiceUserID
        {
            get => m_userServiceUserID;
            set => m_userServiceUserID = value;
        }
        public virtual string UserServiceAccessToken
        {
            get => m_UserServiceAccessToken;
            set => m_UserServiceAccessToken = value;
        }
        public virtual string UserServiceStoreRegion
        {
            get => m_userServiceStoreRegion;
            set => m_userServiceStoreRegion = value;
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
            if (string.IsNullOrEmpty(PlatformAppID))
            {
                LogError("PlatformAppID is null:");
                onPlatformResultCall(false);
                return;
            }
            try
            {
                Log($"Platform GameInitialize Start");
#if UNITY_EDITOR || UNITY_STANDALONE
                StartCoroutine(Sample_SvrAccessToken()); // sample token
#else
                Pico.Platform.CoreService.Initialize(PlatformAppID);
                platform_SvrPermissions();
#endif
            }
            catch (System.Exception ex)
            {
                LogError($"The Platform SDK Must run on the android platform！error :{ex.Message}");

                onPlatformResultCall(false);
            }
        }
     
        void platform_SvrPermissions()
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
                platform_SvrUserID();
            });
        }
        protected virtual void platform_SvrUserID()
        {
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
                this.m_userServiceStoreRegion = ApplicationService.GetSystemInfo().IsCnDevice?"oidc-pico-cn":"oidc-pico-global";
                if (string.IsNullOrEmpty(this.m_userServiceStoreRegion))
                    this.m_userServiceStoreRegion = "oidc-pico-cn";
                Log($"@@@Got StoreRegion {m_userServiceStoreRegion}");
                //platform_SvrAccessToken(); // official user token
                //*****************end************************************//
                StartCoroutine(Sample_SvrAccessToken()); // sample token
            });
            
        }
        
        
            protected IEnumerator Sample_SvrAccessToken()
            {
                Log($"@@@Sample_SvrAccessToken");
                yield return null; //wait picoavtarapp init completed
                OnPicoAvatarAppStartTestModel();
                UnityWebRequest webRequest = UnityWebRequest.Get(NetEnvHelper.GetFullRequestUrl(NetEnvHelper.SampleTokenApi, m_userServiceStoreRegion));
                webRequest.timeout = 30;
                webRequest.SetRequestHeader("Content-Type", "application/json"); 
                yield return webRequest.SendWebRequest();
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.Log("Get Token Failed! Reason:" + webRequest.error);
                    onPlatformResultCall(false);
                    yield break;
                }
                //*****************Special Data For AvatarSDK Sample************************************//
                m_UserServiceAccessToken = JsonConvert.DeserializeObject<JObject>(webRequest.downloadHandler.text)?.Value<string>("data");
                m_userServiceUserID = PicoAvatarPlatformInfoTestModelUtils.UserID;
                //*****************End************************************//
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

                onPlatformResultCall(true);
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

