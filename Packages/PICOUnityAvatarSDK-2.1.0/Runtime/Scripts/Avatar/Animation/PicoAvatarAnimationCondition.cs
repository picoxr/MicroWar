using System.Runtime.InteropServices;
using System.Text;
using System.Collections.Generic;
using AOT;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar animation state transition in statemachine, connects a source state and a destination state 
		/// </summary>
		public class AvatarAnimationCondition : NativeObject
		{
			#region Public Properties

			public AnimatorParameterType type
			{
				get => _type;
			}

			#endregion


			#region Public Methods

			//Constructor with native handle.
			internal AvatarAnimationCondition(System.IntPtr nativeHandle_, AnimatorParameterType type_)
			{
				SetNativeHandle(nativeHandle_, false);
				_type = type_;
				//
				//if (PicoAvatarStats.instance != null)
				//{
				//    PicoAvatarStats.instance.IncreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarAnimationState);
				//}
			}

			/// <summary>
			/// Set animator parameter for this condition
			/// </summary>
			/// <param name="param">Parameter name</param>
			public void SetParameterByName(string param)
			{
				pav_AvatarAnimationCondition_SetParameterStr(nativeHandle, param);
			}

			/// <summary>
			/// Set operator for this condition
			/// </summary>
			/// <param name="conditionOperator">Condition operator</param>
			public void SetOperator(AnimationConditionOperator conditionOperator)
			{
				pav_AvatarAnimationCondition_SetOperator(nativeHandle, (uint)conditionOperator);
			}

			/// <summary>
			/// Set UInt value to do condition check with parameter
			/// </summary>
			/// <param name="threshold">UInt threshold value</param>
			public void SetThresholdUInt(uint threshold)
			{
				if (_type == AnimatorParameterType.UInt)
				{
					pav_AvatarAnimationCondition_SetParamThresholdUInt(nativeHandle, threshold);
				}
			}

			/// <summary>
			/// Set float value to do condition check with parameter
			/// </summary>
			/// <param name="threshold">Float threshold value</param>
			public void SetThresholdFloat(float threshold)
			{
				if (_type == AnimatorParameterType.Float)
				{
					pav_AvatarAnimationCondition_SetParamThresholdFloat(nativeHandle, threshold);
				}
			}

			#endregion


			#region Private Fields

			AnimatorParameterType _type = AnimatorParameterType.Bool;

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationCondition_SetParameterStr(System.IntPtr nativeHandle,
				string parameterName);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationCondition_SetOperator(System.IntPtr nativeHandle,
				uint conditionOperator);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationCondition_SetParamThresholdUInt(
				System.IntPtr nativeHandle, uint threshold);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationCondition_SetParamThresholdFloat(
				System.IntPtr nativeHandle, float threshold);

			#endregion
		}
	}
}