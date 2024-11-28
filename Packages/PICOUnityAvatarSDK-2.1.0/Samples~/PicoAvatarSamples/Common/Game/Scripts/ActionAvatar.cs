using System;
using Pico.Platform;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pico.Avatar.Sample
{
    public class ActionAvatar : MonoBehaviour
    {
        public string userId = "662230622642634752";

        public AvatarManifestationType manifestationType = AvatarManifestationType.Full;

        //
        public bool isMainAvatar = false;
        [Tooltip("whether allow avatar meta loaded from local cache when network is weak.")]
        public bool allowAvatarMetaFromCache = false;

        public AvatarHeadShowType headShowType = AvatarHeadShowType.Normal;
        public bool bodyCulling = false;
        public bool useFT_Lipsync = false;
        private bool cameraTracking = false;
        public RecordBodyAnimLevel recordBodyAnimLevel = RecordBodyAnimLevel.Invalid;

        //创建gameObject 关键节点，用于位置更新。
        public JointType[] criticalJoints;

        public bool enablePlaceHolder = true;//change if needed.

        public AvatarIKSettingsSample ikSettings = null;

        public DeviceInputReaderBuilderInputType deviceInputReaderType = DeviceInputReaderBuilderInputType.Invalid;
        //device actions
        public InputActionProperty[] buttonActions;

        private PicoAvatar avatar;
        private AvatarBodyAnimController _bodyAnimController;
        private Vector3 _cameraOffsetPosition = new Vector3(0.0f, 0.0f, 0.0f);
        private bool _isHeightAutoFitInitialized = false;

        private Vector3 _defaultXrScale = Vector3.one;
        private Vector3 _defaultXrPosition= Vector3.zero;

        //hand pose Skeleton
        private GameObject leftSkeletonGo;
        private GameObject rightSkeletonGo;
        private GameObject leftHandPose;
        private GameObject rightHandPose;
        //load special avatarID,normal is 0
        private string avatarID = string.Empty;
        private string characterType = "0";
        private System.Action<bool> loadedCall;

        //
        public float positionRange = 0.0f;

        private float maxControllerDistance = 1.0f;

        private string jsonAvatar;
        public PicoAvatar Avatar { get { return avatar; } }
        public System.Action<ActionAvatar> loadedFinishCall;

        // whether enable expression for the avatar
        private bool enableExpression = true;

        public void SetEnableExpression(bool enableExpress)
        {
            this.enableExpression = enableExpress;
        }

        /**
         * Invoke manually to set as main avatar.
         */
        public void SetAsMainAvatar()
        {
            this.isMainAvatar = true;
        }

        public void StartAvatar(string userID, InputActionProperty[] btnActions = null, string avatarID = "", string characterType = "0")
        {
            if (this.avatar != null)
            {
                Dispose();
            }

            this.jsonAvatar = "";
            this.userId = userID;
            this.avatarID = avatarID;
            this.characterType = characterType;
            buttonActions = btnActions;

            CreateAvatar();
        }
        public void StartJsonAvatar(string userID, string jsonData, InputActionProperty[] btnActions = null)
        {
            this.jsonAvatar = jsonData;
            this.userId = userID;
            buttonActions = btnActions;

            CreateAvatar();
        }
        public void SetCustomHandPose(GameObject leftHandSkeleton, GameObject rightHandSkeleton, GameObject leftHandPose, GameObject rightHandPose)
        {
            this.leftSkeletonGo = leftHandSkeleton;
            this.rightSkeletonGo = rightHandSkeleton;
            this.leftHandPose = leftHandPose;
            this.rightHandPose = rightHandPose;
        }
        public void SetEntityLoadedCall(System.Action<bool> call)
        {
            loadedCall = call;
        }
        private void InitAvatarCustomData()
        {
            if (!isMainAvatar || (leftSkeletonGo == null && rightSkeletonGo == null) || avatar == null)
                return;
            // Right
            Vector3 right_up = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 right_forward = new Vector3(-1.0f, 0.0f, 0.0f);
            Vector3 right_offset = Vector3.zero;// new Vector3(0.15f,-0.05f,0.05f);

            // Left
            Vector3 left_up = new Vector3(0.0f, 0.0f, -1.0f);
            Vector3 left_forward = new Vector3(1.0f, 0.0f, 0.0f);
            Vector3 left_offset = Vector3.zero;// new Vector3(-0.059f, 0, 00.052f);
            bool state = avatar.entity.SetCustomHand(HandSide.Right, rightSkeletonGo, rightHandPose, right_up, right_forward, right_offset);
            state = avatar.entity.SetCustomHand(HandSide.Left, leftSkeletonGo, leftHandPose, left_up, left_forward, left_offset);
            if (state)
            {
                Debug.Log("pav:InitAvatarCustomData successful !");
                //_bodyAnimController.SetRotationLimitEnable((uint)JointType.LeftHandWrist, false);
                //_bodyAnimController.SetRotationLimitEnable((uint)JointType.RightHandWrist, false);
            }
        }

        public void ReloadAvatar()
        {
            Debug.Log("ReloadAvatar");
            if (avatar != null)
            {
                PicoAvatarManager.instance.UnloadAvatar(avatar);
                avatar = null;
            }

            CreateAvatar();
        }


        void CreateAvatar()
        {
            if (ikSettings == null)
            {
                Debug.LogWarning("[ActionAvatar] If using IK, please add the 'AvatarIKSettingsSample' prefab instance for 'IK Settings' in 'ActionAvatar' component.");
            }

            var capability = new AvatarCapabilities();
            capability.manifestationType = manifestationType;
            capability.controlSourceType = isMainAvatar ? ControlSourceType.MainPlayer : ControlSourceType.OtherPlayer;
            if (deviceInputReaderType == DeviceInputReaderBuilderInputType.Invalid)
            {
                capability.inputSourceType = isMainAvatar ? DeviceInputReaderBuilderInputType.PicoXR :
                    DeviceInputReaderBuilderInputType.RemotePackage;
            }
            else
            {
                capability.inputSourceType = deviceInputReaderType;
            }

#if OpenDebugPanel
            if (isMainAvatar)
            {
                var avatarDebugToolGo = GameObject.Find("AvatarSDKDebugToolPanel");
                if (avatarDebugToolGo != null)
                {
                    // var avatarSDKDebugToolPanel = avatarDebugToolGo.GetComponent<AvatarSDKDebugToolPanel>();
                    // if (avatarSDKDebugToolPanel != null && 
                    //     avatarSDKDebugToolPanel.Config.GetLocalPropValueString(QAConfig.NameType.bodyTrackingMode).ToLower() == "true")
                    // {
                    //     capability.inputSourceType = DeviceInputReaderBuilderInputType.BodyTracking;
                    // }
                }
            }
#endif
            
            
            capability.bodyCulling = bodyCulling;
            capability.recordBodyAnimLevel = recordBodyAnimLevel;
            capability.enablePlaceHolder = enablePlaceHolder; //set Enable PlaceHolder.
            capability.autoStopIK = ikSettings ? ikSettings.autoStopIK : true; //set automatically stop ik when controller is far,idle,etc.
            capability.ikMode = ikSettings ? ikSettings.ikMode : AvatarIKMode.None;
            capability.headShowType = headShowType;
            capability.enableExpression = enableExpression;
            if (manifestationType == AvatarManifestationType.HeadHands || manifestationType == AvatarManifestationType.Hands)
            {// mode must set handAssetId
#if UNITY_EDITOR
                if (Utility.IsCnDevice())
#elif UNITY_ANDROID
                if (ApplicationService.GetSystemInfo().IsCnDevice)
#endif
                {
                    capability.handAssetId = "1550582586916995072";
                }
                else
                {
                    capability.handAssetId = "1550582590019702784";
                }
            }


            if (allowAvatarMetaFromCache)
            {
                capability.flags |= (uint)AvatarCapabilities.Flags.AllowAvatarMetaFromCache;
            }
            //capability.flags |= (uint)AvatarCapabilities.Flags.AllowEdit;

            Action<PicoAvatar, AvatarEntity> callback = (avatar, avatarentity) =>
            {
                if (avatarentity == null)
                {
                    if (loadedCall != null)
                        loadedCall(false);
                    return;
                }

                if (!isMainAvatar)
                {
                    avatar.PlayAnimation("idle", 0, "BottomLayer");
                }
                else
                {
                    _bodyAnimController = avatarentity?.bodyAnimController;
                    InitBodyAnimControllerIK();
                    InitAvatarCustomData();
                    InitAutoFitController();
                    if (useFT_Lipsync)
                    {
#if !UNITY_EDITOR && UNITY_ANDROID
                         _bodyAnimController.StartFaceTrack(true, true);
#endif
                    }
                }

                if (loadedCall != null)
                    loadedCall(true);
                if (loadedFinishCall != null)
                    loadedFinishCall(this);
            };
            Debug.Log("@@@@@@@@@@@@@@@userId" + userId);
            if (!string.IsNullOrEmpty(this.jsonAvatar))
            {
                avatar = PicoAvatarManager.instance.LoadAvatar(new AvatarLoadContext(userId, this.avatarID, this.jsonAvatar, capability), callback);
            }
            else
                avatar = PicoAvatarManager.instance.LoadAvatar(new AvatarLoadContext(userId, this.avatarID, null, capability), callback, characterType);

            if (avatar == null)
                return;
            avatar.criticalJoints = this.criticalJoints;

            var avatarEntity = avatar.entity;

            Transform avatarTransform = avatar?.transform;

            avatarTransform.SetParent(transform);
            avatarTransform.localPosition = Vector3.zero;
            avatarTransform.localRotation = Quaternion.identity;
            avatarTransform.localScale = Vector3.one;

            avatarEntity.buttonActions = buttonActions;

            InitEntityXRTarget(avatarEntity);

            if (cameraTracking && isMainAvatar && PicoAvatarManager.instance.avatarCamera != null)
            {
                PicoAvatarManager.instance.avatarCamera.trakingAvatar = avatar;
            }

            Transform xrRoot = ikSettings != null? ikSettings.XRRoot: null;
            if(xrRoot != null)
            {
                 _defaultXrScale = xrRoot.localScale;
                 _defaultXrPosition = xrRoot.localPosition;
            }
           
            //
            // currently around origin.
            if (positionRange > 0.0f)
            {
                var dir = this.transform.localPosition;
                var dist = dir.magnitude;
                this.transform.localPosition = dir.normalized * (positionRange + dist);
            }
        }
        public void Dispose()
        {
            if (this._bodyAnimController != null)
            {
                var autoFitController = _bodyAnimController.autoFitController;
                autoFitController?.ClearAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);
            }
            PicoAvatarManager.instance.UnloadAvatar(this.avatar);
            this.avatar = null;
        }

        public PicoAvatar GetAvatar()
        {
            return avatar;
        }

        public void AlignAvatarArmSpan()
        {
            if (avatar.entity != null)
            {
                avatar.entity.AlignAvatarArmSpan();
            }
        }

        public void resetXrRoot()
        {
             Transform xrRoot = ikSettings != null? ikSettings.XRRoot: null;
             if(xrRoot != null)
             {
                xrRoot.localScale = _defaultXrScale;
                xrRoot.localPosition = _defaultXrPosition;
                if(_bodyAnimController!= null &&  _bodyAnimController.bipedIKController != null)
                {
                    _bodyAnimController.bipedIKController.controllerScale = xrRoot.localScale.x;
                }
             }
        }

        public void ResetIKTargets()
        {
            if (_bodyAnimController != null)
            {
                var bipedIKController = _bodyAnimController.bipedIKController;
                if(bipedIKController != null)
                {
                    bipedIKController.ResetEffector(IKEffectorType.Head);
                    bipedIKController.ResetEffector(IKEffectorType.LeftHand);
                    bipedIKController.ResetEffector(IKEffectorType.RightHand);
                    bipedIKController.ResetEffector(IKEffectorType.LeftFoot);
                    bipedIKController.ResetEffector(IKEffectorType.RightFoot);
                }
            }
        }

        void InitEntityXRTarget(AvatarEntity avatarEntity)
        {
            ikSettings?.updateIKTargetsConfig(avatarEntity?.avatarIKTargetsConfig);
        }

        void InitBodyAnimControllerIK()
        {
            if (_bodyAnimController != null && _bodyAnimController.bipedIKController != null)
            {
                AvatarEntity avatarEntity = _bodyAnimController.owner;

                _bodyAnimController.bipedIKController.SetValidHipsHeightRange(0.0f, 3.0f);

                //set IK auto stop & invalid region
                _bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerDisconnect, true);
                _bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerIdle, true);
#if UNITY_EDITOR
                _bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerIdle, false);
#endif
                _bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerLoseTracking, true);
                _bodyAnimController.bipedIKController.SetIKAutoStopModeEnable(IKAutoStopMode.ControllerFarAway, true);

                //test creating animation clip layer on top
                //AvatarAnimationLayer animationLayer = _bodyAnimController.CreateAnimationLayerByName("actionLayer");
                //animationLayer.setLayerBlendMode(AnimLayerBlendMode.Override);
                //animationLayer.setSRTEnable(false, true, true);
                //animationLayer.playAnimationClip("walkingLeft", 0, 1, 0);
                //animationLayer.playAnimationClip("holdController", 0, 1, 0);
            }
        }

        #region autofit height
        void InitAutoFitController()
        {
            if (_bodyAnimController == null || ikSettings == null)
                return;

            if (!ikSettings.heightAutoFit.enableAutoFitHeight || ikSettings.heightAutoFit.cameraOffsetTarget == null)
                return;

            //sdk event trigger
            var autoFitController = _bodyAnimController.autoFitController;
            if (autoFitController == null)
                return;

            autoFitController.localAvatarHeightFittingEnable = true;
            autoFitController.ClearAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);
            autoFitController.AddAvatarOffsetChangedCallback(OnAvatarOffsetChangedCallBack);

            //app event trigger 
            var trigger = this.avatar.entity.gameObject.GetComponent<PicoAvatarAutoFitTrigger>();
            if (trigger == null)
                trigger = this.avatar.entity.gameObject.AddComponent<PicoAvatarAutoFitTrigger>();
            trigger.SetTriggerCallback(OnAppAutoFitTrigger);

            //when create avatar finished ,force trigger offset 
            Vector3 initPos = ikSettings.heightAutoFit.cameraOffsetTarget.transform.position;
            autoFitController.SetCurrentAvatarOffset(initPos);
            autoFitController.UpdateAvatarHeightOffset();
            Debug.Log("pav:autoFitController.UpdateAvatarHeightOffset");

            _isHeightAutoFitInitialized = true;
        }

        //sdk trigger changeOffset callBack
        void OnAvatarOffsetChangedCallBack(AvatarAutoFitController cotroller, Vector3 cameraOffsetPos)
        {
            if (ikSettings == null || !ikSettings.heightAutoFit.enableAutoFitHeight)
                return;

            //Debug.Log("pav:OnAvatarOffsetChanged:" + cameraOffsetPos.ToString());
            //更新相机offset
            RefreshCameraOffsetTargetPos(cameraOffsetPos);
            cotroller.SetCurrentAvatarOffset(cameraOffsetPos);
        }
        //app trigger callBack
        void OnAppAutoFitTrigger()
        {
            Debug.Log("pav:OnAppAutoFitTrigger:");
            var autoFitController = _bodyAnimController.autoFitController;
            if (autoFitController == null || ikSettings == null || ikSettings.heightAutoFit.cameraOffsetTarget == null)
                return;

            Vector3 initPos = ikSettings.heightAutoFit.cameraOffsetTarget.transform.position;
            autoFitController.SetCurrentAvatarOffset(initPos);
            autoFitController.UpdateAvatarHeightOffset();
        }
        //refresh camera offset
        void RefreshCameraOffsetTargetPos(Vector3 offset)
        {
            if (ikSettings == null || ikSettings.heightAutoFit.cameraOffsetTarget == null)
                return;

            ikSettings.heightAutoFit.cameraOffsetTarget.transform.position = offset;
            _cameraOffsetPosition = offset;
            //Debug.Log("pav:cameraOffsetTarget pos:" + cameraOffsetTarget.transform.localPosition.ToString());
        }

        #endregion

        private void Update()
        {
            var avatarEntity = avatar?.entity;

            if (avatarEntity == null || ikSettings == null) return;

            if (ikSettings.isDirty)
            {
                if (!_isHeightAutoFitInitialized && ikSettings.heightAutoFit.enableAutoFitHeight)
                {
                    InitAutoFitController();
                }

                ikSettings.UpdateAvatarIKSettings(avatarEntity);
            }

            var autoFitController = _bodyAnimController?.autoFitController;
            if (autoFitController != null &&
                ikSettings.heightAutoFit.cameraOffsetTarget != null &&
                ikSettings.heightAutoFit.enableAutoFitHeight == true)
            {
                Vector3 pos = ikSettings.heightAutoFit.cameraOffsetTarget.transform.position;
                if (Vector3.SqrMagnitude(pos - _cameraOffsetPosition) > 1e-6)
                {
                    autoFitController.SetCurrentAvatarOffset(pos);
                    _cameraOffsetPosition = pos;
                }

            }
        }

    }
}
