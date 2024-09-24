using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using AOT;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar animation layer, used for play animation clips/statemachines.
		/// New animation layers can be created for users to play customized animation clips.
		/// Default state machine layers (IK locomotion layer, default hand pose layers, eyeBlink layer) are not exposed to users for now, you cannot add new animations to those layers.
		/// But you can create new layers to override default animatons.
		/// </summary>
		public class AvatarAnimationLayer : NativeObject
		{
			#region Public Properties

			public string name
			{
				get
				{
					if (_name == "")
					{
						StringBuilder stringBuilder = new StringBuilder("", 128);
						pav_AvatarAnimationLayer_GetName(nativeHandle, stringBuilder);
						_name = stringBuilder.ToString();
					}

					return _name;
				}
				set { _name = value; }
			}

			public uint id { get; set; } = 0;
			public AnimLayerType layerType = AnimLayerType.AnimationClip;
			public AvatarMask avatarMask = null;
			public AvatarBodyAnimController owner { get; set; } = null;

			public AvatarAnimationState entryState
			{
				get
				{
					if (_entryState == null)
					{
						_entryState = GetAnimationStateByName("EntryState");
					}

					return _entryState;
				}
			}

			public AvatarAnimationState exitState
			{
				get
				{
					if (_exitState == null)
					{
						_exitState = GetAnimationStateByName("ExitState");
					}

					return _exitState;
				}
			}

			public AvatarAnimationState currentState
			{
				get { return _currentState; }
			}

			//public List<AvatarAnimationState> animationStates { get; private set; } = new List<AvatarAnimationState>();

			#endregion


			#region Public Methods

			//Constructor with native handle.
			internal AvatarAnimationLayer(System.IntPtr nativeHandle_)
			{
				SetNativeHandle(nativeHandle_, false);
				if(!_nativeHandleMap.ContainsKey(nativeHandle_))
				{	
					_nativeHandleMap.Add(nativeHandle_, this);
				}

				
				InitNativeAnimationStates();

				if (pav_AvatarAnimationLayer_SetStateChangedCallback(nativeHandle, OnStateChangedCallback) ==
				    NativeResult.Success)
				{
					_callBackInitiated = true;
				}
			}

			/// <summary>
			/// Set mask of layer
			/// </summary>
			/// <param name="avatarMask_">To mask animation of this layer</param>
			public void SetAvatarMask(AvatarMask avatarMask_)
			{
				if (avatarMask_ == avatarMask)
				{
					return;
				}

				if (avatarMask_ != null)
				{
					avatarMask_.Retain();
				}

				if (avatarMask != null)
				{
					avatarMask.Release();
				}

				avatarMask = avatarMask_;

				if (avatarMask != null)
				{
					pav_AvatarAnimationLayer_SetAvatarMask(nativeHandle, avatarMask_.nativeHandle);
				}
			}

			/// <summary>
			/// Set layer blend mode
			/// </summary>
			/// <param name="blendMode">Blend mode of this layer</param>
			public void SetLayerBlendMode(AnimLayerBlendMode blendMode)
			{
				pav_AvatarAnimationLayer_SetLayerBlendMode(nativeHandle, (uint)blendMode);
			}

			/// <summary>
			///  Set layer blend weight
			/// </summary>
			/// <param name="blendWeight">Blend weight of this layer</param>
			public void SetLayerBlendWeight(float blendWeight)
			{
				pav_AvatarAnimationLayer_SetLayerBlendWeight(nativeHandle, blendWeight);
			}

			/// <summary>
			/// Set animation layer scale, orientation, position enable, if set to false, this layer will not contribute to this property
			/// </summary>
			/// <param name="enableScale">True will set scale animation data of this layer take effect</param>
			/// <param name="enableRotation">True will set rotation animation data of this layer take effect</param>
			/// <param name="enablePosition">True will set position animation data of this layer take effect</param>
			public void SetSRTEnable(bool enableScale, bool enableRotation, bool enablePosition)
			{
				pav_AvatarAnimationLayer_SetSRTEnable(nativeHandle, enableScale, enableRotation, enablePosition);
			}

			/// <summary>
			/// Play animation by name 
			/// </summary>
			/// <param name="animationName">Animation name to play</param>
			/// <param name="loopTime">Loop time of animtion</param>
			/// <param name="speed">Animtioan play speed</param>
			/// <param name="fadeTime">Fade in time from current playing animation</param>
			public void PlayAnimationClip(string animationName, float loopTime, float speed, float fadeTime)
			{
				pav_AvatarAnimationLayer_PlayAnimationClipByName(nativeHandle, animationName, loopTime, speed,
					fadeTime);
			}

			/// <summary>
			/// Stop all animations in this layer
			/// </summary>
			/// <param name="fadeTime">Fade out time of current playing animation</param>
			public void StopAnimation(float fadeTime)
			{
				pav_AvatarAnimationLayer_StopAnimation(nativeHandle, fadeTime);
			}


			//Animation state

			/// <summary>
			/// Play animation state
			/// </summary>
			/// <param name="animationState">Animation state to play</param>
			/// <param name="fadeTime">Fade out time of current playing state</param>
			public void PlayAnimationState(AvatarAnimationState animationState, float fadeTime)
			{
				pav_AvatarAnimationLayer_PlayAnimationState(nativeHandle, animationState.nativeHandle, fadeTime);
			}

			/// <summary>
			/// Create new AvatarAnimationState
			/// </summary>
			/// <param name="stateName">Name of the new state</param>
			/// <returns>AvatarAnimationState created</returns>
			public AvatarAnimationState CreateAnimationStateByName(string stateName)
			{
				var stateHandler = pav_AvatarAnimationLayer_CreateAnimationStateByName(nativeHandle, stateName);
				if (stateHandler != System.IntPtr.Zero)
				{
					AvatarAnimationState animationState = new AvatarAnimationState(stateHandler);
					animationState.Retain();
					//animationStates.Add(animationState);
					_animationStatesMap[animationState.name] = animationState;
					return animationState;
				}

				return null;
			}

			/// <summary>
			/// Get AvatarAnimationState with stateName
			/// </summary>
			/// <param name="stateName">The name of state to query</param>
			/// <returns>AvatarAnimationState with given name, null if not find</returns>
			public AvatarAnimationState GetAnimationStateByName(string stateName)
			{
				if (_animationStatesMap.ContainsKey(stateName))
				{
					return _animationStatesMap[stateName];
				}

				return null;
			}


			/// <summary>
			/// Add state of this layer status change event callback
			/// </summary>
			/// <param name="stateStatus">The status to registe event</param>
			/// <param name="stateCallBack">Callback function</param>
			public void AddStateChangedCallBack(AnimStateStatus stateStatus,
				System.Action<AvatarAnimationLayer, AvatarAnimationState, AvatarAnimationState> callback)
			{
				if (!_callBackInitiated)
				{
					if (pav_AvatarAnimationLayer_SetStateChangedCallback(nativeHandle, OnStateChangedCallback) ==
					    NativeResult.Success)
					{
						_callBackInitiated = true;
					}
				}

				if (_callBackInitiated)
				{
					if (!_stateChangedCallbacks.ContainsKey(nativeHandle))
					{
						_stateChangedCallbacks.Add(nativeHandle,
							new List<System.Action<AvatarAnimationLayer, AvatarAnimationState,
								AvatarAnimationState>>());
					}

					var selfStateChangedCallbacks = _stateChangedCallbacks[nativeHandle];
					selfStateChangedCallbacks.Add(callback);
				}
			}

			protected override void OnDestroy()
			{
				_nativeHandleMap.Remove(nativeHandle);
				_stateChangedCallbacks.Remove(nativeHandle);
				foreach (var animationState in _animationStatesMap)
				{
					animationState.Value.Release();
				}

				_animationStatesMap.Clear();
				//
				base.OnDestroy();

				if (avatarMask != null)
				{
					avatarMask.Release();
					avatarMask = null;
				}
			}

			#endregion


			#region Private Fields

			private string _name = "";
			private AvatarAnimationState _entryState = null;
			private AvatarAnimationState _exitState = null;
			private AvatarAnimationState _currentState = null;
			private bool _callBackInitiated = false;

			private Dictionary<string, AvatarAnimationState> _animationStatesMap =
				new Dictionary<string, AvatarAnimationState>();

			private static Dictionary<System.IntPtr, AvatarAnimationLayer> _nativeHandleMap =
				new Dictionary<System.IntPtr, AvatarAnimationLayer>();

			private static
				Dictionary<System.IntPtr,
					List<System.Action<AvatarAnimationLayer, AvatarAnimationState, AvatarAnimationState>>>
				_stateChangedCallbacks =
					new Dictionary<System.IntPtr,
						List<System.Action<AvatarAnimationLayer, AvatarAnimationState, AvatarAnimationState>>>();

			private void InitNativeAnimationStates()
			{
				uint stateCount = pav_AvatarAnimationLayer_GetAnimationStatesCount(nativeHandle);
				System.IntPtr[] animationStatesNativeHandle = new System.IntPtr[stateCount];
				var gcHandle = GCHandle.Alloc(animationStatesNativeHandle, GCHandleType.Pinned);
				pav_AvatarAnimationLayer_GetAnimationStates(nativeHandle, stateCount, gcHandle.AddrOfPinnedObject());
				for (uint i = 0; i < stateCount; ++i)
				{
					AvatarAnimationState animationState = new AvatarAnimationState(animationStatesNativeHandle[i]);
					animationState.Retain();
					//animationStates.Add(animationState);
					_animationStatesMap[animationState.name] = animationState;
				}

				gcHandle.Free();
				if (entryState != null)
				{
					entryState.isEntryState = true;
				}

				if (exitState != null)
				{
					exitState.isExitState = true;
				}

				StringBuilder stringBuilder = new StringBuilder("", 128);
				pav_AvatarAnimationLayer_GetCurrentStateName(nativeHandle, stringBuilder);
				string name = stringBuilder.ToString();
				_currentState = GetAnimationStateByName(name);
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void StateChangedCallback(System.IntPtr nativeHandle, string fromState, string toState);

			[MonoPInvokeCallback(typeof(StateChangedCallback))]
			private static void OnStateChangedCallback(System.IntPtr nativeHandle, string fromState, string toState)
			{
				if (_nativeHandleMap.ContainsKey(nativeHandle))
				{
					AvatarAnimationLayer layer = _nativeHandleMap[nativeHandle];
					layer._currentState = layer.GetAnimationStateByName(toState);

					if (_stateChangedCallbacks.ContainsKey(nativeHandle))
					{
						var selfStateChangedCallbacks = _stateChangedCallbacks[nativeHandle];
						foreach (var callback in selfStateChangedCallbacks)
						{
							callback.Invoke(layer, layer.GetAnimationStateByName(fromState),
								layer.GetAnimationStateByName(toState));
						}
					}
				}
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_SetLayerBlendMode(System.IntPtr nativeHandle,
				uint blendMode);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_SetLayerBlendWeight(System.IntPtr nativeHandle,
				float blendWeight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_PlayAnimationClipByName(
				System.IntPtr nativeHandle, string animationName, float loopTime, float speed, float fadeTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_StopAnimation(System.IntPtr nativeHandle,
				float fadeTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_SetSRTEnable(System.IntPtr nativeHandle,
				bool enableScale, bool enableRotation, bool enablePosition);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_SetAvatarMask(System.IntPtr nativeHandle,
				System.IntPtr nativeHandleAvatarMask);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_GetName(System.IntPtr nativeHandle,
				StringBuilder layerName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarAnimationLayer_GetAnimationStatesCount(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_GetAnimationStates(System.IntPtr nativeHandle,
				uint stateCount, System.IntPtr nativeHandleStates);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarAnimationLayer_CreateAnimationStateByName(
				System.IntPtr nativeHandle, string stateName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_PlayAnimationState(System.IntPtr nativeHandle,
				System.IntPtr nativeHandleAnimationState, float fadeTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_GetCurrentStateName(System.IntPtr nativeHandle,
				StringBuilder stateName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationLayer_SetStateChangedCallback(
				System.IntPtr nativeHandle, StateChangedCallback stateChangedCallback);

			#endregion
		}
	}
}