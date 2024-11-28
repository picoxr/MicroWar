using Newtonsoft.Json.Linq;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Pico
{
    namespace Avatar
    {
        namespace Sample
        {
            public class AvatarItem : MonoBehaviour
            {
                public Image thumbnail;
                public Text avatarName;
                public Text avatarId;

                private string _avatarSpec;
                private string _avatarId;
                private Transform _characterRoot;

                public void InitShowContent(JToken avatarData, Transform characterRoot)
                {
                    avatarName.text = avatarData.Value<string>("name");
                    _avatarId = avatarData.Value<string>("avatar_id");
                    avatarId.text = _avatarId;
                    _avatarSpec = avatarData.Value<string>("config");
                    _characterRoot = characterRoot;

                    var preview = avatarData.Value<JArray>("avatar_preview_keys");
                    if (preview != null && preview.Count > 2)
                    {
                        LoadTextureByUrl(preview[2].Value<string>("preview_url"));
                    }
                }

                public void CreateAvatarById()
                {
                    AvatarCapabilities capabilities = new AvatarCapabilities();
                    capabilities.manifestationType = AvatarManifestationType.Full;
                    capabilities.controlSourceType = ControlSourceType.NPC;
                    capabilities.forceLodLevel = AvatarLodLevel.Lod0;
                    capabilities.bodyCulling = true;
                    capabilities.enableExpression = true;
                        
                    var curShowAvatar = PicoAvatarManager.instance.LoadAvatar(
                        AvatarLoadContext.CreateByAvatarId("", _avatarId, capabilities),
                        (avatar, avatarEntity) =>
                        {
                            if (avatarEntity == null)
                                return;
                            avatar.PlayAnimation("idle");
                            avatar.PlayAnimation("smile", 0, "BottomLayer");
                        });
                    if (curShowAvatar != null)
                    {
                        var avatarTrans = curShowAvatar.transform;
                        avatarTrans.parent = _characterRoot == null ? transform : _characterRoot;
                    }
                }

                private void LoadTextureByUrl(string url, int timeOut = 10)
                {
                    var request = UnityWebRequestTexture.GetTexture(url);
                    request.timeout = timeOut;
                    var handler = request.SendWebRequest();
                    handler.completed += operation =>
                    {
                        if (string.IsNullOrEmpty(url) || request == null || thumbnail == null ||
                            thumbnail.IsDestroyed())
                        {
                            request.Dispose();
                            return;
                        }

                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            var rawTex = ((DownloadHandlerTexture)request.downloadHandler).texture;
                            rawTex.hideFlags = HideFlags.DontSaveInEditor;
                            thumbnail.sprite = Sprite.Create(rawTex, new Rect(0, 0, rawTex.width, rawTex.height),
                                new Vector2(0.5f, 0.5f));
                            request = null;
                        }

                        Resources.UnloadUnusedAssets();
                    };
                }
            }
        }
    }
}