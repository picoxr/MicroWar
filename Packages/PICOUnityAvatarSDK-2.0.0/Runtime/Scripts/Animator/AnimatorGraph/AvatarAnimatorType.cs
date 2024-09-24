namespace Pico
{
	namespace Avatar
	{
		public enum AvatarWrapMode
		{
			//repeat time
			Repeat = 1,

			//loop
			Loop = 0,

			//When time reaches the end of the animation clip, time will ping pong back between beginning and end.
			PingPong = -1,

			//play once and repeat last frame
			ClampForever = -2,

			//seek to a frame and play the frame
			Seek = -3
		}

		public enum AvatarBlendType
		{
			//Blend with one parameter
			Blend1D = 0,

			//Blend using two parameters
			Blend2D = 1,

			//Direct blending can directly map the parameters of the animator to the weights of the blend tree animation.
			BlendDirect = 2
		}

		public enum AvatarLayerBlendMode
		{
			// lerp layer, blend with other lerp layers according to blending weight
			Lerp = 0,

			// additive layer, added on other layers according to blending weight, will not affect other layers
			Additive = 1,

			//override layer, will rewrite the value of other layers if has same target
			Override = 2
		}

		public enum AvatarLayerMask
		{
			Head = 1 << 0,
			Body = 1 << 1,
			LeftArm = 1 << 2,
			LeftHand = 1 << 3,
			RightArm = 1 << 4,
			RightHand = 1 << 5,
			LeftLeg = 1 << 6,
			RightLeg = 1 << 7,
			UpperBody = 1 << 8, //include Head, Body, LeftArm, LeftHand, RightArm, RightHand
			LowerBody = 1 << 9 //include LeftLeg, RightLeg
		}

		#region state machine

		public enum AvatarConditionType
		{
			Bool = 0,
			Trigger = 1,
			Uint = 2,
			Float = 3
		}


		public enum BoolCompare
		{
			True = 0,
			False = 1
		}

		public enum UintCompare
		{
			Greater = 0,
			Less = 1,
			Equal = 2,
			NotEqual = 3
		}

		public enum FloatCompare
		{
			Greater = 0,
			Less = 1
		}

		#endregion
	}
}