using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class AvatarMirrorDemoStart : PicoAppLaunchBase
            {
                public Text openFTLipsync;
                private bool m_loginPlatformSDK = false;
                private ActionAvatar _actAvatar;
                private bool _openFTAndLipsync = false;
                private void Awake()
                {
                    System.Action<bool> loginCall = (state) =>
                    {
                        m_loginPlatformSDK = state;
                    };
                    this.SvrPlatformLogin(loginCall);
                }

                IEnumerator Start()
                {
                    this.PXRCheck();

                    while (!m_loginPlatformSDK)
                        yield return null;

                    //waiting PicoAvatarApp finished
                    while (!PicoAvatarApp.isWorking)
                        yield return null;

                    this.PicoAvatarAppStart();

                    //waiting Manager finished
                    while (!PicoAvatarManager.canLoadAvatar)
                        yield return null;


                    //start AvatarLoading
                    var item = GameObject.Find("LocalActionAvatar");
                    if (item)
                    {
                        _actAvatar = item.GetComponent<ActionAvatar>();
                        _actAvatar.StartAvatar(this.UserServiceUserID);
                    }
                    //create mirror
                    var itemMirror = GameObject.Find("LocalActionAvatarMirror");
                    if (itemMirror)
                    {
                        itemMirror.GetComponent<ActionAvatarMirror>().StartAvatar(_actAvatar);
                    }

                }

                public void OpenFTAndLipsync()
                {
#if !UNITY_EDITOR && UNITY_ANDROID
                    if (_actAvatar != null && _actAvatar.Avatar != null 
                                           && _actAvatar.Avatar.entity != null)
                    {
                        _openFTAndLipsync = !_openFTAndLipsync;
                        _actAvatar.Avatar.entity.bodyAnimController?.StartFaceTrack(_openFTAndLipsync, _openFTAndLipsync);
                        openFTLipsync.text = _openFTAndLipsync ? "Close\nFT&Lipsync" : "Open\nFT&Lipsync";
                    }
#endif
                }
            }
        }
    }
}


