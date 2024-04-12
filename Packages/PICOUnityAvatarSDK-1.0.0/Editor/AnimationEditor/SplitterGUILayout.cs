using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SplitterGUILayout
{
    const int kHierarchyMinWidth = 300;
    System.Reflection.Assembly _assembly;
    System.Type _splitterGUILayoutType;
    System.Type _splitterStateType;
    object _splitterState;
    System.Reflection.MethodInfo _beginHorizontalSplit;
    System.Reflection.MethodInfo _endHorizontalSplit;
#if UNITY_2020_1_OR_NEWER
    float[] _realSizes;
#else
    int[] _realSizes;
#endif

    public SplitterGUILayout()
    {
        _assembly = System.Reflection.Assembly.GetAssembly(typeof(EditorGUILayout));
        _splitterGUILayoutType = _assembly.GetType("UnityEditor.SplitterGUILayout");
        _splitterStateType = _assembly.GetType("UnityEditor.SplitterState");
        _beginHorizontalSplit = _splitterGUILayoutType.GetMethod("BeginHorizontalSplit", new System.Type[] { _splitterStateType, typeof(GUILayoutOption[]) });
        _endHorizontalSplit = _splitterGUILayoutType.GetMethod("EndHorizontalSplit");
#if UNITY_2020_1_OR_NEWER
        var FromRelative = _splitterStateType.GetMethod("FromRelative", new System.Type[] { typeof(float[]), typeof(float[]), typeof(float[]) });
        _splitterState = FromRelative.Invoke(null, new object[] { new float[] { kHierarchyMinWidth, kHierarchyMinWidth * 3 }, new float[] { kHierarchyMinWidth, kHierarchyMinWidth }, null });
        var realSizes = _splitterStateType.GetField("realSizes");
        _realSizes = (float[]) realSizes.GetValue(_splitterState);
#else
        var Constructor = _splitterStateType.GetConstructor(new System.Type[] { typeof(float[]), typeof(int[]), typeof(int[]) });
        _splitterState = Constructor.Invoke(new object[] { new float[] { kHierarchyMinWidth, kHierarchyMinWidth * 3 }, new int[] { kHierarchyMinWidth, kHierarchyMinWidth }, null });
        var realSizes = _splitterStateType.GetField("realSizes");
        _realSizes = (int[]) realSizes.GetValue(_splitterState);
#endif
    }

    public void BeginHorizontalSplit()
    {
        _beginHorizontalSplit.Invoke(null, new object[] { _splitterState, null });
    }

    public void EndHorizontalSplit()
    {
        _endHorizontalSplit.Invoke(null, null);
    }

    public float[] GetRealSizes()
    {
        var result = new float[_realSizes.Length];
        for (int i = 0; i < result.Length; ++i)
        {
            result[i] = _realSizes[i];
        }
        return result;
    }
}
