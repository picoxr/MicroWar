using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Pico
{
    namespace Avatar
    {
        public static class EditorGUIHelper
        {
            static EditorGUIHelper()
            {
                //GUISkin skin = GUI.skin;

                splitter = new GUIStyle();
                splitter.normal.background = EditorGUIUtility.whiteTexture;
                splitter.stretchWidth = true;
                splitter.margin = new RectOffset(0, 0, 7, 7);
            }

            #region Splitter

            private static readonly GUIStyle splitter;
            private static readonly Color splitterColor = EditorGUIUtility.isProSkin ? new Color(0.157f, 0.157f, 0.157f) : new Color(0.5f, 0.5f, 0.5f);

            // GUILayout Style
            public static void Splitter(Color rgb, float thickness = 1)
            {
                Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitter, GUILayout.Height(thickness));

                if (Event.current.type == EventType.Repaint)
                {
                    Color restoreColor = GUI.color;
                    GUI.color = rgb;
                    splitter.Draw(position, false, false, false, false);
                    GUI.color = restoreColor;
                }
            }

            public static void Splitter(float thickness, GUIStyle splitterStyle)
            {
                Rect position = GUILayoutUtility.GetRect(GUIContent.none, splitterStyle, GUILayout.Height(thickness));

                if (Event.current.type == EventType.Repaint)
                {
                    Color restoreColor = GUI.color;
                    GUI.color = splitterColor;
                    splitterStyle.Draw(position, false, false, false, false);
                    GUI.color = restoreColor;
                }
            }

            public static void Splitter(float thickness = 1)
            {
                Splitter(thickness, splitter);
            }

            // GUI Style
            public static void Splitter(Rect position)
            {
                if (Event.current.type == EventType.Repaint)
                {
                    Color restoreColor = GUI.color;
                    GUI.color = splitterColor;
                    splitter.Draw(position, false, false, false, false);
                    GUI.color = restoreColor;
                }
            }
            #endregion


            #region Quaternion

            /** Draw quaternion ui.*/
            public static bool QuaternionField(string fieldName, ref Quaternion quat)
            {
                Vector4 v = new Vector4(quat.x, quat.y, quat.z, quat.w);

                EditorGUI.BeginChangeCheck();
                v = EditorGUILayout.Vector4Field(fieldName, v);
                if (EditorGUI.EndChangeCheck())
                {
                    quat.x = v.x;
                    quat.y = v.y;
                    quat.z = v.z;
                    quat.w = v.w;
                    return true;
                }
                return false;
            }
            #endregion

            #region DrawCurveLine

            class LightSpecularItem
            {
                public AnimationCurve curve;
                public int lineId;
                public float specLevel;
                public float glossiness;
                public float soften;
            }

            private static Dictionary<int, LightSpecularItem> _SpecularItems = new Dictionary<int, LightSpecularItem>();
            public static void DrawLightSpecular(int lineId, float specLevel, float glossiness, float soften)
            {
                LightSpecularItem specularItem = null;
                if (!_SpecularItems.TryGetValue(lineId, out specularItem))
                {
                    specularItem = new LightSpecularItem();
                    specularItem.lineId = lineId;
                    specularItem.specLevel = -1;
                    specularItem.glossiness = -1.0f;
                    specularItem.soften = -1.0f;
                    specularItem.curve = new AnimationCurve();
                }

                if (specularItem.specLevel != specLevel
                    || specularItem.glossiness != glossiness
                    || specularItem.soften != soften)
                {
                    specularItem.specLevel = specLevel;
                    specularItem.glossiness = glossiness;
                    specularItem.soften = soften;

                    //
                    int sampleCount = 41;
                    int halfSampleCount = sampleCount >> 1;
                    float xStep = 1.0f / halfSampleCount;
                    var keys = new Keyframe[sampleCount + 2];

                    for (int i = 0; i < sampleCount; ++i)
                    {
                        // dot(normal, (viewDir + lightDir))
                        float curNH = 1.0f;
                        if (i <= halfSampleCount)
                        {
                            curNH = xStep * i;
                        }
                        else if (i >= (halfSampleCount + 1))
                        {
                            curNH = (sampleCount - 1 - i) * xStep;
                        }

                        curNH = Mathf.Pow(curNH, 0.1f);
                        float specVal = specLevel * Mathf.Pow(curNH, (glossiness * 100.0f + 0.01f));

                        keys[i] = new Keyframe((i * xStep), specVal);
                    }
                    keys[sampleCount] = new Keyframe(2.01f, 1.0f, 0.0f, 90.0f);
                    keys[sampleCount + 1] = new Keyframe(2.01f, 1.0f, 90.0f, 0.0f);
                    specularItem.curve.keys = keys;

                    for (int i = 0; i < sampleCount; ++i)
                    {
                        specularItem.curve.SmoothTangents(i, 1.0f);
                    }
                }

                EditorGUILayout.CurveField(specularItem.curve, GUILayout.Height(50.0f), GUILayout.Width(50.0f));
            }

            //public static bool DrawArray<T>(EditorArrayItemInspector<T> itemInspector
            //    , ref T[] array, ref bool foldout, string title, string elementTitle, bool allow_scene_object = true)
            //{
            //    bool isChanged = false;

            //    foldout = EditorGUILayout.Foldout(foldout, title);
            //    if (!foldout)
            //    {
            //        return isChanged;
            //    }

            //    int lastCount = array == null ? 0 : array.Length;
            //    //
            //    var newCount = EditorGUILayout.IntField("Count", lastCount);
            //    if (newCount != lastCount)
            //    {
            //        if (array == null)
            //        {
            //            array = new T[newCount];
            //        }
            //        else
            //        {
            //            System.Array.Resize<T>(ref array, newCount);
            //        }
            //        //
            //        isChanged = true;
            //    }

            //    for (int i = 0; i < newCount; ++i)
            //    {

            //        EditorGUIHelper.Splitter(1);
            //        EditorGUILayout.BeginHorizontal();
            //        EditorGUILayout.PrefixLabel(elementTitle);
            //        // remove
            //        if (GUILayout.Button("-", GUILayout.Width(20f)))
            //        {
            //            // 把后续的往前移.
            //            for (int j = i; j < newCount - 1; ++j)
            //            {
            //                array[j] = array[j + 1];
            //            }

            //            System.Array.Resize<T>(ref array, newCount - 1);
            //            return true;
            //        }

            //        if (GUILayout.Button("+", GUILayout.Width(20f)))
            //        {
            //            System.Array.Resize<T>(ref array, newCount + 1);

            //            // 把当前往后移.
            //            for (int j = newCount; j > i; --j)
            //            {
            //                array[j] = array[j - 1];
            //            }
            //            array[i] = itemInspector.newInstance();
            //            return true;
            //        }
            //        EditorGUILayout.EndHorizontal();
            //        //
            //        if (itemInspector.DrawGUI(ref array, i))
            //        {
            //            isChanged = true;
            //        }
            //    }
            //    EditorGUIHelper.Splitter(1);

            //    return isChanged;
            //}

            public static bool DrawObjectArray<T>(ref T[] array, ref bool foldout, string title, string elementTitle, bool allow_scene_object = true) where T : UnityEngine.Object
            {
                bool isChanged = false;

                foldout = EditorGUILayout.Foldout(foldout, title);
                if (!foldout)
                {
                    return isChanged;
                }

                int lastCount = array == null ? 0 : array.Length;
                //
                var newCount = EditorGUILayout.IntField("Count", lastCount);
                if (newCount != lastCount)
                {
                    if (array == null)
                    {
                        array = new T[newCount];
                    }
                    else
                    {
                        System.Array.Resize<T>(ref array, newCount);
                    }
                    //
                    isChanged = true;
                }

                for (int i = 0; i < newCount; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    //
                    EditorGUI.BeginChangeCheck();
                    array[i] = (T)EditorGUILayout.ObjectField(elementTitle, array[i], typeof(T), allow_scene_object);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isChanged = true;
                    }

                    // remove
                    if (GUILayout.Button("-", GUILayout.Width(20f)))
                    {
                        // 把后续的往前移.
                        for (int j = i; j < newCount - 1; ++j)
                        {
                            array[j] = array[j + 1];
                        }

                        System.Array.Resize<T>(ref array, newCount - 1);
                        return true;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(20f)))
                    {
                        System.Array.Resize<T>(ref array, newCount + 1);

                        // 把当前往后移.
                        for (int j = newCount; j > i; --j)
                        {
                            array[j] = array[j - 1];
                        }
                        array[i] = null;
                        return true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                return isChanged;
            }
            public static bool DrawSimpleArray<T>(ref T[] array, ref bool foldout, string title, string elementTitle)
            {
                bool isChanged = false;

                foldout = EditorGUILayout.Foldout(foldout, title);
                if (!foldout)
                {
                    return isChanged;
                }

                var curCount = array == null ? 0 : array.Length;
                var newCount = EditorGUILayout.IntField("Count", curCount);
                if (newCount != curCount)
                {
                    if (array == null)
                    {
                        array = new T[newCount];
                    }
                    else
                    {
                        System.Array.Resize<T>(ref array, newCount);
                    }
                    //
                    isChanged = true;
                }

                for (int i = 0; i < newCount; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    //
                    if (DrawPrimitiveField(elementTitle, array, i, typeof(T)))
                    {
                        isChanged = true;
                    }

                    // remove
                    if (GUILayout.Button("-", GUILayout.Width(20f)))
                    {
                        // 把后续的往前移.
                        for (int j = i; j < newCount - 1; ++j)
                        {
                            array[j] = array[j + 1];
                        }

                        System.Array.Resize<T>(ref array, newCount - 1);
                        return true;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(20f)))
                    {
                        System.Array.Resize<T>(ref array, newCount + 1);

                        // 把当前往后移.
                        for (int j = newCount; j > i; --j)
                        {
                            array[j] = array[j - 1];
                        }
                        array[i] = default(T);
                        return true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                return isChanged;
            }
            private static bool DrawPrimitiveField(string elementTitle, System.Array arrayData, int index, System.Type elementType)
            {
                EditorGUI.BeginChangeCheck();
                if (elementType == typeof(Color32))
                {
                    var array = arrayData as Color32[];
                    array[index] = (Color32)EditorGUILayout.ColorField(elementTitle, (Color)array[index]);
                }
                else if (elementType == typeof(Color))
                {
                    var array = arrayData as Color[];
                    array[index] = EditorGUILayout.ColorField(elementTitle, array[index]);
                }
                else if (elementType == typeof(Vector4))
                {
                    var array = arrayData as Vector4[];
                    array[index] = EditorGUILayout.ColorField(elementTitle, array[index]);
                }
                else if (elementType == typeof(float))
                {
                    var array = arrayData as float[];
                    array[index] = EditorGUILayout.FloatField(elementTitle, array[index]);
                }
                else if (elementType == typeof(int))
                {
                    var array = arrayData as int[];
                    array[index] = EditorGUILayout.IntField(elementTitle, array[index]);
                }
                //
                return EditorGUI.EndChangeCheck();
            }

            public static bool DrawAnimationCurveArray(ref AnimationCurve[] array, ref bool foldout, string title)
            {
                bool isChanged = false;

                foldout = EditorGUILayout.Foldout(foldout, title);
                if (!foldout)
                {
                    return isChanged;
                }

                int lastCount = array == null ? 0 : array.Length;
                //
                var newCount = EditorGUILayout.IntField("Count", lastCount);
                if (newCount != lastCount)
                {
                    if (array == null)
                    {
                        array = new AnimationCurve[newCount];
                    }
                    else
                    {
                        System.Array.Resize<AnimationCurve>(ref array, newCount);
                    }

                    for (int i = 0; i < newCount; ++i)
                    {
                        if (array[i] == null)
                        {
                            array[i] = CreateAnimationCurve();
                        }
                    }
                    //
                    isChanged = true;
                }

                for (int i = 0; i < newCount; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    //
                    EditorGUI.BeginChangeCheck();
                    array[i] = EditorGUILayout.CurveField("curve", array[i]);
                    if (EditorGUI.EndChangeCheck())
                    {
                        isChanged = true;
                    }

                    // remove
                    if (GUILayout.Button("-", GUILayout.Width(20f)))
                    {
                        // 把后续的往前移.
                        for (int j = i; j < newCount - 1; ++j)
                        {
                            array[j] = array[j + 1];
                        }

                        System.Array.Resize<AnimationCurve>(ref array, newCount - 1);
                        return true;
                    }

                    if (GUILayout.Button("+", GUILayout.Width(20f)))
                    {
                        System.Array.Resize<AnimationCurve>(ref array, newCount + 1);

                        // 把当前往后移.
                        for (int j = newCount; j > i; --j)
                        {
                            array[j] = array[j - 1];
                        }
                        //
                        array[i] = CreateAnimationCurve();
                        return true;
                    }

                    EditorGUILayout.EndHorizontal();
                }

                return isChanged;
            }

            /** Create a default AnimationCurve.*/
            public static AnimationCurve CreateAnimationCurve()
            {
                Keyframe[] ks = new Keyframe[2];

                ks[0].inTangent = ks[0].outTangent = 0;
                ks[0].time = 0f;
                ks[0].value = 1.0f;

                ks[1].inTangent = ks[1].outTangent = 0;
                ks[1].time = 1f;
                ks[1].value = 1f;

                return new AnimationCurve(ks);
            }
            #endregion

            #region Draw Sex
            public static bool DrawSex(ref AvatarSexType val)
            {
                //platform
                EditorGUI.BeginChangeCheck();
                val = (AvatarSexType)EditorGUILayout.Popup("Sex", (int)val, _SexNames);
                if (EditorGUI.EndChangeCheck())
                {
                    return true;
                }
                //
                return false;
            }

            //
            private static string[] _SexNames = new string[]
            {
                "女性",    // Femal = 0
                 "男性",    // Male = 1
                 "其他",   
            };
            #endregion


            #region Draw app debug.
            /**
             * Pause/NextFrame/etc.
             */ 
            public static void DrawAppPause()
            {
                if (Application.isPlaying &&
                    Pico.Avatar.PicoAvatarApp.instance != null &&
                    PicoAvatarManager.instance != null &&
                    PicoAvatarManager.instance.isReady
                    )
                {
                    Splitter(2);

                    if(!Pico.Avatar.PicoAvatarApp.instance.localDebugSettings.enableDebugPauseResume)
                    {
                        if (GUILayout.Button("Enable Pause"))
                        {
                            Pico.Avatar.PicoAvatarApp.instance.localDebugSettings.enableDebugPauseResume = true;
                        }
                    }

                    //
                    if (Pico.Avatar.PicoAvatarApp.instance.localDebugSettings.enableDebugPauseResume)
                    {
                        //
                        // if (Pico.Avatar.PicoAvatarApp.instance.DebugIsPaused())
                        // {
                        //     if (GUILayout.Button("Resume"))
                        //     {
                        //         Pico.Avatar.PicoAvatarApp.instance.DebugPause();
                        //     }
                        // }
                        // else
                        // {
                        //     if (GUILayout.Button("Pause"))
                        //     {
                        //         Pico.Avatar.PicoAvatarApp.instance.DebugPause();
                        //     }
                        // }
                        //
                        // EditorGUILayout.Separator();
                        //
                        // //
                        // if (GUILayout.Button("Next frame"))
                        // {
                        //     Pico.Avatar.PicoAvatarApp.instance.DebugNextFrame();
                        // }
                    }

                    Splitter(2);
                }
            }

            #endregion

        }
    }
}