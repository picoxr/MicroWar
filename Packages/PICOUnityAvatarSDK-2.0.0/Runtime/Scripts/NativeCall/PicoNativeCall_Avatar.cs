namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for Avatar.
		/// </summary>
		class NativeCall_Avatar : NativeCallProxy
		{
			#region Caller Methods

			public void InitializeEdit()
			{
				//
				this._method_InitializeEdit.DoApply();
			}

			public void SetVisible(bool visible)
			{
				var args = this._method_SetVisible.invokeArgumentTable;
				//
				args.SetByteParam(0, visible ? (byte)1 : (byte)0);

				this._method_SetVisible.DoApply();
			}

			public void SetAvatarHeight(float height)
			{
				var args = this._method_AlignHeight.invokeArgumentTable;
				args.SetFloatParam(0, height);
				this._method_AlignHeight.DoApply();
			}

			public void PlayAnimation(string name, float loopTime)
			{
				var args = this._method_PlayAnimation.invokeArgumentTable;
				args.SetStringParam(0, name);
				args.SetFloatParam(1, loopTime);
				this._method_PlayAnimation.DoApply();
			}

			public void StopAnimation(bool immediately)
			{
				var args = this._method_StopAnimation.invokeArgumentTable;
				args.SetBoolParam(0, immediately);
				this._method_StopAnimation.DoApply();
			}

			public string GetAnimationNames()
			{
				this._method_GetAnimationNames.DoApply();
				//
				var returnParams = this._method_GetAnimationNames.returnArgumentTable;
				var animations = returnParams.GetUTF8StringParam(0);
				//
				return animations;
			}

			// Remove animation set by its id
			public bool RemoveAnimationSet(string animationSetId)
			{
				var args = this._method_RemoveAnimationSet.invokeArgumentTable;
				args.SetStringParam(0, animationSetId);
				this._method_RemoveAnimationSet.DoApply();
				//
				var returnParams = this._method_GetAnimationNames.returnArgumentTable;
				bool res = false;
				returnParams.GetBoolParam(0, ref res);
				//
				return res;
			}

			// Add animation set by its id
			public void AddAnimationSet(string animationSetId)
			{
				var args = this._method_AddAnimationSet.invokeArgumentTable;
				args.SetStringParam(0, animationSetId);
				this._method_AddAnimationSet.DoApply();
			}

			public void LoadAnimationsExtern(string assetBundlePath, string animationPathsJson)
			{
				var args = this._method_LoadAnimationsExtern.invokeArgumentTable;
				args.SetStringParam(0, assetBundlePath);
				args.SetStringParam(1, animationPathsJson);
				this._method_LoadAnimationsExtern.DoApply();
			}

			public void LoadAnimationsFromAssetBundle(AvatarAssetBundle ab, string animationPathsJson)
			{
				var args = this._method_LoadAnimationsFromAssetBundle.invokeArgumentTable;
				args.SetObjectParam(0, ab.nativeHandle);
				args.SetStringParam(1, animationPathsJson);
				this._method_LoadAnimationsFromAssetBundle.DoApply();
			}

			public void RemoveAnimation(string name)
			{
				var args = this._method_RemoveAnimation.invokeArgumentTable;
				args.SetStringParam(0, name);
				this._method_RemoveAnimation.DoApply();
			}

			public void ForceUpdateLod()
			{
				var args = this._method_ForceUpdateLod.invokeArgumentTable;
				this._method_ForceUpdateLod.DoApply();
			}

			public void ForceUpdate(uint updateFlags)
			{
				var args = this._method_ForceUpdate.invokeArgumentTable;
				args.SetUIntParam(0, updateFlags);
				this._method_ForceUpdate.DoApply();
			}

			public string GetAvatarSpecText()
			{
				this._method_GetAvatarSpecText.DoApply();
				//
				var args = this._method_GetAvatarSpecText.returnArgumentTable;
				return args.GetStringParam(0);
			}

			#endregion

			#region Callee Methods

			private void OnAvatarSpecUpdated(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				var avatarId = invokeArguments.GetStringParam(0);
				var jsonSpecData = invokeArguments.GetStringParam(1);
				//
				_avatar.Notify_SpecUpdated(avatarId, jsonSpecData);
			}

			private void OnAvatarEntityLoaded(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint nativeEntityId = 0;
				uint lodLevel = 0;
				uint avatarSex = 0;

				invokeArguments.GetUIntParam(0, ref nativeEntityId);
				invokeArguments.GetUIntParam(1, ref lodLevel);
				invokeArguments.GetUIntParam(2, ref avatarSex);
				var styleName = invokeArguments.GetStringParam(3);

				if (nativeEntityId != 0)
				{
					_avatar?.Notify_AvatarEntityLoaded(nativeEntityId, (AvatarLodLevel)lodLevel
						, (AvatarSexType)avatarSex, styleName);
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "Failed to load Avatar.");
				}
			}

			private void OnLoadAnimationsExternComplete(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				string assetBundlePath = invokeArguments.GetUTF8StringParam(0);
				string animationNamesJson = invokeArguments.GetUTF8StringParam(1);
				_avatar.Notify_LoadAnimationsExternComplete(assetBundlePath, animationNamesJson);
			}

			private void OnEntityLodLevelLoadFailed(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				unchecked
				{
					uint nativeEntityId = 0;
					uint lodLevel = (uint)AvatarLodLevel.Invalid;
					invokeArguments.GetUIntParam(0, ref nativeEntityId);
					invokeArguments.GetUIntParam(1, ref lodLevel);
					_avatar.Notify_OnEntityLodLevelLoadFailed(nativeEntityId, (AvatarLodLevel)lodLevel);
				}
			}

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_Avatar(PicoAvatar avatar, uint instanceId)
				: base(instanceId)
			{
				_avatar = avatar;


				/// Callee methods.
				{
					AddCalleeMethod(_attribute_OnAvatarSpecUpdated, OnAvatarSpecUpdated);
					AddCalleeMethod(_attribute_OnAvatarEntityLoaded, OnAvatarEntityLoaded);
					AddCalleeMethod(_attribute_OnLoadAnimationsExternComplete, OnLoadAnimationsExternComplete);
					AddCalleeMethod(_attribute_OnEntityLodLevelLoadFailed, OnEntityLodLevelLoadFailed);
				}

				/// Caller methods.
				{
					this._method_InitializeEdit = AddCallerMethod(_attribute_InitializeEdit);
					this._method_SetVisible = AddCallerMethod(_attribute_SetVisible);
					this._method_AlignHeight = AddCallerMethod(_attribute_AlignHeight);
					this._method_PlayAnimation = AddCallerMethod(_attribute_PlayAnimation);
					this._method_StopAnimation = AddCallerMethod(_attribute_StopAnimation);
					this._method_GetAnimationNames = AddCallerMethod(_attribute_GetAnimationNames);
					this._method_RemoveAnimationSet = AddCallerMethod(_attribute_RemoveAnimationSet);
					this._method_AddAnimationSet = AddCallerMethod(_attribute_AddAnimationSet);
					this._method_LoadAnimationsExtern = AddCallerMethod(_attribute_LoadAnimationsExtern);
					this._method_LoadAnimationsFromAssetBundle =
						AddCallerMethod(_attribute_LoadAnimationsFromAssetBundle);
					this._method_RemoveAnimation = AddCallerMethod(_attribute_RemoveAnimation);
					this._method_ForceUpdateLod = AddCallerMethod(_attribute_ForceUpdateLod);
					this._method_ForceUpdate = AddCallerMethod(_attribute_ForceUpdate);
					this._method_GetAvatarSpecText = AddCallerMethod(_attribute_GetAvatarSpecText);
				}
			}

			#region Private Fields

			private PicoAvatar _avatar;
			private NativeCaller _method_SetVisible;
			private NativeCaller _method_InitializeEdit;
			private NativeCaller _method_AlignHeight;
			private NativeCaller _method_PlayAnimation;
			private NativeCaller _method_StopAnimation;
			private NativeCaller _method_GetAnimationNames;
			private NativeCaller _method_RemoveAnimationSet;
			private NativeCaller _method_AddAnimationSet;
			private NativeCaller _method_LoadAnimationsExtern;
			private NativeCaller _method_LoadAnimationsFromAssetBundle;
			private NativeCaller _method_RemoveAnimation;
			private NativeCaller _method_ForceUpdateLod;
			private NativeCaller _method_ForceUpdate;
			private NativeCaller _method_GetAvatarSpecText;
			private NativeCaller _method_OnEntityLodLevelLoadFailed;

			#endregion

			#region Static Part

			private const string className = "Avatar";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_SetVisible =
				new NativeCallerAttribute(className, "SetVisible", (uint)0);

			private static NativeCallerAttribute _attribute_InitializeEdit =
				new NativeCallerAttribute(className, "InitializeEdit", (uint)0);

			private static NativeCallerAttribute _attribute_AlignHeight =
				new NativeCallerAttribute(className, "AlignHeight", (uint)0);

			private static NativeCallerAttribute _attribute_PlayAnimation =
				new NativeCallerAttribute(className, "PlayAnimation", (uint)0);

			private static NativeCallerAttribute _attribute_StopAnimation =
				new NativeCallerAttribute(className, "StopAnimation", (uint)0);

			private static NativeCallerAttribute _attribute_GetAnimationNames = new NativeCallerAttribute(className,
				"GetAnimationNames", ((uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NoCallArgument));

			private static NativeCallerAttribute _attribute_RemoveAnimationSet =
				new NativeCallerAttribute(className, "RemoveAnimationSet", (uint)0);

			private static NativeCallerAttribute _attribute_AddAnimationSet =
				new NativeCallerAttribute(className, "AddAnimationSet", (uint)0);

			private static NativeCallerAttribute _attribute_LoadAnimationsExtern =
				new NativeCallerAttribute(className, "LoadAnimationsExtern", (uint)0);

			private static NativeCallerAttribute _attribute_LoadAnimationsFromAssetBundle =
				new NativeCallerAttribute(className, "LoadAnimationsFromAssetBundle", (uint)0);

			private static NativeCallerAttribute _attribute_RemoveAnimation =
				new NativeCallerAttribute(className, "RemoveAnimation", (uint)0);

			private static NativeCallerAttribute _attribute_ForceUpdateLod =
				new NativeCallerAttribute(className, "ForceUpdateLod", (uint)0);

			private static NativeCallerAttribute _attribute_ForceUpdate =
				new NativeCallerAttribute(className, "ForceUpdate", (uint)0);

			private static NativeCallerAttribute _attribute_GetAvatarSpecText = new NativeCallerAttribute(className,
				"GetAvatarSpecText", ((uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NoCallArgument));

			/// Callee Attributes.
			private static NativeCalleeAttribute _attribute_OnAvatarEntityLoaded =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAvatarEntityLoaded");

			private static NativeCalleeAttribute _attribute_OnAvatarSpecUpdated =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAvatarSpecUpdated");

			private static NativeCalleeAttribute _attribute_OnLoadAnimationsExternComplete =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnLoadAnimationsExternComplete");

			private static NativeCalleeAttribute _attribute_OnEntityLodLevelLoadFailed =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnEntityLodLevelLoadFailed");

			#endregion

			#endregion
		}
	}
}