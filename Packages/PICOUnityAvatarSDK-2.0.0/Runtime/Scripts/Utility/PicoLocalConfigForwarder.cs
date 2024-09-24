using UnityEngine;
using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.ComponentModel;
using static UnityEngine.GraphicsBuffer;

namespace Pico
{
    namespace Avatar
    {
        /// <summary>
        /// Config forwarder for MonoBehavour. load local config as json file and apply properties to targets.
        /// </summary>
        public class PicoLocalConfigForwarder : MonoBehaviour
        {
            // Start is called before the first frame update
            void Start()
            {
                var dirPath = AvatarEnv.avatarCachePath + "/QA";
                try
                {
                    var files = System.IO.Directory.GetFiles(dirPath, this.gameObject.scene.name + "*.json");
                    if (files != null && files.Length > 0)
                    {
                        foreach (var x in files)
                        {
                            if (!string.IsNullOrEmpty(x) && System.IO.File.Exists(x))
                            {
                                try
                                {
                                    var configContent = System.IO.File.ReadAllText(x);
                                    if (configContent != null)
                                    {
                                        var cmp = GetTargetComponentFromConfigFileNamePath(x);
                                        //configContent
                                        if (cmp != null)
                                        {
                                            JsonUtility.FromJsonOverwrite(configContent, cmp);
                                            AvatarEnv.Log(DebugLogMask.GeneralInfo, String.Format("configuration applied from {0}", x));
                                        }
                                        else
                                        {
                                            UnityEngine.Debug.LogError(string.Format("Failed to find component with config file name: {0} ", x));
                                        }

                                    }
                                }
                                catch (System.Exception e)
                                {
                                    UnityEngine.Debug.LogError(string.Format("found error: {0} in config file of {1}.", e.Message, x));
                                }
                            }
                        }
                    }
                }catch(System.Exception e)
                {
                    // do nothing
                }

            }

            /// <summary>
            /// Gets local config file path of the target. usually localed in "${AvatarCache}/QA/Transform1.Transform2.componentName.json".
            /// </summary>
            private UnityEngine.Component GetTargetComponentFromConfigFileNamePath(string configFileName)
            {
                //target.gameObject
                configFileName = configFileName.Replace('\\', '/');
                configFileName = configFileName.Substring(configFileName.LastIndexOf('/') + 1);
                var transNames = configFileName.Split('.');
                if(transNames == null || transNames.Length < 3)
                {
                    return null;
                }

                int transCount = transNames.Length - 2;
                string transPath = "";
                for (int i = 1; i < transCount; ++i)
                {
                    if(i > 1)
                    {
                        transPath += "/";
                    }
                    transPath += transNames[i];
                }
                var cmpGO = GameObject.Find(transPath);
                var componentTypeName = transNames[transCount];
                if (cmpGO != null)
                {
                    var cmps = cmpGO.GetComponents(typeof(UnityEngine.Component));
                    foreach(var cmp in cmps)
                    {
                        if(cmp.GetType().Name == componentTypeName)
                        {
                            return cmp;
                        }
                    }
                }
                return null;
            }
        }
    }
}