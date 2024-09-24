using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR;

namespace Pico.Avatar.Sample
{
    public class HeightAutoFitDemoStart : PicoAppLaunchBase
    {
        public PicoAvatarAutoFitPanel picoAvatarAutoFitPanel;
        private List<GameObject> cloneGrabList = new List<GameObject>();
        private ActionAvatar localAvatar;
        private ActionAvatarMirror localMirror;
        private AvatarIKSettingsSample defaultIKSettings = null;
        private bool openGrapFunction = false;
        
        #region Mount Point function
        public UnityEngine.XR.Interaction.Toolkit.XRRayInteractor RightRayInteractor;
        private bool rTriggerButtonIsOnClick = false;
        private bool rTriggerButtonIsOnPress = false;

        private bool aButtonIsOnClick = false;
        private bool aButtonIsOnPress = false;
        private GameObject GrabObject = null;
        #endregion


        private bool m_loginPlatformSDK = false;


        private void Awake()
        {
            System.Action<bool> loginCall = (state) =>
            {
                m_loginPlatformSDK = state;
            };
            this.SvrPlatformLogin(loginCall);
            if (picoAvatarAutoFitPanel != null)
            {
                picoAvatarAutoFitPanel.SetHeightAutoFitDemo(this);
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
                   
            if (picoAvatarAutoFitPanel != null)
            {
                picoAvatarAutoFitPanel.SetTargetEntity(localAvatar.Avatar.entity);
                picoAvatarAutoFitPanel.SetIkSettings(localAvatar.ikSettings);
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
            AButtonClickStateUpdate();
            // when a Button is onClick
            if (aButtonIsOnClick)
            {
                localAvatar.AlignAvatarArmSpan();
            }

            //等待Avatar加载完成
            if (!openGrapFunction)
                return;

            MountFunctionState();
        }

        void MountFunctionState()
        {
            rGripButtonClickStateUpdate();
            // when right triggerButton is onClick
            if (rTriggerButtonIsOnClick)
            {
                //ray check
                if (RightRayInteractor == null)
                    return;
                    
                //destory avatar mount point objects
                if (cloneGrabList.Count > 0)
                {
                    destroyAvatarMountPoint();
                    return;
                }
                // select cur RaycastHit obj as GrabObject
                GrabObject = null;
                RaycastHit hit;
                bool state = RightRayInteractor.TryGetCurrent3DRaycastHit(out hit);
                if (RightRayInteractor.TryGetCurrent3DRaycastHit(out hit))
                {
                    var targetObj = (hit.collider.gameObject);
                    if (targetObj.name.StartsWith("HandGrab"))
                    {
                        GrabObject = targetObj;
                        Debug.Log("test:select GrabObject:" + GrabObject.name);
                    }
                }
                Vector3 offset = new Vector3(0.082f, -0.051f, 0.061f);
#if UNITY_EDITOR
                GrabObject = GameObject.Find("StaticScene/HandGrabList/HandGrab (3)");
#endif
                addObjectToAvatar(GrabObject, offset);
            }
        }

        //destory point objects
        void destroyAvatarMountPoint()
        {
            foreach (var item in cloneGrabList)
            {
                GameObject.Destroy(item);
            }
            cloneGrabList.Clear();
            GrabObject.SetActive(true);
        }
        //add object to point
        void addObjectToAvatar(GameObject obj,Vector3 offset)
        {
            if (obj == null)
                return;
            var localPoint = localAvatar.Avatar.GetJointObject(localAvatar.criticalJoints[0]);
            var localMirrorPoint = localMirror.GetJointObject(localAvatar.criticalJoints[0]);
            if (localPoint == null || localMirrorPoint == null)
                return;
            var newObj = GameObject.Instantiate(obj, localPoint.transform);
            var newObjMirror = GameObject.Instantiate(obj, localMirrorPoint.transform);

                   
            newObj.transform.localPosition = offset;
            newObj.transform.localRotation = Quaternion.identity;
            newObjMirror.transform.localPosition = offset;
            newObjMirror.transform.localRotation = Quaternion.identity;
            cloneGrabList.Add(newObj);
            cloneGrabList.Add(newObjMirror);
            obj.SetActive(false);
        }
        void rGripButtonClickStateUpdate()
        {
            rTriggerButtonIsOnClick = false;
            bool rGripButton;
            InputDevices.GetDeviceAtXRNode(XRNode.RightHand).TryGetFeatureValue(CommonUsages.triggerButton, out rGripButton);
            if (rGripButton)
            {
                rTriggerButtonIsOnPress = true;
            }
            else
            {
                if (rTriggerButtonIsOnPress)
                    rTriggerButtonIsOnClick = true;
                rTriggerButtonIsOnPress = false;
            }
#if UNITY_EDITOR
            if (Input.GetKeyUp(KeyCode.Space))
            {
                Debug.Log("rTriggerButtonIsOnClick true");
                rTriggerButtonIsOnClick = true;
                rTriggerButtonIsOnPress = false;
            }
#endif
        }

        void AButtonClickStateUpdate()
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
        }

        public void ReLoadAvatar()
        {
            StartCoroutine("StartReLoadAvatar");
        }
        public IEnumerator StartReLoadAvatar()
        {
            openGrapFunction = false;

            localAvatar.resetXrRoot();
            localMirror.StartAvatar(localAvatar);
            localAvatar.StartAvatar(this.UserServiceUserID);
            //等待Avatar加载完成
            while (!localAvatar.Avatar.isAnyEntityReady)
                yield return null;

            //action set
            if (picoAvatarAutoFitPanel != null)
            {
                picoAvatarAutoFitPanel.SetTargetEntity(localAvatar.Avatar.entity);
                // Reset the localAvatar.ikSettings
                AvatarIKSettingsSample.Copy(defaultIKSettings, localAvatar.ikSettings);
                picoAvatarAutoFitPanel.SetIkSettings(localAvatar.ikSettings);
            }
            //use grap function
            openGrapFunction = true;
        }
    }
}


