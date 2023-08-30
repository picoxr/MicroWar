#if UNITY_EDITOR

using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

[CustomEditor(typeof(MicroWar.GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        var gameManager = target as MicroWar.GameManager;

        if (GUILayout.Button("Start Game"))
        {
            gameManager.StartSession(MicroWar.SessionType.SinglePlayer);
        }

        if (GUILayout.Button("Reload"))
        {
            SceneManager.LoadScene("Main");
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        base.OnInspectorGUI();
    }
}
#endif