using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		class NativeCall_AvatarEntity : NativeCallProxy
		{
			#region Caller Methods

			public void SetVisible(bool visible)
			{
				var args = this._method_SetVisible.invokeArgumentTable;
				//
				args.SetByteParam(0, visible ? (byte)1 : (byte)0);

				this._method_SetVisible.DoApply();
			}

			public void SetLodLevelLoaded(AvatarLodLevel lodLevel)
			{
				var args = this._method_SetLodLevelLoaded.invokeArgumentTable;
				//
				args.SetByteParam(0, (byte)lodLevel);

				this._method_SetLodLevelLoaded.DoApply();
			}

			public void SetForceLodLevel(AvatarLodLevel lodLevel)
			{
				var args = this._method_SetForceLodLevel.invokeArgumentTable;
				//
				args.SetByteParam(0, (byte)lodLevel);

				this._method_SetForceLodLevel.DoApply();
			}

			public void SetMaterialVec4(uint primitiveId, string name, Vector4 value)
			{
				var args = this._method_SetMaterialVec4.invokeArgumentTable;
				args.SetUIntParam(0, primitiveId);
				args.SetStringParam(1, name);
				args.SetFloatParam(2, value.x);
				args.SetFloatParam(3, value.y);
				args.SetFloatParam(4, value.z);
				args.SetFloatParam(5, value.w);
				this._method_SetMaterialVec4.DoApply();
			}

			#endregion

			#region Callee Methods

			private void OnLodChanged(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				if (AvatarEnv.NeedLog(DebugLogMask.NativeCallTrivial))
				{
					AvatarEnv.Log(DebugLogMask.NativeCallTrivial, "OnLodChanged");
				}

				uint level = 0;
				if (invokeArguments.GetUIntParam(0, ref level) == NativeResult.Success)
				{
					this._avatarEntity?.Notify_LodChanged((AvatarLodLevel)level);
				}
			}

			private void OnAnimationStart(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				if (AvatarEnv.NeedLog(DebugLogMask.GeneralInfo))
				{
					AvatarEnv.Log(DebugLogMask.GeneralInfo, "JS=>C#: BodyAnimController.OnAnimationStart");
				}

				//
				this._avatarEntity?.bodyAnimController?.SetDefaultHeadXform();
				this._avatarEntity?.bodyAnimController?.OnAnimationStart();
			}

			private void OnCustomHandSet(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				UnityEngine.Debug.Log("OnCustomHandSet");
				this._avatarEntity?.UpdateCustomHandPose();
			}

			private void OnRebuildMaterials(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint level = 0;
				if (invokeArguments.GetUIntParam(0, ref level) == NativeResult.Success)
				{
					this._avatarEntity?.Notify_RebuildMaterials((AvatarLodLevel)level);
				}
			}

			private void OnLodPartialRebuild(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint level = 0;
				invokeArguments.GetUIntParam(0, ref level);
				var jsonText = invokeArguments.GetStringParam(1);
				//
				this._avatarEntity?.OnLodPartialRebuild((AvatarLodLevel)level, jsonText);
			}

			private void OnUnloadNonActiveLodLevel(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint level = 0;
				invokeArguments.GetUIntParam(0, ref level);
				this._avatarEntity?.Notify_UnloadNonActiveLodLevel((AvatarLodLevel)level);
			}

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarEntity(AvatarEntity avatarEntity, uint instanceId)
				: base(instanceId)
			{
				_avatarEntity = avatarEntity;

				/// Callee methods.
				{
					AddCalleeMethod(_attribute_OnLodChanged, OnLodChanged);
					AddCalleeMethod(_attribute_OnAnimationStart, OnAnimationStart);
					AddCalleeMethod(_attribute_OnCustomHandSet, OnCustomHandSet);
					AddCalleeMethod(_attribute_OnRebuildMaterials, OnRebuildMaterials);
					AddCalleeMethod(_attribute_OnLodPartialRebuild, OnLodPartialRebuild);
					AddCalleeMethod(_attribute_OnUnloadNonActiveLodLevel, OnUnloadNonActiveLodLevel);
				}

				/// Caller methods.
				{
					this._method_SetVisible = AddCallerMethod(_attribute_SetVisible);
					this._method_SetLodLevelLoaded = AddCallerMethod(_attribute_SetLodLevelLoaded);
					this._method_SetForceLodLevel = AddCallerMethod(_attribute_SetForceLodLevel);
				}
			}

			#region Private Fields

			private AvatarEntity _avatarEntity;
			private NativeCaller _method_SetVisible;
			private NativeCaller _method_SetLodLevelLoaded;
			private NativeCaller _method_SetForceLodLevel;
			private NativeCaller _method_SetMaterialVec4;

			#endregion

			#region Static Part

			private const string className = "AvatarEntity";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_SetVisible =
				new NativeCallerAttribute(className, "SetVisible");

			private static NativeCallerAttribute _attribute_SetLodLevelLoaded =
				new NativeCallerAttribute(className, "SetLodLevelLoaded");

			private static NativeCallerAttribute _attribute_SetForceLodLevel =
				new NativeCallerAttribute(className, "SetForceLodLevel");

			/// Callee Attributes.
			private static NativeCalleeAttribute _attribute_OnLodChanged =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnLodChanged");

			private static NativeCalleeAttribute _attribute_OnAnimationStart =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAnimationStart");

			private static NativeCalleeAttribute _attribute_OnCustomHandSet =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnCustomHandSet");

			private static NativeCalleeAttribute _attribute_OnLodPartialRebuild =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnLodPartialRebuild");

			private static NativeCalleeAttribute _attribute_OnRebuildMaterials =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnRebuildMaterials");

			private static NativeCalleeAttribute _attribute_OnUnloadNonActiveLodLevel =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnUnloadNonActiveLodLevel");

			#endregion

			#endregion
		}
	}
}