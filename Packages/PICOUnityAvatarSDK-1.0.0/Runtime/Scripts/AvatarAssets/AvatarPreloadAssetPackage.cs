using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

namespace Pico
{
	namespace Avatar
	{
		// Preload asset package.
		public class AvatarPreloadAssetPackage
		{
			// Package name to identify packages.
			public string packageName { get; private set; }

			// Base path name of assets in the preload package. "Assets/" is excluded in windows/osx os.
			public string basePathName { get; private set; }

			// Package resource type.
			public ResourceRootType resRootType { get; private set; }

			// whether is added to package set.
			public bool isAddedToPackageSet { get; protected set; } = false;

			// Constructor with package information.
			public AvatarPreloadAssetPackage(string packageName, string basePathName, ResourceRootType resRootType)
			{
				this.packageName = packageName;
				this.basePathName = basePathName;
				this.resRootType = resRootType;
			}

			// Hide default constructor.
			private AvatarPreloadAssetPackage()
			{
			}

			// Notification from AvatarPreloadAssetPackageSet when the package added to package set.
			public virtual void OnAddedToPackageSet()
			{
				// Derived class may override the method.
				isAddedToPackageSet = true;
			}

			// Notification from AvatarPreloadAssetPackageSet when the package removed.
			public virtual void OnRemovedFromPackageSet()
			{
				// Derived class may override the method.
				isAddedToPackageSet = false;
			}

			// Gets package meta as json text.
			public virtual IEnumerator GetPackageMetaAsJsonTextAsync(
				System.Action<string, NativeResult, string> callback)
			{
				var manifestTypePathName = this.basePathName + "/PackageManifest.json";

				switch (resRootType)
				{
					case ResourceRootType.Bin:
					{
						var textAsset = Resources.Load<TextAsset>(manifestTypePathName);
						if (textAsset != null)
						{
							callback(textAsset.text, NativeResult.Success, "");
							yield break;
						}
					}
						break;
					case ResourceRootType.LocalFileSystem:
					{
						// add file extension
						manifestTypePathName += ".bytes";

						if (System.IO.File.Exists(manifestTypePathName))
						{
							callback(System.IO.File.ReadAllText(manifestTypePathName), NativeResult.Success, "");
							yield break;
						}
					}
						break;
					case ResourceRootType.ReadOnlyAssets:
					{
						// add file extension
						manifestTypePathName += ".bytes";

						// In StreamingAssets dir.
#if UNITY_EDITOR
						manifestTypePathName = Application.dataPath + "/" + manifestTypePathName;
#elif UNITY_ANDROID
                            manifestTypePathName =
 Application.streamingAssetsPath + "/" + manifestTypePathName; //"jar:file://" + Application.dataPath + "!/assets/" + manifestTypePathName;
#elif UNITY_IOS
                            manifestTypePathName =
 Application.streamingAssetsPath + "/" + manifestTypePathName; //Application.dataPath + "/Raw/" + manifestTypePathName;
#else
                            manifestTypePathName = Application.streamingAssetsPath + "/" + manifestTypePathName;
#endif
						yield return Coroutine_LoadManifestText(manifestTypePathName, callback);
						// return when finished
						yield break;
					}
						break;
				}

				//    AvatarEnv.Log(DebugLogMask.GeneralWarn, String.Format("Failed to read package manifestion. packageName:{0} reason:{1}", packageName ,ex.Message));
				callback(null, NativeResult.NotFound, "");
			}

			// coroutine to load embeded file data.
			protected IEnumerator Coroutine_LoadManifestText(string fileName,
				System.Action<string, NativeResult, string> callback)
			{
				if (fileName == null || fileName.Length < 2)
				{
					callback(null, NativeResult.BadParameter, "");
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
					AvatarEnv.Log(DebugLogMask.AssetTrivial,
						string.Format("Start load manifest file in preload pacakge. package:{0} file: {1}", packageName,
							fileName));
				}

				using (UnityWebRequest webRequest = UnityWebRequest.Get(fileName))
				{
					// Request and wait for the desired page.
					yield return webRequest.SendWebRequest();

					if (webRequest.isNetworkError || string.IsNullOrEmpty(webRequest.downloadHandler.text))
					{
						AvatarEnv.Log(DebugLogMask.GeneralError,
							string.Format("Failed to load manifest file in preload package. package:{0} file: {1}",
								packageName, fileName));
						//TODO: 
						callback(null, NativeResult.Unknown, webRequest.error);
					}
					else
					{
						callback(webRequest.downloadHandler.text, NativeResult.Success, null);
					}
				}
			}
		}
	}
}