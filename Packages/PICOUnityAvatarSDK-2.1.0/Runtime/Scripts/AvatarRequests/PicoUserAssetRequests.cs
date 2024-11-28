namespace Pico
{
	namespace Avatar
	{
		// Request for remove asset.
		public class RemoveAssetRequest : AsyncRequestBase
		{
			public static void DoRequest(string assetId, System.Action<NativeResult, string> callback = null)
			{
				var req = new RemoveAssetRequest(assetId);
				//
				var args = req.invokeArgumentTable;
				args.SetStringParam(0, assetId);
				//
				req.DoApply((IDParameterTable returnParams, NativeCaller invoker) =>
				{
					if (callback != null)
					{
						uint errorCode = 1;
						returnParams.GetUIntParam(0, ref errorCode);
						var returnData = returnParams.GetUTF8StringParam(1);
						//
						callback((NativeResult)errorCode, returnData);
					}
				}, true);
			}

			//
			private RemoveAssetRequest(string avatarId) : base(_Attribte)
			{
			}

			// request invoker attribute.
			private static NativeCallerAttribute _Attribte = new NativeCallerAttribute("AvatarUserAssetManager",
				"RemoveAsset"
				, ((uint)NativeCallFlags.Async | (uint)NativeCallFlags.NeedReturn | (uint)NativeCallFlags.NotReuse));
		}
	}
}