using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
namespace Pico.Avatar.Sample
{


    public class XRAnimationAction : MonoBehaviour
    {
        public enum MenusType
        {
            BlendTree2D,
            MaskHand,
            MaskHandPose,
            LerpAnim,

        }
        private Transform cloneItem;
        private Dictionary<MenusType, string> showButtonList = new Dictionary<MenusType, string>
    {
        { MenusType.BlendTree2D,"Move2D-BlendTree2D"},
        { MenusType.MaskHand,"PlayHandLoop" },
         {MenusType.MaskHandPose, "PlayHandPoseOnce" },
           {MenusType.LerpAnim, "PlayLerp" },

    };
        private List<MenusType> onSelectMenuList = new List<MenusType>();
        private Dictionary<MenusType, GameObject> menuGo = new Dictionary<MenusType, GameObject>();
        protected InputFeatureUsage<Vector3> devicePositionFeature;
        protected InputFeatureUsage<Quaternion> deviceRotationFeature;
        private XRAnimationController controller;

        private Color SelectColor = new Color(1, 1, 1, 1);
        private Color UnSelectColor = new Color(1, 1, 1, 0.45f);
        // Start is called before the first frame update
        void Start()
        {
            cloneItem = gameObject.transform.Find("Canvas/buttonList/Button");
            IntButtonList();

            devicePositionFeature = UnityEngine.XR.CommonUsages.devicePosition;
            deviceRotationFeature = UnityEngine.XR.CommonUsages.deviceRotation;

        }
        public void InitController(XRAnimationController controller)
        {
            this.controller = controller;
        }
        private void OnGUI()
        {
            foreach (var item in showButtonList)
            {
                if (GUILayout.Button(item.Value))
                {
                    OnClick(item.Key);
                }
            }
        }

        void IntButtonList()
        {
            if (cloneItem == null)
                return;
            menuGo.Clear();
            foreach (var itemValue in showButtonList)
            {
                var item = GameObject.Instantiate(cloneItem, cloneItem.parent);
                item.name = ((int)itemValue.Key).ToString();
                var button = item.GetComponent<Button>();
              
                button.onClick.AddListener(delegate () { OnClick(itemValue.Key); });

                menuGo.Add(itemValue.Key, item.gameObject);

                RefreshMenuText(itemValue.Key);
            }
            cloneItem.gameObject.SetActive(false);

        }
        void OnClick(MenusType funcType)
        {
            if (this.controller == null)
            {
                Debug.LogErrorFormat("{0} not have controler", this.gameObject.name);
            }
            Debug.Log("OnClick : " + funcType);
            bool isStop = SetOnSelectMenu(funcType);
            switch (funcType)
            {
                case MenusType.BlendTree2D:
                    this.controller.StartPlay2DBlendTree(isStop);
                    break;
                case MenusType.MaskHand:
                    this.controller.StartPlayMaskHandLoop(isStop);
                    break;
                case MenusType.MaskHandPose:
                    this.controller.StartPlayHandPoseOnce(isStop);
                    break;
                case MenusType.LerpAnim:
                    bool isPlay = this.controller.StartPlayLerpAnimation(isStop);
                    if (isPlay && !onSelectMenuList.Contains(funcType))
                        onSelectMenuList.Add(funcType);
                    else if (!isPlay && onSelectMenuList.Contains(funcType))
                        onSelectMenuList.Remove(funcType);
                    break;
            }

            RefreshMenuText(funcType);

        }
        bool SetOnSelectMenu(MenusType type)
        {
            if (onSelectMenuList.Contains(type))
            {
                onSelectMenuList.Remove(type);
                return true;
            }
            onSelectMenuList.Add(type);
            return false;
        }
        void RefreshMenuText(MenusType type)
        {
            var item = menuGo[type];
            var uiLabel = item.transform.Find("Text").GetComponent<Text>();
            bool isRun = onSelectMenuList.Contains(type);
            string txt = showButtonList[type];
            if (isRun)
            {
                uiLabel.text = txt + "(running)";
                uiLabel.color = SelectColor;
            }
            else
            {
                uiLabel.text = txt;
                uiLabel.color = UnSelectColor;
            }

        }
    }
}
