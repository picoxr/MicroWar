using System.Collections;
using UnityEngine;

namespace Pico.Avatar.Sample
{
  
    public class DynamicAnimationClipDemoStart : PicoAppLaunchBase
    {
        [Header("---------------- App Login --------------")]
        private AnimationDecompressor animationGroupAssets = null;

        private bool m_loginPlatformSDK = false;
        private ActionAvatar _actAvatar;

        private void Awake()
        {
            System.Action<bool> loginCall = (state) =>
            {
                m_loginPlatformSDK = state;
            };
            this.SvrPlatformLogin(loginCall);
            string assetPath = (Application.dataPath + "/PicoAvatarSamples/SampleAndroidXRModel/DynamicAnimationClipSample");
            animationGroupAssets = new AnimationDecompressor("animaz", assetPath, "dev_animation_group_list.txt");

        }

        IEnumerator Start()
        {
            PXRCheck();

            while (!m_loginPlatformSDK)
                yield return null;

            //wait decompressFinish
            while (!animationGroupAssets.HasInitFish)
                yield return animationGroupAssets.StartDecompression();

            Debug.Log("Decompression finish:" + animationGroupAssets.HasInitFish);
            //waiting PicoAvatarApp finished
            while (!PicoAvatarApp.isWorking)
                yield return null;

            this.PicoAvatarAppStart();

            //waiting Manager finished
            while (!PicoAvatarManager.canLoadAvatar)
                yield return null;

            //create Avatar
            var item = GameObject.Find("LocalActionAvatar");
            if (item)
            {
                _actAvatar = item.GetComponent<ActionAvatar>();
                _actAvatar.StartAvatar(this.UserServiceUserID);
            }
            //waiting Avatar finished
            while (!_actAvatar.Avatar.isAnyEntityReady)
                yield return null;

            _actAvatar.Avatar.PlayAnimation("smile");
            //add animation controller
            var controlAnim = item.AddComponent<XRAnimationController>();
            controlAnim.InitAnimationAssets(animationGroupAssets);
            controlAnim.LinkAvatar(_actAvatar.Avatar);

            //action set
            var action = GameObject.Find("XRActionCanvas");
            action.GetComponent<XRAnimationAction>().InitController(controlAnim);
        }
    }
}


