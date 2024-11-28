using System;
namespace Pico
{
	namespace Avatar
	{
		// Request for load avatar.      
		public class LoadAvatarRequest : AsyncRequestBase
		{
			public static void DoRequest(string userId, string avatarId, string capabilities, string characterType, string characterVersion)
			{
				var req = new LoadAvatarRequest();
				var args = req.invokeArgumentTable;
				//
				args.SetStringParam(0, userId.ToString());
				args.SetStringParam(1, avatarId);
				args.SetStringParam(2, capabilities);
				args.SetStringParam(3, characterType);
				args.SetStringParam(4, characterVersion);
				////test
				////{
				//    var bytes = new byte[1024];
				//    bytes[0] = (byte)'a'; bytes[1] = (byte)'b';
				//    var binaryData = new MemoryView(bytes, true);
				//    args.SetObjectParam(100, binaryData.nativeHandle);
				////}

				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					// test.
					//binaryData.CheckDelete();

					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetStringParam(1);
						//

						var avatarBase = PicoAvatarManager.instance.GetAvatar(userId);
						if (avatarBase == null)
						{
							if (errorCode == 0)
							{
								AvatarEnv.Log(DebugLogMask.GeneralError,
									"LoadAvatarRequest returned but avatar with same id has been added!");
							}
							else
							{
								AvatarEnv.Log(DebugLogMask.GeneralError,
									String.Format("LoadAvatarRequest failed. errorCode:{0} reason:{1}", errorCode,
										returnData));
							}
						}
						
						// Note: only avatar spec data arrived, avatar grpahics data has not been ready!
						PicoAvatarManager.instance.ProcessAvatarLoadRequest(userId, (int)errorCode,
							returnData);
					}
					else
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							"LoadAvatarRequest failed since PicoAvatarManager destroyed!");
					}
				});
			}

			// constructor.
			private LoadAvatarRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest", "LoadAvatar"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request for load avatar with json. It will be deleted soon
		[Obsolete]
		public class LoadAvatarWithJsonSpecRequest : AsyncRequestBase
		{
			public static void DoRequest(string userId, string jsonSpecdata, string capabilities)
			{
				var req = new LoadAvatarWithJsonSpecRequest();
				//
				var args = req.invokeArgumentTable;
				//
				args.SetStringParam(0, userId.ToString());
				args.SetStringParam(1, jsonSpecdata);
				args.SetStringParam(2, capabilities);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetStringParam(1);
						var avatarBase = PicoAvatarManager.instance.GetAvatar(userId);
						if (avatarBase == null)
						{
							if (errorCode == 0)
							{
								AvatarEnv.Log(DebugLogMask.GeneralError,
									"LoadAvatarWithJsonSpecRequest returned but avatar with same id has been added!");
							}
							else
							{
								AvatarEnv.Log(DebugLogMask.GeneralError,
									String.Format("LoadAvatarWithJsonSpecRequest failed. errorCode:{0} reason:{1}", errorCode,
										returnData));
							}
						}
						// Note: only avatar spec data arrived, avatar grpahics data has not been ready!
						PicoAvatarManager.instance.ProcessAvatarLoadRequest(userId, (int)errorCode,
							returnData);
					}
				});
			}

			private LoadAvatarWithJsonSpecRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest",
				"LoadAvatarWithJsonSpec"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request verify app token.
		public class VerifyAppTokenRequest : AsyncRequestBase
		{
			public static void DoRequest(System.Action<uint, string> callback = null)
			{
				var req = new VerifyAppTokenRequest();
				//
				var args = req.invokeArgumentTable;
				//
				// args.SetStringParam(0, userId.ToString());
				//args.SetStringParam(1, jsonSpecdata);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					uint errorCode = 1;
					returnParams.GetUIntParam(0, ref errorCode);
					var returnData = returnParams.GetStringParam(1);
					//
					if (PicoAvatarManager.instance != null)
					{
						PicoAvatarManager.instance.OnInitialized(errorCode == 0 ? true : false);
					}

					//
					if (callback != null)
					{
						callback(errorCode, returnData);
					}
				});
			}

			private VerifyAppTokenRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest", "VerifyAppToken"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request app mode
		public class VerifyAppModeRequest : AsyncRequestBase
		{
			public static void DoRequest(string appID, System.Action<int, int> callback = null)
			{
				var req = new VerifyAppModeRequest();
				//
				var args = req.invokeArgumentTable;
				//
				args.SetStringParam(0, appID);
				//args.SetStringParam(1, jsonSpecdata);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					uint errorCode = 1;
					returnParams.GetUIntParam(0, ref errorCode);
					var type = returnParams.GetStringParam(1);

					//
					if (callback != null)
					{
						callback((int)errorCode, int.TryParse(type, out var res) ? res : 0);
					}
				});
			}

			private VerifyAppModeRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest", "VerifyAppMode"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}
		
		// Request for Template Avatar list.
		public class RequestTemplateAvatarRequest : AsyncRequestBase
		{
			public static void DoRequest(System.Action<NativeResult, string> responsed = null)
			{
				var req = new RequestTemplateAvatarRequest();
				//
				var args = req.invokeArgumentTable;

				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//
						responsed?.Invoke((NativeResult)errorCode, returnData);
					}
				});
			}

			//
			private RequestTemplateAvatarRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest",
				"RequestTemplateAvatar"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request for Character list.
		public class RequestCharacterListRequest : AsyncRequestBase
		{
			public static void DoRequest(System.Action<NativeResult, string> responsed = null)
			{
				var req = new RequestCharacterListRequest();
				//
				var args = req.invokeArgumentTable;

				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//
						responsed?.Invoke((NativeResult)errorCode, returnData);
					}
				});
			}

			//
			private RequestCharacterListRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest",
				"RequestCharacterList"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request for AvatarList in Character.
		public class RequestAvatarListInCharacterRequest : AsyncRequestBase
		{
			public static void DoRequest(string characterId, System.Action<NativeResult, string> responsed = null)
			{
				var req = new RequestAvatarListInCharacterRequest();
				//
				var args = req.invokeArgumentTable;

				args.SetStringParam(0, characterId.ToString());

				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//
						responsed?.Invoke((NativeResult)errorCode, returnData);
					}
				});
			}

			//
			private RequestAvatarListInCharacterRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest",
				"RequestAvatarListInCharacter"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}
	}
}