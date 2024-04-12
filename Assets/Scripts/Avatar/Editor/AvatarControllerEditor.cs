#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MicroWar.Avatar.AvatarController))]
public class AvatarControllerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var avatar = target as MicroWar.Avatar.AvatarController;

        if (GUILayout.Button("Draw Outline"))
        {
            avatar.EnableOutline();
        }

        if (GUILayout.Button("Remove Outline"))
        {
            avatar.DisableOutline();
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        base.OnInspectorGUI();
    }
}
#endif