using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems;
using Unity.XR.CoreUtils;

namespace Pico.Avatar.Sample
{

    public class PicoAvatarIKPanel : MonoBehaviour
    {
          public enum MenusType
          {
            SetSpineBendEnable,
            SpineRotationWeight,
            HeadPositionOffsetX,
            HeadRotationOffsetX,
            HeadPositionOffsetY,
            HeadRotationOffsetY,
            HeadPositionOffsetZ,
            HeadRotationOffsetZ,
            HeadRotationLimit,
            ArmStretchEnable,
            LeftHandPositionOffsetX,
            LeftHandRotationOffsetX,
            LeftHandPositionOffsetY,
            LeftHandRotationOffsetY,
            LeftHandPositionOffsetZ,
            LeftHandRotationOffsetZ,
            RightHandPositionOffsetX,
            RightHandRotationOffsetX,
            RightHandPositionOffsetY,
            RightHandRotationOffsetY,
            RightHandPositionOffsetZ,
            RightHandRotationOffsetZ,
            ShoulderRotationLimit,
            UpperArmRotationLimit,
            LowerArmRotationLimit,
            WristRotationLimit,
            StepHeight,
            StepSpeed,
            PositionThreshold,
            AvatarHeight,
            ReloadAvatar,
            WorldSpaceDrive,
            
            // sitting mode
            AutoStandUpEnable,
            AutoStandUpDistance,
            AutoStandUpAngle,
            AutoStandUpTime,
        }

        private const float kDefaultAvatarHeightValue = 1.7f;
        private float _avatarHeightValue = kDefaultAvatarHeightValue;

        [Serializable]
        public class KeyTextPair {
            public MenusType key;
            public Text val;
        }

        [Serializable]
        public class KeySliderPair {
            public MenusType key;
            public Slider val;
        }

        [Serializable]
        public class KeyTogglePair {
            public MenusType key;
            public Toggle val;
        }
        
        public List<KeyTextPair> textList = new List<KeyTextPair>();
        public List<KeySliderPair> sliderList = new List<KeySliderPair>();
        public List<KeyTogglePair> toggleList = new List<KeyTogglePair>();

        public Dictionary<MenusType,Text> sliderTexts = new Dictionary<MenusType,Text>();
        public Dictionary<MenusType,Slider> sliders = new Dictionary<MenusType,Slider>();
        public Dictionary<MenusType,Toggle> toggles =  new Dictionary<MenusType,Toggle>();

        public Text deviceInfo;
        public Text loadingState;
        
        private AvatarIKDemoStart _fitAvatarDemo;
        private InputFeatureUsage<Vector3> _devicePositionFeature;
        private InputFeatureUsage<Quaternion> _deviceRotationFeature;
        private AvatarEntity _controllerEntity;

        private AvatarIKSettingsSample _avatarIKSettings;

        //device panel show
        private string _showDeviceMsg = "Height of VR Headset:{0}\n\n" +
                                       "Height of Avatar's Eye:{1}\n";


        void Awake() {
            foreach (var kvp in textList) {
                sliderTexts[kvp.key] = kvp.val;
            }
            foreach (var kvp in sliderList) {
                sliders[kvp.key] = kvp.val;
            }
            foreach (var kvp in toggleList) {
                toggles[kvp.key] = kvp.val;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            loadingState.text = "Avatar Loading ...";
            _devicePositionFeature = UnityEngine.XR.CommonUsages.devicePosition;
            _deviceRotationFeature = UnityEngine.XR.CommonUsages.deviceRotation;

            foreach(var kvp in sliders)
            {
                kvp.Value.onValueChanged.AddListener((value) =>
                {
                    OnSliderValueChanged(kvp.Key, value);
                });
            }
            foreach(var kvp in toggles)
            {
                kvp.Value.onValueChanged.AddListener((value) =>
                {
                    OnToggleValueChanged(kvp.Key, value);
                });
            }
        }

        private void UpdateValuesFromIKSettings()
        {
            if(_avatarIKSettings == null)
            {
                return;
            }
            toggles[MenusType.SetSpineBendEnable].SetIsOnWithoutNotify(_avatarIKSettings.head.enableSpineBend);
            
            sliders[MenusType.SpineRotationWeight].SetValueWithoutNotify(_avatarIKSettings.head.spineRotationWeight);
            sliders[MenusType.HeadPositionOffsetX].SetValueWithoutNotify(_avatarIKSettings.head.target.positionOffset.x);
            sliders[MenusType.HeadPositionOffsetY].SetValueWithoutNotify(_avatarIKSettings.head.target.positionOffset.y);
            sliders[MenusType.HeadPositionOffsetZ].SetValueWithoutNotify(_avatarIKSettings.head.target.positionOffset.z);
            sliders[MenusType.HeadRotationOffsetX].SetValueWithoutNotify(_avatarIKSettings.head.target.rotationOffset.x);
            sliders[MenusType.HeadRotationOffsetY].SetValueWithoutNotify(_avatarIKSettings.head.target.rotationOffset.y);
            sliders[MenusType.HeadRotationOffsetZ].SetValueWithoutNotify(_avatarIKSettings.head.target.rotationOffset.z);
            toggles[MenusType.HeadRotationLimit].SetIsOnWithoutNotify(_avatarIKSettings.head.rotationLimits.head);
            toggles[MenusType.WorldSpaceDrive].SetIsOnWithoutNotify(_avatarIKSettings.worldSpaceDrive);


            toggles[MenusType.ArmStretchEnable].SetIsOnWithoutNotify(_avatarIKSettings.hands.stretch);
            sliders[MenusType.LeftHandPositionOffsetX].SetValueWithoutNotify(_avatarIKSettings.hands.leftTarget.positionOffset.x);
            sliders[MenusType.LeftHandPositionOffsetY].SetValueWithoutNotify(_avatarIKSettings.hands.leftTarget.positionOffset.y);
            sliders[MenusType.LeftHandPositionOffsetZ].SetValueWithoutNotify(_avatarIKSettings.hands.leftTarget.positionOffset.z);
            sliders[MenusType.LeftHandRotationOffsetX].SetValueWithoutNotify(_avatarIKSettings.hands.leftTarget.rotationOffset.x);
            sliders[MenusType.LeftHandRotationOffsetY].SetValueWithoutNotify(_avatarIKSettings.hands.leftTarget.rotationOffset.y);
            sliders[MenusType.LeftHandRotationOffsetZ].SetValueWithoutNotify(_avatarIKSettings.hands.leftTarget.rotationOffset.z);
            sliders[MenusType.RightHandPositionOffsetX].SetValueWithoutNotify(_avatarIKSettings.hands.rightTarget.positionOffset.x);
            sliders[MenusType.RightHandPositionOffsetY].SetValueWithoutNotify(_avatarIKSettings.hands.rightTarget.positionOffset.y);
            sliders[MenusType.RightHandPositionOffsetZ].SetValueWithoutNotify(_avatarIKSettings.hands.rightTarget.positionOffset.z);
            sliders[MenusType.RightHandRotationOffsetX].SetValueWithoutNotify(_avatarIKSettings.hands.rightTarget.rotationOffset.x);
            sliders[MenusType.RightHandRotationOffsetY].SetValueWithoutNotify(_avatarIKSettings.hands.rightTarget.rotationOffset.y);
            sliders[MenusType.RightHandRotationOffsetZ].SetValueWithoutNotify(_avatarIKSettings.hands.rightTarget.rotationOffset.z);
            toggles[MenusType.ShoulderRotationLimit].SetIsOnWithoutNotify(_avatarIKSettings.hands.rotationLimits.shoulder);
            toggles[MenusType.UpperArmRotationLimit].SetIsOnWithoutNotify(_avatarIKSettings.hands.rotationLimits.upperArm);
            toggles[MenusType.LowerArmRotationLimit].SetIsOnWithoutNotify(_avatarIKSettings.hands.rotationLimits.lowerArm);
            toggles[MenusType.WristRotationLimit].SetIsOnWithoutNotify(_avatarIKSettings.hands.rotationLimits.handWrist);

            sliders[MenusType.StepHeight].SetValueWithoutNotify(_avatarIKSettings.feet.footstep.stepHeight);
            sliders[MenusType.StepSpeed].SetValueWithoutNotify(_avatarIKSettings.feet.footstep.stepSpeed);
            sliders[MenusType.PositionThreshold].SetValueWithoutNotify(_avatarIKSettings.feet.footstep.positionThreshold);

            toggles[MenusType.AutoStandUpEnable].SetIsOnWithoutNotify(_avatarIKSettings.heightAutoFit.sittingMode.autoStandUp);
            sliders[MenusType.AutoStandUpDistance].SetValueWithoutNotify(_avatarIKSettings.heightAutoFit.sittingMode.autoStandUpDistance);
            sliders[MenusType.AutoStandUpAngle].SetValueWithoutNotify(_avatarIKSettings.heightAutoFit.sittingMode.autoStandUpAngle);
            sliders[MenusType.AutoStandUpTime].SetValueWithoutNotify(_avatarIKSettings.heightAutoFit.sittingMode.autoStandUpTime);

            RefreshAllMenuTexts();
        }

        private void OnSliderValueChanged(MenusType funcType ,float value)
        {
            if(_avatarIKSettings == null)
            {
                return;
            }
            switch (funcType)
            {
                case MenusType.SpineRotationWeight:
                    _avatarIKSettings.head.spineRotationWeight = value;
                    break;
                case MenusType.HeadPositionOffsetX:
                    _avatarIKSettings.head.target.positionOffset.x = value;
                    break;
                case MenusType.HeadPositionOffsetY:
                    _avatarIKSettings.head.target.positionOffset.y = value;
                    break;
                case MenusType.HeadPositionOffsetZ:
                    _avatarIKSettings.head.target.positionOffset.z = value;
                    break;
                case MenusType.HeadRotationOffsetX:
                    _avatarIKSettings.head.target.rotationOffset.x = value;
                    break;
                case MenusType.HeadRotationOffsetY:
                    _avatarIKSettings.head.target.rotationOffset.y = value;
                    break;
                case MenusType.HeadRotationOffsetZ:
                    _avatarIKSettings.head.target.rotationOffset.z = value;
                    break;
                case MenusType.LeftHandPositionOffsetX:
                    _avatarIKSettings.hands.leftTarget.positionOffset.x = value;
                    break;
                case MenusType.LeftHandPositionOffsetY:
                    _avatarIKSettings.hands.leftTarget.positionOffset.y = value;
                    break;
                case MenusType.LeftHandPositionOffsetZ:
                    _avatarIKSettings.hands.leftTarget.positionOffset.z = value;
                    break;
                case MenusType.RightHandPositionOffsetX:
                    _avatarIKSettings.hands.rightTarget.positionOffset.x = value;
                    break;
                case MenusType.RightHandPositionOffsetY:
                    _avatarIKSettings.hands.rightTarget.positionOffset.y = value;
                    break;
                case MenusType.RightHandPositionOffsetZ:
                    _avatarIKSettings.hands.rightTarget.positionOffset.z = value;
                    break;
                case MenusType.LeftHandRotationOffsetX:
                    _avatarIKSettings.hands.leftTarget.rotationOffset.x = value;
                    break;
                case MenusType.LeftHandRotationOffsetY:
                    _avatarIKSettings.hands.leftTarget.rotationOffset.y = value;
                    break;
                case MenusType.LeftHandRotationOffsetZ:
                    _avatarIKSettings.hands.leftTarget.rotationOffset.z = value;
                    break;
                case MenusType.RightHandRotationOffsetX:
                    _avatarIKSettings.hands.rightTarget.rotationOffset.x = value;
                    break;
                case MenusType.RightHandRotationOffsetY:
                    _avatarIKSettings.hands.rightTarget.rotationOffset.y = value;
                    break;
                case MenusType.RightHandRotationOffsetZ:
                    _avatarIKSettings.hands.rightTarget.rotationOffset.z = value;
                    break;
                case MenusType.StepHeight:
                    _avatarIKSettings.feet.footstep.stepHeight = value;
                    break;
                case MenusType.StepSpeed:
                    _avatarIKSettings.feet.footstep.stepSpeed = value;
                    break;
                case MenusType.PositionThreshold:
                    _avatarIKSettings.feet.footstep.positionThreshold = value;
                    break;
                case MenusType.AvatarHeight:
                    _avatarHeightValue = value;
                    break;
                case MenusType.AutoStandUpDistance:
                    _avatarIKSettings.heightAutoFit.sittingMode.autoStandUpDistance = value;
                    break;
                case MenusType.AutoStandUpAngle:
                    _avatarIKSettings.heightAutoFit.sittingMode.autoStandUpAngle = value;
                    break;
                case MenusType.AutoStandUpTime:
                    _avatarIKSettings.heightAutoFit.sittingMode.autoStandUpTime = value;
                    break;
                default:
                    break;
            }
            RefreshMenuText(funcType,value);
            _avatarIKSettings.isDirty = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void OnToggleValueChanged(MenusType funcType ,bool value)
        {
            if(_avatarIKSettings == null)
            {
                return;
            }
            switch(funcType)
            {
                case MenusType.WorldSpaceDrive:
                    _avatarIKSettings.worldSpaceDrive = value;
                    break;
                case MenusType.SetSpineBendEnable:
                    _avatarIKSettings.head.enableSpineBend = value;
                    break;
                case MenusType.ArmStretchEnable:
                    _avatarIKSettings.hands.stretch = value;
                    break;
                case MenusType.HeadRotationLimit:
                    _avatarIKSettings.head.rotationLimits.head = value;
                    break;
                case MenusType.ShoulderRotationLimit:
                    _avatarIKSettings.hands.rotationLimits.shoulder = value;
                    break;
                case MenusType.UpperArmRotationLimit:
                    _avatarIKSettings.hands.rotationLimits.upperArm = value;
                    break;
                case MenusType.LowerArmRotationLimit:
                    _avatarIKSettings.hands.rotationLimits.lowerArm = value;
                    break;
                case MenusType.WristRotationLimit:
                    _avatarIKSettings.hands.rotationLimits.handWrist = value;
                    break;
                case MenusType.AutoStandUpEnable:
                    _avatarIKSettings.heightAutoFit.sittingMode.autoStandUp = value;
                    break;
                default:
                    break;
            }
            _avatarIKSettings.isDirty = true;
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void StandUpImmediately()
        {
            var bodyAnimController = _controllerEntity?.bodyAnimController;
            
            if (bodyAnimController == null) return;
            
            if (bodyAnimController.isSitting)
            {
                bodyAnimController.StandUp();
            }
        }

        private void SitDownAtTarget(Transform sitTarget, float sitHeight)
        {
            if (sitTarget == null) return;

            var bodyAnimController = _controllerEntity?.bodyAnimController;
            if (bodyAnimController == null) return;

            var xrOrigin = _avatarIKSettings?.XRRoot?.gameObject.GetComponent<XROrigin>();
            if (xrOrigin == null)
            {
                Debug.LogError("Please ensure the reference of 'XR Root' in 'Avatar IK Settings Sample' component is 'XR Origin' game object!");
                return;
            }

            var cameraOffsetTarget = _avatarIKSettings?.heightAutoFit.cameraOffsetTarget;
            if (cameraOffsetTarget == null)
            {
                Debug.LogError("Please ensure the reference of 'Camera Offset Target' in 'Avatar IK Settings Sample' component is 'Camera Offset' game object!");
                return;
            }

            // Teleport to sit position and match sit rotation
            var heightAdjustment = xrOrigin.Origin.transform.up * xrOrigin.CameraInOriginSpaceHeight;
            var cameraDestination = sitTarget.position + heightAdjustment;
            xrOrigin.MoveCameraToWorldLocation(cameraDestination);
            xrOrigin.MatchOriginUpCameraForward(xrOrigin.Origin.transform.up, sitTarget.forward);

            // Move avatar
            bodyAnimController.owner.owner.transform.position = xrOrigin.Origin.transform.position;
            bodyAnimController.owner.owner.transform.rotation = xrOrigin.Origin.transform.rotation;
            bodyAnimController.autoFitController?.SetCurrentAvatarOffset(cameraOffsetTarget.position);

            bodyAnimController.SitDown(sitTarget, sitHeight);
        }

        public void SetTargetEntity(AvatarEntity controller)
        {
            this._controllerEntity = controller;
            loadingState.text = "";
        }

        public void SetIkSettings(AvatarIKSettingsSample avatarIKSettings)
        {
            _avatarIKSettings = avatarIKSettings;
            _avatarIKSettings?.SetOnValidateCallback(UpdateValuesFromIKSettings);
            UpdateValuesFromIKSettings();
        }

        public void SetAvatarIKDemo(AvatarIKDemoStart avatarIKDemo)
        {
            _fitAvatarDemo = avatarIKDemo;
        }
        
        public void ResetAvatarHeightValue()
        {
            _avatarHeightValue = kDefaultAvatarHeightValue;
            sliders[MenusType.AvatarHeight].SetValueWithoutNotify(_avatarHeightValue);
        }

        public void OnSetAvatarHeight()
        {
            if (_controllerEntity == null 
                || _controllerEntity.bodyAnimController == null 
                || _controllerEntity.bodyAnimController.autoFitController == null)
                return;
            _controllerEntity.bodyAnimController.SetAvatarHeight(_avatarHeightValue);
            EventSystem.current.SetSelectedGameObject(null);
        }

        public void OnSitStatusChanged(int sitStatus)
        {
            if (_avatarIKSettings == null) return;

            if (!_avatarIKSettings.heightAutoFit.enableAutoFitHeight)
            {
                Debug.LogError("Please check 'Enable Auto Fit Height' checkbox in 'Avatar IK Settings Sample' component!");
                return;
            }

            if (_avatarIKSettings.heightAutoFit.sittingMode.sittingTarget == null)
            {
                Debug.LogError("Please add a sitting target for 'Sitting Target' in 'Avatar IK Settings Sample' component!");
                return;
            }

            switch (sitStatus)
            {
                case 0:
                    StandUpImmediately();
                    break;
                case 1:
                    SitDownAtTarget(_avatarIKSettings.heightAutoFit.sittingMode.sittingTarget, _avatarIKSettings.heightAutoFit.sittingMode.sittingHeight);
                    break;
                default:
                    break;
            }
        }
        
        public void OnReloadAvatar()
        {
            if (_fitAvatarDemo== null)
                return;
            _controllerEntity = null;
            loadingState.text = "Avatar Loading ...";
            _fitAvatarDemo.ReLoadAvatar();
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        void RefreshMenuText(MenusType type, float value)
        {
            if(sliderTexts.ContainsKey(type))
            {
                Text valueText = sliderTexts[type];
                valueText.text = value.ToString("F2");
            }
        }

        void RefreshAllMenuTexts()
        {
            foreach(var kvp in sliders)
            {
                MenusType type = kvp.Key;
                Slider slider = kvp.Value;
                RefreshMenuText(type,slider.value);
            }
        }


        // Update is called once per frame
        void Update()
        {
            ShowDriveInfo();
        }
        void ShowDriveInfo()
        {
            // HMD height
            InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(_devicePositionFeature, out var devicePos);
            float avatarEyeHeight =  0;
            if (_controllerEntity != null && _controllerEntity.bodyAnimController != null)
            {
                XForm localEyeXForm = _controllerEntity.bodyAnimController.GetEyeXForm();
                avatarEyeHeight = localEyeXForm.position.y;
            }
            
            string content = string.Format(_showDeviceMsg,
                System.Math.Round(devicePos.y, 2),
                System.Math.Round(avatarEyeHeight, 2));
            deviceInfo.text = content;
        }
    }
}

