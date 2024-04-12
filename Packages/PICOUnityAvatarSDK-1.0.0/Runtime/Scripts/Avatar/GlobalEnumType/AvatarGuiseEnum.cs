namespace Pico.Avatar
{
    /// <summary>
    /// Lod level of avatar
    /// </summary>
    public enum AvatarLodLevel : int
    {
        Invalid = -2,
        /// <summary>
        /// Invisible
        /// </summary>
        Invisible = -1,
        /// <summary>
        /// Lod0, about 40000 triangles an avatar
        /// </summary>
        Lod0 = 0,
        /// <summary>
        /// Lod1, about 20000 triangles an avatar
        /// </summary>
        Lod1 = 1,
        /// <summary>
        /// Lod2,about 10000 triangles an avatar
        /// </summary>
        Lod2 = 2,
        /// <summary>
        /// Lod3,about 4000 triangles an avatar
        /// </summary>
        Lod3 = 3,
        /// <summary>
        /// Lod4,about 2000 triangles an avatar
        /// </summary>
        Lod4 = 4,
        /// <summary>
        /// Lod5,about 1000 triangles an avatar
        /// </summary>
        Lod5 = 5,
        Count
    }

	/// <summary>
	/// Playback snapshot level
	/// </summary>
	/// <remarks>
	/// This setting will affect the action data packet size of the AvatarEntity.GetFixedPacketMemoryView,
	/// the later the larger the amount of data
	/// </remarks>
	public enum RecordBodyAnimLevel : int
	{
		Invalid = -1, // invalid playback level.

		/// <summary>
		/// Data only record device input, remote client need recalculate ik, if swift is enabled, data will also contains swift data
		/// </summary>
		DeviceInput = 0,

		/// <summary>
		/// Data contains basic bone data, remote client do not need recalculate ik
		/// </summary>
		BasicBone = 1,

		/// <summary>
		/// Data contains full bone data, remote client do not need recalculate ik, FullBone provides more precise result than BasicBone but with larger package
		/// </summary>
		[UnityEngine.InspectorName("FullBone")]
		FullBone = 2,

		/// <summary>
		/// [Obsolete("This enum value is deprecated when AnimationRecordConfig.recordVersion >= 5.")]
		/// FullBone with 52 facetracking blendshape value
		/// </summary>
		[UnityEngine.InspectorName("FullBone")]
		BasicBlendShape = 3,

		/// <summary>
		/// [Obsolete("This enum value is deprecated when AnimationRecordConfig.recordVersion >= 5.")]
		/// BasicBlendShape with 20 lipsync blendshape value
		/// </summary>
		[UnityEngine.InspectorName("FullBone")]
		FullBlendShape = 4,

		/// <summary>
		/// FullBone with attach bone data 
		/// </summary>
		AttachBone = 5,
		Count
	}

	/// <summary>
	/// Playback packet apply mode
	/// </summary>
	/// <remarks>
	/// This setting will affect how the record packet apply to the system
	/// </remarks>
	public enum RecordPacketApplyMode : int
	{
		Invalid = -1,

		/// <summary>
		/// Blend record packet with animation
		/// </summary>
		BlendWithAnimation = 0,

		/// <summary>
		/// Direct apply record packet
		/// </summary>
		WriteDirectly = 1
	}

	/// <summary>
	/// Avatar IK mode
	/// </summary>
	public enum AvatarIKMode : int
	{
		/// <summary>
		/// No IK
		/// </summary>
		None = 0,

		/// <summary>
		/// Upper body
		/// </summary>
		UpperBody = 1,

		/// <summary>
		/// Full body
		/// </summary>
		FullBody = 2,
	}

	/// <summary>
	/// How avatar viewed.
	/// </summary>
	public enum AvatarHeadShowType : uint
	{
		/// <summary>
		/// Show head
		/// </summary>
		Normal = 0,

		/// <summary>
		/// Hide self head
		/// </summary>
		Hide = 1,

		/// <summary>
		/// Transparent self head.
		/// </summary>
		Transparent = 2,

		/// <summary>
		/// Edit and do not merge
		/// </summary>
		Edit = 3,
	}

	/// <summary>
	/// Avatar is who
	/// </summary>
	public enum ControlSourceType : uint
	{
		/// <summary>
		/// Self
		/// </summary>
		MainPlayer = 0,

		/// <summary>
		/// Other
		/// </summary>
		OtherPlayer = 1,

		/// local ai control. (reserve)
		NPC = 2,
	};

	/// <summary>
	/// Avatar manifest type
	/// </summary>
	public enum AvatarManifestationType : uint
	{
		/// <summary>
		/// No avatar parts manifested
		/// </summary>
		None = 0,

		/// <summary>
		/// All body parts
		/// </summary>
		Full = 1 << 0,

		/// <summary>
		/// Upper body only
		/// </summary>
		Half = 1 << 1,

		/// <summary>
		/// Head and hands only
		/// </summary>
		HeadHands = 1 << 2,

		/// <summary>
		/// Head only
		/// </summary>
		Head = 1 << 3,

		/// <summary>
		/// Hands only
		/// </summary>
		Hands = 1 << 4,

		///  All manifestations requested.
		All = Full | Half | HeadHands | Head | Hands,
	}
}