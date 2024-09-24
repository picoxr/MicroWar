using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pico
{
    namespace Avatar
    {
        [CustomEditor(typeof(AvatarAnimator))]
        public class AvatarAnimatorEditor : Editor
        {
            AvatarAnimator avatarAnimator;
            bool _started = false;
            const string _editorScenePath = "Packages/org.byted.avatar.sdk/Editor/AnimationEditor/EditorResources/AvatarAnimationEditor.unity";
            const string _sceneName = "AvatarAnimationEditor";
            const string _avatarAppPrefabPath = "Packages/org.byted.avatar.sdk/Editor/AnimationEditor/EditorResources/AvatarAnimationEditor_PicoAvatarApp.prefab";

            Camera _camera;
            List<AvatarAnimationLayer> layerList = null;


            [CanEditMultipleObjects]
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                AvatarAnimator avatarAmimator = target as AvatarAnimator;
                //create new layer and add to animator
                if (GUILayout.Button("New Layer", GUILayout.Height(40)))
                {
                    AvatarAnimationLayerGraph layer = ScriptableObject.CreateInstance<AvatarAnimationLayerGraph>();

                    avatarAmimator.layers.Add(layer);
                    layer.name = "Avatar Animation Layer" + avatarAmimator.layers.Count;
                    layer.parentAnimator = avatarAmimator;
                    AssetDatabase.AddObjectToAsset(layer, avatarAmimator);

                    AssetDatabase.SaveAssets();
                    //use ImportAsset method refresh asset
                    AssetDatabase.Refresh();
                }

                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                base.OnInspectorGUI();

                GUILayout.Space(EditorGUIUtility.singleLineHeight);
                if (!EditorApplication.isPlaying && GUILayout.Button("Preview", GUILayout.Height(40)))
                {
                    LoadAnimatorPreviewScene();
                }

                if (avatarAmimator.waiting && Application.isPlaying)
                {
                    avatarAmimator.waiting = false;
                    if (!_started)
                    {
                        _started = true;
                        Start();
                    }
                }
                serializedObject.ApplyModifiedProperties();
            }

            //Load AvatarAnimatorEditor Scene
            public void LoadAnimatorPreviewScene()
            {
                if (EditorApplication.isPlaying)
                {
                    if (EditorSceneManager.GetActiveScene().name != _sceneName)
                    {
                        EditorUtility.DisplayDialog("Warning", "Please stop the current scene first", "OK");
                        return;
                    }
                }
                if (!EditorApplication.isPlaying)
                {
                    //if current scene doesn't modify or click Preview then open new scene
                    if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                    {
                        EditorSceneManager.OpenScene(_editorScenePath, OpenSceneMode.Single);
                        _started = false;
                        EditorApplication.EnterPlaymode();
                        (target as AvatarAnimator).waiting = true;
                    }
                }
            }

            void Start()
            {
                if (avatarAnimator == null)
                    avatarAnimator = target as AvatarAnimator;
                //create AvatarAnimatorSceneInit script
                AvatarAnimatorSceneInit sceneInit = new GameObject("InitObj").AddComponent<AvatarAnimatorSceneInit>();

                sceneInit.avatarApp = AssetDatabase.LoadMainAssetAtPath(_avatarAppPrefabPath) as GameObject;
                sceneInit.avatarAnimator = avatarAnimator;
            }

        }

    }
}
