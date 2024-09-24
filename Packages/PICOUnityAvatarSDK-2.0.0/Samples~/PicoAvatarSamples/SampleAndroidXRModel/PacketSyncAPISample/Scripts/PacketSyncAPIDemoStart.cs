using System.Collections;
using UnityEngine;
namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class PacketSyncAPIDemoStart : PicoAppLaunchBase
            {
                public string otherUserID = "8388ef7f-a01c-461b-83e4-763329e20612";
                
                public override void PicoAvatarAppStart()
                {
                    PicoAvatarApp.instance.netBodyPlaybackSettings.avgPlaybackDelayTime = 0.1f;

                    base.PicoAvatarAppStart();
                }
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
                    PXRCheck();

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
                    ActionAvatar actAvatar = null;
                    if (item)
                    {
                        actAvatar = item.GetComponent<ActionAvatar>();
                        actAvatar.StartAvatar(this.UserServiceUserID);
                    }
                    //设置镜像
                    var itemMirror = GameObject.Find("LocalActionAvatarMirror");
                    if (itemMirror)
                    {
                        itemMirror.GetComponent<ActionAvatarMirror>().StartAvatar(actAvatar);
                    }

                    //开始Avatar加载
                    var otherAvatar = GameObject.Find("OtherActionAvatar");
                    if (otherAvatar)
                    {
                        var otherAvatarAction = otherAvatar.GetComponent<ActionAvatar>();
                        otherAvatarAction.StartAvatar(otherUserID);
                    }

                    //网络采样设置
                    var syncObj = GameObject.Find("SyncAvatarTest");
                    if (syncObj)
                    {
                        var syncTest = syncObj.GetComponent<SyncTestDemo>();
                        syncTest.playbackTimeDelay = 0.1f;
                        syncTest.recordInterval = 0.1f;
                    }

                }
            }
        }
    }
}


