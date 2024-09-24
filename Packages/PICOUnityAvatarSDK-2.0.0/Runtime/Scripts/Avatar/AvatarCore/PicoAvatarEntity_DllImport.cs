using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		public partial class AvatarEntity
		{
			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarEntity_GetObject(uint avatarEntityId);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_SetVisibleThisFrame(System.IntPtr nativeHandle,
				bool visibleThisFrame);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_SetNeedUpdateSimulationRenderData(System.IntPtr nativeHandle,
				bool needUpdate);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_SetAnimationPlaybackLevel(System.IntPtr nativeHandle,
				uint playbackLevel);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_SetLocalAnimationConfig(System.IntPtr nativeHandle,
				ref LocalAnimationConfig localAnimationConfig);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_SetAnimationRecordConfig(System.IntPtr nativeHandle,
				ref AnimationRecordConfig animationRecordConfig);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_SetAnimationPlaybackConfig(System.IntPtr nativeHandle,
				ref AnimationPlaybackConfig animationPlaybackConfig);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_SetFaceExpressionRecordConfig(System.IntPtr nativeHandle,
				ref FaceExpressionRecordConfig faceExpressionRecordConfig);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_SetFaceExpressionPlaybackConfig(System.IntPtr nativeHandle,
				ref FaceExpressionPlaybackConfig faceExpressionPlaybackConfig);


			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern MemoryView.NativeData pav_AvatarEntity_GetFixedPacketMemoryView(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_RecordAnimationPacket(System.IntPtr nativeHandle,
				double timestamp);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_ApplyAnimationPacket(System.IntPtr nativeHandle,
				System.IntPtr nativeMemoryViewHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern MemoryView.NativeData pav_AvatarEntity_GetFixedFaceExpressionPacketMemoryView(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_RecordFaceExpressionPacket(System.IntPtr nativeHandle,
				double timestamp);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_ApplyFaceExpressionPacket(System.IntPtr nativeHandle,
				System.IntPtr nativeMemoryViewHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_GetActiveLodLevel(System.IntPtr entityHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr
				pav_AvatarEntity_GetAvatarLod(System.IntPtr entityHandle, uint lodLevel);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern float pav_AvatarEntity_GetPing(System.IntPtr entityHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarEntity_GetXForm(System.IntPtr entityHandle,
				ref XForm entityXForm);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarEntity_GetBodyAnimController(System.IntPtr entityHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarEntity_GetDeviceInputReader(System.IntPtr entityHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarEntity_GetCustomHandSide(System.IntPtr nativeHandle, uint side);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_SetCustomHandSide(System.IntPtr nativeHandle, uint side);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_ClearCustomHandSide(System.IntPtr nativeHandle, uint side);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarEntity_SetPacketApplyMode(System.IntPtr nativeHandle, int method);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarEntity_UpdateSimulationRenderDataT(System.IntPtr nativeHandle);

			#endregion
		}
	}
}