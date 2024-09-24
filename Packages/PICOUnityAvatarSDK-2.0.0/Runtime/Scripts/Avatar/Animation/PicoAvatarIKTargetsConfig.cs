using UnityEngine;

namespace Pico.Avatar
{
	/// <summary>
	/// avatar ik target settings
	/// </summary>
	public class AvatarIKTargetsConfig
	{
		#region Public Properties

		/// <summary>
		/// IK driver settings, whether use world space or relative space driving
		/// </summary>
		public bool worldSpaceDrive
		{
			get => _worldSpaceDrive;
			set
			{
				_worldSpaceDrive = value;
				_targetDirty = true;
			}
		}


		/// <summary>
		/// IK driver settings, XR Origin GameObject.
		/// </summary>
		public Transform xrRoot
		{
			get => _xrRoot;
			set
			{
				_xrRoot = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, head-mounted pose input source GameObject, default use xr device input
		/// In XR Origin can be set to XR Origin/Camera Offset/Main Camera
		/// </summary>
		public Transform headTarget
		{
			get => _headTarget;
			set
			{
				_headTarget = value;
				_targetDirty = true;
			}
		}


		/// <summary>
		/// IK driver settings, left-hand pose input source, default use xr device input
		/// In XR Origin can be set to XR Origin/Camera Offset/LeftHand Controller
		/// </summary>
		public Transform leftHandTarget
		{
			get => _leftHandTarget;
			set
			{
				_leftHandTarget = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, right hand pose input source, default use xr device input
		/// In XR Origin can be set to XR Origin/Camera Offset/RightHand Controller
		/// </summary>
		public Transform rightHandTarget
		{
			get => _rightHandTarget;
			set
			{
				_rightHandTarget = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, hips pose input source, default use null
		/// Temporarily for internal use only. 
		/// </summary>
		internal Transform hipsTarget
		{
			get => _hipsTarget;
			set
			{
				_hipsTarget = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, left foot pose input source, default use null
		/// Can be set to any custom gameobjects in the scene
		/// </summary>
		public Transform leftFootTarget
		{
			get => _leftFootTarget;
			set
			{
				_leftFootTarget = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, right foot pose input source, default use null
		/// Can be set to any custom gameobjects in the scene
		/// </summary>
		public Transform rightFootTarget
		{
			get => _rightFootTarget;
			set
			{
				_rightFootTarget = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, head input position offset
		/// </summary>
		public Vector3 eyePositionOffset
		{
			get => _eyePositionOffset;
			set
			{
				_eyePositionOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, head input rotation offset
		/// </summary>
		public Quaternion eyeRotationOffset
		{
			get => _eyeRotationOffset;
			set
			{
				_eyeRotationOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, left hand input position offset
		/// </summary>
		public Vector3 leftHandPositionOffset
		{
			get => _leftHandPositionOffset;
			set
			{
				_leftHandPositionOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, left hand input rotation offset
		/// </summary>
		public Quaternion leftHandRotationOffset
		{
			get => _leftHandRotationOffset;
			set
			{
				_leftHandRotationOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, right hand input position offset
		/// </summary>
		public Vector3 rightHandPositionOffset
		{
			get => _rightHandPositionOffset;
			set
			{
				_rightHandPositionOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, right hand input rotation offset
		/// </summary>
		public Quaternion rightHandRotationOffset
		{
			get => _rightHandRotationOffset;
			set
			{
				_rightHandRotationOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, left foot input position offset
		/// </summary>
		public Vector3 leftFootPositionOffset
		{
			get => _leftFootPositionOffset;
			set
			{
				_leftFootPositionOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, left foot input rotation offset
		/// </summary>
		public Quaternion leftFootRotationOffset
		{
			get => _leftFootRotationOffset;
			set
			{
				_leftFootRotationOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, right foot input position offset
		/// </summary>
		public Vector3 rightFootPositionOffset
		{
			get => _rightFootPositionOffset;
			set
			{
				_rightFootPositionOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, right foot input rotation offset
		/// </summary>
		public Quaternion rightFootRotationOffset
		{
			get => _rightFootRotationOffset;
			set
			{
				_rightFootRotationOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, hips input position offset
		/// Temporarily for internal use only. 
		/// </summary>
		public Vector3 hipsPositionOffset
		{
			get => _hipsPositionOffset;
			set
			{
				_hipsPositionOffset = value;
				_targetDirty = true;
			}
		}

		/// <summary>
		/// IK driver settings, hips input rotation offset
		/// Temporarily for internal use only. 
		/// </summary>
		public Quaternion hipsRotationOffset
		{
			get => _hipsRotationOffset;
			set
			{
				_hipsRotationOffset = value;
				_targetDirty = true;
			}
		}

		#endregion

		#region Public Methods

		internal void SetDefault()
		{
			_eyePositionOffset = new Vector3(0.0f, 0.1f, 0.1f);
			_eyeRotationOffset = Quaternion.identity;

			leftHandPositionOffset = new Vector3(0.036f, 0.041f, 0.081f);
			leftHandRotationOffset = Quaternion.AngleAxis(20f, Vector3.left);

			rightHandPositionOffset = new Vector3(-0.036f, 0.041f, 0.081f);
			rightHandRotationOffset = Quaternion.AngleAxis(20f, Vector3.left);

			_leftFootPositionOffset = Vector3.zero;
			_leftFootRotationOffset = Quaternion.identity;

			_rightFootPositionOffset = Vector3.zero;
			_rightFootRotationOffset = Quaternion.identity;

			_hipsPositionOffset = Vector3.zero;
			_hipsRotationOffset = Quaternion.identity;
		}

		internal void SetTargetDirty(bool dirty)
		{
			_targetDirty = dirty;
		}

		internal bool IsTargetDirty()
		{
			return _targetDirty;
		}

		#endregion

		#region Private Fields

		// Ik targets
		private bool _worldSpaceDrive = true;

		private Transform _xrRoot = null;
		private Transform _headTarget = null;
		private Transform _leftHandTarget = null;
		private Transform _rightHandTarget = null;
		private Transform _hipsTarget = null;
		private Transform _rightFootTarget = null;
		private Transform _leftFootTarget = null;

		private Vector3 _eyePositionOffset = new Vector3(0.0f, 0.1f, 0.1f);
		private Quaternion _eyeRotationOffset = Quaternion.identity;

		private Vector3 _leftHandPositionOffset = new Vector3(0.036f, 0.041f, 0.081f);
		private Quaternion _leftHandRotationOffset = Quaternion.AngleAxis(20f, Vector3.left);

		private Vector3 _rightHandPositionOffset = new Vector3(-0.036f, 0.041f, 0.081f);
		private Quaternion _rightHandRotationOffset = Quaternion.AngleAxis(20f, Vector3.left);

		private Vector3 _leftFootPositionOffset = Vector3.zero;
		private Quaternion _leftFootRotationOffset = Quaternion.identity;

		private Vector3 _rightFootPositionOffset = Vector3.zero;
		private Quaternion _rightFootRotationOffset = Quaternion.identity;

		private Vector3 _hipsPositionOffset = Vector3.zero;
		private Quaternion _hipsRotationOffset = Quaternion.identity;

		private bool _targetDirty = true;

		#endregion
	}
}