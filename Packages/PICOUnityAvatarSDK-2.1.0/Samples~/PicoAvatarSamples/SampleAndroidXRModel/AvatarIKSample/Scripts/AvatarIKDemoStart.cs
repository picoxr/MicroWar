using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace Pico.Avatar.Sample
{
    public class AvatarIKDemoStart : PicoAppLaunchBase
    {
        public PicoAvatarIKPanel picoAvatarIKPanel;
        private ActionAvatar localAvatar;
        private ActionAvatarMirror localMirror;
        private AvatarIKSettingsSample defaultIKSettings = null;
        
        #region Button Actions
        public UnityEngine.XR.Interaction.Toolkit.XRRayInteractor RightRayInteractor;
        private bool aButtonIsOnClick = false;
        private bool aButtonIsOnPress = false;
        private bool xButtonIsOnClick = false;
        private bool xButtonIsOnPress = false;


        #endregion


        private bool m_loginPlatformSDK = false;


        private void Awake()
        {
            System.Action<bool> loginCall = (state) =>
            {
                m_loginPlatformSDK = state;
            };
            this.SvrPlatformLogin(loginCall);
            if (picoAvatarIKPanel != null)
            {
                picoAvatarIKPanel.SetAvatarIKDemo(this);
            }
        }
        IEnumerator Start()
        {
            this.PXRCheck();
            
            while (!m_loginPlatformSDK)
                yield return null;

            //等待PicoAvatarApp准备完成
            while (!PicoAvatarApp.isWorking)
                yield return null;

            this.PicoAvatarAppStart();

           

            //等待Manager启动完成
            while (!PicoAvatarManager.canLoadAvatar)
                yield return null;

            //开始Avatar加载
            var item = GameObject.Find("LocalActionAvatar");
            if (item)
            {
                localAvatar = item.GetComponent<ActionAvatar>();
                localAvatar.StartAvatar(UserServiceUserID);
            }
            //设置镜像
            var itemMirror = GameObject.Find("LocalActionAvatarMirror");
            if (itemMirror)
            {
                localMirror = itemMirror.GetComponent<ActionAvatarMirror>();
                localMirror.StartAvatar(localAvatar);
            }

            //等待Avatar加载完成
            while (!localAvatar.Avatar.isAnyEntityReady)
                yield return null;
                   
            if (picoAvatarIKPanel != null)
            {
                picoAvatarIKPanel.SetTargetEntity(localAvatar.Avatar.entity);
                picoAvatarIKPanel.SetIkSettings(localAvatar.ikSettings);
            }
            //use grap function
            //openGrapFunction = true;

            // Record the initial value of ikSettings.
            GameObject defaultIKSettingsGo = new GameObject("DefaultAvatarIKSettings");
            defaultIKSettingsGo.transform.SetParent(localAvatar.transform);
            defaultIKSettings = defaultIKSettingsGo.AddComponent<AvatarIKSettingsSample>();
            defaultIKSettingsGo.SetActive(false);
            AvatarIKSettingsSample.Copy(localAvatar.ikSettings, defaultIKSettings);
        }
        
        //Fetching functio
        private void Update()
        {
            UpdateButtonActions();
        }

        void UpdateButtonActions()
        {
            rGripButtonClickStateUpdate();
            // when right triggerButton is onClick
            if (aButtonIsOnClick)
            {
                //TODO: align arm span, scale rig
                localAvatar.AlignAvatarArmSpan();
            }
            if (xButtonIsOnClick)
            {
                localAvatar.ResetIKTargets();
            }
        }

        void rGripButtonClickStateUpdate()
        {
            aButtonIsOnClick = false;
            bool aButton;
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.primaryButton, out aButton);
            if (aButton)
            {
                aButtonIsOnPress = true;
            }
            else
            {
                if (aButtonIsOnPress)
                    aButtonIsOnClick = true;
                aButtonIsOnPress = false;
            }
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.A))
            {
                Debug.Log("AButtonIsOnClick true");
                aButtonIsOnClick = true;
                aButtonIsOnPress = false;
            }
#endif
            xButtonIsOnClick = false;
            bool xButton;
            InputDevices.GetDeviceAtXRNode(XRNode.LeftHand).TryGetFeatureValue(CommonUsages.primaryButton, out xButton);
            if (xButton)
            {
                xButtonIsOnPress = true;
            }
            else
            {
                if (xButtonIsOnPress)
                    xButtonIsOnClick = true;
                xButtonIsOnPress = false;
            }
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.X))
            {
                Debug.Log("XButtonIsOnClick true");
                xButtonIsOnClick = true;
                xButtonIsOnPress = false;
            }
#endif
        }
        public void ReLoadAvatar()
        {
            StartCoroutine("StartReLoadAvatar");
        }
        public IEnumerator StartReLoadAvatar()
        {
            localAvatar.resetXrRoot();
            localMirror.StartAvatar(localAvatar);
            localAvatar.StartAvatar(this.UserServiceUserID);
            //等待Avatar加载完成
            while (!localAvatar.Avatar.isAnyEntityReady)
                yield return null;

            //action set
            if (picoAvatarIKPanel != null)
            {
                picoAvatarIKPanel.SetTargetEntity(localAvatar.Avatar.entity);
                picoAvatarIKPanel.ResetAvatarHeightValue();
                // Reset the localAvatar.ikSettings
                AvatarIKSettingsSample.Copy(defaultIKSettings, localAvatar.ikSettings);
                picoAvatarIKPanel.SetIkSettings(localAvatar.ikSettings);
            }
        }
    }
}


