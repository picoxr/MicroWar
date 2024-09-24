namespace Pico.Avatar
{
	/// <summary>
	/// Avatar Animation state status for callback
	/// </summary>
	public enum AnimStateStatus : int
	{
		/// <summary>
		/// State not playing
		/// </summary>
		StateIdle = 0,

		/// <summary>
		/// State entering
		/// </summary>
		StateEnter = 1,

		/// <summary>
		/// State running
		/// </summary>
		StateRunning = 2,

		/// <summary>
		/// Animation playback ended in this state
		/// </summary>
		StateEnd = 3,

		/// <summary>
		/// State leaving, some other state enters
		/// </summary>
		StateLeave = 4
	}

	/// <summary>
	/// Avatar animation layer blend mode
	/// </summary>
	public enum AnimLayerBlendMode : int
	{
		/// <summary>
		/// Lerp layer, blend with other lerp layers according to blending weight
		/// </summary>
		Lerp = 0,

		/// <summary>
		/// Additive layer, added on other layers according to blending weight, will not affect other layers
		/// </summary>
		Additive = 1,

		/// <summary>
		/// Override layer, will rewrite the value of other layers if has same target
		/// </summary>
		Override = 2,

		/// <summary>
		/// Count
		/// </summary>
		Count
	};

	/// <summary>
	/// Avatar animation layer source type
	/// </summary>
	public enum AnimLayerType : int
	{
		/// <summary>
		/// Animation clip
		/// </summary>
		AnimationClip = 0,

		/// <summary>
		/// Remote package from net
		/// </summary>
		RemotePackage = 1,

		Count
	}

	/// <summary>
	/// Avatar animation blendTree type
	/// </summary>
	public enum AnimBlendTreeType : int
	{
		Invalid = -1,

		/// <summary>
		/// 1d blendTree
		/// </summary>
		BlendTree1D = 0,

		/// <summary>
		/// 2d blendTree
		/// </summary>
		BlendTree2D = 1,

		/// <summary>
		/// BlendTreeDirect
		/// </summary>
		BlendTreeDirect = 2
	}

	/// <summary>
	/// Avatar animation parameter type
	/// </summary>
	public enum AnimatorParameterType : int
	{
		/// <summary>
		/// Bool
		/// </summary>
		Bool = 0,

		/// <summary>
		/// Trigger
		/// </summary>
		Trigger = 1,

		/// <summary>
		/// UInt
		/// </summary>
		UInt = 2,

		/// <summary>
		/// Float
		/// </summary>
		Float = 3
	}

	/// <summary>
	/// Avatar animation condition check operator
	/// </summary>
	public enum AnimationConditionOperator : int
	{
		/// <summary>
		/// True
		/// </summary>
		True = 0,

		/// <summary>
		/// False
		/// </summary>
		False = 1,

		/// <summary>
		/// Greater
		/// </summary>
		Greater = 2,

		/// <summary>
		/// Less
		/// </summary>
		Less = 3,

		/// <summary>
		/// Equal
		/// </summary>
		Equal = 4,

		/// <summary>
		/// Not Equal
		/// </summary>
		NotEqual = 5,
	}
}