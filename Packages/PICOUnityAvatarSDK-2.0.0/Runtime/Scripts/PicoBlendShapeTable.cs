namespace Pico
{
	namespace Avatar
	{
		// Avatar Blendshape channel type.
		public enum BSType : int
		{
			//standard 52 channels
			Invalid = -1,
			eyeLookDown_L = 0,
			noseSneer_L = 1,
			eyeLookIn_L = 2,
			browInnerUp = 3,
			browDown_R = 4,
			mouthClose = 5,
			mouthLowerDown_R = 6,
			jawOpen = 7,
			mouthUpperUp_R = 8,

			mouthShrugUpper = 9,
			mouthFunnel = 10,
			eyeLookIn_R = 11,
			eyeLookDown_R = 12,
			noseSneer_R = 13,
			mouthRollUpper = 14,
			jawRight = 15,
			browDown_L = 16,
			mouthShrugLower = 17,
			mouthRollLower = 18,
			mouthSmile_L = 19,
			mouthPress_L = 20,
			mouthSmile_R = 21,
			mouthPress_R = 22,
			mouthDimple_R = 23,
			mouthLeft = 24,
			jawForward = 25,
			eyeSquint_L = 26,
			mouthFrown_L = 27,
			eyeBlink_L = 28,
			cheekSquint_L = 29,
			browOuterUp_L = 30,
			eyeLookUp_L = 31,
			jawLeft = 32,
			mouthStretch_L = 33,
			mouthPucker = 34,
			eyeLookUp_R = 35,
			browOuterUp_R = 36,
			cheekSquint_R = 37,
			eyeBlink_R = 38,
			mouthUpperUp_L = 39,
			mouthFrown_R = 40,
			eyeSquint_R = 41,
			mouthStretch_R = 42,
			cheekPuff = 43,
			eyeLookOut_L = 44,
			eyeLookOut_R = 45,
			eyeWide_R = 46,
			eyeWide_L = 47,
			mouthRight = 48,
			mouthDimple_L = 49,
			mouthLowerDown_L = 50,
			tongueOut = 51,

			//pico avatar custom channels
			head_Happy = 52,
			head_Surprise = 53,

			Count
		};
	}
}