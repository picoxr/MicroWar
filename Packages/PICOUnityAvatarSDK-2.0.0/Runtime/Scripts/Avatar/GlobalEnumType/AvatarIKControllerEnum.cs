namespace Pico.Avatar
{
	/// <summary>
	/// Avatar IK Effector type. Used to identify ik targers.
	/// </summary>
	public enum IKEffectorType : int
	{
		/// <summary>
		/// Root IKEffectorType
		/// </summary>
		Root = 0,

		/// <summary>
		/// Head IKEffectorType
		/// </summary>
		Head = 1,

		/// <summary>
		/// LeftHand IKEffectorType
		/// </summary>
		LeftHand = 2,

		/// <summary>
		/// RightHand IKEffectorType
		/// </summary>
		RightHand = 3,

		/// <summary>
		/// Hips IKEffectorType
		/// </summary>
		Hips = 4,

		/// <summary>
		/// LeftFoot IKEffectorType
		/// </summary>
		LeftFoot = 5,

		/// <summary>
		/// RightFoot IKEffectorType
		/// </summary>
		RightFoot = 6,

		Count
	};


	/// <summary>
	/// Avatar IK Effector tracking source.
	/// Default: head and hand effectors follow Device input, foot effectors follow animation.
	/// You can specify custom ik targets by setting Custom IKTrackingSource as well.
	/// </summary>
	public enum IKTrackingSource : int
	{
		/// <summary>
		/// By deviece
		/// </summary>
		DeviceInput = 0,

		/// <summary>
		/// By animation
		/// </summary>
		Animation = 1,

		/// <summary>
		/// By custom
		/// </summary>
		Custom = 2,

		/// <summary>
		/// By defaultLocomotion
		/// </summary>
		DefaultLocomotion = 3
	}

	/// <summary>
	/// Avatar IK auto stop mode, used to stop IK at some certain conditions
	/// </summary>
	public enum IKAutoStopMode : int
	{
		/// <summary>
		/// ControllerDisconnect
		/// </summary>
		ControllerDisconnect = 1,

		/// <summary>
		/// ControllerIdle
		/// </summary>
		ControllerIdle = 2,

		/// <summary>
		/// ControllerLoseTracking
		/// </summary>
		ControllerLoseTracking = 3,

		/// <summary>
		/// ControllerFarAway =
		/// </summary>
		ControllerFarAway = 4,
		Count
	}
}