using System;
using System.Reflection;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Events;
using Pico.Avatar.XNode;

namespace Pico
{
	namespace Avatar
	{
		public class AvatarAnimationStateNode : Node
		{
			[Pico.Avatar.XNode.Node.Input(backingValue = ShowBackingValue.Never)]
			public Enter enter;

			[Pico.Avatar.XNode.Node.Output(connectionType = ConnectionType.Override,
				typeConstraint = TypeConstraint.InheritedInverse, dynamicPortList = true)]
			public Transition[] transitions = new Transition[0];

			//Animation state name
			[Delayed] public string stateName;

			// //Animation clip name
			// public string clipNameDropDown.currentString;
			public StringDropDown clipName /* = new StringDropDown(nameof(clipName))*/;

			//Clip speed
			[Delayed] public float speed = 1;

			//Animation play mode
			public AvatarWrapMode wrapMode = AvatarWrapMode.Repeat;

			//If wrapMode selects Repeat, you need to set the number of repeat time 
			[Delayed] public float repeatTime = 1;

			//If wrapMode selects Seek, you need to set the seek time
			[Delayed] public float seekTime = 0f;

			//Time to start playing
			[Delayed] public float startTime = 0f;


			//Set callback methods for different animation states
			[HideInInspector] public AnimationCallback animationCallback;
			[HideInInspector] public AvatarAnimationState animationState;
			[HideInInspector] protected List<TransAndData> stateTrans = new List<TransAndData>();
			[HideInInspector] public bool isPlayingState = false;

			private int currAutoSwitchIndex = -1;


			public override void InitStateNode(AvatarAnimationLayer animationLayer)
			{
				// Debug.Log("InitStateNode stateName = " + stateName);
				InitBaseField(animationLayer);
				InitAnimationClip();
			}

			public override void InitTransition(AvatarAnimationLayer animationLayer)
			{
				if (animationState == null) return;

				int index = -1;
				foreach (var port in DynamicOutputs)
				{
					index++;
					List<NodePort> nodePortList = port.GetConnections();
					Debug.Log($"index = {index} , nodePortList.Count = {nodePortList.Count}");
					for (int i = 0; i < nodePortList.Count; i++)
					{
						AvatarAnimationStateNode nextNode = nodePortList[i].node as AvatarAnimationStateNode;
						if (nextNode == null || nextNode.animationState == null) continue;
						Debug.Log("port : " + port.ToString() + ", nextNode : " + nextNode.stateName);
						AvatarAnimationStateTransition currTransi =
							animationState.AddTransition(nextNode.animationState);
						AnimatorParameterType paraType = (AnimatorParameterType)transitions[index].conditionType;
						AvatarAnimationCondition condition = currTransi.AddCondition(paraType);
						stateTrans.Add(new TransAndData(currTransi, condition, transitions[index]));
						SetCondition(currTransi, condition, transitions[index]);
					}
				}
			}

			public void SetCondition(AvatarAnimationStateTransition transition, AvatarAnimationCondition condition,
				Transition data)
			{
				// Debug.Log( $"AutonSwitch = {data.autoSwitch}  conditionType = {data.conditionType}    uintOperator = {data.uintOperator}  comparedUint = {data.comparedUint} "
				//         + $" floatOperator = {data.floatOperator}  comparedFloat = {data.comparedFloat}");
				transition.SetDuration(data.duration);
				transition.SetExitWhenStateEnd(data.autoSwitch);
				AnimatorParameterType paraType = (AnimatorParameterType)data.conditionType;
				// Debug.Log($"conditionType = {data.conditionType} (AnimatorParameterType) = {paraType}");
				// AvatarAnimationCondition condition = transition.AddCondition(paraType);
				condition.SetParameterByName(data.parameter.currentString);
				switch (paraType)
				{
					case AnimatorParameterType.Bool:
						if (data.boolOperator == BoolCompare.True)
							condition.SetOperator(AnimationConditionOperator.True);
						else
							condition.SetOperator(AnimationConditionOperator.False);
						break;
					case AnimatorParameterType.Trigger:
						break;
					case AnimatorParameterType.UInt:
						switch (data.uintOperator)
						{
							case UintCompare.Greater:
								condition.SetOperator(AnimationConditionOperator.Greater);
								break;
							case UintCompare.Less:
								condition.SetOperator(AnimationConditionOperator.Less);
								break;
							case UintCompare.Equal:
								condition.SetOperator(AnimationConditionOperator.Equal);
								break;
							case UintCompare.NotEqual:
								condition.SetOperator(AnimationConditionOperator.NotEqual);
								break;
						}

						condition.SetThresholdUInt(data.comparedUint);
						break;
					case AnimatorParameterType.Float:
						if (data.floatOperator == FloatCompare.Greater)
							condition.SetOperator(AnimationConditionOperator.Greater);
						else
							condition.SetOperator(AnimationConditionOperator.Less);
						condition.SetThresholdFloat(data.comparedFloat);
						break;
				}
			}

			public void InitBaseField(AvatarAnimationLayer animationLayer)
			{
				animationState = animationLayer.CreateAnimationStateByName(stateName);
				animationState.SetSpeed(speed);
				SetWrapMode(animationState);
				animationState.SetStartTime(startTime);
				SetAnimationCallback(animationState);
				isPlayingState = false;
			}

			//Add Animation Clip 
			public void InitAnimationClip()
			{
				if (layer.parentAnimator.loadedAnimations != null &&
				    layer.parentAnimator.loadedAnimations.Exists(s => { return s.Equals(clipName.currentString); }))
				{
					animationState.AddAnimationClip(clipName.currentString);
				}
				else
				{
					layer.parentAnimator.bodyAnimController.owner.owner.OnLoadAnimationsExternComplete +=
						OnLoadAnimationsExternComplete;
				}
			}

			//If the external animation is not loaded, wait for the loading to complete and add the Clip.
			public void OnLoadAnimationsExternComplete(string assetBundlePath, string animationNamesJson)
			{
				var anims = JsonConvert.DeserializeObject<List<System.Object>>(animationNamesJson);
				if (anims != null)
					for (int i = 0; i < anims.Count; i++)
					{
						string animName = anims[i].ToString();
						//animName = animName.Remove(animName.LastIndexOf("."));
						if (animName == clipName.currentString)
						{
							//add animation to state when load complete
							animationState.AddAnimationClip(clipName.currentString);
						}
					}
			}

#if UNITY_EDITOR
			//Called when editor parameters are modified
			private void OnValidate()
			{
				if (!Application.isPlaying)
				{
					CheckAutoSwitch();
				}

				if (Application.isPlaying)
				{
					UpdateStateNode();
					UpdateTransition();
				}

				UpdateTransitionParameter();
			}
#endif

			//Ensure that at most one transition select AutoSwitch
			private void CheckAutoSwitch()
			{
				if (transitions == null) return;
				bool haveAutoSwitch = false;
				for (int i = 0; i < transitions.Length; i++)
				{
					if (transitions[i].autoSwitch)
					{
						haveAutoSwitch = true;
						if (currAutoSwitchIndex == -1)
						{
							currAutoSwitchIndex = i;
						}
						else if (currAutoSwitchIndex != i)
						{
							transitions[currAutoSwitchIndex].autoSwitch = false;
							currAutoSwitchIndex = i;
							break;
						}
					}
				}

				//no one choose AutoSwitch
				if (haveAutoSwitch == false)
					currAutoSwitchIndex = -1;
			}

			public override void UpdatePlayingState(AvatarAnimationState currentState)
			{
				if (currentState == animationState)
				{
					isPlayingState = true;
				}
				else
				{
					isPlayingState = false;
				}
			}

			#region set state

			public override void UpdateStateNode()
			{
				if (animationState == null) return;
				animationState.SetSpeed(speed);
				SetWrapMode(animationState);
				animationState.SetStartTime(startTime);
				SetAnimationCallback(animationState);
				// if (layer is AvatarAnimationLayerGraph)
				//     (layer as AvatarAnimationLayerGraph).animationLayer.PlayAnimationState(animationState, 0);
			}


			public void SetWrapMode(AvatarAnimationState animationState)
			{
				if (wrapMode == AvatarWrapMode.Loop || wrapMode == AvatarWrapMode.PingPong ||
				    wrapMode == AvatarWrapMode.ClampForever)
					animationState.SetWrapMode((int)wrapMode);
				else if (wrapMode == AvatarWrapMode.Repeat)
					animationState.SetWrapMode(repeatTime);
				else if (wrapMode == AvatarWrapMode.Seek)
				{
					animationState.SetWrapMode((int)wrapMode);
					animationState.Seek(seekTime);
				}
			}

			//set animation callback
			public void SetAnimationCallback(AvatarAnimationState animationState)
			{
				AnimationCallbackEvent idleEvent = animationCallback.idleCallback;
				AnimationCallbackEvent enterEvent = animationCallback.enterCallback;
				AnimationCallbackEvent runningEvent = animationCallback.runningCallback;
				AnimationCallbackEvent endEvent = animationCallback.endCallback;
				AnimationCallbackEvent leaveEvent = animationCallback.leaveCallback;

				SetEvent(animationState, AnimStateStatus.StateIdle, idleEvent);
				SetEvent(animationState, AnimStateStatus.StateEnter, enterEvent);
				SetEvent(animationState, AnimStateStatus.StateRunning, runningEvent);
				SetEvent(animationState, AnimStateStatus.StateEnd, endEvent);
				SetEvent(animationState, AnimStateStatus.StateLeave, leaveEvent);
			}

			public void SetEvent(AvatarAnimationState animationState, AnimStateStatus stateStatus,
				AnimationCallbackEvent targetEvent)
			{
				//Get method count 
				int count = targetEvent.GetPersistentEventCount();
				if (count > 0)
				{
					for (int i = 0; i < count; i++)
					{
						//Get method name
						string methodName = targetEvent.GetPersistentMethodName(i);
						//Get method target object
						UnityEngine.Object obj = targetEvent.GetPersistentTarget(i);
						//Get method info
						MethodInfo methodHandler = null;
						try
						{
							methodHandler = obj.GetType().GetMethod(methodName,
								BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
								BindingFlags.Static);
						}
						catch (System.Reflection.AmbiguousMatchException)
						{
							Debug.LogError("Failed to get callback method!");
						}


						if (methodHandler != null)
						{
							// Debug.Log(methodName + " obj.type = " + obj.GetType() + "  methodHandler =" + methodHandler.Name);
							//Use method to create delegate
							System.Action<AvatarAnimationState> d =
								Delegate.CreateDelegate(typeof(System.Action<AvatarAnimationState>), obj, methodHandler)
									as System.Action<AvatarAnimationState>;
							//Add delegate to animation
							animationState.AddStateStatusCallBack(stateStatus, d);
						}
					}
				}
			}

			#endregion

			#region transition update

			//update transition parameter
			public void UpdateTransitionParameter()
			{
				if (transitions == null) return;
				for (int i = 0; i < transitions.Length; i++)
				{
					if (transitions[i] == null)
						transitions[i] = new Transition();
					transitions[i].UpdateParameters(this);
				}
			}

			//update transition runtime
			public void UpdateTransition()
			{
				for (int i = 0; i < stateTrans.Count; i++)
				{
					SetCondition(stateTrans[i].trans, stateTrans[i].cond, stateTrans[i].data);
				}
			}


			public void CheckConnecteSelf()
			{
				NodePort inputPort = GetInputPort(nameof(enter));
				List<NodePort> cons = inputPort.GetConnections();
				for (int i = cons.Count - 1; i >= 0; i--)
				{
					if (cons[i].node == this)
					{
						Debug.Log("disconnect self " + this.stateName);
						inputPort.Disconnect(cons[i]);
					}
				}
			}

			#endregion


			public override void OnCreateConnection(NodePort from, NodePort to)
			{
				//forbid to connect self
				if (from.node == to.node)
				{
					from.Disconnect(to);
				}
			}


			// Use this for initialization
			protected override void Init()
			{
				base.Init();
			}

			// Return the correct value of an output port when requested
			public override object GetValue(NodePort port)
			{
				return null; // Replace this
			}
		}

		[System.Serializable]
		public class AnimationCallback
		{
			public AvatarAnimationStateNode animationState; //Corresponding AnimationStateNode
			public AnimationCallbackEvent idleCallback; //Callback when state not playing
			public AnimationCallbackEvent enterCallback; //Callback when state entering
			public AnimationCallbackEvent runningCallback; //Callback when state running
			public AnimationCallbackEvent endCallback; //Callback when animation playback ended in this state
			public AnimationCallbackEvent leaveCallback; //Callback when state leaving, some other state enters
		}

		[System.Serializable]
		public class AnimationCallbackEvent : UnityEvent<AvatarAnimationState>
		{
		}

		public struct TransAndData
		{
			public AvatarAnimationStateTransition trans;
			public AvatarAnimationCondition cond;
			public Transition data;

			public TransAndData(AvatarAnimationStateTransition trans, AvatarAnimationCondition cond, Transition data)
			{
				this.trans = trans;
				this.cond = cond;
				this.data = data;
			}
		}
	}
}