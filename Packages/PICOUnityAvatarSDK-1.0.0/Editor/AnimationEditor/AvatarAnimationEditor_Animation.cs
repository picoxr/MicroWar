using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;

namespace Pico
{
    namespace Avatar
    {
        public partial class AvatarAnimationEditor : EditorWindow
        {
            bool _playAnimation = false;
            int _playAnimationIndex = 0;
            List<string> _animations;
            bool _isLoadingAnimationsExtern = false;
            LinkedList<string> _animationsExternToLoad = null;
            IEnumerator _loadAnimationsExternCoroutine = null;

            void OnLoadAnimationsExternComplete(string assetBundlePath, string animationNamesJson)
            {
                if (_avatar)
                {
                    _animations = ParseAnimations(_avatar.GetAnimationNames());
                }
                _isLoadingAnimationsExtern = false;
            }

            List<string> ParseAnimations(string animations)
            {
                List<string> result = null;
                var anims = JsonConvert.DeserializeObject<List<object>>(animations);
        
                if (anims != null)
                {
                    result = new List<string>();
                    for (int i = 0; i < anims.Count; ++i)
                    {
                        result.Add(anims[i].ToString());
                    }
                }
                return result;
            }

            void ImportClips()
            {
                if (_runtimeBehaviour.target == null || _runtimeBehaviour.clips == null)
                {
                    return;
                }

                string outDir = Application.dataPath + "/../animaz";
                if (!Directory.Exists(outDir))
                {
                    Directory.CreateDirectory(outDir);
                }

                Transform target = _runtimeBehaviour.target;

                var clipNames = AnimationConverter.ConvertClipsToAnimaz(target, _runtimeBehaviour.clips, null, outDir);

                {
                    string zipPath = outDir + "/" + target.name + ".zip";
                    if (File.Exists(zipPath))
                    {
                        File.Delete(zipPath);
                    }

                    using (ZipArchive archive = new ZipArchive(File.OpenWrite(zipPath), ZipArchiveMode.Create))
                    {
                        for (int i = 0; i < clipNames.Length; ++i)
                        {
                            var name = clipNames[i];
                            var animazPath = outDir + "/" + target.name + "." + name + ".animaz";
                            if (File.Exists(animazPath))
                            {
                                ZipArchiveEntry entry = archive.CreateEntry(target.name + "." + name + ".animaz");
                                using (BinaryWriter writer = new BinaryWriter(entry.Open()))
                                {
                                    writer.Write(File.ReadAllBytes(animazPath));
                                    writer.Close();
                                }
                            }
                        }
                    }

                    if (_animationsExternToLoad == null)
                    {
                        _animationsExternToLoad = new LinkedList<string>();
                    }
                    _animationsExternToLoad.AddLast(zipPath);

                    if (_loadAnimationsExternCoroutine == null)
                    {
                        _loadAnimationsExternCoroutine = LoadAnimationsExtern();
                        _runtimeBehaviour.StartCoroutine(_loadAnimationsExternCoroutine);
                    }
                }
            }

            IEnumerator LoadAnimationsExtern()
            {
                while (_animationsExternToLoad.Count > 0)
                {
                    if (!_isLoadingAnimationsExtern)
                    {
                        _isLoadingAnimationsExtern = true;
                        string zipPath = _animationsExternToLoad.First.Value;
                        _animationsExternToLoad.RemoveFirst();

                        StringBuilder sb = new StringBuilder();
                        sb.Append("[");

                        using (ZipArchive archive = new ZipArchive(File.OpenRead(zipPath), ZipArchiveMode.Read))
                        {
                            int entryCount = 0;
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (entryCount > 0)
                                {
                                    sb.Append(",");
                                }
                                sb.Append("\"");
                                sb.Append(entry.FullName);
                                sb.Append("\"");

                                entryCount += 1;
                            }
                        }

                        sb.Append("]");
                        string animationPathsJson = sb.ToString();
                        // _avatar.LoadAnimationsExtern(zipPath, animationPathsJson);
                        Pico.Avatar.AvatarAssetBundle ab = new Pico.Avatar.AvatarAssetBundle();
                        ab.LoadFromZipFile(zipPath);
                        _avatar.LoadAnimationsFromAssetBundle(ab, animationPathsJson);
                    }
                    yield return null;
                }

                _loadAnimationsExternCoroutine = null;
                yield return null;
            }

            void RemoveAnimationExtern(string anim)
            {
                _avatar.RemoveAnimation(anim);
                _animations = ParseAnimations(_avatar.GetAnimationNames());
            }

            void TestLoadAnimaz()
            {
                if (_animationsExternToLoad == null)
                {
                    _animationsExternToLoad = new LinkedList<string>();
                }
                _animationsExternToLoad.AddLast(@"D:\Project\avatar_sdk_unity\animaz\eyeBlink_out.zip");

                if (_loadAnimationsExternCoroutine == null)
                {
                    _loadAnimationsExternCoroutine = LoadAnimationsExtern();
                    _runtimeBehaviour.StartCoroutine(_loadAnimationsExternCoroutine);
                }
            }
        }
    }
}
