using UnityEditor;
using UnityEngine;

namespace Pico.Avatar.XNodeEditor
{
    /// <summary> Override graph inspector to show an 'Open Graph' button at the top </summary>
    [CustomEditor(typeof(Pico.Avatar.XNode.NodeGraph), true)]
#if ODIN_INSPECTOR_DISCARD
    public class GlobalGraphEditor : OdinEditor {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Edit Layer", GUILayout.Height(40))) {
                NodeEditorWindow.Open(serializedObject.targetObject as Pico.Avatar.XNode.NodeGraph);
            }
            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (GUILayout.Button("Delete Layer", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Delete Layer", "Deleting this Layer will delete all Animation State it contains.\nAre you sure you want to delete Layer?", "Delete", "Cancel"))
                {
                    AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                    DeleteLayer(layer);
                    return;
                }
            }
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            base.OnInspectorGUI();
        }
    }
#else
    [CanEditMultipleObjects]
    public class GlobalGraphEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Edit Layer", GUILayout.Height(40)))
            {
                NodeEditorWindow.Open(serializedObject.targetObject as Pico.Avatar.XNode.NodeGraph);
            }
            GUILayout.Space(EditorGUIUtility.singleLineHeight);

            if (GUILayout.Button("Delete Layer", GUILayout.Height(40)))
            {
                if (EditorUtility.DisplayDialog("Delete Layer", "Deleting this Layer will delete all Animation State it contains.\nAre you sure you want to delete Layer?", "Delete", "Cancel"))
                {
                    AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                    DeleteLayer(layer);
                    return;
                }
            }
            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            if(target is AvatarAnimationLayerGraph)
            {
                AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                SerializedProperty parentAnimator = serializedObject.FindProperty(nameof(Pico.Avatar.XNode.NodeGraph.parentAnimator));
                SerializedProperty nodes = serializedObject.FindProperty(nameof(Pico.Avatar.XNode.NodeGraph.nodes));
                SerializedProperty layerName = serializedObject.FindProperty(nameof(AvatarAnimationLayerGraph.layerName));
                SerializedProperty weight = serializedObject.FindProperty(nameof(AvatarAnimationLayerGraph.weight));
                SerializedProperty layerBlendMode = serializedObject.FindProperty(nameof(AvatarAnimationLayerGraph.layerBlendMode));
                SerializedProperty mask = serializedObject.FindProperty(nameof(AvatarAnimationLayerGraph.mask));

                EditorGUILayout.PropertyField(parentAnimator);
                EditorGUILayout.PropertyField(nodes);

                if(Application.isPlaying)
                    EditorGUI.BeginDisabledGroup(true);

                EditorGUILayout.PropertyField(layerName);

                if(Application.isPlaying)
                    EditorGUI.EndDisabledGroup();

                EditorGUILayout.PropertyField(weight);
                EditorGUILayout.PropertyField(layerBlendMode);
                EditorGUILayout.PropertyField(mask);
            }   
            else{
                DrawDefaultInspector();
            }

            RenameLayerName();

            serializedObject.ApplyModifiedProperties();
        }
        // delete layer
        public void DeleteLayer(AvatarAnimationLayerGraph layer)
        {
            if (layer != null && layer.parentAnimator.layers.Contains(layer))
            {
                for (int i = layer.nodes.Count - 1; i >= 0; i--)
                {
                    if (layer.nodes[i] == null) continue;
                    DeleteNode(layer.nodes[i], false);
                }
                layer.parentAnimator.layers.Remove(layer);
                DestroyImmediate(layer, true);
                AssetDatabase.SaveAssets();
            }
            AssetDatabase.Refresh();
        }

        // delete node
        public void DeleteNode(Pico.Avatar.XNode.Node node, bool saveImmediate)
        {
            if (node != null)
            {
                if (node.layer != null && node.layer.nodes.Contains(node))
                    node.layer.nodes.Remove(node);
                DestroyImmediate(node, true);
                if (saveImmediate) AssetDatabase.SaveAssets();
            }
            if (saveImmediate) AssetDatabase.Refresh();
        }

        public void RenameLayerName()
        {
            if (serializedObject.targetObject is AvatarAnimationLayerGraph)
            {
                SerializedProperty layerName = serializedObject.FindProperty(nameof(AvatarAnimationLayerGraph.layerName));
                AvatarAnimationLayerGraph layer = serializedObject.targetObject as AvatarAnimationLayerGraph;
                if (layer.name != layerName.stringValue && !string.IsNullOrEmpty(layerName.stringValue))
                {
                    layer.name = layerName.stringValue;
                    //Debug.Log("Layer name rename serializedObject.targetObject = " + serializedObject.targetObject + "  target = " + target);
                    if (NodeEditorPreferences.GetSettings().autoSave && !Application.isPlaying)
                    {
                        AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(serializedObject.targetObject));
                        AssetDatabase.Refresh();
                    }
                }
            }
        }
    }
#endif

    [CustomEditor(typeof(Pico.Avatar.XNode.Node), true)]
#if ODIN_INSPECTOR_DISCARD
    public class GlobalNodeEditor : OdinEditor {
        public override void OnInspectorGUI() {
            if (GUILayout.Button("Edit layer", GUILayout.Height(40))) {
                SerializedProperty graphProp = serializedObject.FindProperty(nameof(XNode.Node.layer));
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as Pico.Avatar.XNode.NodeGraph);
                w.Home(); // Focus selected node
            }
            base.OnInspectorGUI();
        }
    }
#else
    [CanEditMultipleObjects]
    public class GlobalNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (GUILayout.Button("Edit layer", GUILayout.Height(40)))
            {
                SerializedProperty graphProp = serializedObject.FindProperty(nameof(Pico.Avatar.XNode.Node.layer));
                NodeEditorWindow w = NodeEditorWindow.Open(graphProp.objectReferenceValue as Pico.Avatar.XNode.NodeGraph);
                w.Home(); // Focus selected node
            }

            GUILayout.Space(EditorGUIUtility.singleLineHeight);
            GUILayout.Label("Raw data", "BoldLabel");

            // Now draw the node itself.
            DrawDefaultInspector();

            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}