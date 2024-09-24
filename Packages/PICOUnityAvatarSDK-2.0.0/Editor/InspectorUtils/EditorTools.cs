using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Linq;
using System.IO;
//using Pico.AvatarAssetBuilder;

namespace Pico
{
    namespace Avatar
    {
        public class EditorTools
        {
#region Component Helpers
            [MenuItem("Assets/Tools/Check Miss Component")]
            static void CheckMissComponent()
            {
                GameObject[] goList = GameObject.FindObjectsOfType<GameObject>();
                foreach (var item in goList)
                {
                    CheckComponents(item.transform);
                }
                Debug.Log("Check Complete!");
            }

            static void CheckComponents(Transform t)
            {
                if (t == null)
                {
                    return;
                }

                var mList = t.GetComponents<Component>();
                foreach (var m in mList)
                {
                    if (m == null)
                    {
                        Debug.Log("Miss:" + GetGameObjectPath(t.gameObject));
                    }
                }

                var count = t.childCount;
                for (int i = 0; i < count; i++)
                {
                    CheckComponents(t.GetChild(i));
                }
            }

            public static string GetGameObjectPath(GameObject obj)
            {
                string path = "/" + obj.name;
                while (obj.transform.parent != null)
                {
                    obj = obj.transform.parent.gameObject;
                    path = "/" + obj.name + path;
                }
                return path;
            }
#endregion


#region Miscs
            public static void CheckDirectory(string assetPath)
            {
                string dirPath;
                // check path root.
                if (assetPath.LastIndexOf(".") > assetPath.LastIndexOf("/"))
                {
                    dirPath = assetPath.Substring(0, assetPath.LastIndexOf("/"));
                }
                else
                {
                    dirPath = assetPath;
                }

                if (!System.IO.Directory.Exists(dirPath))
                {
                    int startIndex = assetPath.IndexOf(Application.dataPath);
                    if (startIndex == 0)
                    {
                        dirPath = dirPath.Replace(Application.dataPath, "Assets");
                    }

                    var dirNames = dirPath.Split('/');
                    string parentPathName = "Assets";
                    for (int i = 1; i < dirNames.Length; ++i)
                    {
                        string newPathName = parentPathName + "/" + dirNames[i];
                        if (!System.IO.Directory.Exists(newPathName))
                        {
                            UnityEditor.AssetDatabase.CreateFolder(parentPathName, dirNames[i]);
                        }
                        parentPathName = newPathName;
                    }
                }
            }

            public static void MoveGameObjectAsset(string srcPath, string destPath)
            {
                UnityEngine.Object PrefabNow = UnityEditor.AssetDatabase.LoadAssetAtPath(destPath, typeof(GameObject));
                if (PrefabNow != null)
                {
                    UnityEditor.AssetDatabase.DeleteAsset(destPath);
                }
                CheckDirectory(destPath);
                UnityEditor.AssetDatabase.MoveAsset(srcPath, destPath);
            }

            public static void SetTextureAssetReadable(string texture, bool readable, bool isTransparent, bool isCompress)
            {
                var finalImporter = TextureImporter.GetAtPath(texture) as TextureImporter;
                finalImporter.isReadable = readable;
                finalImporter.alphaIsTransparency = isTransparent;

                var platformSettings = finalImporter.GetDefaultPlatformTextureSettings();
                if (isCompress)
                {
                    platformSettings.textureCompression = TextureImporterCompression.CompressedHQ;
                }
                else
                {
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                }
                finalImporter.SetPlatformTextureSettings(platformSettings);
                AssetDatabase.ImportAsset(texture);
            }
            public static void SetTextureAssetReadable(ref Texture2D tex, bool readable, bool isTransparent, bool isCompress)
            {
                if (tex == null)
                {
                    return;
                }

                var resPath = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(resPath))
                {
                    return;
                }
                var finalImporter = TextureImporter.GetAtPath(resPath) as TextureImporter;
                finalImporter.isReadable = readable;
                finalImporter.alphaIsTransparency = isTransparent;

                var platformSettings = finalImporter.GetDefaultPlatformTextureSettings();
                if (isCompress)
                {
                    platformSettings.textureCompression = TextureImporterCompression.CompressedHQ;
                }
                else
                {
                    platformSettings.textureCompression = TextureImporterCompression.Uncompressed;
                    platformSettings.format = TextureImporterFormat.ARGB32;
                }
                finalImporter.SetPlatformTextureSettings(platformSettings);
                AssetDatabase.ImportAsset(resPath);
                // reload
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(resPath);
            }

            public static void SetTextureAssetReadable(ref Texture2D tex)
            {
                if (tex == null)
                {
                    return;
                }

                var resPath = AssetDatabase.GetAssetPath(tex);
                if (string.IsNullOrEmpty(resPath))
                {
                    return;
                }
                var finalImporter = TextureImporter.GetAtPath(resPath) as TextureImporter;
                finalImporter.isReadable = true;
                //
                AssetDatabase.ImportAsset(resPath);
                // reload
                tex = AssetDatabase.LoadAssetAtPath<Texture2D>(resPath);
            }
#endregion


#region Extract Texture

            /** Extract part texture from source texture. 
             * @param pixelRegion pixel region in sourceTexture. comply to unity convention.
             */
            public static Texture2D ExtractTexture(Texture2D sourceTex, Vector4 pixelRegion)
            {
                if (pixelRegion.x < 0 || pixelRegion.y < 0
                    || pixelRegion.z > sourceTex.width || pixelRegion.w > sourceTex.height)
                {
                    UnityEngine.Debug.LogError("ExtractTexture parameter not valid!");
                    return null;
                }

                //
                int startX = (int)pixelRegion.x;
                int startY = (int)pixelRegion.y;
                int cols = (int)pixelRegion.z;
                int rows = (int)pixelRegion.w;

                var pixels = sourceTex.GetPixels32();
                var destPixels = new Color32[cols * rows];
                //
                for (int r = 0; r < rows; ++r)
                {
                    int srcRowStart = (r + startY) * sourceTex.width + startX;
                    int destRowStart = r * cols;
                    for (int c = 0; c < cols; ++c)
                    {
                        destPixels[destRowStart + c] = pixels[srcRowStart + c];
                    }
                }
                //
                Texture2D retTex = new Texture2D((int)pixelRegion.z, (int)pixelRegion.w, TextureFormat.RGBA32, false);
                retTex.alphaIsTransparency = sourceTex.alphaIsTransparency;
                //
                retTex.SetPixels32(destPixels);
                //
                retTex.Apply();
                //
                return retTex;
            }

            /** Extract part texture from source texture. 
             * @param srcRegion unit length region in srcTex. comply to unity convention.
             *                  x: start u position, y: start v position. z: u width, w: v height.
             * @param outTwoPowerSize whether align output size to 2^n.
             */
            public static Texture2D ExtractTexture(Texture2D srcTex, Vector4 srcRegion
                , bool outTwoPowerSize, int maxSize = 1024)
            {
                //
                int outWidth = (int)(srcRegion.z * srcTex.width);
                int outHeight = (int)(srcRegion.w * srcTex.height);

                if (outTwoPowerSize)
                {
                    int maxTwoPower = 2;
                    while ((maxTwoPower + (maxTwoPower >> 2)) < outWidth)
                    {
                        maxTwoPower = maxTwoPower << 1;
                    }
                    outWidth = maxTwoPower;
                    //
                    maxTwoPower = 2;
                    while ((maxTwoPower + (maxTwoPower >> 2)) < outHeight)
                    {
                        maxTwoPower = maxTwoPower << 1;
                    }
                    outHeight = maxTwoPower;
                }

                // check max size.
                if (outWidth > maxSize || outHeight > maxSize)
                {
                    if (outWidth > outHeight)
                    {
                        outHeight = (int)((float)maxSize * ((float)outHeight / (float)outWidth));
                        outWidth = maxSize;
                    }
                    else if (outWidth < outHeight)
                    {
                        outWidth = (int)((float)maxSize * ((float)outWidth / (float)outHeight));
                        outHeight = maxSize;
                    }
                    else
                    {
                        outWidth = outHeight = maxSize;
                    }
                }

                var destPixels = new Color32[outWidth * outHeight];

                //
                Matrix4x4 transformMat = Matrix4x4.TRS(new Vector3(srcRegion.x, srcRegion.y, 0.0f), Quaternion.identity, new Vector3(srcRegion.z, srcRegion.w, 1.0f));

                //
                Texture2D retTex = new Texture2D(outWidth, outHeight, TextureFormat.RGBA32, false);
                retTex.alphaIsTransparency = srcTex.alphaIsTransparency;
                //
                throw new System.Exception("Not implemented yet.");
            }
#endregion


#region IO Helper

            public static List<Transform> CollectChildLeaves(Transform root)
            {
                List<Transform> list = new List<Transform>();
                CollectChildLeaves(root, list);
                return list;
            }

            public static void CollectChildLeaves(Transform node, List<Transform> list)
            {
                //add self.
                list.Add(node);
                // add children.
                for (int i = 0; i < node.childCount; ++i)
                {
                    var child = node.GetChild(i);
                    CollectChildLeaves(child, list);
                }
            }

            public static void CopyDirectory(string destPath, string srcPath, string[] filePatterns)
            {
                if (!destPath.EndsWith("/") && !destPath.EndsWith("\\"))
                {
                    destPath += "/";
                }

                var dirs = System.IO.Directory.GetDirectories(srcPath, "*", System.IO.SearchOption.AllDirectories);

                // create directories
                foreach (var x in dirs)
                {
                    var dirName = x.Replace(srcPath, destPath);
                    System.IO.Directory.CreateDirectory(dirName);
                }

                //
                foreach (var pattern in filePatterns)
                {
                    CopyOnlyFiles(destPath, srcPath, pattern);
                }
            }

            private static void CopyOnlyFiles(string destPath, string srcPath, string filePattern)
            {
                var files = System.IO.Directory.GetFiles(srcPath, filePattern, System.IO.SearchOption.AllDirectories);
                foreach (var x in files)
                {
                    var newFileName = x.Replace(srcPath, destPath);
                    System.IO.File.Copy(x, newFileName);
                }
            }
#endregion


#region Create Assets

            /**
             * @brief Load asset at asset path. if failed, create new one and save to the path.
             */ 
            public static T LoadOrCreateAsset<T>(string assetPathName) where T : ScriptableObject
            {
                var assetObj = AssetDatabase.LoadAssetAtPath<T>(assetPathName);
                if (assetObj == null)
                {
                    assetObj = ScriptableObject.CreateInstance<T>();
                    Pico.Avatar.EditorTools.CheckDirectory(assetPathName);
                    AssetDatabase.CreateAsset(assetObj, assetPathName);
                    EditorUtility.SetDirty(assetObj);
                    AssetDatabase.SaveAssetIfDirty(assetObj);
                }
                return assetObj;
            }

            /**
             * @brief Delete asset and create new asset file.
             */ 
            public static void ReCreateAssetAt(UnityEngine.Object obj, string assetPathName)
            {
                /*var fullPath = Application.dataPath.Replace("Assets", assetPathName);
                if(System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }*/ //can not save because unity unkonw status
                //Pico.Avatar.EditorTools.CheckDirectory(assetPathName);
       
                if(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPathName) != null)
                {
                    AssetDatabase.DeleteAsset(assetPathName);
                }
                AssetDatabase.CreateAsset(obj, assetPathName);
            }

            public static ScriptableObject CreateAsset(System.Type assetType
                , ref string assetPath, ref string baseName, string postName)
            {
                if (Selection.activeObject != null)
                {
                    assetPath = AssetDatabase.GetAssetPath(Selection.activeObject);
                    if (!string.IsNullOrEmpty(assetPath))
                    {
                        assetPath.Replace('\\', '/');
                        assetPath = assetPath.Substring(0, assetPath.LastIndexOf('/'));
                    }
                }
                //
                if (!string.IsNullOrEmpty(assetPath) && string.IsNullOrEmpty(baseName))
                {
                    baseName = assetPath.Substring(assetPath.LastIndexOf('/') + 1);
                }
                //
                if (string.IsNullOrEmpty(postName))
                {
                    postName = assetType.Name.ToLower();
                }
                //
                assetPath = EditorUtility.SaveFilePanelInProject("Create"
                    , string.Format("{0}_{1}", baseName, postName)
                    , "asset", "New " + assetType.Name, assetPath);
                if (string.IsNullOrEmpty(assetPath))
                {
                    return null;
                }
                //
                var asset = ScriptableObject.CreateInstance(assetType);
                //
                asset.name = baseName;
                //
                AssetDatabase.CreateAsset(asset, assetPath);
                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
                //
                return asset;
            }
           
            public static void RefreshAsset(string assetFilePathName)
            {
                string assetPath;
                var pathName = assetFilePathName.Replace('\\', '/');
                if (pathName.StartsWith("Assets/"))
                {
                    assetPath = pathName;
                }
                else
                {
                    assetPath = pathName.Replace(Application.dataPath, "Assets");
                }
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);
            }

            public static string CalculateMD5(string filename)
            {
                using (var md5 = MD5.Create())
                {
                    using (var stream = File.OpenRead(filename))
                    {
                        var hash = md5.ComputeHash(stream);
                        return System.BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                    }
                }
            }
#endregion


#region File Collector

            // 
            public static void AddSpecifyFilesByExtension(ref List<string> srcFiles, string dir, string pattern)
            {
                var fileInfoArr = SearchSpecifyFilesByExtension(ref srcFiles, dir, pattern);
                foreach (var file in fileInfoArr)
                {
                    srcFiles.Add(file);
                }
            }

            //
            public static void RemoveSpecifyFilesByExtension(ref List<string> srcFiles, string dir, string pattern, List<string>ignoreList = null)
            {
                var fileInfoArr = SearchSpecifyFilesByExtension(ref srcFiles, dir, pattern);
                foreach (var file in fileInfoArr)
                {
                    bool skip = false;
                    if(ignoreList != null)
                    {
                        foreach(var item in ignoreList)
                        {
                            if(file.EndsWith(item))
                            {
                                skip = true;
                                break;
                            }
                        }
                    }
                    if (skip)
                        continue;
                    srcFiles.Remove(file);
                }
            }

            //
            public static List<string> SearchSpecifyFilesByExtension(ref List<string> srcFiles, string dir, string pattern)
            {
                List<string> pathNames = new List<string>();
                if (Directory.Exists(dir))
                {
                    pathNames = System.IO.Directory.GetFiles(dir, pattern, System.IO.SearchOption.AllDirectories)
                                            .Where(item => System.IO.Path.GetExtension(item) != ".meta")
                                            .Where(item => System.IO.Path.GetExtension(item) != ".DS_Store")
                                            .ToList();

                    // convert to standard unix path style.
                    var count = pathNames.Count;
                    for (int i = 0; i < count; i++)
                    {
                        pathNames[i] = pathNames[i].Replace('\\', '/');
                    }
                }
                return pathNames;
            }

#endregion

        }
    }
}