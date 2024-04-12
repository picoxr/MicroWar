using System.Collections;
using System.Collections.Generic;
using Pico.Avatar;
using UnityEngine;

public class TestAvatarLoad : MonoBehaviour
{
    private string nation;
    private string userId;
    private string accessToken;

    private bool isPlatformFinish = false;

    void Awake()
    {
        //Platform initialize
        Pico.Platform.CoreService.Initialize();
    }


    IEnumerator Start()
    {

        Platform_SvrPermissions();


        //wait for login
        while (!isPlatformFinish)
            yield return null;


        //wait for PicoAvatarApp initialize finish
        while (!PicoAvatarApp.isWorking)
            yield return null;

        //set up PicoAvatarApp and start PicoAvatarManager
        PicoAvatarAppStart();

        //wait for PicoAvatarManager initialize finish
        while (!PicoAvatarManager.canLoadAvatar)
            yield return null;

        // load avatar
        LoadAvatar();
    }


    void LoadAvatar()
    {
        AvatarCapabilities capabilities = new AvatarCapabilities();
        capabilities.manifestationType = AvatarManifestationType.Full;
        capabilities.controlSourceType = ControlSourceType.MainPlayer;
        capabilities.bodyCulling = true;

        var avatar = PicoAvatarManager.instance.LoadAvatar(new AvatarLoadContext(userId, "", "", capabilities));
        if (avatar == null)
        {
            Debug.LogError("Load avatar failed");
            return;
        }
        avatar.transform.parent = transform;
        avatar.transform.localPosition = Vector3.zero;
        avatar.transform.localRotation = Quaternion.identity;

    }

    void PicoAvatarAppStart()
    {
        //get AvatarApp instance
        var avatarApp = PicoAvatarApp.instance;
        avatarApp.accessType = AccessType.ThirdApp;

        avatarApp.StartAvatarManager();
    }

#region Login

    // request avatar access permission
    void Platform_SvrPermissions()
    {
        Debug.Log($"RequestUserPermissions Start");
        Pico.Platform.UserService.RequestUserPermissions(new string[2] { "user_info", "avatar" }).OnComplete(msg =>
        {
            if (msg.IsError)
            {
                Debug.Log($"RequestUserPermissions failed:code={msg.Error.Code},message={msg.Error.Message}");
                return;
            }

            Debug.Log($"Request permission successfully:{(msg.Data)}");
            Platform_SvrUserID();
        });
    }

    // request user info
    void Platform_SvrUserID()
    {
        Debug.Log($"GetLoggedInUser Start");
        Pico.Platform.UserService.GetLoggedInUser().OnComplete((user) =>
        {
            if (this.gameObject == null || user == null)
                return;

            if (user.IsError)
            {
                var err = user.GetError();
                Debug.Log($"Got loggedInUser error {err.Message} code={err.Code}");
                return;
            }

            if (user.Data == null)
            {
                Debug.LogError("Got user.Data is Null");
                return;
            }

            Debug.LogError($"Got userID {user.Data.ID},name = {user.Data.DisplayName}");

            nation = user.Data.StoreRegion;
            if (string.IsNullOrEmpty(nation))
                nation = "cn";
            userId = user.Data.ID;

            Platform_SvrAccessToken();
        });
    }

    // get accesstoken
    void Platform_SvrAccessToken()
    {
        Debug.Log($"Platform:SvrAccessToken");
        Pico.Platform.UserService.GetAccessToken().OnComplete(delegate (Pico.Platform.Message<string> message)
        {
            if (message.IsError)
            {
                var err = message.GetError();
                Debug.Log($"Got access token error {err.Message} code={err.Code}");
                return;
            }

            accessToken = message.Data;
            Debug.Log($"Got accessToken {accessToken}");

            isPlatformFinish = true;
        });
    }

#endregion
}