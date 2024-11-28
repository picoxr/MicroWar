using System.IO;
using System.IO.Compression;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pico.Avatar.XNode;
using System;

namespace Pico
{
    namespace Avatar
    {
        [CustomNodeGraphEditor(typeof(AvatarAnimationLayerGraph))]
        public class AvatarLayerGraphEditor : XNodeEditor.NodeGraphEditor
        {
            List<string> animationAnimaz = new List<string>();
            string clipNameLabel = "Clip Name";
            Vector2 entryPos = new Vector2(-300f, -50f);
            Vector2 exitPos = new Vector2(100f, -50f);

            public override void OnOpen()
            {
                NodePort entryConnectionTo;
                CheckEntryExit(out entryConnectionTo);
                EntryConnect(entryConnectionTo);
            }

            public override void OnWindowFocus()
            {
                base.OnWindowFocus();
                // Debug.Log("AvatarAnimationLayerGraph OnWindowFocus! ");
                LoadAniaz();
            }

            public override Node CreateNode(Type type, Vector2 position)
            {
                AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                if ((type == typeof(AvatarAnimationEntry) && layer.entry != null) ||
                    (type == typeof(AvatarAnimationExit) && layer.exit != null))
                {
                    return null;
                }
                Node node = base.CreateNode(type, position);
                if (animationAnimaz.Count == 0)
                    animationAnimaz.AddRange(AvatarAnimator.internalAnimation);
                UpdateStateClipName(node, animationAnimaz.ToArray());
                return node;
            }

            public override void OnGUI()
            {
                base.OnGUI();
                AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                //Update clipName array
                if (animationAnimaz.Count > 0)
                {
                    UpdateStateClipName(layer);
                }
                layer.UpdateNodePlayingState();
            }


            //Get all animation name from zip 
            private void LoadAniaz()
            {
                AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                animationAnimaz = layer.parentAnimator.animationAnimaz;
                animationAnimaz.Clear();
                animationAnimaz.AddRange(AvatarAnimator.internalAnimation);
                string fileType = "*.zip";

                //iterate over all paths
                for (int j = 0; j < layer.parentAnimator.animationPathRelativeAssets.Length; j++)
                {
                    string folderPath = Application.dataPath + "/" + layer.parentAnimator.animationPathRelativeAssets[j];
                    string[] filePaths = Directory.GetFiles(folderPath, fileType);

                    //iterate over all zips
                    for (int i = 0; i < filePaths.Length; i++)
                    {
                        using (ZipArchive archive = new ZipArchive(File.OpenRead(filePaths[i]), ZipArchiveMode.Read))
                        {
                            //Traverse all the animaz of the zip
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string animName = entry.FullName.Remove(entry.FullName.LastIndexOf("."));
                                if (!animationAnimaz.Exists(s => { return animName.Equals(s); }))
                                {
                                    animationAnimaz.Add(animName);
                                    // Debug.Log(animName);
                                }
                            }
                        }
                    }
                }

                //Update clipName array
                UpdateStateClipName(layer);
            }

            public void UpdateStateClipName(AvatarAnimationLayerGraph layer)
            {
                //Update clipName array
                string[] animazArray = animationAnimaz.ToArray();
                for (int i = 0; i < layer.nodes.Count; i++)
                {
                    UpdateStateClipName(layer.nodes[i], animazArray);
                }
            }

            //update AnimationState and BlendTreeClipList clipName 
            public void UpdateStateClipName(Node node, string[] animazArray)
            {
                if (node is AvatarBlendTreeStateNode)
                {
                    var blendNode = node as AvatarBlendTreeStateNode;

                    switch (blendNode.blendType)
                    {
                        case AvatarBlendType.Blend1D:
                            for (int j = 0; j < blendNode.Blend1DClips.Count; j++)
                                UpdateStringDropDown(ref blendNode.Blend1DClips[j].clipName, animazArray);
                            break;
                        case AvatarBlendType.Blend2D:
                            for (int j = 0; j < blendNode.Blend2DClips.Count; j++)
                                UpdateStringDropDown(ref blendNode.Blend2DClips[j].clipName, animazArray);
                            break;
                        case AvatarBlendType.BlendDirect:
                            for (int j = 0; j < blendNode.BlendDirectClips.Count; j++)
                                UpdateStringDropDown(ref blendNode.BlendDirectClips[j].clipName, animazArray);
                            break;
                    }
                }
                else if (node is AvatarAnimationStateNode)
                {
                    var stateNode = node as AvatarAnimationStateNode;
                    UpdateStringDropDown(ref stateNode.clipName, animazArray);
                }
            }

            //update clipNameLabel and stringArray for clipName 
            public void UpdateStringDropDown(ref StringDropDown clipName, string[] animazArray)
            {
                if (clipName == null)
                {
                    clipName = new StringDropDown(animazArray, clipNameLabel);
                }
                else
                {
                    if (clipName.name == null || !clipName.name.Equals(clipNameLabel))
                        clipName.name = clipNameLabel;
                    clipName.stringArray = animazArray;
                }
            }

            //check whether layer has only one entry and exit
            void CheckEntryExit(out NodePort entryConnection)
            {
                AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                bool haveEntry = false, haveExit = false;
                entryConnection = null;

                for (int i = layer.nodes.Count - 1; i >= 0; i--)
                {
                    if (layer.nodes[i] is AvatarAnimationEntry)
                    {
                        if (haveEntry == false)
                        {
                            haveEntry = true;
                            layer.entry = layer.nodes[i] as AvatarAnimationEntry;
                        }
                        else
                        {
                            NodePort port = layer.nodes[i].GetOutputPort(nameof(AvatarAnimationEntry.entry)).Connection;
                            if (port != null)
                            {
                                entryConnection = port;
                            }
                            base.RemoveNode(layer.nodes[i]);
                        }
                    }
                    else if (layer.nodes[i] is AvatarAnimationExit)
                    {
                        if (haveExit == false)
                        {
                            haveExit = true;
                            layer.exit = layer.nodes[i] as AvatarAnimationExit;
                        }
                        else base.RemoveNode(layer.nodes[i]);
                    }
                }


                if (haveEntry == false)
                {
                    layer.entry = base.CreateNode(typeof(AvatarAnimationEntry), entryPos) as AvatarAnimationEntry;
                }
                if (haveExit == false)
                {
                    layer.exit = base.CreateNode(typeof(AvatarAnimationExit), exitPos) as AvatarAnimationExit;
                }
            }

            //if entry output connection is null, connect to a state
            void EntryConnect(NodePort another)
            {
                AvatarAnimationLayerGraph layer = target as AvatarAnimationLayerGraph;
                if (layer.entry == null) return;

                NodePort entryPort = layer.entry.GetOutputPort(nameof(AvatarAnimationEntry.entry));
                if (another != null)
                {
                    entryPort.Connect(another);
                    return;
                }
                NodePort connectionPort = entryPort.Connection;
                if (connectionPort == null)
                {
                    for (int i = 0; i < layer.nodes.Count; i++)
                    {
                        if (layer.nodes[i] is AvatarAnimationEntry || layer.nodes[i] is AvatarAnimationExit)
                            continue;
                        entryPort.Connect(layer.nodes[i].GetInputPort(nameof(AvatarAnimationStateNode.enter)));
                        break;
                    }
                }
            }
        }
    }
}
