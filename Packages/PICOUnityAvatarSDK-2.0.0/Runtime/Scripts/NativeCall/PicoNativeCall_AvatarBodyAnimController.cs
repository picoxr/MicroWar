namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for Avatar.
		/// </summary>
		class NativeCall_AvatarBodyAnimController : NativeCallProxy
		{
			#region Caller Methods

			public void SetUseBodyTracking(bool useBodyTracking)
			{
				var args = this._method_SetUseBodyTracking.invokeArgumentTable;
				//
				args.SetBoolParam(0, useBodyTracking);

				this._method_SetUseBodyTracking.DoApply();
			}

			public void StartFaceTracker(bool enableLipSync, bool enableFT, bool useXR)
			{
				var args = this._method_StartFaceTracker.invokeArgumentTable;
				//
				args.SetBoolParam(0, enableLipSync);
				args.SetBoolParam(1, enableFT);
				args.SetBoolParam(2, useXR);

				this._method_StartFaceTracker.DoApply();
			}

			public void StopFaceTracker()
			{
				var args = this._method_StopFaceTracker.invokeArgumentTable;
				//

				this._method_StopFaceTracker.DoApply();
			}

			public void SetIdleEnable(bool useIdle)
			{
				var args = this._method_SetIdleEnable.invokeArgumentTable;
				//
				args.SetBoolParam(0, useIdle);

				this._method_SetIdleEnable.DoApply();
			}

			public void PlayThreePointAnimationByPath(string path)
			{
				var args = this._method_PlayThreePointAnimationByPath.invokeArgumentTable;
				//
				args.SetStringParam(0, path);

				this._method_PlayThreePointAnimationByPath.DoApply();
			}

			public void StopThreePointAnimation()
			{
				this._method_StopThreePointAnimation.DoApply();
			}

			#endregion

			#region Callee Methods

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarBodyAnimController(AvatarBodyAnimController avatarBodyAnimController,
				uint instanceId)
				: base(instanceId)
			{
				_avatarBodyAnimController = avatarBodyAnimController;


				/// Callee methods.
				{
				}

				/// Caller methods.
				{
					this._method_SetUseBodyTracking = AddCallerMethod(_attribute_SetUseBodyTracking);
					this._method_StartFaceTracker = AddCallerMethod(_attribute_StartFaceTracker);
					this._method_StopFaceTracker = AddCallerMethod(_attribute_StopFaceTracker);
					this._method_SetIdleEnable = AddCallerMethod(_attribute_SetIdleEnable);
					this._method_PlayThreePointAnimationByPath =
						AddCallerMethod(_attribute_PlayThreePointAnimationByPath);
					this._method_StopThreePointAnimation = AddCallerMethod(_attribute_StopThreePointAnimation);
				}
			}

			#region Private Fields

			private AvatarBodyAnimController _avatarBodyAnimController;
			private NativeCaller _method_SetUseBodyTracking;
			private NativeCaller _method_StartFaceTracker;
			private NativeCaller _method_StopFaceTracker;
			private NativeCaller _method_SetIdleEnable;
			private NativeCaller _method_PlayThreePointAnimationByPath;
			private NativeCaller _method_StopThreePointAnimation;

			#endregion

			#region Static Part

			private const string className = "AvatarBodyAnimController";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_SetUseBodyTracking =
				new NativeCallerAttribute(className, "SetUseBodyTracking", (uint)0);

			private static NativeCallerAttribute _attribute_StartFaceTracker =
				new NativeCallerAttribute(className, "StartFaceTracker", (uint)0);

			private static NativeCallerAttribute _attribute_StopFaceTracker =
				new NativeCallerAttribute(className, "StopFaceTracker", (uint)0);

			private static NativeCallerAttribute _attribute_SetIdleEnable =
				new NativeCallerAttribute(className, "SetIdleEnable", (uint)0);

			private static NativeCallerAttribute _attribute_PlayThreePointAnimationByPath =
				new NativeCallerAttribute(className, "PlayThreePointAnimationByPath", (uint)0);

			private static NativeCallerAttribute _attribute_StopThreePointAnimation =
				new NativeCallerAttribute(className, "StopThreePointAnimation", (uint)0);
			/// Callee Attributes.

			#endregion

			#endregion
		}
	}
}