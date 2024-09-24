using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Pico.Avatar.Sample
{
#if UNITY_EDITOR
    public class SceneDescriptionUtil
    {
        private static SceneDescription _sceneDescConfig;
        
        public static string GetDescription(string sceneName)
        {
            if (_sceneDescConfig == null)
                _sceneDescConfig = AssetDatabase.LoadAssetAtPath<SceneDescription>
                    ("Assets/PicoAvatarSamples/SampleAndroidXRModel/MenuEntry/SceneDescription.asset");
            if (_sceneDescConfig == null)
                return string.Empty;
            foreach (var sceneDesc in _sceneDescConfig.sceneDescGroup)
            {
                if (sceneDesc.sceneName.Equals(sceneName))
                    return sceneDesc.sceneDesc;
            }
            return string.Empty;
        }
    }
#endif
    

    [Serializable]
    public class SceneDescriptionGroup
    {
        public string sceneName;
        public string sceneDesc;
    }
    
    [CreateAssetMenu(menuName = "Scriptable Object/SceneDescConfig",order = 2)]
    public class SceneDescription : ScriptableObject
    {
        public List<SceneDescriptionGroup> sceneDescGroup = new List<SceneDescriptionGroup>();
    }
}