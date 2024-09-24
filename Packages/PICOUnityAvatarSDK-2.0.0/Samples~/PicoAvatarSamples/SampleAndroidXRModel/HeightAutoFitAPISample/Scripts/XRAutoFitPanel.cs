using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
namespace Pico.Avatar.Sample
{

    public class XRAutoFitPanel : MonoBehaviour
    {
        public enum MenusType
        {
            //双脚离地最大时间
            FlyTimeMaxRang,
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
            {MenusType.AvatarScale,1.6f},
            {MenusType.ReLoadAvatar,0f},
        };
        //当前生效值
        public Dictionary<MenusType, float> curRangeValue = new Dictionary<MenusType, float>()
        {
            {MenusType.FlyTimeMaxRang,1},
            {MenusType.CrouchTimeMaxRang,3},
            {MenusType.CrouchHeightMaxRang,0.2f},
            {MenusType.CrouchHeightToIdleRang,0.7f},
            {MenusType.AvatarScale,1.6f},
            {MenusType.ReLoadAvatar,0f},
        };
        //当前菜单描述
        public Dictionary<MenusType, string> mensNameConfig = new Dictionary<MenusType, string>()
        {
            {MenusType.FlyTimeMaxRang,"双脚离地阈值"},
            {MenusType.CrouchTimeMaxRang,"下蹲时间阈值"},
            {MenusType.CrouchHeightMaxRang,"下蹲高度值"},
            {MenusType.CrouchHeightToIdleRang,"下蹲一定距离\n站立值"},
            {MenusType.AvatarScale,"身高"},
            {MenusType.ReLoadAvatar,"重加载"},
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
        //device panel show
        private string showDeviceMsg = "头盔高度:{0}\n" +
            "Avatar眼高:{1}\n" +
            "评估身高:{2}\n" +
            "缩放比例:{3}\n" +
            "触发校准:{4}";

        private Transform listClone;
        private Dictionary<MenusType, GameObject> menuGo = new Dictionary<MenusType, GameObject>();
        private Text deviceLabel;
        protected InputFeatureUsage<Vector3> devicePositionFeature;
        protected InputFeatureUsage<Quaternion> deviceRotationFeature;
        private AvatarEntity controllerEntity;
        private Text loadingState;
        //test script
        public static int avatarTriggerCount;
        public HeightAutoFitDemoStart fitAvatarDemo;
        // Start is called before the first frame update
        void Start()
        {
            listClone = gameObject.transform.Find("ActionPanel/Scroll View/Viewport/Content/listItem");
            deviceLabel = gameObject.transform.Find("MsgPanel/Content").GetComponent<Text>();
            loadingState = gameObject.transform.Find("ActionPanel/Loading").GetComponent<Text>();
            loadingState.text = "avatar loading ...";
            InitScrollViewItems();

            devicePositionFeature = UnityEngine.XR.CommonUsages.devicePosition;
            deviceRotationFeature = UnityEngine.XR.CommonUsages.deviceRotation;

        }
        public void SetTargetEntity(AvatarEntity controller)
        {
            this.controllerEntity = controller;
            loadingState.text = "avatar finish";
        }
       

        void InitScrollViewItems()
        {
            if (listClone == null)
                return;
            menuGo.Clear();
            foreach (var itemValue in mensNameConfig)
            {
                var item = GameObject.Instantiate(listClone, listClone.parent);
                item.name = ((int)itemValue.Key).ToString();
                menuGo.Add(itemValue.Key, item.gameObject);

                OnFullItem(item.gameObject, itemValue.Key);
            }
            listClone.gameObject.SetActive(false);

        }
        void OnFullItem(GameObject item, MenusType btnType)
        {
            var nextBtn = item.transform.Find("NextButton");
            var preBtn = item.transform.Find("PreButton");
            var applyButton = item.transform.Find("ApplyButton");
            applyButton.gameObject.SetActive(btnType >= MenusType.AvatarScale);
            nextBtn.gameObject.SetActive(btnType != MenusType.ReLoadAvatar);
            preBtn.gameObject.SetActive(btnType != MenusType.ReLoadAvatar);

            applyButton.GetComponent<Button>().onClick.AddListener(delegate () { OnClickForceTrigger(btnType); });
            nextBtn.GetComponent<Button>().onClick.AddListener(delegate () { OnClickItem(btnType,true); });
            preBtn.GetComponent<Button>().onClick.AddListener(delegate () { OnClickItem(btnType,false); });

            RefreshMenuText(btnType);
        }
       public void OnClickItem(MenusType funcType,bool next)
        {
            if (next)
            {
                curRangeValue[funcType] += rangeChangeOffsetConfig[funcType];
            }
            else
                curRangeValue[funcType] -= rangeChangeOffsetConfig[funcType];

            float newValue = curRangeValue[funcType];

            RefreshMenuText(funcType);
            var autoFitController = this.controllerEntity.bodyAnimController.autoFitController;
            switch (funcType)
            {
                case MenusType.CrouchHeightMaxRang:
                    autoFitController?.SetCrouchingDistance(newValue);
                    break;
                case MenusType.CrouchHeightToIdleRang:
                    autoFitController?.SetMaxCrouchingDistance(newValue);
                    break;
                case MenusType.CrouchTimeMaxRang:
                    autoFitController?.SetMaxCrouchingTime(newValue);
                    break;
                case MenusType.FlyTimeMaxRang:
                    autoFitController?.SetMaxFloatingTime(newValue);
                    break;
            }
            DebugLog("OnClickItem:" + funcType.ToString());
        }
        void OnClickForceTrigger(MenusType type)
        {
            DebugLog("OnClickForceTrigger:" + type);
            float newValue = curRangeValue[type];
            switch (type)
            {
                case MenusType.AvatarScale:
                    this.controllerEntity.bodyAnimController.SetAvatarHeight(newValue);
                    break;
                case MenusType.ReLoadAvatar:
                    this.controllerEntity = null;
                    loadingState.text = "avatar loading";
                    this.fitAvatarDemo.ReLoadAvatar();
                    foreach (var item in this.defaultConfigRangeValue)
                    {
                        this.curRangeValue[item.Key] = item.Value;
                        RefreshMenuText(item.Key);
                    }
                    break;
            }
        }

       
        void RefreshMenuText(MenusType type)
        {
            var item = menuGo[type];
            var uiLabel = item.transform.Find("name").GetComponent<Text>();
            uiLabel.text = mensNameConfig[type];
            var valueLb = item.transform.Find("value").GetComponent<Text>();
            valueLb.text = string.Format(mensValueNameConfig[type], curRangeValue[type]);
        }

        // Update is called once per frame
        void Update()
        {
            ShowDriveInfo();
        }
        void ShowDriveInfo()
        {
            //头盔高度
            Vector3 devicePos = Vector3.zero;
            InputDevices.GetDeviceAtXRNode(XRNode.Head).TryGetFeatureValue(devicePositionFeature, out devicePos);
            //眼睛高度，这样获取是算上模型 offset的高度吗
            //这样是否正确，和父节点无关？
            float avatarEyeHeight =  0;
            if (controllerEntity && controllerEntity)
            {
                XForm localEyeXForm = controllerEntity.bodyAnimController.GetEyeXForm();
                avatarEyeHeight = localEyeXForm.position.y;
            }
            //身体评估高度
            float avatarVirtualHeight = 0;
            //avatar缩放
            float avatarScale = 0;
            //触发次数和条件，目前只能区分sdk侧和app侧，不能细分。

            string content = string.Format(showDeviceMsg,
                System.Math.Round(devicePos.y, 2),
                System.Math.Round(avatarEyeHeight, 2),
                System.Math.Round(avatarVirtualHeight, 2),
                System.Math.Round(avatarScale, 2),
                avatarTriggerCount);
            deviceLabel.text = content;
        }

        void DebugLog(string content)
        {
            Debug.Log("Example10:" + content);
        }
      

    }
}
