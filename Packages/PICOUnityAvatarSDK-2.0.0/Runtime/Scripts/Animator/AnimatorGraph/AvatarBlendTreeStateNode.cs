using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using Pico.Avatar.XNode;

namespace Pico
{
	namespace Avatar
	{
		public class AvatarBlendTreeStateNode : AvatarAnimationStateNode
		{
			[Header("Blend Tree")]
			//Blend type
			public AvatarBlendType blendType = AvatarBlendType.Blend1D;

			public StringDropDown parameter; //If Blend1D is selected, this parameter needs to be set
			public StringDropDown parameterX; //If Blend2D is selected, the parameter is the first parameter

			public StringDropDown parameterY; //If Blend2D is selected, the parameter is the second parameter

			//Store all clip information under Blend1D
			public List<Blend1DClipItem> Blend1DClips = new List<Blend1DClipItem>();

			//Store all clip information under Blend2D
			public List<Blend2DClipItem> Blend2DClips = new List<Blend2DClipItem>();

			//Store all clip information under BlendDirectClips
			public List<BlendDirectClipItem> BlendDirectClips = new List<BlendDirectClipItem>(1);


			public override void InitStateNode(AvatarAnimationLayer animationLayer)
			{
				Debug.Log("InitBlendStateNode stateName = " + stateName);
				InitBaseField(animationLayer);

				animationState.CreateBlendTree((AnimBlendTreeType)blendType);
				switch (blendType)
				{
					case AvatarBlendType.Blend1D:
						InitBlend1DList(animationState);
						break;
					case AvatarBlendType.Blend2D:
						InitBlend2DList(animationState);
						break;
					case AvatarBlendType.BlendDirect:
						InitBlendDirectList(animationState);
						break;
				}
			}

#if UNITY_EDITOR
			private void OnValidate()
			{
				// Debug.Log(" AvatarBlendTreeStateNode OnValidate");
				if (Application.isPlaying)
				{
					UpdateStateNode();
					UpdateTransition();
				}

				UpdateStringDropDownStringArray();
				UpdateTransitionParameter();
			}

#endif
			public override void UpdateStateNode()
			{
				if (animationState == null) return;
				animationState.SetSpeed(speed);
				SetWrapMode(animationState);
				animationState.SetStartTime(startTime);
				SetAnimationCallback(animationState);
				switch (blendType)
				{
					case AvatarBlendType.Blend1D:
						animationState.blendTree.SetParameterByName(0, parameter.currentString);
						UpdateBlend1DList();
						break;
					case AvatarBlendType.Blend2D:
						animationState.blendTree.SetParameterByName(0, parameterX.currentString);
						animationState.blendTree.SetParameterByName(1, parameterY.currentString);
						UpdateBlend2DList();
						break;
					case AvatarBlendType.BlendDirect:
						UpdateBlendDirectList();
						break;
				}
				// if (layer is AvatarAnimationLayerGraph)
				//     (layer as AvatarAnimationLayerGraph).animationLayer.PlayAnimationState(animationState, 0);
			}

			//Init Blend1D clip list
			public void InitBlend1DList(AvatarAnimationState animationState)
			{
				animationState.blendTree.AddParameterByName(parameter.currentString);
				for (int i = 0; i < Blend1DClips.Count; i++)
				{
					InitBlendAnimationClip(Blend1DClips[i].clipName.currentString);
					animationState.blendTree.SetThreshold1D((uint)i, Blend1DClips[i].threshold);
				}
			}

			//Init Blend2D clip list
			public void InitBlend2DList(AvatarAnimationState animationState)
			{
				animationState.blendTree.AddParameterByName(parameterX.currentString);
				animationState.blendTree.AddParameterByName(parameterY.currentString);
				for (int i = 0; i < Blend2DClips.Count; i++)
				{
					InitBlendAnimationClip(Blend2DClips[i].clipName.currentString);
					animationState.blendTree.SetThreshold2D((uint)i, Blend2DClips[i].x, Blend2DClips[i].y);
				}
			}

			//Init Blend Direct clip list
			public void InitBlendDirectList(AvatarAnimationState animationState)
			{
				for (int i = 0; i < BlendDirectClips.Count; i++)
				{
					InitBlendAnimationClip(BlendDirectClips[i].clipName.currentString);
					animationState.blendTree.AddParameterByName(BlendDirectClips[i].parameter.currentString);
				}
			}

			public void InitBlendAnimationClip(string blendClipname)
			{
				if (layer.parentAnimator.loadedAnimations != null &&
				    layer.parentAnimator.loadedAnimations.Exists(s => { return s.Equals(blendClipname); }))
				{
					animationState.blendTree.AddAnimationClip(blendClipname);
				}
				else
				{
					layer.parentAnimator.bodyAnimController.owner.owner.OnLoadAnimationsExternComplete +=
						(assetBundlePath, animationNamesJson) =>
						{
							var anims = JsonConvert.DeserializeObject<List<System.Object>>(animationNamesJson);
							if (anims != null)
								for (int i = 0; i < anims.Count; i++)
								{
									Debug.Log("zjd : item = " + anims[i]);
									string animName = anims[i].ToString();
									//animName = animName.Remove(animName.LastIndexOf("."));
									if (animName == blendClipname)
									{
										try
										{
											//add animation to state when load complete
											animationState.blendTree.AddAnimationClip(blendClipname);
										}
										catch (Exception e)
										{
											Debug.LogError("clip is already in a state, please use another clip");
										}
									}
								}
						};
				}
			}

			//Update Blend1D clip list
			public void UpdateBlend1DList()
			{
				for (int i = 0; i < Blend1DClips.Count; i++)
				{
					animationState.blendTree.SetThreshold1D((uint)i, Blend1DClips[i].threshold);
				}
			}

			//Update Blend2D clip list
			public void UpdateBlend2DList()
			{
				for (int i = 0; i < Blend2DClips.Count; i++)
				{
					animationState.blendTree.SetThreshold2D((uint)i, Blend2DClips[i].x, Blend2DClips[i].y);
				}
			}


			//Update Blend Direct clip list
			public void UpdateBlendDirectList()
			{
				for (int i = 0; i < BlendDirectClips.Count; i++)
				{
					animationState.blendTree.SetParameterByName((uint)i, BlendDirectClips[i].parameter.currentString);
				}
			}


			// Use this for initialization
			protected override void Init()
			{
				base.Init();
				InitStringDropDown();
			}

			// Return the correct value of an output port when requested
			public override object GetValue(NodePort port)
			{
				return null; // Replace this
			}

			//Init StringDropDown
			public void InitStringDropDown()
			{
				if (parameter == null) parameter = new StringDropDown(this, nameof(parameter));
				if (parameterX == null) parameterX = new StringDropDown(this, nameof(parameterX));
				if (parameterY == null) parameterY = new StringDropDown(this, nameof(parameterY));
				InitBlendDirectClips();
			}

			//Init BlendDirectClips's StringDropDown
			public void InitBlendDirectClips()
			{
				for (int i = 0; i < BlendDirectClips.Count; i++)
				{
					if (BlendDirectClips[i].parameter == null || BlendDirectClips[i].parameter.node == null)
					{
						BlendDirectClips[i].parameter = new StringDropDown(this);
					}
				}
			}

			//Update all float parameters arrays under StringDropDown
			public void UpdateStringDropDownStringArray()
			{
				InitStringDropDown();
				if (parameter.stringArray != null)
				{
					parameter.stringArray = layer.parentAnimator.FloatParametersArray;
					parameterX.stringArray = layer.parentAnimator.FloatParametersArray;
					parameterY.stringArray = layer.parentAnimator.FloatParametersArray;
					for (int i = 0; i < BlendDirectClips.Count; i++)
					{
						if (BlendDirectClips[i].parameter != null)
						{
							BlendDirectClips[i].parameter.stringArray = layer.parentAnimator.FloatParametersArray;
						}
					}
				}
			}
		}


		#region BlendTreeClipItem

		[Serializable]
		public class Blend1DClipItem
		{
			public StringDropDown clipName;
			[EnableArrayEdit] public float threshold;
		}

		[Serializable]
		public class Blend2DClipItem
		{
			public StringDropDown clipName;
			[EnableArrayEdit] public float x;
			[EnableArrayEdit] public float y;
		}

		[Serializable]
		public class BlendDirectClipItem
		{
			public StringDropDown clipName;
			[EnableArrayEdit] public StringDropDown parameter = null;
		}

		#endregion
	}
}