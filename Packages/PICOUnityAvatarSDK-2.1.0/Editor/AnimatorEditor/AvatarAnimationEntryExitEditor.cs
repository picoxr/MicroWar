using UnityEditor;
using Pico.Avatar.XNode;

namespace Pico
{
    namespace Avatar
    {
        [CustomEditor(typeof(AvatarAnimationEntry))]
        public class AvatarAnimationEntryEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Node.layer)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Node.position)));
                EditorGUILayout.EndVertical();
                serializedObject.ApplyModifiedProperties();
            }
        }

        [CustomEditor(typeof(AvatarAnimationExit))]
        public class AvatarAnimationExitEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                EditorGUILayout.BeginVertical();
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Node.layer)));
                EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(Node.position)));
                EditorGUILayout.EndVertical();
                serializedObject.ApplyModifiedProperties();
            }
        }
    }
}
