namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Proxy for native invoke for AvatarManager.
		/// </summary>
		class NativeCall_AvatarManager : NativeCallProxy
		{
			#region Caller Methods

			public void LoadAvatar(string userId)
			{
				var args = this._method_LoadAvatar.invokeArgumentTable;
				//
				args.SetStringParam(0, userId);

				//
				this._method_LoadAvatar.DoApply();
			}

			public void UnloadAvatar(string userId)
			{
				var args = this._method_UnloadAvatar.invokeArgumentTable;
				//
				args.SetStringParam(0, userId);

				//
				this._method_UnloadAvatar.DoApply();
			}

			public void PostInitialize(string version, string paramsJsonText)
			{
				var args = this._method_PostInitialize.invokeArgumentTable;
				args.SetStringParam(0, version);
				args.SetStringParam(1, paramsJsonText);
				this._method_PostInitialize.DoApply();
			}

			public void OnApplicationPause(bool paused)
			{
				var args = this._method_OnApplicationPause.invokeArgumentTable;
				args.SetBoolParam(0, paused);
				this._method_OnApplicationPause.DoApply();
			}

			public void Uninitialize()
			{
				var args = this._method_Uninitialize.invokeArgumentTable;
				this._method_Uninitialize.DoApply();
			}

			public void SetEnableWeakNetworkMode(bool isEnabled)
			{
				var args = this._method_SetEnableWeakNetworkMode.invokeArgumentTable;
				args.SetBoolParam(0, isEnabled);
				this._method_SetEnableWeakNetworkMode.DoApply();
			}

			#endregion


			#region Callee Methods

			private void OnInitialized(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint resultCode = 1;
				invokeArguments.GetUIntParam(0, ref resultCode);
				string resultStr = "";
				resultStr = invokeArguments.GetUTF8StringParam(1);
				if (resultCode == 0)
				{
					_avatarManager.OnInitialized(true);
				}
				else
				{
					var desc = invokeArguments.GetStringParam(1);

					if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "C# AvatarManager OnInitialized failed" + desc);
					}

					//notify avatar manager that failed to initialize.
					_avatarManager.OnInitialized(false);
				}
			}

			private void OnMessage(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint messageType = 1;
				invokeArguments.GetUIntParam(0, ref messageType);
				var content = invokeArguments.GetStringParam(1);
				//
				_avatarManager.Notify_Message(messageType, content);
			}

			private void OnAttachNativeAvatar(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint nativeAvatarId = 0;

				var userIdStr = invokeArguments.GetStringParam(0);
				var avatarId = invokeArguments.GetStringParam(1);
				invokeArguments.GetUIntParam(2, ref nativeAvatarId);

				var userId = userIdStr;
				_avatarManager.OnAttachNativeAvatar(userId, avatarId, nativeAvatarId);
			}

			private void OnAvatarLoadFailed(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				var userIdStr = invokeArguments.GetStringParam(0);
				var avatarId = invokeArguments.GetStringParam(1);
				// 
				uint errorCode = 1;
				invokeArguments.GetUIntParam(2, ref errorCode);
				var errorDesc = invokeArguments.GetStringParam(3);

				var userId = userIdStr;
				_avatarManager.Notify_AvatarLoadFailed(userId, avatarId, (NativeResult)errorCode, errorDesc);
			}

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarManager(PicoAvatarManager AvatarManager, uint instanceId)
				: base(0)
			{
				_avatarManager = AvatarManager;

				/// Callee methods.
				{
					AddCalleeMethod(_attribute_OnInitialized, OnInitialized);
					AddCalleeMethod(_attribute_OnMessage, OnMessage);
					AddCalleeMethod(_attribute_OnAttachAvatar, OnAttachNativeAvatar);
					AddCalleeMethod(_attribute_OnAvatarLoadFailed, OnAvatarLoadFailed);
				}

				/// Caller methods.
				{
					this._method_LoadAvatar = AddCallerMethod(_attribute_LoadAvatar);
					this._method_UnloadAvatar = AddCallerMethod(_attribute_UnloadAvatar);
					this._method_OnApplicationPause = AddCallerMethod(_attribute_OnApplicationPause);
					this._method_PostInitialize = AddCallerMethod(_attribute_PostInitialize);
					this._method_Uninitialize = AddCallerMethod(_attribute_Uninitialize);
					this._method_SetEnableWeakNetworkMode = AddCallerMethod(_attribute_SetEnableWeakNetworkMode);
				}
			}

			#region Private Fields

			private PicoAvatarManager _avatarManager;
			private NativeCaller _method_LoadAvatar;
			private NativeCaller _method_UnloadAvatar;
			private NativeCaller _method_OnApplicationPause;
			private NativeCaller _method_PostInitialize;
			private NativeCaller _method_Uninitialize;
			private NativeCaller _method_SetEnableWeakNetworkMode;

			#endregion

			#region Static Part

			private const string className = "AvatarManager";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_LoadAvatar =
				new NativeCallerAttribute(className, "LoadAvatar", (uint)0);

			private static NativeCallerAttribute _attribute_UnloadAvatar =
				new NativeCallerAttribute(className, "UnloadAvatar", (uint)0);

			private static NativeCallerAttribute _attribute_OnApplicationPause =
				new NativeCallerAttribute(className, "OnApplicationPause");

			private static NativeCallerAttribute _attribute_PostInitialize =
				new NativeCallerAttribute(className, "PostInitialize");

			private static NativeCallerAttribute _attribute_Uninitialize =
				new NativeCallerAttribute(className, "Uninitialize");

			private static NativeCallerAttribute _attribute_SetEnableWeakNetworkMode =
				new NativeCallerAttribute(className, "SetEnableWeakNetworkMode");

			/// Callee Attributes.
			private static NativeCalleeAttribute _attribute_OnInitialized =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnInitialized");

			private static NativeCalleeAttribute _attribute_OnMessage =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnMessage");

			private static NativeCalleeAttribute _attribute_OnAttachAvatar =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAttachAvatar");

			private static NativeCalleeAttribute _attribute_OnAvatarLoadFailed =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnAvatarLoadFailed");

			#endregion

			#endregion
		}
	}
}