using UnityEditor;
using UnityEngine;
using System.IO;

namespace Pico.Avatar.XNodeEditor
{
    /// <summary> Deals with modified assets </summary>
    class NodeEditorAssetModProcessor : UnityEditor.AssetModificationProcessor
    {

        /// <summary> Automatically delete Node sub-assets before deleting their script.
        /// This is important to do, because you can't delete null sub assets.
        /// <para/> For another workaround, see: https://gitlab.com/RotaryHeart-UnityShare/subassetmissingscriptdelete </summary> 
        private static AssetDeleteResult OnWillDeleteAsset(string path, RemoveAssetOptions options)
        {
            // Skip processing anything without the .cs extension
            if (Path.GetExtension(path) != ".cs") return AssetDeleteResult.DidNotDelete;

            // Get the object that is requested for deletion
            UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(path);

            // If we aren't deleting a script, return
            if (!(obj is UnityEditor.MonoScript)) return AssetDeleteResult.DidNotDelete;

            // Check script type. Return if deleting a non-node script
            UnityEditor.MonoScript script = obj as UnityEditor.MonoScript;
            System.Type scriptType = script.GetClass();
            if (scriptType == null || (scriptType != typeof(Pico.Avatar.XNode.Node) && !scriptType.IsSubclassOf(typeof(Pico.Avatar.XNode.Node)))) return AssetDeleteResult.DidNotDelete;

            // Find all ScriptableObjects using this script
            string[] guids = AssetDatabase.FindAssets("t:" + scriptType);
            for (int i = 0; i < guids.Length; i++)
            {
                string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetpath);
                for (int k = 0; k < objs.Length; k++)
                {
                    Pico.Avatar.XNode.Node node = objs[k] as Pico.Avatar.XNode.Node;
                    if (node.GetType() == scriptType)
                    {
                        if (node != null && node.layer != null)
                        {
                            // Delete the node and notify the user
                            Debug.LogWarning(node.name + " of " + node.layer + " depended on deleted script and has been removed automatically.", node.layer);
                            node.layer.RemoveNode(node);
                        }
                    }
                }
            }
            // We didn't actually delete the script. Tell the internal system to carry on with normal deletion procedure
            return AssetDeleteResult.DidNotDelete;
        }

        /// <summary> Automatically re-add loose node assets to the Graph node list </summary>
        // [InitializeOnLoadMethod]
        private static void OnReloadEditor()
        {
            // Find all NodeGraph assets
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(Pico.Avatar.XNode.NodeGraph));
            for (int i = 0; i < guids.Length; i++)
            {
                string assetpath = AssetDatabase.GUIDToAssetPath(guids[i]);
                Pico.Avatar.XNode.NodeGraph graph = AssetDatabase.LoadAssetAtPath(assetpath, typeof(Pico.Avatar.XNode.NodeGraph)) as Pico.Avatar.XNode.NodeGraph;
                graph.nodes.RemoveAll(x => x == null); //Remove null items
                Object[] objs = AssetDatabase.LoadAllAssetRepresentationsAtPath(assetpath);
                // Ensure that all sub node assets are present in the graph node list
                for (int u = 0; u < objs.Length; u++)
                {
                    // Ignore null sub assets
                    if (objs[u] == null) continue;
                    if (!graph.nodes.Contains(objs[u] as Pico.Avatar.XNode.Node)) graph.nodes.Add(objs[u] as Pico.Avatar.XNode.Node);
                }
            }
        }
    }
}