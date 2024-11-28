using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

#if UNITY_2019_1_OR_NEWER && USE_ADVANCED_GENERIC_MENU
using GenericMenu = XNodeEditor.AdvancedGenericMenu;
#endif

namespace Pico.Avatar.XNodeEditor
{
    /// <summary> Base class to derive custom Node editors from. Use this to create your own custom inspectors and editors for your nodes. </summary>
    [CustomNodeEditor(typeof(Pico.Avatar.XNode.Node))]
    public class NodeEditor : Pico.Avatar.XNodeEditor.Internal.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, Pico.Avatar.XNode.Node>
    {

        /// <summary> Fires every whenever a node was modified through the editor </summary>
        public static Action<Pico.Avatar.XNode.Node> onUpdateNode;
        public readonly static Dictionary<Pico.Avatar.XNode.NodePort, Vector2> portPositions = new Dictionary<Pico.Avatar.XNode.NodePort, Vector2>();

        public virtual void OnHeaderGUI()
        {
            GUILayout.Label(target.name, NodeEditorResources.styles.nodeHeader, GUILayout.Height(30));
        }

        /// <summary> Draws standard field editors for all public fields </summary>
        public virtual void OnBodyGUI()
        {
            serializedObject.Update();
            string[] excludes = { "m_Script", nameof(Pico.Avatar.XNode.Node.layer), "position", "ports" };
            
            AvatarWrapMode wrapMode = GetWrapMode();
            AvatarBlendType blendType = GetBlendType();
            //update float parameter list and BlendDirectClips list
            // UpdateBlendTreeFloatParameter(blendType);
            // Iterate through serialized properties and draw them like the Inspector (But with ports)
            SerializedProperty iterator = serializedObject.GetIterator();
            bool enterChildren = true;
            while (iterator.NextVisible(enterChildren))
            {
                enterChildren = false;
                if (excludes.Contains(iterator.name)) continue;
                //Renamed node name to state name.

                RenameNode(iterator);
                if (IsDrawProperty(iterator, wrapMode, blendType))
                {
                    bool isDisableEdit = IsDisableEdit(iterator);
                    if(isDisableEdit)
                        EditorGUI.BeginDisabledGroup(true);
                        
                    NodeEditorGUILayout.PropertyField(iterator, true);

                    if(isDisableEdit)
                        EditorGUI.EndDisabledGroup();
                }
            }

            // Iterate through dynamic ports and draw them in the order in which they are serialized
            foreach (Pico.Avatar.XNode.NodePort dynamicPort in target.DynamicPorts)
            {
                if (NodeEditorGUILayout.IsDynamicPortListPort(dynamicPort)) continue;
                NodeEditorGUILayout.PortField(dynamicPort);
            }

            serializedObject.ApplyModifiedProperties();
        }

        //update Blend Direct Clips list
        public void UpdateBlendTreeFloatParameter(AvatarBlendType blendType)
        {
            if (target is AvatarBlendTreeStateNode)
            {
                AvatarBlendTreeStateNode blendNode = target as AvatarBlendTreeStateNode;
                if (blendType == AvatarBlendType.BlendDirect)
                    blendNode.InitBlendDirectClips();
                blendNode.UpdateStringDropDownStringArray();
            }
        }

        public void RenameNode(SerializedProperty property)
        {
            if (property.name == nameof(AvatarBlendTreeStateNode.stateName) && (target.name != property.stringValue) && !string.IsNullOrEmpty(property.stringValue))
            {
                target.name = property.stringValue;
                if (NodeEditorPreferences.GetSettings().autoSave && !Application.isPlaying)
                {
                    AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
                    AssetDatabase.Refresh();
                }
            }
        }

        public bool IsDisableEdit(SerializedProperty property)
        {
            return Application.isPlaying && 
                (property.name == nameof(AvatarAnimationStateNode.stateName) || 
                property.name == nameof(AvatarAnimationStateNode.clipName) ||
                property.name == nameof(AvatarBlendTreeStateNode.blendType) ||
                property.name == nameof(AvatarBlendTreeStateNode.Blend1DClips) ||
                property.name == nameof(AvatarBlendTreeStateNode.Blend2DClips) ||
                property.name == nameof(AvatarBlendTreeStateNode.BlendDirectClips));
        }

        public bool IsDrawProperty(SerializedProperty iterator, AvatarWrapMode wrapMode, AvatarBlendType blendType)
        {
            return IsDrawWrapModeInputText(iterator, wrapMode) && IsDrawBlendClipList(iterator, blendType) && IsDrawParameter(iterator, blendType) && IsDrawClipNameText(iterator);
        }

        /// <summary>return whether to draw clipName.</summary>
        public bool IsDrawClipNameText(SerializedProperty property)
        {
            if (property.name == nameof(AvatarBlendTreeStateNode.clipName) && (target is AvatarBlendTreeStateNode)) return false;
            else return true;
        }

        /// <summary>return whether to draw parameter.</summary>
        public bool IsDrawParameter(SerializedProperty property, AvatarBlendType blendType)
        {
            if (property.name == nameof(AvatarBlendTreeStateNode.parameter) && blendType != AvatarBlendType.Blend1D) { return false; }
            else if (property.name == nameof(AvatarBlendTreeStateNode.parameterX) && blendType != AvatarBlendType.Blend2D) { return false; }
            else if (property.name == nameof(AvatarBlendTreeStateNode.parameterY) && blendType != AvatarBlendType.Blend2D) { return false; }
            else return true;
        }

        /// <summary>return whether to draw blend clip list.</summary>
        public bool IsDrawBlendClipList(SerializedProperty property, AvatarBlendType blendType)
        {
            if (property.name == nameof(AvatarBlendTreeStateNode.Blend1DClips) && blendType != AvatarBlendType.Blend1D) { return false; }
            else if (property.name == nameof(AvatarBlendTreeStateNode.Blend2DClips) && blendType != AvatarBlendType.Blend2D) { return false; }
            else if (property.name == nameof(AvatarBlendTreeStateNode.BlendDirectClips) && blendType != AvatarBlendType.BlendDirect) { return false; }
            else return true;
        }

        /// <summary> get blendTree's blend type</summary>
        public AvatarBlendType GetBlendType()
        {
            SerializedProperty blendType = serializedObject.FindProperty(nameof(AvatarBlendTreeStateNode.blendType));
            if (blendType != null)
            {
                AvatarBlendType result;
                if (Enum.TryParse<AvatarBlendType>(blendType.enumNames[blendType.enumValueIndex], out result))
                    return result;
            }
            return AvatarBlendType.Blend1D;
        }

        /// <summary>return whether to draw wrap mode input text. If it is not an input box, draw it directly </summary>
        public bool IsDrawWrapModeInputText(SerializedProperty property, AvatarWrapMode wrapMode)
        {
            if (property.name == nameof(AvatarAnimationStateNode.repeatTime))
            {
                if (wrapMode == AvatarWrapMode.Repeat)
                {
                    if (property.floatValue <= 0) property.floatValue = 1;
                    return true;
                }
                return false;
            }
            else if (property.name == nameof(AvatarAnimationStateNode.seekTime))
            {
                if (wrapMode == AvatarWrapMode.Seek)
                {
                    if (property.floatValue < 0) property.floatValue = 0;
                    return true;
                }
                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary> get Avatar animation state wrap mode</summary>
        public AvatarWrapMode GetWrapMode()
        {
            SerializedProperty wrap = serializedObject.FindProperty(nameof(AvatarAnimationStateNode.wrapMode));
            if (wrap != null)
            {
                AvatarWrapMode wrapMode;
                if (Enum.TryParse<AvatarWrapMode>(wrap.enumNames[wrap.enumValueIndex], out wrapMode))
                {
                    return wrapMode;
                }
            }
            //return loop , don't display input text
            return AvatarWrapMode.Loop;
        }

        public virtual int GetWidth()
        {
            Type type = target.GetType();
            int width;
            if (type.TryGetAttributeWidth(out width)) return width;
            else return 220;
        }

        /// <summary> Returns color for target node </summary>
        public virtual Color GetTint()
        {
            // Try get color from [NodeTint] attribute
            Type type = target.GetType();
            Color color;
            if (type.TryGetAttributeTint(out color)) return color;
            // Return default color (grey)
            else return NodeEditorPreferences.GetSettings().tintColor;
        }

        public virtual GUIStyle GetBodyStyle()
        {
            return NodeEditorResources.styles.nodeBody;
        }

        public virtual GUIStyle GetBodyHighlightStyle()
        {
            return NodeEditorResources.styles.nodeHighlight;
        }

        /// <summary> Override to display custom node header tooltips </summary>
        public virtual string GetHeaderTooltip()
        {
            return null;
        }

        /// <summary> Add items for the context menu when right-clicking this node. Override to add custom menu items. </summary>
        public virtual void AddContextMenuItems(GenericMenu menu)
        {
            bool canRemove = true;
            // Actions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is Pico.Avatar.XNode.Node)
            {
                Pico.Avatar.XNode.Node node = Selection.activeObject as Pico.Avatar.XNode.Node;
                menu.AddItem(new GUIContent("Move To Top"), false, () => NodeEditorWindow.current.MoveNodeToTop(node));
                menu.AddItem(new GUIContent("Rename"), false, NodeEditorWindow.current.RenameSelectedNode);

                canRemove = NodeGraphEditor.GetEditor(node.layer, NodeEditorWindow.current).CanRemove(node);
            }

            // Add actions to any number of selected nodes
            menu.AddItem(new GUIContent("Copy"), false, NodeEditorWindow.current.CopySelectedNodes);
            menu.AddItem(new GUIContent("Duplicate"), false, NodeEditorWindow.current.DuplicateSelectedNodes);

            if (canRemove) menu.AddItem(new GUIContent("Remove"), false, NodeEditorWindow.current.RemoveSelectedNodes);
            else menu.AddItem(new GUIContent("Remove"), false, null);

            // Custom sctions if only one node is selected
            if (Selection.objects.Length == 1 && Selection.activeObject is Pico.Avatar.XNode.Node)
            {
                Pico.Avatar.XNode.Node node = Selection.activeObject as Pico.Avatar.XNode.Node;
                menu.AddCustomContextMenuItems(node);
            }
        }

        /// <summary> Rename the node asset. This will trigger a reimport of the node. </summary>
        public void Rename(string newName)
        {
            if (newName == null || newName.Trim() == "") newName = NodeEditorUtilities.NodeDefaultName(target.GetType());
            target.name = newName;
            OnRename();
            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(target));
        }

        /// <summary> Called after this node's name has changed. </summary>
        public virtual void OnRename() { }

        [AttributeUsage(AttributeTargets.Class)]
        public class CustomNodeEditorAttribute : Attribute,
        Pico.Avatar.XNodeEditor.Internal.NodeEditorBase<NodeEditor, NodeEditor.CustomNodeEditorAttribute, Pico.Avatar.XNode.Node>.INodeEditorAttrib
        {
            private Type inspectedType;
            /// <summary> Tells a NodeEditor which Node type it is an editor for </summary>
            /// <param name="inspectedType">Type that this editor can edit</param>
            public CustomNodeEditorAttribute(Type inspectedType)
            {
                this.inspectedType = inspectedType;
            }

            public Type GetInspectedType()
            {
                return inspectedType;
            }
        }
    }
}