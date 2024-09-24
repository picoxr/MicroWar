#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MicroWar.Avatar.AvatarManager))]
public class AvatarManagerEditor : Editor
{

    public override void OnInspectorGUI()
    {
        var avatarManager = target as MicroWar.Avatar.AvatarManager;

        if (GUILayout.Button("Load User Avatar"))
        {
            avatarManager.LoadUserAvatar(avatarManager.debugUserID);
        }

        if (GUILayout.Button("Load Avatar"))
        {
            avatarManager.LoadAvatar(avatarManager.debugUserID, null);
        }

        if (GUILayout.Button("Request User Avatars"))
        {
            avatarManager.RequestUserAvatars();
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        base.OnInspectorGUI();
    }
}
#endif