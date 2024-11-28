using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		public class NativeCalls_AvatarCamera : NativeCallProxy
		{
			#region Caller Methods

			public void Move(Vector3 pos, Quaternion quat)
			{
				var args = this._method_Move.invokeArgumentTable;
				//
				args.SetPointParam(0, pos);
				args.SetQuaternionParam(1, quat);
				//
				this._method_Move.DoApply();
			}

			public void SetConfig(Camera camera)
			{
				if (camera == null)
					return;

				var args = this._method_SetConfig.invokeArgumentTable;
				//
				args.SetFloatParam(0, camera.fieldOfView);
				args.SetFloatParam(1, camera.aspect);
				args.SetFloatParam(2, camera.nearClipPlane);
				args.SetFloatParam(3, camera.farClipPlane);
				args.SetFloatParam(4, camera.stereoSeparation);
				//
				args.SetPointParam(5, Vector3.up);
				args.SetPointParam(6, Vector3.left);
				args.SetPointParam(7, Vector3.forward);
				//
				this._method_SetConfig.DoApply();
			}

			#endregion

			#region Callee Methods

			#endregion

			#region NativeCall Framework Methods/Fields

			public NativeCalls_AvatarCamera(PicoAvatarCamera camera, uint instanceId)
				: base(0)
			{
				_avatarCamera = camera;

				/// Callee methods.
				{
					// AddCalleeMethod(_attribute_OnLodChanged, OnLodChanged);
				}

				/// Caller methods.
				{
					this._method_Move = AddCallerMethod(_attribute_Move);
					this._method_SetConfig = AddCallerMethod(_attribute_SetConfig);
				}
			}

			#region Private Fields

			private PicoAvatarCamera _avatarCamera;
			private NativeCaller _method_Move;
			private NativeCaller _method_SetConfig;

			#endregion

			#region Static Part

			private const string className = "AvatarCamera";

			/// Caller Attributes.
			private static NativeCallerAttribute
				_attribute_Move = new NativeCallerAttribute(className, "Move", (uint)0);

			private static NativeCallerAttribute _attribute_SetConfig =
				new NativeCallerAttribute(className, "SetConfig", (uint)0);
			/// Callee Attributes.

			#endregion

			#endregion
		}
	}
}