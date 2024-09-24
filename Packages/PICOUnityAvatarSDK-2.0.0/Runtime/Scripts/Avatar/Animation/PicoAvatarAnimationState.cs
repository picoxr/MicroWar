using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using AOT;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar animation state, used to play a set of animation clips
		/// </summary>
		/// @note
		/// Supports single or multiple clips
		public class AvatarAnimationState : NativeObject
		{
			#region Public Properties

			public string name
			{
				get
				{
					if (_name == "")
					{
						StringBuilder stringBuilder = new StringBuilder("", 128);
						pav_AvatarAnimationState_GetName(nativeHandle, stringBuilder);
						_name = stringBuilder.ToString();
					}

					return _name;
				}
				set { _name = value; }
			}

			public AvatarBodyAnimController owner { get; set; } = null;

			public AvatarAnimationBlendTree blendTree
			{
				get => _blendTree;
			}


			public AnimStateStatus status;
			public bool isEntryState;
			public bool isExitState;

			#endregion


			#region Public Methods

			//Constructor with native handle.
			internal AvatarAnimationState(System.IntPtr nativeHandle_)
			{
				SetNativeHandle(nativeHandle_, false);
				_nativeHandleMap.Add(nativeHandle_, this);
			}

			/// <summary>
			/// Set state play speed
			/// </summary>
			/// <param name="speed">State play speed</param>
			public void SetSpeed(float speed)
			{
				pav_AvatarAnimationState_SetSpeed(nativeHandle, speed);
			}

			/// <summary>
			/// Get state duration
			/// </summary>
			/// <returns>State duration</returns>
			public float GetDuration()
			{
				return pav_AvatarAnimationState_GetDuration(nativeHandle);
			}

			/// <summary>
			/// Get state elapsed time
			/// </summary>
			/// <returns>Elapsed time</returns>
			public float GetElapsedTime()
			{
				return pav_AvatarAnimationState_GetElapsedTime(nativeHandle);
			}

			/// <summary>
			/// Set state wrap mode/loop time
			/// </summary>
			/// <param name="wrapMode">
			/// state wrap mode : \n
			/// if > 0: repeat time \n
			/// if == 0: LOOP \n
			/// if == -1(PINGPONG): PINGPONG \n
			/// if == -2(CLAMP_FOREVER): play once and repeat last frame \n
			/// if == -3(SEEK): seek to a frame and play the frame \n
			/// </param>
			public void SetWrapMode(float wrapMode)
			{
				pav_AvatarAnimationState_SetWrapMode(nativeHandle, wrapMode);
			}

			/// <summary>
			/// Seek to time
			/// </summary>
			/// <param name="seekTime">Time to seek</param>
			public void Seek(float seekTime)
			{
				pav_AvatarAnimationState_Seek(nativeHandle, seekTime);
			}


			/// <summary>
			/// Set state start time
			/// </summary>
			/// <param name="startTime">State start time</param>
			public void SetStartTime(float startTime)
			{
				pav_AvatarAnimationState_SetStartTime(nativeHandle, startTime);
			}


			/// <summary>
			/// Add animation clip to the state. You can add clip directly to the state if the state only contains a single clip, if multiple clips needed, please create blendTree for the state and add clips to the blendTree.
			/// </summary>
			/// <param name="animationName">Animation clip name</param>
			public void AddAnimationClip(string animationName)
			{
				pav_AvatarAnimationState_AddAnimazClip(nativeHandle, animationName);
			}

			/// <summary>
			/// Create blend tree for the state
			/// </summary>
			/// <param name="blendTreeType">Blend tree type</param>
			public void CreateBlendTree(AnimBlendTreeType blendTreeType)
			{
				if (blendTree != null)
				{
					blendTree.Release();
				}

				_blendTree = null;
				var blendTreeNativeHandle = pav_AvatarAnimationState_CreateBlendTree(nativeHandle, (uint)blendTreeType);
				if (blendTreeNativeHandle != System.IntPtr.Zero)
				{
					_blendTree = new AvatarAnimationBlendTree(blendTreeNativeHandle, blendTreeType);
					_blendTree.Retain();
					_blendTree.owner = this;
				}
			}

			/// <summary>
			/// Create transition from current state to dstState
			/// </summary>
			/// <param name="dstState">Transition destination state</param>
			/// <returns>The created AvatarAnimationStateTransition</returns>
			public AvatarAnimationStateTransition AddTransition(AvatarAnimationState dstState)
			{
				var transitionNativeHandle =
					pav_AvatarAnimationState_AddTransition(nativeHandle, dstState.nativeHandle);
				if (transitionNativeHandle != System.IntPtr.Zero)
				{
					AvatarAnimationStateTransition transition =
						new AvatarAnimationStateTransition(transitionNativeHandle);
					transition.Retain();
					transition.srcState = this;
					transition.dstState = dstState;
					if (isEntryState)
					{
						transition.SetEntryTransition(true);
					}

					_transitions.Add(transition);
					return transition;
				}

				return null;
			}

			/// <summary>
			/// Add state status change event callback
			/// </summary>
			/// <param name="stateStatus">The status to registe event</param>
			/// <param name="stateCallBack">Callback function</param>
			public void AddStateStatusCallBack(AnimStateStatus stateStatus,
				System.Action<AvatarAnimationState> stateCallBack)
			{
				if (!_callBackInitiated)
				{
					if (pav_AvatarAnimationState_SetAnimationStateCallBack(nativeHandle, OnStateStatusCallback) ==
					    NativeResult.Success)
					{
						_callBackInitiated = true;
					}
				}

				switch (stateStatus)
				{
					case AnimStateStatus.StateEnter:
						if (!_stateEnterCallbacks.ContainsKey(nativeHandle))
						{
							_stateEnterCallbacks.Add(nativeHandle, new List<System.Action<AvatarAnimationState>>());
						}

						var selfStateEnterCallbacks = _stateEnterCallbacks[nativeHandle];
						selfStateEnterCallbacks.Add(stateCallBack);
						break;
					case AnimStateStatus.StateEnd:
						if (!_stateEndCallbacks.ContainsKey(nativeHandle))
						{
							_stateEndCallbacks.Add(nativeHandle, new List<System.Action<AvatarAnimationState>>());
						}

						var selfStateEndCallbacks = _stateEndCallbacks[nativeHandle];
						selfStateEndCallbacks.Add(stateCallBack);
						break;
					case AnimStateStatus.StateLeave:
						if (!_stateLeaveCallbacks.ContainsKey(nativeHandle))
						{
							_stateLeaveCallbacks.Add(nativeHandle, new List<System.Action<AvatarAnimationState>>());
						}

						var selfStateLeaveCallbacks = _stateLeaveCallbacks[nativeHandle];
						selfStateLeaveCallbacks.Add(stateCallBack);
						break;
				}
			}

			protected override void OnDestroy()
			{
				_stateEnterCallbacks.Remove(nativeHandle);
				_stateEndCallbacks.Remove(nativeHandle);
				_stateLeaveCallbacks.Remove(nativeHandle);
				_nativeHandleMap.Remove(nativeHandle);

				//blendTree.Release();
				ReleaseField(ref _blendTree);
				_blendTree = null;

				foreach (var transition in _transitions)
				{
					transition.Release();
				}

				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			private string _name = "";
			private bool _callBackInitiated = false;
			private AvatarAnimationBlendTree _blendTree = null;
			private List<AvatarAnimationStateTransition> _transitions = new List<AvatarAnimationStateTransition>();

			private static Dictionary<System.IntPtr, List<System.Action<AvatarAnimationState>>> _stateEnterCallbacks =
				new Dictionary<System.IntPtr, List<System.Action<AvatarAnimationState>>>();

			private static Dictionary<System.IntPtr, List<System.Action<AvatarAnimationState>>> _stateEndCallbacks =
				new Dictionary<System.IntPtr, List<System.Action<AvatarAnimationState>>>();

			private static Dictionary<System.IntPtr, List<System.Action<AvatarAnimationState>>> _stateLeaveCallbacks =
				new Dictionary<System.IntPtr, List<System.Action<AvatarAnimationState>>>();

			private static Dictionary<System.IntPtr, AvatarAnimationState> _nativeHandleMap =
				new Dictionary<System.IntPtr, AvatarAnimationState>();

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void StateStatusCallback(uint stateStatus, System.IntPtr nativeHandle);

			[MonoPInvokeCallback(typeof(StateStatusCallback))]
			private static void OnStateStatusCallback(uint stateStatus, System.IntPtr nativeHandle)
			{
				switch ((AnimStateStatus)stateStatus)
				{
					case AnimStateStatus.StateEnter:
						if (_stateEnterCallbacks.ContainsKey(nativeHandle))
						{
							var selfStateEnterCallbacks = _stateEnterCallbacks[nativeHandle];
							foreach (var callback in selfStateEnterCallbacks)
							{
								callback.Invoke(_nativeHandleMap[nativeHandle]);
							}
						}

						break;
					case AnimStateStatus.StateEnd:
						if (_stateEndCallbacks.ContainsKey(nativeHandle))
						{
							var selfStateEndCallbacks = _stateEndCallbacks[nativeHandle];
							foreach (var callback in selfStateEndCallbacks)
							{
								callback.Invoke(_nativeHandleMap[nativeHandle]);
							}
						}

						break;
					case AnimStateStatus.StateLeave:
						if (_stateLeaveCallbacks.ContainsKey(nativeHandle))
						{
							var selfStateLeaveCallbacks = _stateLeaveCallbacks[nativeHandle];
							foreach (var callback in selfStateLeaveCallbacks)
							{
								callback.Invoke(_nativeHandleMap[nativeHandle]);
							}
						}

						break;
				}
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationState_GetName(System.IntPtr nativeHandle,
				StringBuilder stateName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationState_SetSpeed(System.IntPtr nativeHandle,
				float speed);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationState_SetWrapMode(System.IntPtr nativeHandle,
				float wrapMode);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationState_SetStartTime(System.IntPtr nativeHandle,
				float startTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarAnimationState_GetDuration(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarAnimationState_GetElapsedTime(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult
				pav_AvatarAnimationState_Seek(System.IntPtr nativeHandle, float seekTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationState_AddAnimazClip(System.IntPtr nativeHandle,
				string animationName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarAnimationState_CreateBlendTree(System.IntPtr nativeHandle,
				uint blendTreeType);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarAnimationState_AddTransition(System.IntPtr nativeHandle,
				System.IntPtr dstNativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationState_SetAnimationStateCallBack(
				System.IntPtr nativeHandle, StateStatusCallback stateStatusCallback);

			#endregion
		}
	}
}