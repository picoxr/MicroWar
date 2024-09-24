using System.Collections;
using UnityEngine;
namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class BodyTrackingDemoStart : PicoAppLaunchBase
            {
                private bool m_loginPlatformSDK = false;


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
                    ActionAvatar actAvatar = null;
                    if (item)
                    {
                        actAvatar = item.GetComponent<ActionAvatar>();
                        
                        //init body tracking UI
                        actAvatar.loadedFinishCall = InitBodyTrackingUIManager;
                        
                        actAvatar.StartAvatar(this.UserServiceUserID);
                    }
                    //create mirror
                    var itemMirror = GameObject.Find("LocalActionAvatarMirror");
                    if (itemMirror)
                    {
                        itemMirror.GetComponent<ActionAvatarMirror>().StartAvatar(actAvatar);
                    }
                }

                private void InitBodyTrackingUIManager(ActionAvatar avatar)
                {
                    if (avatar.isMainAvatar)
                    {
                        IDeviceInputReader deviceInputReader = avatar.Avatar.entity.deviceInputReader;
                        if (deviceInputReader is BodyTrackingDeviceInputReader)
                        {
                            BodyTrackingDemoUIManager.Instance.ResetBodyTrackingDeviceInputReader((BodyTrackingDeviceInputReader)deviceInputReader);
                        }
                        else
                        {
                            Debug.Log("Please select the 'Body Tracking' of 'Local Avatar Input Type' in 'Pico Avatar App'");
                        }
                    }
                }
            }
        }
    }
}


