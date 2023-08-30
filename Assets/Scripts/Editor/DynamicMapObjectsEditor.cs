#if UNITY_EDITOR

using MicroWar;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(DynamicMapObjectsManager))]
public class DynamicMapObjects : Editor
{
    public override void OnInspectorGUI()
    {
        
        var dynamicMapObjManager = target as DynamicMapObjectsManager;

        if (GUILayout.Button("Spawn Crate"))
        {
            dynamicMapObjManager.SpawnCrate(UnityEngine.Random.value > 0.5f ? CrateType.Health : CrateType.Shield);
        }

        if (GUILayout.Button("Spawn Turret Activator"))
        {
            dynamicMapObjManager.SpawnTurretActivator();
        }

        EditorGUILayout.Space(EditorGUIUtility.singleLineHeight);

        base.OnInspectorGUI();
    }
}
#endif