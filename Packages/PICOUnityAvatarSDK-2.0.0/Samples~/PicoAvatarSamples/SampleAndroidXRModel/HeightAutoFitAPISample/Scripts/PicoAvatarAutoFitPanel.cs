using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.EventSystems;
namespace Pico.Avatar.Sample
{

    public class PicoAvatarAutoFitPanel : MonoBehaviour
    {
          public enum MenusType
        {
            //双脚离地最大时间
            FlyTimeMaxRang = 0,
            //下蹲最大时间
            CrouchTimeMaxRang,
            //下蹲高度值
            CrouchHeightMaxRang,
            //下蹲距离切换至站立距离值
            CrouchHeightToIdleRang,
            //身体缩放
            AvatarScale,
            //销毁重加载
            ReLoadAvatar,
        }
        private Dictionary<MenusType, float> defaultConfigRangeValue = new Dictionary<MenusType, float>()
        {
            {MenusType.FlyTimeMaxRang,1},
            {MenusType.CrouchTimeMaxRang,3},
            {MenusType.CrouchHeightMaxRang,0.2f},
            {MenusType.CrouchHeightToIdleRang,0.7f},
            {MenusType.AvatarScale,1.7f},
            {MenusType.ReLoadAvatar,0f},
        };
        
        //当前生效值
        public Dictionary<MenusType, float> curRangeValue = new Dictionary<MenusType, float>()
        {
            {MenusType.FlyTimeMaxRang,1},
            {MenusType.CrouchTimeMaxRang,3},
            {MenusType.CrouchHeightMaxRang,0.2f},
            {MenusType.CrouchHeightToIdleRang,0.7f},
            {MenusType.AvatarScale,1.7f},
            {MenusType.ReLoadAvatar,0f},
        };

        //菜单值显示格式
        public Dictionary<MenusType, string> mensValueNameConfig = new Dictionary<MenusType, string>()
        {
            {MenusType.FlyTimeMaxRang,"{0:N1} S"},
            {MenusType.CrouchTimeMaxRang,"{0:N1} S"},
            {MenusType.CrouchHeightMaxRang,"{0:N2} m"},
            {MenusType.CrouchHeightToIdleRang,"{0:N2} m"},
            {MenusType.AvatarScale,"{0:N2}"},
            {MenusType.ReLoadAvatar,"{0:N2}"},
        };
        //每次变化值
        public Dictionary<MenusType, float> rangeChangeOffsetConfig = new Dictionary<MenusType, float>()
        {
            {MenusType.FlyTimeMaxRang,0.1f},
            {MenusType.CrouchTimeMaxRang,0.1f},
            {MenusType.CrouchHeightMaxRang,0.01f},
            {MenusType.CrouchHeightToIdleRang,0.01f},
            {MenusType.AvatarScale,0.01f},
            {MenusType.ReLoadAvatar,0.1f},
        };
       
        
        public List<Text> menuGo = new List<Text>();
        public List<Slider> menuSlider = new List<Slider>();
        public Text deviceInfo;
        public Text loadingState;
        
        private HeightAutoFitDemoStart _fitAvatarDemo;
        private InputFeatureUsage<Vector3> _devicePositionFeature;
        private InputFeatureUsage<Quaternion> _deviceRotationFeature;
        private AvatarEntity _controllerEntity;

        private AvatarIKSettingsSample _avatarIKSettings;

        //device panel show
        private string _showDeviceMsg = "Height of VR Headset:{0}\n\n" +
                                       "Height of Avatar's Eye:{1}\n";
        // Start is called before the first frame update
        void Start()
        {
            loadingState.text = "Avatar Loading ...";
            _devicePositionFeature = UnityEngine.XR.CommonUsages.devicePosition;
            _deviceRotationFeature = UnityEngine.XR.CommonUsages.deviceRotation;
            foreach (var item in this.defaultConfigRangeValue)
            {
                curRangeValue[item.Key] = item.Value;
                RefreshMenuText(item.Key);
                var index = (int)item.Key;
                if (index < menuSlider.Count)
                {
                    menuSlider[index].onValueChanged.AddListener((value) =>
                    {
                        OnSliderValueChanged(item.Key, value);
                    });
                }
                
            }
        }

        private void OnSliderValueChanged(MenusType funcType ,float value)
        {
            if (_avatarIKSettings == null) return;

            curRangeValue[funcType] = value*defaultConfigRangeValue[funcType];
            var newValue = curRangeValue[funcType];
            switch (funcType)
            {
                case MenusType.CrouchHeightMaxRang:
                    _avatarIKSettings.heightAutoFit.standingMode.thresholds.crouchingDistance = newValue;
                    break;
                case MenusType.CrouchHeightToIdleRang:
                    _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxCrouchingDistance = newValue;
                    break;
                case MenusType.CrouchTimeMaxRang:
                    _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxCrouchingTime = newValue;
                    break;
                case MenusType.FlyTimeMaxRang:
                    _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxFloatingTime = newValue;
                    break;
            }
            _avatarIKSettings.isDirty = true;
            RefreshMenuText(funcType);
            EventSystem.current.SetSelectedGameObject(null);
        }

        private void UpdateValuesFromIKSettings()
        {
            if (_avatarIKSettings == null) return;

            curRangeValue[MenusType.FlyTimeMaxRang] = _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxFloatingTime;
            curRangeValue[MenusType.CrouchTimeMaxRang] = _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxCrouchingTime;
            curRangeValue[MenusType.CrouchHeightMaxRang] = _avatarIKSettings.heightAutoFit.standingMode.thresholds.crouchingDistance;
            curRangeValue[MenusType.CrouchHeightToIdleRang] = _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxCrouchingDistance;

            foreach (var item in this.defaultConfigRangeValue)
            {
                RefreshMenuText(item.Key);
            }
        }

        public void SetTargetEntity(AvatarEntity controller)
        {
            this._controllerEntity = controller;
            loadingState.text = "Avatar Loading Finish";
        }

        public void SetIkSettings(AvatarIKSettingsSample avatarIKSettings)
        {
            _avatarIKSettings = avatarIKSettings;
            _avatarIKSettings?.SetOnValidateCallback(UpdateValuesFromIKSettings);
            UpdateValuesFromIKSettings();
        }

        public void SetHeightAutoFitDemo(HeightAutoFitDemoStart autoFit)
        {
            _fitAvatarDemo = autoFit;
        }
        public void OnAddClick(string inputtype)
        {
            OnClickItem(inputtype, true);
        }
    
        public void OnDecClick(string inputtype)
        {
            OnClickItem(inputtype, false);
        }
        
        private void OnClickItem(string inputtype, bool add)
        {
            if (_avatarIKSettings == null) return;
            
            if (Enum.TryParse(inputtype, out MenusType funcType))
            {
                if (add)
                {
                    curRangeValue[funcType] += rangeChangeOffsetConfig[funcType];
                }
                else
                    curRangeValue[funcType] -= rangeChangeOffsetConfig[funcType];

                float newValue = curRangeValue[funcType];
                if (newValue < 0)
                {
                    newValue = 0;
                    curRangeValue[funcType] = newValue;
                }
                RefreshMenuText(funcType);
                
                switch (funcType)
                {
                    case MenusType.CrouchHeightMaxRang:
                        _avatarIKSettings.heightAutoFit.standingMode.thresholds.crouchingDistance = newValue;
                        break;
                    case MenusType.CrouchHeightToIdleRang:
                        _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxCrouchingDistance = newValue;
                        break;
                    case MenusType.CrouchTimeMaxRang:
                        _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxCrouchingTime = newValue;
                        break;
                    case MenusType.FlyTimeMaxRang:
                        _avatarIKSettings.heightAutoFit.standingMode.thresholds.maxFloatingTime = newValue;
                        break;
                }
                _avatarIKSettings.isDirty = true;
            }
            EventSystem.current.SetSelectedGameObject(null);
        }


        public void OnSetAvatarHeight()
        {
            float newValue = curRangeValue[MenusType.AvatarScale];
            if (_controllerEntity== null 
                || _controllerEntity.bodyAnimController == null 
                || _controllerEntity.bodyAnimController.autoFitController == null)
                return;
            _controllerEntity.bodyAnimController.SetAvatarHeight(newValue);
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        public void OnReloadAvatar()
        {
            if (_fitAvatarDemo== null)
                return;
            _controllerEntity = null;
            loadingState.text = "Avatar Loading";
            _fitAvatarDemo.ReLoadAvatar();
            foreach (var item in this.defaultConfigRangeValue)
            {
                curRangeValue[item.Key] = item.Value;
                RefreshMenuText(item.Key);
            }
            EventSystem.current.SetSelectedGameObject(null);
        }
        
        void RefreshMenuText(MenusType type)
        {
            var index = (int)type;
            if (index >= menuGo.Count)
                return;
            var item = menuGo[index];
            if (item != null)
                item.text = string.Format(mensValueNameConfig[type], curRangeValue[type]);
            var slider = menuSlider[index];
            slider.SetValueWithoutNotify(curRangeValue[type] / defaultConfigRangeValue[type]);
        }

        // Update is called once per frame
        void Update()
        {
            ShowDriveInfo();
        }
        void ShowDriveInfo()
        {
            //头盔高度
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

