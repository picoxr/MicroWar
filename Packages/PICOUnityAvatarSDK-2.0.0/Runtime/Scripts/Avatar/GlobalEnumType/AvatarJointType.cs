namespace Pico.Avatar
{
	/// <summary>
	/// avatar Joint type. Used to identify joint in game logic.
	/// </summary>
	/// <remarks>
	/// Application can query joint xform with JointType. pico maintains a 
	/// conversion table from joint name to JointType.
	/// </remarks>
	public enum JointType : int
	{
		/// <summary>
		/// Invalid
		/// </summary>
		Invalid = -1,

		/// <summary>
		/// Root
		/// </summary>
		Root = 0,

		/// <summary>
		/// RootScale
		/// </summary>
		RootScale = 1,

		/// <summary>
		/// Hips
		/// </summary>
		Hips = 2,

		/// <summary>
		/// SpineLower
		/// </summary>
		SpineLower = 3,

		/// <summary>
		/// SpineMiddle
		/// </summary>
		SpineMiddle = 4,

		/// <summary>
		/// SpineUpper
		/// </summary>
		SpineUpper = 5,

		/// <summary>
		/// Chest
		/// </summary>
		Chest = 6,

		/// <summary>
		/// Neck
		/// </summary>
		Neck = 7,

		/// <summary>
		/// Head
		/// </summary>
		Head = 8,

		/// <summary>
		/// Hair
		/// </summary>
		Hair = 9,

		/// <summary>
		/// LeftLegUpper
		/// </summary>
		LeftLegUpper = 20,

		/// <summary>
		/// LeftLegLower
		/// </summary>
		LeftLegLower = 21,

		/// <summary>
		/// LeftFootAnkle
		/// </summary>
		LeftFootAnkle = 22,

		/// <summary>
		/// LeftToe
		/// </summary>
		LeftToe = 23,

		/// <summary>
		/// LeftToeEnd
		/// </summary>
		LeftToeEnd = 24,

		/// <summary>
		/// RightLegUpper
		/// </summary>
		RightLegUpper = 25,

		/// <summary>
		/// RightLegLower
		/// </summary>
		RightLegLower = 26,

		/// <summary>
		/// RightFootAnkle
		/// </summary>
		RightFootAnkle = 27,

		/// <summary>
		/// RightToe
		/// </summary>
		RightToe = 28,

		/// <summary>
		/// RightToeEnd
		/// </summary>
		RightToeEnd = 29,

		/// <summary>
		/// LeftShoulder
		/// </summary>
		LeftShoulder = 30,

		/// <summary>
		/// LeftArmUpper
		/// </summary>
		LeftArmUpper = 31,

		/// <summary>
		/// LeftArmUpperTwist
		/// </summary>
		LeftArmUpperTwist = 33,

		/// <summary>
		/// LeftArmLower
		/// </summary>
		LeftArmLower = 34,

		/// <summary>
		/// LeftHandTwist
		/// </summary>
		LeftHandTwist = 35,

		/// <summary>
		/// LeftHandTwist2
		/// </summary>
		LeftHandTwist2 = 36,

		/// <summary>
		/// LeftHandWrist
		/// </summary>
		LeftHandWrist = 37,

		/// <summary>
		/// RightShoulder
		/// </summary>
		RightShoulder = 38,

		/// <summary>
		/// RightArmUpper
		/// </summary>
		RightArmUpper = 39,

		/// <summary>
		/// RightArmUpperTwist
		/// </summary>
		RightArmUpperTwist = 40,

		/// <summary>
		/// RightArmLower
		/// </summary>
		RightArmLower = 41,

		/// <summary>
		/// RightHandTwist
		/// </summary>
		RightHandTwist = 42,

		/// <summary>
		/// RightHandTwist2
		/// </summary>
		RightHandTwist2 = 43,

		/// <summary>
		/// RightHandWrist
		/// </summary>
		RightHandWrist = 44,

		/// <summary>
		/// BasicJointCount
		/// </summary>
		BasicJointCount = 45,

		/// <summary>
		/// LeftHandThumbTrapezium
		/// </summary>
		LeftHandThumbTrapezium = 50,

		/// <summary>
		/// LeftHandThumbMetacarpal
		/// </summary>
		LeftHandThumbMetacarpal = 51,

		/// <summary>
		/// LeftHandThumbProximal
		/// </summary>
		LeftHandThumbProximal = 52,

		/// <summary>
		/// LeftHandThumbDistal
		/// </summary>
		LeftHandThumbDistal = 53,

		/// <summary>
		/// LeftHandThumbTip
		/// </summary>
		LeftHandThumbTip = 54,

		/// <summary>
		/// LeftHandIndexMetacarpal
		/// </summary>
		LeftHandIndexMetacarpal = 55,

		/// <summary>
		/// LeftHandIndexProximal
		/// </summary>
		LeftHandIndexProximal = 56,

		/// <summary>
		/// LeftHandIndexIntermediate
		/// </summary>
		LeftHandIndexIntermediate = 57,

		/// <summary>
		/// LeftHandIndexDistal
		/// </summary>
		LeftHandIndexDistal = 58,

		/// <summary>
		/// LeftHandIndexTip
		/// </summary>
		LeftHandIndexTip = 59,

		/// <summary>
		/// LeftHandMiddleMetacarpal
		/// </summary>
		LeftHandMiddleMetacarpal = 60,

		/// <summary>
		/// LeftHandMiddleProximal
		/// </summary>
		LeftHandMiddleProximal = 61,

		/// <summary>
		/// LeftHandMiddleIntermediate
		/// </summary>
		LeftHandMiddleIntermediate = 62,

		/// <summary>
		/// LeftHandMiddleDistal
		/// </summary>
		LeftHandMiddleDistal = 63,

		/// <summary>
		/// LeftHandMiddleTip
		/// </summary>
		LeftHandMiddleTip = 64,

		/// <summary>
		/// LeftHandRingMetacarpal
		/// </summary>
		LeftHandRingMetacarpal = 65,

		/// <summary>
		/// LeftHandRingProximal
		/// </summary>
		LeftHandRingProximal = 66,

		/// <summary>
		/// LeftHandRingIntermediate
		/// </summary>
		LeftHandRingIntermediate = 67,

		/// <summary>
		/// LeftHandRingDistal
		/// </summary>
		LeftHandRingDistal = 68,

		/// <summary>
		/// LeftHandRingTip
		/// </summary>
		LeftHandRingTip = 69,

		/// <summary>
		/// LeftHandPinkyMetacarpal
		/// </summary>
		LeftHandPinkyMetacarpal = 70,

		/// <summary>
		/// LeftHandPinkyProximal
		/// </summary>
		LeftHandPinkyProximal = 71,

		/// <summary>
		/// LeftHandPinkyIntermediate
		/// </summary>
		LeftHandPinkyIntermediate = 72,

		/// <summary>
		/// LeftHandPinkyDistal
		/// </summary>
		LeftHandPinkyDistal = 73,

		/// <summary>
		/// LeftHandPinkyTip
		/// </summary>
		LeftHandPinkyTip = 74,

		/// <summary>
		/// RightHandThumbTrapezium
		/// </summary>
		RightHandThumbTrapezium = 80,

		/// <summary>
		/// RightHandThumbMetacarpal
		/// </summary>
		RightHandThumbMetacarpal = 81,

		/// <summary>
		/// RightHandThumbProximal
		/// </summary>
		RightHandThumbProximal = 82,

		/// <summary>
		/// RightHandThumbDistal
		/// </summary>
		RightHandThumbDistal = 83,

		/// <summary>
		/// RightHandThumbTip
		/// </summary>
		RightHandThumbTip = 84,

		/// <summary>
		/// RightHandIndexMetacarpal
		/// </summary>
		RightHandIndexMetacarpal = 85,

		/// <summary>
		/// RightHandIndexProximal
		/// </summary>
		RightHandIndexProximal = 86,

		/// <summary>
		/// RightHandIndexIntermediate
		/// </summary>
		RightHandIndexIntermediate = 87,

		/// <summary>
		/// RightHandIndexDistal
		/// </summary>
		RightHandIndexDistal = 88,

		/// <summary>
		/// RightHandIndexTip
		/// </summary>
		RightHandIndexTip = 89,

		/// <summary>
		/// RightHandMiddleMetacarpal
		/// </summary>
		RightHandMiddleMetacarpal = 90,

		/// <summary>
		/// RightHandMiddleProximal
		/// </summary>
		RightHandMiddleProximal = 91,

		/// <summary>
		/// RightHandMiddleIntermediate
		/// </summary>
		RightHandMiddleIntermediate = 92,

		/// <summary>
		/// RightHandMiddleDistal
		/// </summary>
		RightHandMiddleDistal = 93,

		/// <summary>
		/// RightHandMiddleTip
		/// </summary>
		RightHandMiddleTip = 94,

		/// <summary>
		/// RightHandRingMetacarpal
		/// </summary>
		RightHandRingMetacarpal = 95,

		/// <summary>
		/// RightHandRingProximal
		/// </summary>
		RightHandRingProximal = 96,

		/// <summary>
		/// RightHandRingIntermediate
		/// </summary>
		RightHandRingIntermediate = 97,

		/// <summary>
		/// RightHandRingDistal
		/// </summary>
		RightHandRingDistal = 98,

		/// <summary>
		/// RightHandRingTip
		/// </summary>
		RightHandRingTip = 99,

		/// <summary>
		/// RightHandPinkyMetacarpal
		/// </summary>
		RightHandPinkyMetacarpal = 100,

		/// <summary>
		/// RightHandPinkyProximal
		/// </summary>
		RightHandPinkyProximal = 101,

		/// <summary>
		/// RightHandPinkyIntermediate
		/// </summary>
		RightHandPinkyIntermediate = 102,

		/// <summary>
		/// RightHandPinkyDistal
		/// </summary>
		RightHandPinkyDistal = 103,

		/// <summary>
		/// RightHandPinkyTip
		/// </summary>
		RightHandPinkyTip = 104,

		/// <summary>
		/// Expression
		/// </summary>
		Jaw = 110,

		/// <summary>
		/// LeftEye
		/// </summary>
		LeftEye = 111,

		/// <summary>
		/// RightEye
		/// </summary>
		RightEye = 112,

		/// <summary>
		/// Count
		/// </summary>
		Count = 113,

		/// <summary>
		/// LeftArmUpperTwist1
		/// </summary>
		LeftArmUpperTwist1 = 200,

		/// <summary>
		/// LeftArmUpperTwist2
		/// </summary>
		LeftArmUpperTwist2 = 201,

		/// <summary>
		/// LeftArmLowerTwist1
		/// </summary>
		LeftArmLowerTwist1 = 210,

		/// <summary>
		/// LeftArmLowerTwist2
		/// </summary>
		LeftArmLowerTwist2 = 211,

		/// <summary>
		/// LeftArmLowerTwist3
		/// </summary>
		LeftArmLowerTwist3 = 212,

		/// <summary>
		/// LeftLegUpperTwist1
		/// </summary>
		LeftLegUpperTwist1 = 250,

		/// <summary>
		/// LeftLegUpperTwist2
		/// </summary>
		LeftLegUpperTwist2 = 251,

		/// <summary>
		/// RightArmUpperTwist1
		/// </summary>
		RightArmUpperTwist1 = 300,

		/// <summary>
		/// RightArmUpperTwist2
		/// </summary>
		RightArmUpperTwist2 = 301,

		/// <summary>
		/// RightArmLowerTwist1
		/// </summary>
		RightArmLowerTwist1 = 310,

		/// <summary>
		/// RightArmLowerTwist2
		/// </summary>
		RightArmLowerTwist2 = 311,

		/// <summary>
		/// RightArmLowerTwist3
		/// </summary>
		RightArmLowerTwist3 = 312,

		/// <summary>
		/// RightLegUpperTwist1
		/// </summary>
		RightLegUpperTwist1 = 350,

		/// <summary>
		/// RightLegUpperTwist2
		/// </summary>
		RightLegUpperTwist2 = 351
	};
}