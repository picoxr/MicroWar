namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for AvatarManager.
		/// </summary>
		class NativeCall_AvatarEditState : NativeCallProxy
		{
			#region Caller Methods

			public void EnterState(uint editAspect)
			{
				var args = this._method_EnterState.invokeArgumentTable;
				//
				args.SetUIntParam(0, editAspect);

				this._method_EnterState.DoApply();
			}

			public void ExitState()
			{
				var args = this._method_ExitState.invokeArgumentTable;
				//
				this._method_ExitState.DoApply();
			}

			public void SetEditConfig(string editConfig)
			{
				var args = this._method_setEditConfig.invokeArgumentTable;
				args.SetStringParam(0, editConfig);
				//
				this._method_setEditConfig.DoApply();
			}

			public string SetAssetPinDIYParams(string assetId, string diyParamsText, string paramGroup,
				uint operationId)
			{
				var args = this._method_SetAssetPinDIYParams.invokeArgumentTable;
				//
				args.SetStringParam(0, assetId);
				args.SetStringParam(1, diyParamsText);
				args.SetStringParam(2, paramGroup);
				args.SetUIntParam(3, operationId);

				this._method_SetAssetPinDIYParams.DoApply();

				var returnParams = this._method_SetAssetPinDIYParams.returnArgumentTable;
				return returnParams.GetStringParam(0);
			}

			public string SetShapingParam(uint paramGroup, uint paramId, uint paramKind, string paramVal,
				uint operationId, bool forceUpdate)
			{
				var args = this._method_SetShapingParam.invokeArgumentTable;
				//
				args.SetUIntParam(0, paramGroup);
				args.SetUIntParam(1, paramId);
				args.SetUIntParam(2, paramKind);
				args.SetStringParam(3, paramVal);
				args.SetUIntParam(4, operationId);
				args.SetBoolParam(5, forceUpdate);
				//
				this._method_SetShapingParam.DoApply();

				//
				var returnParams = this._method_SetShapingParam.returnArgumentTable;
				return returnParams.GetStringParam(0);
			}

			public string GetShapingParam(uint paramGroup, uint paramId, uint paramKind)
			{
				var args = this._method_GetShapingParam.invokeArgumentTable;
				//
				args.SetUIntParam(0, paramGroup);
				args.SetUIntParam(1, paramId);
				args.SetUIntParam(2, paramKind);
				//
				this._method_GetShapingParam.DoApply();

				//
				var returnParams = this._method_GetShapingParam.returnArgumentTable;
				uint errorCode = 0;
				returnParams.GetUIntParam(0, ref errorCode);
				if (errorCode == (uint)NativeResult.Success)
				{
					return returnParams.GetStringParam(1);
				}

				return null;
			}

			public void ForceUpdate()
			{
				var args = this._method_ForceUpdate.invokeArgumentTable;

				this._method_ForceUpdate.DoApply();
			}

			public void SetShapingPreset(string presetTypeName, string paramsJsonText, uint operationId)
			{
				var args = this._method_SetShapingPreset.invokeArgumentTable;
				//
				args.SetStringParam(0, presetTypeName);
				args.SetStringParam(1, paramsJsonText);
				args.SetUIntParam(2, operationId);

				this._method_SetShapingPreset.DoApply();
			}

			public string GetShapingPreset(string presetTypeName)
			{
				var args = this._method_GetShapingPreset.invokeArgumentTable;
				//
				args.SetStringParam(0, presetTypeName);
				//
				this._method_GetShapingPreset.DoApply();

				//
				var returnParams = this._method_GetShapingPreset.returnArgumentTable;
				return returnParams.GetStringParam(0);
			}

			public bool StartCompareShapingParam(uint paramGroup, uint paramId)
			{
				var args = this._method_StartCompare.invokeArgumentTable;
				//
				args.SetUIntParam(0, paramGroup);
				args.SetUIntParam(1, paramId);
				//
				this._method_StartCompare.DoApply();
				//
				var returnParams = this._method_StartCompare.returnArgumentTable;
				uint errorCode = 0;
				returnParams.GetUIntParam(0, ref errorCode);
				//
				return errorCode == (uint)NativeResult.Success;
			}

			public void EndCompareShapingParam()
			{
				this._method_EndCompare.DoApply();
			}

			public string GetAllShapingParamConfig()
			{
				this._method_GetAllShapingParamConfig.DoApply();

				var returnParams = this._method_GetAllShapingParamConfig.returnArgumentTable;
				return returnParams.GetStringParam(0);
			}

			public string GetUpdatedAvatarSpecText()
			{
				this._method_GetUpdatedAvatarSpecText.DoApply();
				//
				var returnParams = this._method_GetUpdatedAvatarSpecText.returnArgumentTable;
				return returnParams.GetStringParam(0);
			}

			public string MakeUIAssetPinProtoText(string avatarSpecAssetPinProtoText)
			{
				var args = this._method_MakeUIAssetPinProtoText.invokeArgumentTable;
				args.SetStringParam(0, avatarSpecAssetPinProtoText);

				this._method_MakeUIAssetPinProtoText.DoApply();
				//
				var returnParams = this._method_MakeUIAssetPinProtoText.returnArgumentTable;
				return returnParams.GetStringParam(0);
			}

			#endregion


			#region Callee Methods

			// Notification from native that error happen.
			private void OnEnterState(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint errorCode = 0;
				invokeArguments.GetUIntParam(0, ref errorCode);

				if (_Object != null)
				{
					_Object.Notify_EnterState((NativeResult)errorCode);
				}
				else
				{
					string erroDesc = invokeArguments.GetStringParam(1);
					//
					if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, " OnEnterState error: " + erroDesc);
					}
				}
			}

			// Notification from ntive that spec changed.
			private void OnAvatrSpecChanged(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				var avatarSpecText = invokeArguments.GetStringParam(0);

				if (_Object != null)
				{
					_Object.Notify_AvatrSpecChanged(avatarSpecText);
				}
			}

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarEditState(AvatarEditState obj, uint instanceId)
				: base(instanceId)
			{
				_Object = obj;

				/// Callee methods.
				{
					AddCalleeMethod(_attribute_OnEnterState, OnEnterState);
					AddCalleeMethod(_attribute_OnAvatrSpecChanged, OnAvatrSpecChanged);
				}

				/// Caller methods.
				{
					this._method_EnterState = AddCallerMethod(_attribute_EnterState);
					this._method_ExitState = AddCallerMethod(_attribute_ExitState);
					this._method_setEditConfig = AddCallerMethod(_attribute_SetEditConfig);
					this._method_SetAssetPinDIYParams = AddCallerMethod(_attribute_SetAssetPinDIYParams);
					this._method_SetShapingParam = AddCallerMethod(_attribute_SetShapingParam);
					this._method_GetShapingParam = AddCallerMethod(_attribute_GetShapingParam);
					this._method_ForceUpdate = AddCallerMethod(_attribute_ForceUpdate);
					this._method_SetShapingPreset = AddCallerMethod(_attribute_SetShapingPreset);
					this._method_GetShapingPreset = AddCallerMethod(_attribute_GetShapingPreset);
					this._method_StartCompare = AddCallerMethod(_attribute_StartCompare);
					this._method_EndCompare = AddCallerMethod(_attribute_EndCompare);
					this._method_GetAllShapingParamConfig = AddCallerMethod(_attribute_GetAllShapingParamConfig);
					this._method_GetUpdatedAvatarSpecText = AddCallerMethod(_attribute_GetUpdatedAvatarSpecText);
					this._method_MakeUIAssetPinProtoText = AddCallerMethod(_attribute_MakeUIAssetPinProtoText);
				}
			}


			#region Private Fields

			private AvatarEditState _Object;

			private NativeCaller _method_EnterState;
			private NativeCaller _method_ExitState;
			private NativeCaller _method_setEditConfig;
			private NativeCaller _method_SetShapingParam;
			private NativeCaller _method_SetAssetPinDIYParams;
			private NativeCaller _method_GetShapingParam;
			private NativeCaller _method_ForceUpdate;
			private NativeCaller _method_SetShapingPreset;
			private NativeCaller _method_GetShapingPreset;
			private NativeCaller _method_StartCompare;
			private NativeCaller _method_EndCompare;
			private NativeCaller _method_GetAllShapingParamConfig;
			private NativeCaller _method_GetUpdatedAvatarSpecText;
			private NativeCaller _method_MakeUIAssetPinProtoText;

			#endregion


			#region Static Part

			private const string className = "AvatarEditState";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_EnterState =
				new NativeCallerAttribute(className, "EnterState");

			private static NativeCallerAttribute _attribute_ExitState =
				new NativeCallerAttribute(className, "ExitState");

			private static NativeCallerAttribute _attribute_SetEditConfig =
				new NativeCallerAttribute(className, "SetEditConfig");

			private static NativeCallerAttribute _attribute_SetAssetPinDIYParams =
				new NativeCallerAttribute(className, "SetAssetPinDIYParams");

			private static NativeCallerAttribute _attribute_SetShapingParam =
				new NativeCallerAttribute(className, "SetShapingParam");

			private static NativeCallerAttribute _attribute_GetShapingParam =
				new NativeCallerAttribute(className, "GetShapingParam");

			private static NativeCallerAttribute _attribute_ForceUpdate =
				new NativeCallerAttribute(className, "ForceUpdate");

			private static NativeCallerAttribute _attribute_SetShapingPreset =
				new NativeCallerAttribute(className, "SetShapingPreset");

			private static NativeCallerAttribute _attribute_GetShapingPreset =
				new NativeCallerAttribute(className, "GetShapingPreset");

			private static NativeCallerAttribute _attribute_StartCompare = new NativeCallerAttribute(className,
				"StartCompare", ((uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NoCallArgument));

			private static NativeCallerAttribute _attribute_EndCompare =
				new NativeCallerAttribute(className, "EndCompare");

			private static NativeCallerAttribute _attribute_GetAllShapingParamConfig =
				new NativeCallerAttribute(className, "GetAllShapingParamConfig");

			private static NativeCallerAttribute _attribute_GetUpdatedAvatarSpecText =
				new NativeCallerAttribute(className, "GetUpdatedAvatarSpecText");

			private static NativeCallerAttribute _attribute_MakeUIAssetPinProtoText =
				new NativeCallerAttribute(className, "MakeUIAssetPinProtoText");

			/// Callee Attributes.
			private static NativeCalleeAttribute _attribute_OnEnterState =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnEnterState");

			private static NativeCalleeAttribute _attribute_OnAvatrSpecChanged =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAvatrSpecChanged");

			#endregion

			#endregion
		}
	}
}