using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar animation state transition in statemachine, connects a source state and a destination state 
		/// </summary>
		public class AvatarAnimationStateTransition : NativeObject
		{
			#region Public Properties

			public AvatarAnimationState srcState { get; set; } = null;
			public AvatarAnimationState dstState { get; set; } = null;

			#endregion


			#region Public Methods

			//Constructor with native handle.
			internal AvatarAnimationStateTransition(System.IntPtr nativeHandle_)
			{
				SetNativeHandle(nativeHandle_, false);
			}

			/// <summary>
			/// Set duration for the transition
			/// </summary>
			/// <param name="duration">Transition duration</param>
			public void SetDuration(float duration)
			{
				pav_AvatarAnimationStateTransition_SetDuration(nativeHandle, duration);
			}


			/// <summary>
			/// Set exit when state end for state transition
			/// </summary>
			/// <param name="exitWhenStateEnd">If set to true, this transition will automatically be triggered when animations end</param>
			public void SetExitWhenStateEnd(bool exitWhenStateEnd)
			{
				pav_AvatarAnimationStateTransition_SetExitWhenStateEnd(nativeHandle, exitWhenStateEnd);
			}

			/// <summary>
			/// Add a transion condition
			/// </summary>
			/// <param name="type">Condition parameter type</param>
			/// <returns>The AvatarAnimationCondition created </returns>
			public AvatarAnimationCondition AddCondition(AnimatorParameterType type)
			{
				var conditionNativeHandle = pav_AvatarAnimationStateTransition_AddCondition(nativeHandle, (uint)type);
				if (conditionNativeHandle != System.IntPtr.Zero)
				{
					AvatarAnimationCondition animatonCondition =
						new AvatarAnimationCondition(conditionNativeHandle, type);
					animatonCondition.Retain();
					_conditions.Add(animatonCondition);
					return animatonCondition;
				}

				return null;
			}


			// Set whether this transition is an entry transition
			internal void SetEntryTransition(bool entryTransition)
			{
				pav_AvatarAnimationStateTransition_SetEntryTransition(nativeHandle, entryTransition);
			}


			protected override void OnDestroy()
			{
				foreach (var condition in _conditions)
				{
					condition.Release();
				}

				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			private List<AvatarAnimationCondition> _conditions = new List<AvatarAnimationCondition>();

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationStateTransition_SetDuration(
				System.IntPtr nativeHandle, float duration);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationStateTransition_SetEntryTransition(
				System.IntPtr nativeHandle, bool entryTransition);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarAnimationStateTransition_SetExitWhenStateEnd(
				System.IntPtr nativeHandle, bool exitWhenStateEnd);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarAnimationStateTransition_AddCondition(
				System.IntPtr nativeHandle, uint animatorParameterType);

			#endregion
		}
	}
}