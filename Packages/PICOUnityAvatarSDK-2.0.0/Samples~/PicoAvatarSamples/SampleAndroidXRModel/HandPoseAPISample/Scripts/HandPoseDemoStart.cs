using System.Collections;
using UnityEngine;

namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class HandPoseDemoStart : PicoAppLaunchBase
            {
                private GameObject m_customHandAssets;
                private bool m_loginPlatformSDK = false;


                private void Awake()
                {
                    System.Action<bool> loginCall = (state) =>
                    {
                        m_loginPlatformSDK = state;
                    };
                    this.SvrPlatformLogin(loginCall);

                    m_customHandAssets = GameObject.Find("CustomHandAssets");
                }

                private void Update()
                {
                    //update hand pos test
                    Camera camera = Camera.main;
                    if (camera != null && m_customHandAssets)
                    {
                        Vector3 cameraPos = camera.transform.position;
                        m_customHandAssets.transform.position = new Vector3(0, cameraPos.y/2, cameraPos.z + 0.2f);
                    }

                }
                IEnumerator Start()
                {
                    PXRCheck();

                    while (!m_loginPlatformSDK)
                        yield return null;

                    //waiting PicoAvatarApp finished
                    while (!PicoAvatarApp.isWorking)
                        yield return null;

                    this.PicoAvatarAppStart();

                    //waiting Manager finished
                    while (!PicoAvatarManager.canLoadAvatar)
                        yield return null;


                    CreateAvatarByUserID();

                }
                
                void CreateAvatarByUserID()
                {

                    //create Avatar
                    var item = GameObject.Find("LocalActionAvatar");
                    ActionAvatar actAvatar = null;
                    if (item)
                    {
                        actAvatar = item.GetComponent<ActionAvatar>();
                        actAvatar.StartAvatar(this.UserServiceUserID);

                        //set Handpose skeleton
                        GameObject leftHandPostSkeleton = GameObject.Find("CustomHand/b_l_hand_pose");
                        GameObject rightHandPostSkeleton = GameObject.Find("CustomHand/b_r_hand_pose");

                        //handpose org prefab
                        GameObject lefttHandPostGo = GameObject.Find("CustomHandAssets/HandPose_L/L_Open/b_l_root/b_l_hand_pose");
                        GameObject rightHandPostGo = GameObject.Find("CustomHandAssets/HandPose_R/R_Open/b_r_root/b_r_hand_pose");

                        actAvatar.SetCustomHandPose(leftHandPostSkeleton, rightHandPostSkeleton, lefttHandPostGo, rightHandPostGo);
                    }
                    //set mirror
                    var itemMirror = GameObject.Find("LocalActionAvatarMirror");
                    if (itemMirror)
                    {
                        itemMirror.GetComponent<ActionAvatarMirror>().StartAvatar(actAvatar);
                    }

                }
                
            }
        }

    }
}



