using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// PicoHandJointType
		/// </summary>
		public class PicoHandJointType : MonoBehaviour
		{
			/// <summary>
			/// jointType
			/// </summary>
			public HandJointType jointType;
		}

		/// <summary>
		/// HandJointType
		/// </summary>
		public enum HandJointType
		{
			/// <summary>
			/// Invalid
			/// </summary>
			Invalid = -1,

			/// <summary>
			/// Wrist
			/// </summary>
			Wrist = 0,

			/// <summary>
			/// ThumbTrapezium
			/// </summary>
			ThumbTrapezium = 1,

			/// <summary>
			/// ThumbMetacarpal
			/// </summary>
			ThumbMetacarpal = 2,

			/// <summary>
			/// ThumbProximal
			/// </summary>
			ThumbProximal = 3,

			/// <summary>
			/// ThumbDistal
			/// </summary>
			ThumbDistal = 4,

			/// <summary>
			/// ThumbTip
			/// </summary>
			ThumbTip = 5,

			/// <summary>
			/// IndexMetacarpal
			/// </summary>
			IndexMetacarpal = 6,

			/// <summary>
			/// IndexProximal
			/// </summary>
			IndexProximal = 7,

			/// <summary>
			/// IndexIntermediate
			/// </summary>
			IndexIntermediate = 8,

			/// <summary>
			/// IndexDistal
			/// </summary>
			IndexDistal = 9,

			/// <summary>
			/// IndexTip
			/// </summary>
			IndexTip = 10,

			/// <summary>
			/// MiddleMetacarpal
			/// </summary>
			MiddleMetacarpal = 11,

			/// <summary>
			/// MiddleProximal
			/// </summary>
			MiddleProximal = 12,

			/// <summary>
			/// MiddleIntermediate
			/// </summary>
			MiddleIntermediate = 13,

			/// <summary>
			/// MiddleDistal
			/// </summary>
			MiddleDistal = 14,

			/// <summary>
			/// MiddleTip
			/// </summary>
			MiddleTip = 15,

			/// <summary>
			/// RingMetacarpal
			/// </summary>
			RingMetacarpal = 16,

			/// <summary>
			/// RingProximal
			/// </summary>
			RingProximal = 17,

			/// <summary>
			/// RingIntermediate
			/// </summary>
			RingIntermediate = 18,

			/// <summary>
			/// RingDistal
			/// </summary>
			RingDistal = 19,

			/// <summary>
			/// RingTip
			/// </summary>
			RingTip = 20,

			/// <summary>
			/// PinkyMetacarpal
			/// </summary>
			PinkyMetacarpal = 21,

			/// <summary>
			/// PinkyProximal
			/// </summary>
			PinkyProximal = 22,

			/// <summary>
			/// PinkyIntermediate
			/// </summary>
			PinkyIntermediate = 23,

			/// <summary>
			/// PinkyDistal
			/// </summary>
			PinkyDistal = 24,

			/// <summary>
			/// PinkyTip
			/// </summary>
			PinkyTip = 25,

			/// <summary>
			/// Count
			/// </summary>
			Count = 26
		}
	}
}