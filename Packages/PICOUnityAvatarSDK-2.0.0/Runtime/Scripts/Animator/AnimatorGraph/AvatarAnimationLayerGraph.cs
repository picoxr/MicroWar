using System;
using System.Collections.Generic;
using UnityEngine;
using Pico.Avatar.XNode;

namespace Pico
{
	namespace Avatar
	{
		//[CreateAssetMenu(fileName = "new layer", menuName = "AvatarAnimator/Layer")]
		public class AvatarAnimationLayerGraph : NodeGraph
		{
			[Header("Layer Setting")] [Delayed] public string layerName;

			//the weight of the layer
			[Range(0f, 1f)] [Delayed] public float weight = 1f;

			public AvatarLayerBlendMode layerBlendMode = AvatarLayerBlendMode.Override;

			//Mask the specified part of the animation
			[EnumMulti] public AvatarLayerMask mask;

			[HideInInspector] public AvatarAnimationEntry entry;
			[HideInInspector] public AvatarAnimationExit exit;
			[HideInInspector] public AvatarAnimationLayer animationLayer;


			//Init AvatarAnimationLayer
			public void InitAnimationLayer(AvatarAnimationLayer animationLayer)
			{
				this.animationLayer = animationLayer;
				animationLayer.SetLayerBlendWeight(weight);
				// Debug.Log("layerBlendMode = " + ((uint)layerBlendMode) + " (AnimLayerBlendMode)layerBlendMode = " + ((uint)(AnimLayerBlendMode)layerBlendMode));
				animationLayer.SetLayerBlendMode((AnimLayerBlendMode)layerBlendMode);
				List<AvatarLayerMask> masks = GetAvatarMask(mask);
				SetLayerMask(animationLayer, masks);
				//Init each animation state node
				for (int i = 0; i < nodes.Count; i++)
				{
					if (nodes[i] == null)
						continue;
					nodes[i].InitStateNode(animationLayer);
				}

				//Init state transition after init all state node
				for (int i = 0; i < nodes.Count; i++)
				{
					if (nodes[i] == null)
						continue;
					nodes[i].InitTransition(animationLayer);
				}
			}

#if UNITY_EDITOR
			private void OnValidate()
			{
				UpdateAnimationLayer();
			}

			public void UpdateAnimationLayer()
			{
				if (animationLayer == null) return;
				animationLayer.SetLayerBlendWeight(weight);
				animationLayer.SetLayerBlendMode((AnimLayerBlendMode)layerBlendMode);
				animationLayer.SetSRTEnable(false, true, false);
				List<AvatarLayerMask> masks = GetAvatarMask(mask);
				SetLayerMask(animationLayer, masks);
			}

			public void UpdateNodePlayingState()
			{
				if (Application.isPlaying && animationLayer != null && animationLayer.currentState != null)
				{
					//TODO: Set node's playingStae.
					for (int i = 0; i < nodes.Count; i++)
					{
						nodes[i]?.UpdatePlayingState(animationLayer.currentState);
					}
				}
			}
#endif


			//Init layer mask
			private void SetLayerMask(AvatarAnimationLayer layer, List<AvatarLayerMask> masks)
			{
				Pico.Avatar.AvatarMask mask = new Pico.Avatar.AvatarMask(layer.owner);

				foreach (AvatarLayerMask item in masks)
				{
					switch (item)
					{
						case AvatarLayerMask.Head:
							MaskAddJoint(mask, JointType.Neck, JointType.Hair);
							break;
						case AvatarLayerMask.Body:
							MaskAddJoint(mask, JointType.Root, JointType.Chest);
							break;
						case AvatarLayerMask.LeftArm:
							MaskAddJoint(mask, JointType.LeftShoulder, JointType.LeftHandWrist);
							break;
						case AvatarLayerMask.LeftHand:
							MaskAddJoint(mask, JointType.LeftHandThumbTrapezium, JointType.LeftHandPinkyTip);
							break;
						case AvatarLayerMask.RightArm:
							MaskAddJoint(mask, JointType.RightShoulder, JointType.RightHandWrist);
							break;
						case AvatarLayerMask.RightHand:
							MaskAddJoint(mask, JointType.RightHandThumbTrapezium, JointType.RightHandPinkyTip);
							break;
						case AvatarLayerMask.LeftLeg:
							MaskAddJoint(mask, JointType.LeftLegUpper, JointType.LeftToeEnd);
							break;
						case AvatarLayerMask.RightLeg:
							MaskAddJoint(mask, JointType.RightLegUpper, JointType.RightToeEnd);
							break;
						default:
							break;
					}
				}

				//Make sure the animated feet don't drift
				mask.SetJointPositionEnable(JointType.Hips, true);
				layer.SetAvatarMask(mask);
			}

			//add joint to mask, include start and end
			public void MaskAddJoint(Pico.Avatar.AvatarMask mask, JointType start, JointType end)
			{
				for (uint i = (uint)start; i <= (uint)end; ++i)
				{
					mask.SetJointRotationEnable((JointType)i, false);
				}
			}

			//get all selected AvatarMask
			public static List<AvatarLayerMask> GetAvatarMask(AvatarLayerMask mask)
			{
				List<AvatarLayerMask> masks = new List<AvatarLayerMask>();
				//get all enum value
				AvatarLayerMask[] valuesArray = Enum.GetValues(typeof(AvatarLayerMask)) as AvatarLayerMask[];
				//selected upper body, add head, arm, hand, body
				if (IsSelectedLayerMask(mask, AvatarLayerMask.UpperBody))
				{
					masks.Add(AvatarLayerMask.Head);
					masks.Add(AvatarLayerMask.Body);
					masks.Add(AvatarLayerMask.LeftArm);
					masks.Add(AvatarLayerMask.LeftHand);
					masks.Add(AvatarLayerMask.RightArm);
					masks.Add(AvatarLayerMask.RightHand);
				}

				//selected upper body, add left leg and right leg
				if (IsSelectedLayerMask(mask, AvatarLayerMask.LowerBody))
				{
					masks.Add(AvatarLayerMask.LeftLeg);
					masks.Add(AvatarLayerMask.RightLeg);
				}

				//No need to judge whether there is upper body and lower body
				for (int i = 0; i < valuesArray.Length - 2; i++)
				{
					if (!masks.Exists(item => item == valuesArray[i]) && IsSelectedLayerMask(mask, valuesArray[i]))
					{
						masks.Add(valuesArray[i]);
					}
				}
				// foreach (var item in masks) { Debug.Log(item + " : " + (uint)item + "  Selected !!"); }

				return masks;
			}

			//return whether selected assign AvatarLayerMask enum.
			public static bool IsSelectedLayerMask(AvatarLayerMask mask, AvatarLayerMask assignMask)
			{
				return (((uint)mask & (uint)assignMask) == ((uint)assignMask));
			}
		}
	}
}