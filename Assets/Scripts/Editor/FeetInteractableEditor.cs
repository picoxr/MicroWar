#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MicroWar.HittableObject))]
public class FeetInteractableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        
        var feetInteractable = target as MicroWar.HittableObject;

        if (GUILayout.Button("Simulate Hit"))
        {
            feetInteractable.SimulateOnTriggerEnter();
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        base.OnInspectorGUI();
    }
}
#endif