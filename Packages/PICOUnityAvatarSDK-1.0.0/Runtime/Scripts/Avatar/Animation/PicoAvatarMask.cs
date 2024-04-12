using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Avatar mask, a collection of joints maskedout in a skeleton parent and child joint can be maskedout separately.
		/// </summary>
		public class AvatarMask : NativeObject
		{
			#region Public Properties

			#endregion

			#region Public Methods

			/// <summary>
			/// Default constructor
			/// </summary>
			// public AvatarMask()
			// {
			//     System.IntPtr nativeHandle_ = System.IntPtr.Zero;

			//     nativeHandle_ = pav_AvatarMask_New();
			//     SetNativeHandle(nativeHandle_, false);
			// }

			/// <summary>
			/// Constructor with AvatarBodyAnimController
			/// </summary>
			/// <param name="avatarBodyAnimController">Constructor parameter</param>
			public AvatarMask(AvatarBodyAnimController avatarBodyAnimController)
			{
				System.IntPtr nativeHandle_ = System.IntPtr.Zero;
				if (avatarBodyAnimController != null)
				{
					nativeHandle_ =
						pav_AvatarMask_NewWithBodyAnimationController(avatarBodyAnimController.nativeHandle);
					SetNativeHandle(nativeHandle_, false);
					return;
				}

				nativeHandle_ = pav_AvatarMask_New();
				SetNativeHandle(nativeHandle_, false);
			}

			/// <summary>
			/// Add basic bone as syntactic sugar for compatibility with FullBone
			/// </summary>
			public void AddBasicBone()
			{
				pav_AvatarMask_AddPresetBoneMask(nativeHandle, 0);
			}

			/// <summary>
			/// Add full bone as syntactic sugar for compatibility with FullBone
			/// </summary>
			public void AddFullBone()
			{
				pav_AvatarMask_AddPresetBoneMask(nativeHandle, 1);
			}

			/// <summary>
			/// Enable/disable joint rotation update for animation
			/// </summary>
			/// <param name="jointType">Type of joint to set rotation enable</param>
			/// <param name="enable">Joint rotation enable or not</param>
			public void SetJointRotationEnable(JointType jointType, bool enable)
			{
				pav_AvatarMask_SetJointRotationEnable(nativeHandle, (uint)jointType, enable);
			}

			/// <summary>
			/// Enable/disable joint position update for animation
			/// </summary>
			/// <param name="jointType">Type of joint to set position enable</param>
			/// <param name="enable">Joint position enable or not</param>
			public void SetJointPositionEnable(JointType jointType, bool enable)
			{
				pav_AvatarMask_SetJointPositionEnable(nativeHandle, (uint)jointType, enable);
			}

			/// <summary>
			/// Enable/disable joint scale update for animation
			/// </summary>
			/// <param name="jointType">Type of joint to set scale enable</param>
			/// <param name="enable">Joint scale enable or not</param>
			public void SetJointScaleEnable(JointType jointType, bool enable)
			{
				pav_AvatarMask_SetJointScaleEnable(nativeHandle, (uint)jointType, enable);
			}

			/// <summary>
			/// Enable/disable joint rotation update for animation
			/// </summary>
			/// <param name="jointName">Name of joint to set rotation enable</param>
			/// <param name="enable">Joint rotation enable or not</param>
			public void SetJointRotationEnableWithName(string jointName, bool enable)
			{
				pav_AvatarMask_SetJointRotationEnableWithName(nativeHandle, jointName, enable);
			}

			/// <summary>
			/// Enable/disable joint position update for animation
			/// </summary>
			/// <param name="jointName">Name of joint to set position enable</param>
			/// <param name="enable">Joint position enable or not</param>
			public void SetJointPositionEnableWithName(string jointName, bool enable)
			{
				pav_AvatarMask_SetJointPositionEnableWithName(nativeHandle, jointName, enable);
			}

			/// <summary>
			/// Enable/disable joint scale update for animation
			/// </summary>
			/// <param name="jointName">Name of joint to set scale enable</param>
			/// <param name="enable">Joint scale enable or not</param>
			public void SetJointScaleEnableWithName(string jointName, bool enable)
			{
				pav_AvatarMask_SetJointScaleEnableWithName(nativeHandle, jointName, enable);
			}

			/// <summary>
			/// Clear avatar mask, avatar mask will restore to default, all joint rotation enabled, position and scale disabled
			/// </summary>
			public void Clear()
			{
				pav_AvatarMask_Clear(nativeHandle);
			}

			/// <summary>
			/// Enable/disable all joints rotation update for animation
			/// </summary>
			/// <param name="enable">Joint rotation enable or not</param>
			public void SetAllJointsRotationEnable(bool enable)
			{
				pav_AvatarMask_SetAllJointsRotationEnable(nativeHandle, enable);
			}

			/// <summary>
			/// Enable/disable all joints position update for animation
			/// </summary>
			/// <param name="enable">Joint position enable or not</param>
			public void SetAllJointsPositionEnable(bool enable)
			{
				pav_AvatarMask_SetAllJointsPositionEnable(nativeHandle, enable);
			}

			/// <summary>
			/// Enable/disable all joints scale update for animation
			/// </summary>
			/// <param name="enable">Joint scale enable or not</param>
			public void SetAllJointsScaleEnable(bool enable)
			{
				pav_AvatarMask_SetAllJointsScaleEnable(nativeHandle, enable);
			}

			#endregion

			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarMask_New();

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarMask_NewWithBodyAnimationController(
				System.IntPtr nativeHandleBodyAnimationController);

			
            //@brief add preset Bone mask by type
            //@param presetBoneMaskType 0 means basic bone, 1 means full bone
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_AddPresetBoneMask(System.IntPtr nativeHandle,
				int presetBoneMaskType);

			// set all joints
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetAllJointsRotationEnable(System.IntPtr nativeHandle,
				bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetAllJointsPositionEnable(System.IntPtr nativeHandle,
				bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetAllJointsScaleEnable(System.IntPtr nativeHandle,
				bool enable);

			// clear
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_Clear(System.IntPtr nativeHandle);

			// set with jointType
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetJointRotationEnable(System.IntPtr nativeHandle,
				uint jointTypeId, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetJointPositionEnable(System.IntPtr nativeHandle,
				uint jointTypeId, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetJointScaleEnable(System.IntPtr nativeHandle,
				uint jointTypeId, bool enable);

			// set with jointName
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetJointRotationEnableWithName(System.IntPtr nativeHandle,
				string jointTypeId, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetJointPositionEnableWithName(System.IntPtr nativeHandle,
				string jointTypeId, bool enable);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMask_SetJointScaleEnableWithName(System.IntPtr nativeHandle,
				string jointTypeId, bool enable);

			#endregion
		}
	}
}