using System.Runtime.InteropServices;
using System.Text;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// avatar animation blendTree, used to blend a set of animation clips
		/// </summary>
		/// @note
		/// Similar to unity blendTree:
		/// 1.BlendTree1D, several clips controlled by a single float parameter.
		///   Each clip has a preset threshold for blending.
		///   Eg. Idle animation with threshold 0, walking animation with threshold 2,  running animation with threshold 5.
		///   When parameter value is 1, the result animation will be a blending of idle and walking, when parameter is 3, result will be blending of walking and running
		/// 2.BlendTree2D, several clips controlled by two float parameters.
		///   Each clip has a preset threshold (x,y) for blending.
		///   Eg. Idle animation with threshold (0,0), walking right animation with threshold(1,0), walking forward animation with threshold(0,1)
		///   When parameter value is (0.5,0.5), the result animation will be a blending of these 3 animations
		/// 3.BlendTreeDirect, several clips controlled by several float parameters.
		///   Each parameters directly controlls the blendWeight of a animation clip
		public class AvatarAnimationBlendTree : NativeObject
		{
			#region Public Properties

			public AvatarAnimationState owner { get; set; } = null;

			public AnimBlendTreeType blendType
			{
				get => _blendType;
			}

			#endregion


			#region Public Methods

			// Constructor with native handle.
			internal AvatarAnimationBlendTree(System.IntPtr nativeHandle_, AnimBlendTreeType blendType_)
			{
				SetNativeHandle(nativeHandle_, false);
				_blendType = blendType_;
			}

			/// <summary>
			/// Add animation clip to the blendTree
			/// </summary>
			/// <param name="animationName">Name of new animation clip</param>
			public void AddAnimationClip(string animationName)
			{
				pav_AvatarAnimationBlendTree_AddAnimazClip(nativeHandle, animationName);
			}

			/// <summary>
			/// Add animation parameter that contributes to this blend tree
			/// </summary>
			/// <param name="param">New parameter name</param>
			/// @note
			/// 1.Parameter must be registered in animController
			/// 2.For 1D blendTree, only 1 param is needed
			/// 3.For 2D blendTree, 2 params are required
			/// 4.For Direct blendTree, number of param need to match number of clips in this blend tree
			public void AddParameterByName(string param)
			{
				pav_AvatarAnimationBlendTree_AddParameterStr(nativeHandle, param);
			}

			/// <summary>
			/// Set animation parameter that contributes to this blend tree
			/// </summary>
			/// <param name="index">Param index</param>
			/// <param name="param">Param name</param>
			public void SetParameterByName(uint index, string param)
			{
				pav_AvatarAnimationBlendTree_SetParameterStr(nativeHandle, index, param);
			}

			/// <summary>
			/// Set threshold for blendTree 1D
			/// </summary>
			/// <param name="index">Param index</param>
			/// <param name="threshold">Threshold of param</param>
			public void SetThreshold1D(uint index, float threshold)
			{
				if (blendType != AnimBlendTreeType.BlendTree1D)
				{
					return;
				}

				pav_AvatarAnimationBlendTree1D_SetThreshold(nativeHandle, index, threshold);
			}

			/// <summary>
			/// Set threshold for blendTree 2D
			/// </summary>
			/// <param name="index">Param index</param>
			/// <param name="thresholdX">Threshold of x</param>
			/// <param name="thresholdY">Threshold of y</param>
			public void SetThreshold2D(uint index, float thresholdX, float thresholdY)
			{
				if (blendType != AnimBlendTreeType.BlendTree2D)
				{
					return;
				}

				pav_AvatarAnimationBlendTree2D_SetThreshold(nativeHandle, index, thresholdX, thresholdY);
			}

			#endregion


			#region Private Fields

			private AnimBlendTreeType _blendType = AnimBlendTreeType.Invalid;

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationBlendTree_AddAnimazClip(System.IntPtr nativeHandle,
				string animationName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationBlendTree_SetParameterStr(System.IntPtr nativeHandle,
				uint index, string paramStr);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationBlendTree_AddParameterStr(System.IntPtr nativeHandle,
				string paramStr);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationBlendTree_AddParameterID(System.IntPtr nativeHandle,
				uint paramID);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationBlendTree1D_SetThreshold(System.IntPtr nativeHandle,
				uint index, float threshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationBlendTree2D_SetThreshold(System.IntPtr nativeHandle,
				uint index, float thresholdX, float thresholdY);

			#endregion
		}
	}
}