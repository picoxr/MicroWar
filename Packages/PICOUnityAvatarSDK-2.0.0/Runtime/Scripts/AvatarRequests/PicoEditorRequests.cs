namespace Pico
{
	namespace Avatar
	{
		// Request for download asset.
		public class DownloadAssetRequest : AsyncRequestBase
		{
			public static void DoRequest(string assetId, uint assetType,
				System.Action<NativeResult, string> responsed = null)
			{
				var req = new DownloadAssetRequest(assetId);
				//
				var args = req.invokeArgumentTable;
				args.SetStringParam(0, assetId);
				args.SetUIntParam(1, assetType);
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
				}, true);
			}

			//
			private DownloadAssetRequest(string avatarId) : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest", "DownloadAsset"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request for capture avatar preview: head/half/full
		public class UpdateAvatarPreviewRequest : AsyncRequestBase
		{
			public static void DoRequest(string headImagePath, string halfImagePath, string fullImagePath,
				System.Action<NativeResult, string> responsed = null)
			{
				var req = new UpdateAvatarPreviewRequest();
				//
				var args = req.invokeArgumentTable;
				args.SetStringParam(0, headImagePath);
				args.SetStringParam(1, halfImagePath);
				args.SetStringParam(2, fullImagePath);

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
			private UpdateAvatarPreviewRequest() : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest",
				"UpdateAvatarPreview"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Request for general msg to sdk
		public class CustomRequest : AsyncRequestBase
		{
			public static void DoRequest(string jsonData, System.Action<int, string> responsed = null)
			{
				var req = new CustomRequest();
				var args = req.invokeArgumentTable;
				args.SetStringParam(0, jsonData);
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);

						responsed?.Invoke((int)errorCode, returnData);
					}
				}, true);
			}

			private CustomRequest() : base(_Attribte)
			{
			}

			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarRequest", "Custom",
				((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}


		// PutOn asset.
		public class PutOnAssetRequest : AsyncRequestBase
		{
			public static void DoRequest(string userId, string assetId, uint assetType, string diyParamsJsonText,
				string controlParamsJsonText, uint operationId,
				System.Action<NativeResult, string> responsed = null)
			{
				var req = new PutOnAssetRequest();
				var args = req.invokeArgumentTable;
				args.SetStringParam(0, assetId);
				args.SetUIntParam(1, assetType);
				if (diyParamsJsonText != null)
				{
					args.SetStringParam(2, diyParamsJsonText);
				}

				args.SetUIntParam(3, operationId);
				args.SetStringParam(4, userId);
				args.SetStringParam(5, controlParamsJsonText);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//TODO:
						responsed((NativeResult)errorCode, returnData);
					}
				}, true);
			}

			private PutOnAssetRequest() : base(_Attribte)
			{
			}

			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarEditState", "PutOnAsset",
				((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		/// <summary>
		/// Undo commands.
		/// </summary>
		public class UndoCommandsRequest : AsyncRequestBase
		{
			public static void DoRequest(uint operationId, System.Action<NativeResult, string> responsed = null)
			{
				var req = new UndoCommandsRequest();
				var args = req.invokeArgumentTable;
				args.SetUIntParam(0, operationId);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//TODO:
						responsed((NativeResult)errorCode, returnData);
					}
				}, true);
			}

			private UndoCommandsRequest() : base(_Attribte)
			{
			}

			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarEditState",
				"UndoCommands",
				((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}

		// Redo commands.
		public class RedoCommandsRequest : AsyncRequestBase
		{
			public static void DoRequest(uint operationId, System.Action<NativeResult, string> responsed = null)
			{
				var req = new RedoCommandsRequest();
				var args = req.invokeArgumentTable;
				args.SetUIntParam(0, operationId);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (PicoAvatarManager.instance != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//TODO:
						responsed((NativeResult)errorCode, returnData);
					}
				}, true);
			}

			private RedoCommandsRequest() : base(_Attribte)
			{
			}

			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarEditState",
				"RedoCommands",
				((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}
	}
}