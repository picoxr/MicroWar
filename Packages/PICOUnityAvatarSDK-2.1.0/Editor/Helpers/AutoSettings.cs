using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

[InitializeOnLoad]
public class AutoSettings : MonoBehaviour
{
    static AutoSettings()
    {
        EditorApplication.delayCall += AutoSDKSettings;
    }
    
    [MenuItem("AvatarSDK/Auto Setting", false, 5)]
    static void AutoSDKSettings()
    {
        EditorApplication.delayCall -= AutoSDKSettings;
        if (PlayerSettings.Android.forceSDCardPermission || !EditorUtility.DisplayDialog(
                "Auto Setting",
                "I can configure player settings for you, but editor will restart. Do you agree?\nNote:If you disagree, you can call this again by AvatarSDK/Auto Setting.",
                "Agree",
                "Disagree")) return;
        GraphicsSettings.renderPipelineAsset =
            AssetDatabase.LoadAssetAtPath<RenderPipelineAsset>("Packages/org.byted.avatar.sdk/Runtime/Settings/PipelineSettings/URP/UniversalRP-ForwardRenderer-LowQuality.asset");
        
        QualitySettings.renderPipeline = GraphicsSettings.renderPipelineAsset;
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);
        PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
        PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new [] { GraphicsDeviceType.OpenGLES3 });
            
        PlayerSettings.SetNormalMapEncoding(BuildTargetGroup.Android,NormalMapEncoding.XYZ);
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
