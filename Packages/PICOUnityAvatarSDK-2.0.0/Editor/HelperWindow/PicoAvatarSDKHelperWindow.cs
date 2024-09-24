using System;
using System.IO;
using Pico.Avatar;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;
using UnityEngine.Rendering;


public class PicoAvatarSDKHelperWindow : EditorWindow
{
    private Button docBtn;
    private Button faqBtn;
    private Button oncallBtn;

    private VisualElement oncallPanel;
    private VisualElement oncallPanelClose;
    private VisualElement copyBtn;
    
    private const string DocsUrl_CN = "https://developer-cn.picoxr.com/document/unity-avatar/";
    private const string DocsUrl_OVERSEA = "https://developer.picoxr.com/document/unity-avatar/";
    [MenuItem("AvatarSDK/Show Helper", false, 2)]
    public static void ShowExample()
    {
        PicoAvatarSDKHelperWindow wnd = ScriptableObject.CreateInstance(typeof(PicoAvatarSDKHelperWindow)) as PicoAvatarSDKHelperWindow;
        //wnd.titleContent = new GUIContent("Pico Avatar SDK Helper v2.8.0");
        // wnd.maxSize = new Vector2(1276, 750);
        // wnd.minSize = new Vector2(1276, 750);
        wnd.Show();
    }

    public PicoAvatarSDKHelperWindow()
    {
        EditorApplication.delayCall += AutoSDKSettings;
    }

    private void OnEnable()
    {
        titleContent = new GUIContent("PICO Avatar SDK For Unity v2.0.0");
        
    }
    
    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Import UXML
        var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/org.byted.avatar.sdk/Editor/HelperWindow/PicoAvatarSDKHelperWindow.uxml");
        VisualElement view = visualTree.Instantiate();
        root.Add(view);
        view.style.height = Length.Percent(100);
        view.style.width = Length.Percent(100);

        docBtn = view.Q<Button>("docBtn");
        //faqBtn = view.Q<Button>("faqBtn");
        oncallBtn = view.Q<Button>("oncallBtn");
        copyBtn = view.Q<Button>("CopyBtn");
        oncallPanel = view.Q("OncallPanel");
        oncallPanelClose = view.Q("OncallPanelClose");
        
        docBtn.RegisterCallback<ClickEvent>(OnDocBtnClick);
        //faqBtn.RegisterCallback<ClickEvent>(OnFAQBtnClick);
        oncallBtn.RegisterCallback<ClickEvent>(OnOnCallBtnClick);
        copyBtn.RegisterCallback<ClickEvent>(OnCopyBtnClick);
        oncallPanelClose.RegisterCallback<ClickEvent>(OnOnCallPanelClick);
        AddVisualElementHoverMask(docBtn);
        //AddVisualElementHoverMask(faqBtn);
        AddVisualElementHoverMask(oncallBtn);
        AddVisualElementHoverMask(copyBtn);
    }

    private void OnDocBtnClick(ClickEvent evt)
    {
        if (Utility.GetPCNation() == NationType.China)
        {
            Application.OpenURL(DocsUrl_CN);
        }
        else
        {
            Application.OpenURL(DocsUrl_OVERSEA);
        }
      
    }

    private void OnFAQBtnClick(ClickEvent evt)
    {
        //Application.OpenURL(FAQUrl);
    }

    private void OnOnCallBtnClick(ClickEvent evt)
    {
        SetOncallPanelEnable(true);
    }
    
    private void OnOnCallPanelClick(ClickEvent evt)
    {
        SetOncallPanelEnable(false);
    }

    private void OnCopyBtnClick(ClickEvent evt)
    {
        GUIUtility.systemCopyBuffer = "pico_devsupport@bytedance.com";
        SetOncallPanelEnable(false);
    }

    private void SetOncallPanelEnable(bool enable)
    {
        oncallPanel.style.display = enable ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    private void AddVisualElementHoverMask(VisualElement maskTarget, VisualElement triggerTarget = null, bool attachToMaskTargetParent = false, float alpha = 0.08f)
    {
        if (triggerTarget == null)
            triggerTarget = maskTarget; 
            
        VisualElement ve = new VisualElement();
        ve.name = "hoverMask";
        if (attachToMaskTargetParent)
            maskTarget.parent.Add(ve);
        else
            maskTarget.Add(ve);
            
        ve.pickingMode = PickingMode.Ignore;
        ve.style.position = Position.Absolute;
        ve.style.left = 0;
        ve.style.right = 0;
        ve.style.top = 0;
        ve.style.bottom = 0;
        ve.style.borderBottomLeftRadius = maskTarget.resolvedStyle.borderBottomLeftRadius;
        ve.style.borderBottomRightRadius = maskTarget.resolvedStyle.borderBottomRightRadius;
        ve.style.borderTopLeftRadius = maskTarget.resolvedStyle.borderTopLeftRadius;
        ve.style.borderTopRightRadius = maskTarget.resolvedStyle.borderTopRightRadius;
        ve.style.backgroundColor = new Color(1, 1, 1, alpha);
            
        triggerTarget.RegisterCallback<MouseEnterEvent>(OnMouseEnter);
        triggerTarget.RegisterCallback<MouseLeaveEvent>(OnMouseLeave);
        ve.style.display = DisplayStyle.None;
        void OnMouseEnter(MouseEnterEvent @event)
        {
            ve.style.display = DisplayStyle.Flex;
            ve.style.opacity = 1;
        }
            
        void OnMouseLeave(MouseLeaveEvent @event)
        {
            ve.style.opacity = 0;
            ve.style.display = DisplayStyle.None;
        }
    }
    static void AutoSDKSettings()
    {
        EditorApplication.delayCall -= AutoSDKSettings;
        if (PlayerSettings.Android.forceSDCardPermission || !EditorUtility.DisplayDialog(
                "Auto Setting",
                "I can configure playersetting for you, but editor will restart. Do you agree?",
                "Agree",
                "Disagree")) return;
        GraphicsSettings.renderPipelineAsset =
            Resources.Load<RenderPipelineAsset>("UniversalRP-ForwardRenderer-LowQuality");
          
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new [] { GraphicsDeviceType.OpenGLES3 });
            
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
        PlayerSettings.Android.forceSDCardPermission = true;
        PlayerSettings.openGLRequireES31 = true;
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel29;
        AssetDatabase.SaveAssets();
        EditorApplication.OpenProject(GetCurrentProjectPath());
    }

    private static string GetCurrentProjectPath()
    {
        return Directory.GetParent(Application.dataPath)?.FullName;
    }
}