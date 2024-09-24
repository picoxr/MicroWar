using UnityEngine;

namespace Pico.Avatar
{
	public class PicoAvatarCamera
	{
		#region Public Properties

		public Camera mainCamera { get; set; } = null;

		/// <summary>
		/// avatar taht the camera bound to. 
		/// </summary>
		public PicoAvatar trakingAvatar
		{
			get => _trackingAvatar;
			set
			{
				_trackingAvatar = value;
				_avatarBodyAnimController = null;
			}
		}

		#endregion


		#region Public Methods

		internal void Initialize()
		{
			_rmiObject = new NativeCalls_AvatarCamera(this, 0);
			_rmiObject.Retain();

			Camera camera = mainCamera ?? Camera.main;

			if (camera != null && _rmiObject != null)
			{
				_rmiObject.SetConfig(camera);
			}
		}

		internal void Destroy()
		{
			if (_rmiObject != null)
			{
				_rmiObject.Release();
				_rmiObject = null;
			}
		}

		internal void PreUpdateFrame()
		{
			Camera camera = mainCamera ?? PicoAvatarApp.instance.currentCullingCamera;
			if (_avatarBodyAnimController == null)
			{
				_avatarBodyAnimController = trakingAvatar?.entity?.bodyAnimController;
			}

			var trans = camera.transform;
			//
			var newPos = trans.position;
			var newOrientation = trans.rotation;

			bool dirty = false;

			//check changed
			if (Vector3.SqrMagnitude(newPos - _lastNativePosition) >
			    PicoAvatarApp.instance.squaredPositionErrorThreshold ||
			    (Quaternion.Dot(newOrientation, _lastNativeRotation) <
			     (1.0f - PicoAvatarApp.instance.optimizationSettings.orientationErrorThreshold)))
			{
				_lastNativePosition = newPos;
				_lastNativeRotation = newOrientation;
				//
				dirty = true;
			}

			if (dirty)
			{
				if (_rmiObject != null)
				{
					_rmiObject.Move(newPos, newOrientation);
				}
			}
		}

		internal void PostUpdateFrame()
		{
			if (trakingAvatar == null)
			{
				return;
			}

			Camera camera = mainCamera ?? PicoAvatarApp.instance.currentCullingCamera;
			if (_avatarBodyAnimController != null)
			{
				XForm cameraXform = _avatarBodyAnimController.GetEyeXForm();
#if UNITY_2021_3_OR_NEWER || UNITY_2022_2_OR_NEWER
				camera.transform.SetLocalPositionAndRotation(cameraXform.position, cameraXform.orientation);
#else
                camera.transform.localPosition = cameraXform.position;
                camera.transform.localRotation = cameraXform.orientation;
#endif
			}
		}

		#endregion


		#region Private Fields

		/// <summary>
		/// NativeAvatarCamera
		/// </summary>
		private NativeCalls_AvatarCamera _rmiObject = null;

		/// <summary>
		/// AvatarBodyAnimController
		/// </summary>
		private AvatarBodyAnimController _avatarBodyAnimController;

		/// <summary>
		/// avatar who the camera will attach to.
		/// </summary>
		private PicoAvatar _trackingAvatar = null;

		private Vector3 _lastNativePosition = Vector3.zero;
		private Quaternion _lastNativeRotation = Quaternion.identity;

		#endregion
	}
}