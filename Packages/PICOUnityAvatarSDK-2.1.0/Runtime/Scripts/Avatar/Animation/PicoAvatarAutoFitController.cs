using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using AOT;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Automatically moves camera/controller to fit user setting or status
		/// </summary>
		public class AvatarAutoFitController : NativeObject
		{
			#region Public Properties

			public AvatarBodyAnimController owner;
			public Vector3 avatarOffset;


			public struct AvatarAutoFitParam
			{
				public AvatarAutoFitParam(float maxFloatingTime_, float maxCrouchingTime_, float crouchingDistance_,
					float maxCrouchingDistance_)
				{
					maxFloatingTime = maxFloatingTime_;
					maxCrouchingTime = maxCrouchingTime_;
					crouchingDistance = crouchingDistance_;
					maxCrouchingDistance = maxCrouchingDistance_;
				}

				public float maxFloatingTime;
				public float maxCrouchingTime;
				public float crouchingDistance;
				public float maxCrouchingDistance;
			}
			//param presets

			public AvatarAutoFitParam presetSitting = new AvatarAutoFitParam(1.0f, 3.0f, 0.15f, 0.3f);
			public AvatarAutoFitParam presetStanding = new AvatarAutoFitParam(1.0f, 3.0f, 0.2f, 0.7f);

			public bool localAvatarHeightFittingEnable
			{
				get { return _localAvatarHeightFittingEnable; }
				set
				{
					_localAvatarHeightFittingEnable = value;
					pav_AvatarAutoFitController_SetLocalAvatarHeightFittingEnable(nativeHandle,
						_localAvatarHeightFittingEnable);
				}
			}

			#endregion


			#region Public Methods

			internal AvatarAutoFitController(System.IntPtr nativeHandler_, AvatarBodyAnimController owner_)
			{
				SetNativeHandle(nativeHandler_, false);
				_nativeHandleMap.Add(nativeHandler_, this);
				owner = owner_;
			}

			/// <summary>
			/// Perform avatar height offset auto fitting in late update
			/// </summary>
			public void UpdateAvatarHeightOffset()
			{
				_needUpdateOffset = true;
			}

			/// <summary>
			/// Set current avatar offset
			/// </summary>
			/// <param name="currentOffset">Offset value</param>
			public void SetCurrentAvatarOffset(Vector3 currentOffset)
			{
				avatarOffset = currentOffset;
				pav_AvatarAutoFitController_SetCurrentAvatarOffset(nativeHandle, ref currentOffset);
			}

			/// <summary>
			/// Get avatar offset calculated by sdk
			/// </summary>
			/// <returns>Current offset value</returns>
			public Vector3 GetAvatarOffset()
			{
				Vector3 avatarOffset = new Vector3();
				pav_AvatarAutoFitController_GetAvatarOffset(nativeHandle, ref avatarOffset);
				return avatarOffset;
			}

			/// <summary>
			/// Set avatar target hips height.
			/// Auto fit controller will adjust avatar offset to keep avatar hips position around target height. If set to negative value, will use avatar default standing height
			/// </summary>
			/// <param name="targetHipsHeight">Target hips height</param>
			public void SetTargetHipsHeight(float targetHipsHeight)
			{
				pav_AvatarAutoFitController_SetTargetHipsHeight(nativeHandle, targetHipsHeight);
			}

			/// <summary>
			/// Set avatar max floating time.
			/// If current height is larger than avatar target height for max floating time, Auto fit controller will recalculate avatar offset to move avatar to ground, it does not take effect if maxFloatingTime set to negative value.
			/// </summary>
			/// <param name="maxFloatingTime">The time threshold for Avatar's feet off the ground in seconds.</param>
			public void SetMaxFloatingTime(float maxFloatingTime)
			{
				_maxFloatingTime = maxFloatingTime;
				pav_AvatarAutoFitController_SetMaxFloatingTime(nativeHandle, maxFloatingTime);
			}

			/// <summary>
			/// Set avatar max crouching time.
			/// If avatar crouches deeper than crouching distance max crouching time, Auto fit controller will recalculate avatar offset to make avatar stand on the ground, it does not take effect if maxFloatingTime set to negative value.
			/// </summary>
			/// <param name="maxCrouchingTime">The crouching time threshold for Avatar in seconds</param>
			public void SetMaxCrouchingTime(float maxCrouchingTime)
			{
				_maxCrouchingTime = maxCrouchingTime;
				pav_AvatarAutoFitController_SetMaxCrouchingTime(nativeHandle, maxCrouchingTime);
			}

			/// <summary>
			/// Set avatar crouching distance.
			/// If the avatar hips moves down more than crouching distance, this avatar will be considered as crouching, height auto fit will be triggered according to max crouching time, it does not take effect if crouchingDistance set to negative value.
			/// </summary>
			/// <param name="crouchingDistance">The distance threshold for the crouching state of Avatar in meters</param>
			public void SetCrouchingDistance(float crouchingDistance)
			{
				_crouchingDistance = crouchingDistance;
				pav_AvatarAutoFitController_SetCrouchingDistance(nativeHandle, crouchingDistance);
			}


			/// <summary>
			/// Set avatar max crouching distance.\n
			/// If the avatar hips moves down more than max crouching distance, height auto fit will be triggered immediately.
			/// </summary>
			/// <param name="maxCrouchingDistance">Max distance can crouch before auto fit trigger</param>
			public void SetMaxCrouchingDistance(float maxCrouchingDistance)
			{
				_maxCrouchingDistance = maxCrouchingDistance;
				pav_AvatarAutoFitController_SetMaxCrouchingDistance(nativeHandle, maxCrouchingDistance);
			}

			/// <summary>
			/// Set avatar auto fit params using preset
			/// </summary>
			/// <param name="autoFitParam">AutoFit preset value</param>
			public void ApplyPreset(AvatarAutoFitParam autoFitParam)
			{
				SetMaxFloatingTime(autoFitParam.maxFloatingTime);
				SetMaxCrouchingTime(autoFitParam.maxCrouchingTime);
				SetCrouchingDistance(autoFitParam.crouchingDistance);
				SetMaxCrouchingDistance(autoFitParam.maxCrouchingDistance);
			}

			/// <summary>
			/// Set callback information to monitor height calibration.
			/// When the sdk internal height Self-Adaptation is triggered, this callback is called and returns the latest coordinates of the head (camera).
			/// </summary>
			/// <param name="callback">Action will be invoked</param>
			public void AddAvatarOffsetChangedCallback(System.Action<AvatarAutoFitController, Vector3> callback)
			{
				if (!_callBackInitiated)
				{
					if (pav_AvatarAutoFitController_SetAvatarOffsetChangedCallback(nativeHandle,
						    OnAvatarOffsetChangedCallback) == NativeResult.Success)
					{
						_callBackInitiated = true;
					}
				}

				if (_callBackInitiated)
				{
					if (!_avatarOffsetChangedCallbacks.ContainsKey(nativeHandle))
					{
						_avatarOffsetChangedCallbacks.Add(nativeHandle,
							new List<System.Action<AvatarAutoFitController, Vector3>>());
					}

					var selfAvatarOffsetChangedCallbacks = _avatarOffsetChangedCallbacks[nativeHandle];
					selfAvatarOffsetChangedCallbacks.Add(callback);
				}
			}

			/// <summary>
			/// Remove callback information to monitor height calibration.
			/// </summary>
			/// <param name="callback">Action to be removed</param>
			public void ClearAvatarOffsetChangedCallback(System.Action<AvatarAutoFitController, Vector3> callback)
			{
				if (!_callBackInitiated)
					return;
				if (!_avatarOffsetChangedCallbacks.ContainsKey(nativeHandle))
					return;
				var selfAvatarOffsetChangedCallbacks = _avatarOffsetChangedCallbacks[nativeHandle];
				selfAvatarOffsetChangedCallbacks.Remove(callback);
			}


			internal void UpdateFrame()
			{
				//Invoke callbacks
				if (_avatarOffsetChangedCallbackToInvoke.ContainsKey(nativeHandle) &&
				    _avatarOffsetChangedCallbacks.ContainsKey(nativeHandle))
				{
					var selfAvatarOffsetChangedCallbacks = _avatarOffsetChangedCallbacks[nativeHandle];
					foreach (var callback in selfAvatarOffsetChangedCallbacks)
					{
						callback.Invoke(_nativeHandleMap[nativeHandle],
							_avatarOffsetChangedCallbackToInvoke[nativeHandle]);
					}

					_avatarOffsetChangedCallbackToInvoke.Remove(nativeHandle);
				}
			}


			internal void LateUpdateFrame()
			{
				if (_needUpdateOffset == true)
				{
					pav_AvatarAutoFitController_UpdateAvatarHeightOffset(nativeHandle);
					_needUpdateOffset = false;
				}
			}

			protected override void OnDestroy()
			{
				_avatarOffsetChangedCallbacks.Remove(nativeHandle);
				_avatarOffsetChangedCallbackToInvoke.Remove(nativeHandle);
				_nativeHandleMap.Remove(nativeHandle);
				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			private bool _localAvatarHeightFittingEnable = false;
			private float _maxFloatingTime = 1.0f;
			private float _maxCrouchingTime = 3.0f;
			private float _crouchingDistance = 0.2f;
			private float _maxCrouchingDistance = 0.7f;

			private bool _callBackInitiated = false;
			private bool _needUpdateOffset = false;

			// Record callback functions.
			private static Dictionary<System.IntPtr, List<System.Action<AvatarAutoFitController, Vector3>>>
				_avatarOffsetChangedCallbacks =
					new Dictionary<System.IntPtr, List<System.Action<AvatarAutoFitController, Vector3>>>();

			// Record callback data (avatarOffset).
			private static Dictionary<System.IntPtr, Vector3> _avatarOffsetChangedCallbackToInvoke =
				new Dictionary<System.IntPtr, Vector3>();

			// Record nativeHandler to avatarAutoFitController.
			private static Dictionary<System.IntPtr, AvatarAutoFitController> _nativeHandleMap =
				new Dictionary<System.IntPtr, AvatarAutoFitController>();

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
			internal delegate void AvatarOffsetChangedCallback(System.IntPtr nativeHandle, Vector3 avatarOffset);

			[MonoPInvokeCallback(typeof(AvatarOffsetChangedCallback))]
			private static void OnAvatarOffsetChangedCallback(System.IntPtr nativeHandle, Vector3 avatarOffset)
			{
				//if (_avatarOffsetChangedCallbacks.ContainsKey(nativeHandle))
				//{
				//    var selfAvatarOffsetChangedCallbacks = _avatarOffsetChangedCallbacks[nativeHandle];
				//    foreach (var callback in selfAvatarOffsetChangedCallbacks)
				//    {
				//        callback.Invoke(_nativeHandleMap[nativeHandle], avatarOffset);
				//    }
				//}
				if (!_avatarOffsetChangedCallbackToInvoke.ContainsKey(nativeHandle))
				{
					_avatarOffsetChangedCallbackToInvoke.Add(nativeHandle, avatarOffset);
				}
				else
				{
					_avatarOffsetChangedCallbackToInvoke[nativeHandle] = avatarOffset;
				}
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_UpdateAvatarHeightOffset(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_GetAvatarOffset(System.IntPtr nativeHandle,
				ref Vector3 avatarOffset);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetCurrentAvatarOffset(
				System.IntPtr nativeHandle, ref Vector3 currentOffset);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetLocalAvatarHeightFittingEnable(
				System.IntPtr nativeHandle, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetTargetHipsHeight(
				System.IntPtr nativeHandle, float targetHipsHeight);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetMaxFloatingTime(
				System.IntPtr nativeHandle, float maxFloatingTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetMaxCrouchingTime(
				System.IntPtr nativeHandle, float maxCrouchingTime);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetCrouchingDistance(
				System.IntPtr nativeHandle, float crouchingDistance);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetMaxCrouchingDistance(
				System.IntPtr nativeHandle, float maxCrouchingDistance);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAutoFitController_SetAvatarOffsetChangedCallback(
				System.IntPtr nativeHandle, AvatarOffsetChangedCallback callback);

			#endregion
		}
	}
}