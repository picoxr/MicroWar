namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for Avatar.
		/// </summary>
		class NativeCall_AvatarBipedIKController : NativeCallProxy
		{
			#region Caller Methods

			public void SetIKEnable(uint ikEffectorType, bool enable)
			{
				var args = this._method_SetIKEnable.invokeArgumentTable;
				//
				args.SetUIntParam(0, ikEffectorType);
				args.SetBoolParam(1, enable);

				this._method_SetIKEnable.DoApply();
			}

			public void SetMaxControllerDistance(float distance)
			{
				var args = this._method_SetMaxControllerDistance.invokeArgumentTable;
				//
				args.SetFloatParam(0, distance);

				this._method_SetMaxControllerDistance.DoApply();
			}

			public void SetUpdateIKTargetFromDevice(uint ikEffectorType, bool enable)
			{
				var args = this._method_SetUpdateIKTargetFromDevice.invokeArgumentTable;
				//
				args.SetUIntParam(0, ikEffectorType);
				args.SetBoolParam(1, enable);

				this._method_SetUpdateIKTargetFromDevice.DoApply();
			}


			public void SetIKAutoStopModeEnable(uint ikAutoStopMode, bool enable)
			{
				var args = this._method_SetIKAutoStopModeEnable.invokeArgumentTable;
				//
				args.SetUIntParam(0, ikAutoStopMode);
				args.SetBoolParam(1, enable);

				this._method_SetIKAutoStopModeEnable.DoApply();
			}

			public void SetIKHandInvalidRegionEnable(bool enable)
			{
				var args = this._method_SetIKHandInvalidRegionEnable.invokeArgumentTable;
				//
				args.SetBoolParam(0, enable);

				this._method_SetIKHandInvalidRegionEnable.DoApply();
			}

			#endregion

			#region Callee Methods

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarBipedIKController(AvatarBipedIKController avatarBipedIKController, uint instanceId)
				: base(instanceId)
			{
				_avatarBipedIKController = avatarBipedIKController;


				/// Callee methods.
				{
				}

				/// Caller methods.
				{
					this._method_SetIKEnable = AddCallerMethod(_attribute_SetIKEnable);

					this._method_SetMaxControllerDistance = AddCallerMethod(_attribute_SetMaxControllerDistance);
					this._method_SetUpdateIKTargetFromDevice = AddCallerMethod(_attribute_SetUpdateIKTargetFromDevice);
					this._method_SetIKAutoStopModeEnable = AddCallerMethod(_attribute_SetIKAutoStopModeEnable);
					this._method_SetIKHandInvalidRegionEnable =
						AddCallerMethod(_attribute_SetIKHandInvalidRegionEnable);
				}
			}

			#region Private Fields

			private AvatarBipedIKController _avatarBipedIKController;
			private NativeCaller _method_SetIKEnable;
			private NativeCaller _method_SetMaxControllerDistance;
			private NativeCaller _method_SetUpdateIKTargetFromDevice;
			private NativeCaller _method_SetIKAutoStopModeEnable;
			private NativeCaller _method_SetIKHandInvalidRegionEnable;

			#endregion

			#region Static Part

			private const string className = "AvatarBipedIKController";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_SetIKEnable =
				new NativeCallerAttribute(className, "SetIKEnable", (uint)0);

			private static NativeCallerAttribute _attribute_SetMaxControllerDistance =
				new NativeCallerAttribute(className, "SetMaxControllerDistance", (uint)0);

			private static NativeCallerAttribute _attribute_SetUpdateIKTargetFromDevice =
				new NativeCallerAttribute(className, "SetUpdateIKTargetFromDevice", (uint)0);

			private static NativeCallerAttribute _attribute_SetIKAutoStopModeEnable =
				new NativeCallerAttribute(className, "SetIKAutoStopModeEnable", (uint)0);

			private static NativeCallerAttribute _attribute_SetIKHandInvalidRegionEnable =
				new NativeCallerAttribute(className, "SetIKHandInvalidRegionEnable", (uint)0);
			/// Callee Attributes.

			#endregion

			#endregion
		}
	}
}