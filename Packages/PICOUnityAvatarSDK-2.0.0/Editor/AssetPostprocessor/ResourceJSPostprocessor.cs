using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using UnityEditor.Build.Reporting;
using UnityEditor.Rendering;



[InitializeOnLoad]
public class ResourceJSPostprocessor : AssetPostprocessor
{
    public static bool CopyFolder(string sourceFolder, string destFolder, List<string> ignoreList)
    {
        try
        {
            if (ignoreList != null && IsContainsInList(ignoreList, destFolder))
                return false;
            if (sourceFolder.EndsWith(".meta") && destFolder.EndsWith(".meta"))
            {
                System.IO.File.Copy(sourceFolder, destFolder, true);//复制文件
                return true;
            }
            //如果目标路径不存在,则创建目标路径
            if (!System.IO.Directory.Exists(destFolder))
            {
                System.IO.Directory.CreateDirectory(destFolder);
            }
            //得到原文件根目录下的所有文件
            string[] files = System.IO.Directory.GetFiles(sourceFolder);
            foreach (string file in files)
            {
                string name = System.IO.Path.GetFileName(file);
                string dest = System.IO.Path.Combine(destFolder, name);
                System.IO.File.Copy(file, dest, true);//复制文件
            }
            //得到原文件根目录下的所有文件夹
            string[] folders = System.IO.Directory.GetDirectories(sourceFolder);
            foreach (string folder in folders)
            {
                string name = System.IO.Path.GetFileName(folder);
                string dest = System.IO.Path.Combine(destFolder, name);
                CopyFolder(folder, dest, ignoreList);//构建目标路径,递归复制文件
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
            return false;
        }

    }
    static bool DeleteDir(string file, List<string> IgnoreList, bool skipRoot = true)
    {

        try
        {
            //去除文件夹和子文件的只读属性
            //去除文件夹的只读属性
            System.IO.DirectoryInfo fileInfo = new DirectoryInfo(file);
            if (fileInfo == null)
                return true;
            fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

            //去除文件的只读属性
            System.IO.File.SetAttributes(file, System.IO.FileAttributes.Normal);

            //判断文件夹是否还存在
            if (Directory.Exists(file) && !IsContainsInList(IgnoreList, file))
            {

                foreach (string f in Directory.GetFileSystemEntries(file))
                {

                    if (File.Exists(f) && !IsContainsInList(IgnoreList, f))
                    {
                        //如果有子文件删除文件
                        File.Delete(f);
                        Console.WriteLine(f);
                    }
                    else
                    {
                        //循环递归删除子文件夹
                        DeleteDir(f, IgnoreList, false);
                    }

                }

                //删除空文件夹
                if (!skipRoot)
                    Directory.Delete(file);

            }
            return true;
        }
        catch (Exception ex) // 异常处理
        {
            UnityEngine.Debug.LogError(ex.Message);
            return false;
        }

    }
    public static bool IsContainsInList(List<string> list, string fullPath)
    {
        if (list == null)
            return false;
        foreach (var item in list)
        {
            if (fullPath.Contains(item))
                return true;
        }
        return false;
    }

    public static void CopySceneResource(bool force)
    {
        string packageName = "com.unity.pico.avatar";
        string localPath = "PicoAvatar/Editor/AnimationEditor/EditorResources";
        string localPackageFile = Path.GetFullPath("Packages/" + packageName +"/"+ localPath);
        string localAssetResource = Path.Combine(Application.dataPath ,localPath);

        if(Directory.Exists(localAssetResource)&&!force)
        {
            return;
        }
        if (!Directory.Exists(localPackageFile))
        {
            return;
        }
        CopyFolder(localPackageFile, localAssetResource,null);

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("copy AvatarAnimationEditor finish: \n from " + localPackageFile + "\nto:" + localAssetResource);
    }
    public static bool CopyJSResource(bool replace = false,bool showLog = false)
    {
        string packageName = "com.unity.pico.avatar";

        string localPackageFile = Path.GetFullPath("Packages/" + packageName + "/PicoAvatar/Resources/AvatarSDKScript.bytes");

        string localAssetResource = Application.dataPath + "/Resources/PicoAvatar/AvatarSDKScript.bytes";
        string dirPath = Path.GetDirectoryName(localAssetResource);

        if (File.Exists(localAssetResource))
        {
            if (replace || !File.Exists(localPackageFile))
                File.Delete(localAssetResource);
            else
            {
                if (showLog)
                    Debug.Log("The file already exists：" + localAssetResource);
                return true;
            }
        }
        if (!File.Exists(localPackageFile))
        {
            if (showLog)
                Debug.LogError("The file does not exist：" + localPackageFile);
            return false;
        }
        try
        {
            if (!System.IO.Directory.Exists(dirPath))
            {
                System.IO.Directory.CreateDirectory(dirPath);
            }
            File.Copy(localPackageFile, localAssetResource, true);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        catch (Exception ex)
        {
            Debug.LogError("copy js failed! please click 'AvatarSDK/CopyScriptFromPackage' \n error:" + ex.Message);
            return false;
        }
        Debug.Log("copy js finish: \n from "+localPackageFile + "\nto:" + localAssetResource);
        return true;
    }
    public static void CheckJSResourceFile()
    {
        CopyJSResource(false, false);
    }
    [UnityEditor.Callbacks.DidReloadScripts]
    static void AllScriptsReloaded()
    {
        CheckJSResourceFile();
        CopySceneResource(false);

    }
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        CheckJSResourceFile();
        CopySceneResource(false);
    }
}
