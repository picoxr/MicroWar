namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Net stats
		/// </summary>
		public class NetStats
		{
			public uint ping;
			public uint maxBandWidth;
			public uint avgBandWidth;
		}


		/// <summary>
		/// Proxy for native invoke for AvatarManager.
		/// </summary>
		class NativeCall_AvatarApp : NativeCallProxy
		{
			#region Caller Methods

			public void Shutdown(uint reason)
			{
				var args = this._method_Shutdown.invokeArgumentTable;
				//
				args.SetUIntParam(0, reason);

				this._method_Shutdown.DoApply();
			}

			public void SetBurden(uint burdenLevel)
			{
				var args = this._method_SetBurden.invokeArgumentTable;
				//
				args.SetUIntParam(0, burdenLevel);

				this._method_SetBurden.DoApply();
			}

			public void SetDebugConfig(string debugConfigText)
			{
				var args = this._method_SetDebugConfig.invokeArgumentTable;
				//
				args.SetStringParam(0, debugConfigText);

				this._method_SetDebugConfig.DoApply();
			}

			public void SetDebugLogMasks(uint debugLogMasks)
			{
				var args = this._method_SetDebugLogMasks.invokeArgumentTable;
				args.SetUIntParam(0, debugLogMasks);
				this._method_SetDebugLogMasks.DoApply();
			}

			public NetStats GetNetStats()
			{
				var netStats = new NetStats();
				//
				this._method_GetNetStats.DoApply();
				//
				var returnParams = this._method_GetNetStats.returnArgumentTable;
				returnParams.GetUIntParam(0, ref netStats.ping);
				returnParams.GetUIntParam(1, ref netStats.maxBandWidth);
				returnParams.GetUIntParam(2, ref netStats.avgBandWidth);
				//
				return netStats;
			}

			public void GarbageCollection(uint gcLevel)
			{
				var args = this._method_GarbageCollection.invokeArgumentTable;
				args.SetUIntParam(0, (uint)gcLevel);
				this._method_GarbageCollection.DoApply();
			}
			
			public void SetEnableAvatarTracker(bool enableTrace)
			{
				var args = this._method_SetEnableAvatarTracker.invokeArgumentTable;
				args.SetBoolParam(0, enableTrace);
				this._method_SetEnableAvatarTracker.DoApply();
			}

			#endregion

			#region Callee Methods

			/// <summary>
			/// Notification from native that error happen.
			/// </summary>
			/// <param name="invokeArguments"></param>
			/// <param name="invokee"></param>
			private void OnError(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				uint errorCode = 0;
				invokeArguments.GetUIntParam(0, ref errorCode);
				string erroDesc = invokeArguments.GetStringParam(1);

				if (errorCode == 0)
				{
				}
				else
				{
					if (AvatarEnv.NeedLog(DebugLogMask.GeneralError))
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "OnJSError " + erroDesc);
					}
				}
			}

			private void OnCreateSkeleton(IDParameterTable invokeArguments, NativeCallee invokee)
			{
				if (AvatarEnv.NeedLog(DebugLogMask.NativeCallTrivial))
				{
					AvatarEnv.Log(DebugLogMask.NativeCallTrivial, "OnLodChanged");
				}

				string content = "";
				content = invokeArguments.GetStringParam(0);
				EditorDrawEntity.BuildByJson(content);

				// this._avatarEntity?.OnLodChanged((AvatarLodLevel)level);
			}

			#endregion


			#region NativeCall Framework Methods/Fields

			public NativeCall_AvatarApp(PicoAvatarApp obj, uint instanceId)
				: base(0)
			{
				_Object = obj;


				/// Callee methods.
				{
					AddCalleeMethod(_attribute_OnError, OnError);
					AddCalleeMethod(_attribute_OnDrawEntity, OnCreateSkeleton);
				}

				/// Caller methods.
				{
					this._method_Shutdown = AddCallerMethod(_attribute_Shutdown);
					this._method_SetBurden = AddCallerMethod(_attribute_SetBurden);
					this._method_SetDebugConfig = AddCallerMethod(_attribute_SetDebugConfig);
					this._method_SetDebugLogMasks = AddCallerMethod(_attribute_SetDebugLogMasks);
					this._method_GetNetStats = AddCallerMethod(_attribute_GetNetStats);
					this._method_GarbageCollection = AddCallerMethod(_attribute_GarbageCollection);
					this._method_SetEnableAvatarTracker = AddCallerMethod(_attribute_SetEnableAvatarTracker);
				}
			}


			#region Private Fields

			private PicoAvatarApp _Object;

			private NativeCaller _method_Shutdown;
			private NativeCaller _method_SetBurden;
			private NativeCaller _method_SetDebugConfig;
			private NativeCaller _method_SetDebugLogMasks;
			private NativeCaller _method_GetNetStats;
			private NativeCaller _method_SetEnableAvatarTracker;
			private NativeCaller _method_GarbageCollection;

			#endregion

			#region Static Part

			private const string className = "AvatarApp";

			/// Caller Attributes.
			private static NativeCallerAttribute _attribute_Shutdown = new NativeCallerAttribute(className, "Shutdown");

			private static NativeCallerAttribute _attribute_SetBurden =
				new NativeCallerAttribute(className, "SetBurden");

			private static NativeCallerAttribute _attribute_SetDebugConfig =
				new NativeCallerAttribute(className, "SetDebugConfig");

			private static NativeCallerAttribute _attribute_SetDebugLogMasks =
				new NativeCallerAttribute(className, "SetDebugLogMasks");

			private static NativeCallerAttribute _attribute_GetNetStats = new NativeCallerAttribute(className,
				"GetNetStats", ((uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NoCallArgument));

			private static NativeCallerAttribute _attribute_GarbageCollection =
				new NativeCallerAttribute(className, "GarbageCollection");

			private static NativeCallerAttribute _attribute_SetEnableAvatarTracker =
				new NativeCallerAttribute(className, "SetEnableAvatarTracker");

			/// Callee Attributes.
			private static NativeCalleeAttribute _attribute_OnError =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnError");

			private static NativeCalleeAttribute _attribute_OnDrawEntity =
				new NativeCalleeAttribute(typeof(NativeCallee), className, "OnDrawEntity");

			#endregion

			#endregion
		}
	}
}