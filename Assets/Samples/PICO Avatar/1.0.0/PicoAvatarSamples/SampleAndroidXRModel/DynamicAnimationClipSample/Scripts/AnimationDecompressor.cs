using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace Pico.Avatar.Sample
{
    public class AnimationDecompressor
    {
        // m_relativePath base on Application.streamingAssetsPath
        private bool m_hasInitFish = false;
        private string m_relativeFolder = "";
        private string m_pcDebugStreamingPath = "";
        private string fileListFileName = "animation_group_list.txt";
        private Dictionary<string, List<string>> m_animGroup = new Dictionary<string, List<string>>();
        private string LocalPath
        {
            get { return Path.Combine(Application.persistentDataPath, m_relativeFolder); }
        }
        private string OrgPath
        {

            get
            {
                if (string.IsNullOrEmpty(m_pcDebugStreamingPath))
                    return Path.Combine(Application.streamingAssetsPath, m_relativeFolder);
                else
                    return Path.Combine(m_pcDebugStreamingPath, m_relativeFolder);
            }
        }

        public AnimationDecompressor(string floder, string pcTestPath = "", string fileListName = "")
        {
            m_relativeFolder = floder;
#if UNITY_EDITOR
            if (!string.IsNullOrEmpty(pcTestPath))
                m_pcDebugStreamingPath = pcTestPath;
#endif
            if (!string.IsNullOrEmpty(fileListName))
                fileListFileName = fileListName;
            m_hasInitFish = false;
        }
        public bool HasInitFish
        {
            get { return m_hasInitFish; }
        }

        public string GetGroupPath(string groupName)
        {
            if (!m_animGroup.ContainsKey(groupName))
                return null;
            return Path.Combine(Application.persistentDataPath, m_relativeFolder, groupName);
        }


        public IEnumerator StartDecompression()
        {
            string localAnimFileList = Path.Combine(LocalPath, fileListFileName);
            if (File.Exists(localAnimFileList))
            {
                Debug.Log("file exit :" + localAnimFileList);
                initFileList(localAnimFileList, null);
                m_hasInitFish = true;
                yield break;
            }
            string filrListURL = Path.Combine(OrgPath, fileListFileName);
#if UNITY_EDITOR
            WWW m_fileRequest = new WWW("file://" + filrListURL);
#else
            WWW m_fileRequest = new WWW(filrListURL);
#endif
            yield return m_fileRequest;

            if (!string.IsNullOrEmpty(m_fileRequest.error))
            {
                Debug.LogError(m_fileRequest.error + ":" + m_fileRequest.url);
            }
            else if (!m_fileRequest.isDone)
            {
                Debug.LogErrorFormat("DecompressionAnimation Support animation_file_list fail");
            }
            string waitWriteText = "";
            try
            {
                string directory = Path.GetDirectoryName(localAnimFileList);
                if (m_fileRequest != null)
                {
                    if (!Directory.Exists(directory))
                        Directory.CreateDirectory(directory);

                    waitWriteText = m_fileRequest.text;
                    Debug.LogFormat("txt content {0}", waitWriteText);

                    initFileList(localAnimFileList, m_fileRequest.text);
                }

            }
            catch (System.Exception ex)
            {
                Debug.LogErrorFormat("DecompressionAnimation Support fail,IO error {0}", ex.Message);
                m_hasInitFish = true;
                yield break;
            }
            m_fileRequest.Dispose();

            //copy asset to persistentDataPath
            foreach (var item in m_animGroup)
            {
                string folderName = item.Key;
                List<string> fileList = item.Value;

                string folderTargetFullPath = Path.Combine(LocalPath, folderName);
                string folderLocalFullPath = Path.Combine(OrgPath, folderName);
                if (!Directory.Exists(folderTargetFullPath))
                    Directory.CreateDirectory(folderTargetFullPath);

                for (int i = 0; i < fileList.Count; i++)
                {
                    string fileName = fileList[i];
                    string localFileFullPath = Path.Combine(folderLocalFullPath, fileName);
                    string targetFileFullPath = Path.Combine(folderTargetFullPath, fileName);
#if UNITY_EDITOR
                    WWW www = new WWW("file://" + localFileFullPath);
#else
                WWW www = new WWW(localFileFullPath);
#endif
                    yield return www;

                    if (!string.IsNullOrEmpty(www.error))
                    {
                        Debug.LogError(www.error + ":" + www.url);
                    }
                    else if (!www.isDone)
                    {
                        Debug.LogErrorFormat("DecompressionAnimation Support animation_file_list fail");
                    }

                    try
                    {
                        if (www != null)
                        {
                            File.WriteAllBytes(targetFileFullPath, www.bytes);
                            Debug.LogFormat("Copy Finish file:{0}", targetFileFullPath);
                        }

                    }
                    catch (System.Exception ex)
                    {
                        Debug.LogErrorFormat("WriteFile error {0}", ex.Message);
                    }
                }


            }

            if (!string.IsNullOrEmpty(waitWriteText))
                File.WriteAllText(localAnimFileList, waitWriteText);

            m_hasInitFish = true;

        }
        private void initFileList(string path, string content)
        {
            m_animGroup.Clear();

            if (content == null)
            {
                try
                {
                    content = File.ReadAllText(path);

                }
                catch (Exception ex)
                {
                    Debug.LogErrorFormat("initFileList:{0}", ex.Message);
                }
            }
            if (string.IsNullOrEmpty(content))
                return;
            var result = content.Split(',');
            foreach (var item in result)
            {
                string fileName = Path.GetFileName(item);
                string relativeDir = Path.GetDirectoryName(item);
                List<string> fileList = null;
                if (!m_animGroup.TryGetValue(relativeDir, out fileList))
                {
                    fileList = new List<string>();
                    fileList.Add(fileName);
                    m_animGroup.Add(relativeDir, fileList);
                }
                else
                    fileList.Add(fileName);
            }
        }

    }
}

