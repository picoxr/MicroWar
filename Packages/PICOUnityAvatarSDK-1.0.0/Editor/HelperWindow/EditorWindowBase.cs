#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Pico.Avatar
{
    public class EditorWindowBase : EditorWindow
    {
        public virtual string Version
        {
            get { return "v1.0.0"; }
        } 

        private const string BecomeAPartnerUrl = "https://bytedance.feishu.cn/docx/doxcnQRCQLNZRYZh6ujQDrGSbAd";
        private const string BecomePartnerText = "为您提供了一些跳转信息，点击下方按钮";

        private const string DocsUrl = "https://bytedance.feishu.cn/docx/doxcnQRCQLNZRYZh6ujQDrGSbAd";
        private const string faqUrl = "https://bytedance.feishu.cn/docx/doxcnLdBONyNxcojmZV7QWN8xvh";

        private readonly Vector2 headerSize = new Vector2(320, 10);

        private static Texture2D banner = null;
        private int width = 512;
        private int height = 200;

        private GUIStyle headerTextStyle = null;
        private GUIStyle webButtonStyle = null;

        private readonly GUILayoutOption windowWidth = GUILayout.Width(512);

        private void LoadAssets()
        {
            banner = new Texture2D(1536, 600);
            string localPath = "/PicoAvatar/Editor/HelperWindow/avatar_sdk_log.png";
            //string localPath = Application.dataPath + "/PicoAvatar/Editor/HelperWindow/avatar_sdk_log.png";
            try
            {
                //var dataArr = System.IO.File.ReadAllBytes(localPath);
                //var orgTexture = new Texture2D(width, height);
                //banner.LoadImage(dataArr);
                //banner.Resize(width, height);
                //banner = orgTexture;// ScaleTexture(orgTexture, 512, 180);
                var temp_banner = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/"+ localPath);
                if (temp_banner)
                    banner = temp_banner;
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.Message);
            }


            if (headerTextStyle == null)
            {
                headerTextStyle = new GUIStyle();
                headerTextStyle.fontSize = 18;
                headerTextStyle.richText = true;
                headerTextStyle.fontStyle = FontStyle.Bold;
                headerTextStyle.normal.textColor = Color.white;
            }


            if (webButtonStyle == null)
            {
                webButtonStyle = new GUIStyle(GUI.skin.button);
                webButtonStyle.fontSize = 12;
                webButtonStyle.fixedWidth = 166;
                webButtonStyle.padding = new RectOffset(5, 5, 5, 5);
            }
        }
        protected virtual void OnGUI()
        {

        }

        public void DrawContent(Action content)
        {
            LoadAssets();

            CreateHorizontalUtil(() =>
            {
                GUILayout.FlexibleSpace();
                CreateVerticalUtils(() => {
                    DrawBanner();
                    content?.Invoke();
                    DrawExternalLinks();
                }, windowWidth);
                GUILayout.FlexibleSpace();
            });
        }

        private void DrawBanner()
        {
            if (banner != null)
            {
                GUI.DrawTexture(new Rect((position.size.x - width) / 2, 0, width, height), banner);
                EditorGUI.DropShadowLabel(new Rect((position.size.x - headerSize.x) / 2, height - 30, headerSize.y, height), $"PicoAvatarSDK For Unity  { Version }", headerTextStyle);
                GUILayout.Space(height + 10);
            }

        }

        private void DrawExternalLinks()
        {
            CreateVerticalUtils(() =>
            {
                EditorGUILayout.HelpBox(BecomePartnerText, MessageType.Info);

            }, true);
            EditorGUILayout.BeginHorizontal("Box");
            if (GUILayout.Button("Documentation", webButtonStyle))
            {
                Application.OpenURL(DocsUrl);
            }
            if (GUILayout.Button("Official", webButtonStyle))
            {
                Application.OpenURL(BecomeAPartnerUrl);
            }
            if (GUILayout.Button("FAQ", webButtonStyle))
            {
                Application.OpenURL(faqUrl);
            }
            EditorGUILayout.EndHorizontal();
        }

        public void CreateVerticalUtils(Action content, bool isBox = false)
        {
            EditorGUILayout.BeginVertical(isBox ? "Box" : GUIStyle.none);
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        public void CreateVerticalUtils(Action content, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginVertical(options);
            content?.Invoke();
            EditorGUILayout.EndVertical();
        }

        public void CreateHorizontalUtil(Action content, bool isBox = false)
        {
            EditorGUILayout.BeginHorizontal(isBox ? "Box" : GUIStyle.none);
            content?.Invoke();
            EditorGUILayout.EndHorizontal();
        }

        public void CreateHorizontalUtil(Action content, params GUILayoutOption[] options)
        {
            EditorGUILayout.BeginHorizontal(options);
            content?.Invoke();
            EditorGUILayout.EndHorizontal();
        }
        Texture2D ScaleTexture(Texture2D source, float targetWidth, float targetHeight)
        {
            Texture2D result = new Texture2D((int)targetWidth, (int)targetHeight, source.format, false);

            float incX = (1.0f / targetWidth);
            float incY = (1.0f / targetHeight);

            for (int i = 0; i < result.height; ++i)
            {
                for (int j = 0; j < result.width; ++j)
                {
                    Color newColor = source.GetPixelBilinear((float)j / (float)result.width, (float)i / (float)result.height);
                    result.SetPixel(j, i, newColor);
                }
            }

            result.Apply();
            return result;
        }

    }
}
#endif