using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class AvatarImportPackageCallback
{
    static AvatarImportPackageCallback()
    {
        AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
    }

    private static void OnImportPackageCompleted(string packagename)
    {
        Debug.Log($"Imported package: {packagename}");

        AssetDatabase.ImportAsset("Packages/org.byted.avatar.sdk/Shaders", ImportAssetOptions.ForceUpdate | ImportAssetOptions.ImportRecursive);
    }
}

#if false
class AvatarImportPackagePostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            Debug.Log("Reimported Asset: " + str);
        }
        foreach (string str in deletedAssets)
        {
            Debug.Log("Deleted Asset: " + str);
        }
        for (int i = 0; i < movedAssets.Length; i++)
        {
            Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
        }
    }
}
#endif
