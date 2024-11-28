using System.Collections;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class LoadAvatar : PicoAppLaunchBase
            {
                private bool m_loginPlatformSDK = false;
                
                public GameObject characterRoot;
                public GameObject characterItem;
                public Transform characterContent;
                public GameObject avatarItem;
                public Transform avatarContent;
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

                    //CreateAvatar();
                    RequestCharacterList();
                }

                void RequestCharacterList()
                {
                    RequestCharacterListRequest.DoRequest((NativeResult errorCode, string message) =>
                    {
                        var characterList = JsonConvert.DeserializeObject<JArray>(message);
                        Debug.Log("LoadAvatar RequestCharacterList, errorCode= " + errorCode + ", message= " + message);
                        for (int i = 0; i < characterList.Count; i++)
                        {
                            var go = Instantiate(characterItem, characterContent);
                            var charcterBtn = go.GetComponent<Button>();
                            if (charcterBtn != null)
                            {
                                var data = characterList[i];
                                if (i == 0)
                                {
                                    RequestAvatarList(data);
                                }
                                charcterBtn.onClick.AddListener(() =>
                                {
                                    RequestAvatarList(data);
                                });
                               
                                var text = charcterBtn.GetComponentInChildren<Text>();
                                if (text != null)
                                {
                                    text.text =  data.Value<string>("show_name");
                                }
                            }
                        }
                    });    
                }

                void RequestAvatarList(JToken characterdata)
                {
                    //This is used to show the PICO OS system role
                    for (int i = 0; i < avatarContent.childCount; i++)
                    {
                        Destroy(avatarContent.GetChild(i).gameObject);
                    }
                    var imageUrl = characterdata.Value<string>("cover");
                    RequestAvatarListInCharacterRequest.DoRequest(characterdata.Value<string>("character_id"), (NativeResult errorCode, string message) =>
                    {
                        var avatarObj = JsonConvert.DeserializeObject<JObject>(message);
                        var count  = avatarObj.Value<int>("count");
                        if (count <= 0)
                        {
                            if (characterdata.Value<string>("character_type").Equals("2"))
                            {//This is used to display the roles uploaded by the PICO avatar upload tool
                                AvatarCapabilities capabilities = new AvatarCapabilities();
                                capabilities.manifestationType = AvatarManifestationType.Full;
                                capabilities.controlSourceType = ControlSourceType.NPC;
                                capabilities.forceLodLevel = AvatarLodLevel.Lod0;
                                capabilities.bodyCulling = true;
                                capabilities.enableExpression = true;

                                var curShowAvatar = PicoAvatarManager.instance.LoadAvatar(
                                    AvatarLoadContext.CreateByAvatarId(string.Empty,
                                        characterdata.Value<string>("character_id"), capabilities),
                                    (avatar, avatarEntity) =>
                                    {
                                        if (avatar == null || avatarEntity == null)
                                            return;
                                        avatar.PlayAnimation("idle");
                                        avatar.PlayAnimation("smile", 0, "BottomLayer");
                                    }, "2", characterdata.Value<string>("item_online_version"));
                                if (curShowAvatar != null)
                                {
                                    var avatarTrans = curShowAvatar.transform;
                                    avatarTrans.parent = this.transform;
                                }
                                Resources.UnloadUnusedAssets();
                                return;
                            }
                        }
                        var avatarList = avatarObj.Value<JArray>("avatars");
                        for (int i = 0; i < count; i++)
                        {
                            var go = Instantiate(avatarItem, avatarContent);
                            var avatarItemClass = go.GetComponent<AvatarItem>();
                            if (avatarItemClass != null)
                            {
                                avatarItemClass.InitShowContent(avatarList[i], characterRoot.transform);

                            }
                        }
                    });
                    
                    Resources.UnloadUnusedAssets();
                }
            }
        }
    }
}


