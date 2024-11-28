using UnityEngine;
using System.Collections;
using UnityEngine.Networking;


namespace Pico
{
	namespace Avatar
	{
		
		public delegate bool ApplicationAssetDataProvider(string fileName, ResourceRootType root
			, System.Action<MemoryView, NativeResult, string> callback);

		/// <summary>
		/// NativeCallee to load application embeded file data. 
		/// </summary>
		internal class NativeCallee_Misc_LoadEmbededFileData : NativeCallee
		{
			#region Register

			public static void Register()
			{
				NativeCallMarshal.RegisterCallee(_attribute);
			}

			//
			private static NativeCalleeAttribute _attribute = new NativeCalleeAttribute(
				typeof(NativeCallee_Misc_LoadEmbededFileData), "Misc", "LoadEmbededFileData");

			// asset data provider..
			public ApplicationAssetDataProvider assetDataProvider;

			#endregion

			public override void onCalled()
			{
				unchecked
				{
					var returnArguments = this.GetReturnArgumentTable();

					if (callArguments != null)
					{
						var fileName = callArguments.GetStringParam(0);
						uint resRoot = 0;
						callArguments.GetUIntParam(1, ref resRoot);

						LoadEmbededFileDataAsync(fileName, (ResourceRootType)resRoot,
							(memoryView, errorCode, errorDesc) =>
							{
								unchecked
								{
									if (memoryView != null)
									{
										returnArguments.SetObjectParam(0, memoryView.nativeHandle);
									}
									else
									{
										returnArguments.SetObjectParam(0, System.IntPtr.Zero);
									}

									//
									returnArguments.SetUIntParam(1, (uint)errorCode);
									if (!string.IsNullOrEmpty(errorDesc))
									{
										returnArguments.SetStringParam(2, errorDesc);
									}
								}

								//
								this.DoReturn();
							});
					}
					else
					{
						returnArguments.SetUIntParam(0, (uint)NativeResult.Failed);
						//
						this.DoReturn();
					}
				}
			}

			#region Load embeded file data.

			/// <summary>
			/// 
			/// </summary>
			/// <param name="fileName"></param>
			/// <param name="root"></param>
			/// <param name="callback"></param>
			public void LoadEmbededFileDataAsync(string fileName, ResourceRootType root
				, System.Action<MemoryView, NativeResult, string> callback)
			{
				var returnArguments = this.GetReturnArgumentTable();
				if (assetDataProvider != null && assetDataProvider(fileName, root, callback))
				{
					return;
				}

				if (root == ResourceRootType.Bin)
				{
					var res = Resources.Load<TextAsset>(fileName);
					if (res != null)
					{
						var mv = new MemoryView(res.bytes, false);
						mv.Retain();
						callback(mv, (uint)NativeResult.Success, "");
						mv.Release();
					}
					else
					{
						callback(null, NativeResult.FileNotFound, null);
					}
				}
				else if (root == ResourceRootType.LocalFileSystem)
				{
					// add file extension
					fileName += ".bytes";

					var bytes = System.IO.File.ReadAllBytes(fileName);
					if (bytes != null && bytes.Length > 0)
					{
						var mv = new MemoryView(bytes, false);
						mv.Retain();
						callback(mv, (uint)NativeResult.Success, "");
						mv.Release();
					}
					else
					{
						callback(null, NativeResult.FileNotFound, null);
					}
				}
				else if (root == ResourceRootType.ReadOnlyAssets)
				{
					// add file extension
					fileName += ".bytes";

					// In StreamingAssets dir.                          
#if UNITY_EDITOR
					fileName = Application.dataPath + "/" + fileName;
#elif UNITY_ANDROID
                    fileName =
 Application.streamingAssetsPath + "/" + fileName; // "jar:file://" + Application.dataPath + "!/assets/" + fileName;
#elif UNITY_IOS
                    fileName =
 Application.streamingAssetsPath + "/" + fileName; // Application.dataPath + "/Raw/" + fileName;
#else
                    fileName = Application.streamingAssetsPath + "/" + fileName;
#endif
					PicoAvatarApp.instance.StartCoroutine(Coroutine_LoadEmbededFileData(fileName, callback));
				}
				else
				{
					// to support more.
					callback(null, NativeResult.Unsupported, "");
				}
			}

			// coroutine to load embeded file data.
			IEnumerator Coroutine_LoadEmbededFileData(string fileName,
				System.Action<MemoryView, NativeResult, string> callback)
			{
				if (fileName == null || fileName.Length < 2)
				{
					callback(null, NativeResult.ParameterError, "");
					yield break;
				}

				// if absolut file path, add protocol of "file://"
				if (fileName[0] == '/' || fileName[1] == ':')
				{
					fileName = "file://" + fileName;
				}

				//
				if (AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
				{
					AvatarEnv.Log(DebugLogMask.AssetTrivial, string.Format("Start load embeded file: {0}", fileName));
				}

				using (UnityWebRequest webRequest = UnityWebRequest.Get(fileName))
				{
					// Request and wait for the desired page.
					yield return webRequest.SendWebRequest();

					if (webRequest.isNetworkError || webRequest.downloadedBytes == 0)
					{
						AvatarEnv.Log(DebugLogMask.GeneralWarn,
							string.Format("Failed to load embeded file: {0}", fileName));

						//TODO: 
						callback(null, NativeResult.Failed, webRequest.error);
					}
					else
					{
						var mv = new MemoryView(webRequest.downloadHandler.data, false);
						mv.Retain();
						callback(mv, NativeResult.Success, null);
						mv.Release();
					}
				}
			}

			#endregion
		}
	}
}