using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Pico
{
    namespace Avatar
    {
        public partial class AvatarAnimationEditor : EditorWindow
        {
            SplitterGUILayout _splitter;
            Vector2 _scrollLeft = Vector2.zero;
            Vector2 _scrollRight = Vector2.zero;
            RenderTexture _renderTexture;
    
            [MenuItem("AvatarSDK/Animation Editor", false, 1)]
            public static void ShowWindow()
            {
                GetWindow(typeof(AvatarAnimationEditor), true, "Avatar Animation Editor", true);
            }

            void OnGUI()
            {
                if (_splitter == null)
                {
                    _splitter = new SplitterGUILayout();
                }

                if (EditorApplication.isPlaying)
                {
                    if (EditorSceneManager.GetActiveScene().name != _sceneName)
                    {
                        EditorGUILayout.LabelField("Do not open Avatar Animation Editor in playing mode");
                        return;
                    }
                }
                if (!EditorApplication.isPlaying)
                {
                    EditorSceneManager.OpenScene(_editorScenePath, OpenSceneMode.Single);
                    _started = false;
                    EditorApplication.EnterPlaymode();
                }
                if (Application.isPlaying)
                {
                    if (!_started)
                    {
                        _started = true;
                        OnStart();
                    }
                }

                GUILayout.BeginHorizontal();
                _splitter.BeginHorizontalSplit();

                GUILayout.BeginVertical();
                OnLeftView();
                GUILayout.EndVertical();

                GUILayout.BeginVertical();
                OnRightView();
                GUILayout.EndVertical();

                _splitter.EndHorizontalSplit();
                GUILayout.EndHorizontal();

                const float sepWidth = 2;
                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.gray;
                GUI.Box(new Rect(_splitter.GetRealSizes()[0], 0, sepWidth, this.position.height), "");
                GUI.backgroundColor = backgroundColor;

                Repaint();
            }

            private bool firstLoad = false;
            private bool officialFemaleToggle = true;
            private bool officialMaleToggle = false;
            void OnLeftView()
            {
                _scrollLeft = EditorGUILayout.BeginScrollView(_scrollLeft);

                EditorGUILayout.LabelField("Avatar", EditorStyles.boldLabel);

                if (_avatarEnvReady && _loadAvatarCoroutine == null)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Toggle(officialFemaleToggle, "OS Avatar-Female") && (!officialFemaleToggle || !firstLoad))
                    {
                        firstLoad = true;
                        officialFemaleToggle = true;
                        officialMaleToggle = false;
                        if (_loadAvatarCoroutine == null)
                        {
                            if (_avatar)
                            {
                                UnloadAvatar();
                            }

                            _loadAvatarCoroutine = LoadOfficialAvatar(AvatarSexType.Female);
                            _runtimeBehaviour.StartCoroutine(_loadAvatarCoroutine);
                        }
                    }

                    if (GUILayout.Toggle(officialMaleToggle, "OS Avatar-Male") && !officialMaleToggle)
                    {
                        officialMaleToggle = true;
                        officialFemaleToggle = false;
                        if (_loadAvatarCoroutine == null)
                        {
                            if (_avatar)
                            {
                                UnloadAvatar();
                            }

                            _loadAvatarCoroutine = LoadOfficialAvatar(AvatarSexType.Male);
                            _runtimeBehaviour.StartCoroutine(_loadAvatarCoroutine);
                        }
                    }
                    EditorGUILayout.EndHorizontal();
                    EditorGUILayout.Space();
                    EditorGUILayout.Space();
                }

                // rotation
                if (_avatar)
                {
                    float rotation = Slider("Rotation", _rotation, 0, 360);
                    if (_rotation != rotation)
                    {
                        _rotation = rotation;
                        _avatar.transform.localRotation = Quaternion.Euler(0, _rotation, 0);
                    }

                    //_drawJoints = Toggle("Draw Joints", _drawJoints);
                }

                // animation
                if (_avatar)
                {
                    Separator();
                    EditorGUILayout.LabelField("Animation", EditorStyles.boldLabel);

                    bool doPlayAnimation = false;
                    bool doStopAnimation = false;
                    bool playAnimation = Toggle("Play Animation", _playAnimation);
                    if (_playAnimation != playAnimation)
                    {
                        _playAnimation = playAnimation;

                        if (_playAnimation)
                        {
                            doPlayAnimation = true;
                        }
                        else
                        {
                            doStopAnimation = true;
                        }
                    }
                    if (_playAnimation)
                    {
                        int index = Popup("Animation", _playAnimationIndex, _animations.ToArray());
                        if (_playAnimationIndex != index)
                        {
                            _playAnimationIndex = index;
                            doPlayAnimation = true;
                        }

                        var target = _runtimeBehaviour.target;
                        EditorGUILayout.PropertyField(_runtimeBehaviourObject.FindProperty("target"));
                        _runtimeBehaviourObject.ApplyModifiedProperties();
                        if (_runtimeBehaviour.target != target)
                        {
                            var modelPath = AssetDatabase.GetAssetPath(_runtimeBehaviour.target);
                            var assets = AssetDatabase.LoadAllAssetRepresentationsAtPath(modelPath);
                            List<AnimationClip> clips = new List<AnimationClip>();
                            for (int i = 0; i < assets.Length; ++i)
                            {
                                if (assets[i] is AnimationClip)
                                {
                                    clips.Add(assets[i] as AnimationClip);
                                }
                            }
                            _runtimeBehaviour.clips = clips.ToArray();
                            _runtimeBehaviourObject.Update();
                        }
                        EditorGUILayout.PropertyField(_runtimeBehaviourObject.FindProperty("clips"));
                        _runtimeBehaviourObject.ApplyModifiedProperties();

                        if (GUILayout.Button("Import"))
                        {
                            ImportClips();
                        }

                        //if (GUILayout.Button("Test Load Animaz"))
                        //{
                        //    TestLoadAnimaz();
                        //}
                    }
                    if (doPlayAnimation)
                    {
                        //_avatar.PlayAnimation(_animations[_playAnimationIndex]);
                        PlayAnimationByName(_animations[_playAnimationIndex]);
                    }
                    if (doStopAnimation)
                    {
                        //_avatar.StopAnimation();
                        StopAnimation();
                    }
                }

                EditorGUILayout.EndScrollView();
            }

            void OnRightView()
            {
                _scrollRight = EditorGUILayout.BeginScrollView(_scrollRight);

                if (_renderTexture)
                {
                    var textureRect = GUILayoutUtility.GetAspectRect(_renderTexture.width / (float)_renderTexture.height);
                    EditorGUI.DrawPreviewTexture(textureRect, _renderTexture);
                }

                EditorGUILayout.EndScrollView();
            }

            const int _labelWidth = 100;

            string TextField(string label, string text)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
                string result = EditorGUILayout.TextField(text);
                EditorGUILayout.EndHorizontal();
                return result;
            }

            void LabelField(string label, string text)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
                EditorGUILayout.LabelField(text);
                EditorGUILayout.EndHorizontal();
            }

            bool Toggle(string label, bool value)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
                bool result = EditorGUILayout.Toggle(value);
                EditorGUILayout.EndHorizontal();
                return result;
            }

            int Popup(string label, int select, string[] items)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
                int result = EditorGUILayout.Popup(select, items);
                EditorGUILayout.EndHorizontal();
                return result;
            }

            float Slider(string label, float value, float left, float right)
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(label, GUILayout.Width(_labelWidth));
                float result = EditorGUILayout.Slider(value, left, right);
                EditorGUILayout.EndHorizontal();
                return result;
            }

            void Separator()
            {
                const int height = 1;
                var rect = GUILayoutUtility.GetRect(new GUIContent(""), GUIStyle.none);
                rect.y += rect.height / 2 - height / 2;
                rect.height = height;
                var backgroundColor = GUI.backgroundColor;
                GUI.backgroundColor = Color.gray;
                GUI.Box(rect, "");
                GUI.backgroundColor = backgroundColor;
            }
        }
    }
}
