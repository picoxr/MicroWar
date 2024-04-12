using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		// The class object manages a set of PreloadAssetPackage.
		public class AvatarPreloadAssetPackageSet
		{
			#region Public Properties

			// Singleton instance of the class.
			public static AvatarPreloadAssetPackageSet intance
			{
				get
				{
					if (_intance == null)
					{
						_intance = new AvatarPreloadAssetPackageSet();
					}

					return _intance;
				}
			}

			#endregion


			#region Public Methods

			// Initialize the object.
			internal void Initialize()
			{
				if (_intance != this)
				{
					throw new System.Exception("Only one instance for AvatarPreloadAssetPackageSet can be created!");
				}

				if (_rmiObject == null)
				{
					_rmiObject = new NativeCall_AvatarPreloadAssetPackageSet(this, 0);
					_rmiObject.Retain();
				}
			}

			internal void Uninitialize()
			{
				if (_intance != this)
				{
					throw new System.Exception("Only one instance for AvatarPreloadAssetPackageSet can be created!");
				}

				if (_rmiObject != null)
				{
					_rmiObject.Release();
					_rmiObject = null;
				}

				//
				_lastLoadingPackageCmd = -1;
			}

			/// <summary>
			/// Add preload package to 
			/// </summary>
			/// <param name="preloadPackage">preloadPackage package to add</param>
			/// <param name="finishedCallback">invoked when the preload packaged added to native part</param>
			public void AddPreloadPackage(AvatarPreloadAssetPackage preloadPackage,
				System.Action<bool> finishedCallback)
			{
				if (_packages == null)
				{
					_packages = new Dictionary<string, AvatarPreloadAssetPackage>();
				}

				if (!_packages.ContainsKey(preloadPackage.packageName))
				{
					_packages.Add(preloadPackage.packageName, preloadPackage);
				}

				// async load.
				PicoAvatarApp.instance.StartCoroutine(preloadPackage.GetPackageMetaAsJsonTextAsync(
					(text, result, errorText) =>
					{
						if (_rmiObject != null && _packages.ContainsKey(preloadPackage.packageName))
						{
							if (!string.IsNullOrEmpty(text))
							{
								_rmiObject.AddPackageMetaAsString(preloadPackage.packageName, text,
									preloadPackage.resRootType, preloadPackage.basePathName);
								// notify.
								preloadPackage.OnAddedToPackageSet();

								//
								if (AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
								{
									AvatarEnv.Log(DebugLogMask.AssetTrivial,
										String.Format("PreloadPackage added. name:{0} basePath:{1}",
											preloadPackage.packageName, preloadPackage.basePathName));
								}

								if (finishedCallback != null)
								{
									finishedCallback(true);
								}
							}
							else
							{
								AvatarEnv.Log(DebugLogMask.GeneralError,
									String.Format("Faied to get package manifest text when AddPreloadPackage {0}",
										preloadPackage.packageName));
								if (finishedCallback != null)
								{
									finishedCallback(false);
								}
							}
						}
						else
						{
							if (finishedCallback != null)
							{
								finishedCallback(false);
							}
						}
					}));
			}

			/// <summary>
			/// Remove preload package.
			/// </summary>
			/// <param name="preloadPackage"></param>
			public void RemovePreloadPackage(AvatarPreloadAssetPackage preloadPackage)
			{
				if (_packages != null)
				{
					if (_packages.ContainsKey(preloadPackage.packageName))
					{
						_packages.Remove(preloadPackage.packageName);
					}
				}

				// notify removed from package set.
				preloadPackage.OnRemovedFromPackageSet();

				if (_rmiObject != null)
				{
					_rmiObject.RemovePackage(preloadPackage.packageName);
				}
			}

			/// <summary>
			/// Remove preload packages.
			/// </summary>
			public void RemovePreloadPackages()
			{
				if (_packages != null)
				{
					foreach (var x in _packages)
					{
						x.Value.OnRemovedFromPackageSet();
					}

					_packages.Clear();
				}

				if (_rmiObject != null)
				{
					_rmiObject.RemovePackages();
				}
			}

			/// <summary>
			/// Load package assets.
			/// </summary>
			/// <param name="importAsset"></param>
			/// <param name="callback"></param>
			public void LoadPackageAssets(PicoAvatarPreloadAssetPackageImportAsset importAsset,
				System.Action<NativeResult, string> callback)
			{
				if (string.IsNullOrEmpty(importAsset.packagePathName) ||
				    importAsset.avatarSpecTexts == null ||
				    importAsset.avatarSpecTexts.Length == 0 ||
				    importAsset.lodLevels == null ||
				    importAsset.lodLevels.Length < (int)AvatarLodLevel.Count)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError,
						"PicoAvatarPreloadAssetPackageImportAsset data is invalid");
					callback(NativeResult.BadParameter, "Please check data.");
					return;
				}

				var packageOutDir = importAsset.packagePathName;
				if (importAsset.packagePathName.StartsWith("Assets/"))
				{
					packageOutDir = Application.dataPath + importAsset.packagePathName.Substring("Assets".Length);
				}

				//else if (importAsset.packagePathName[0] == '/' || importAsset.packagePathName[1] == ':')
				//{
				//    // do nothing.
				//}
				if (!System.IO.Directory.Exists(packageOutDir))
				{
					callback(NativeResult.NotFound,
						String.Format("packagePathName is not a valid directory path. {0}",
							importAsset.packagePathName));
					return;
				}

				// clear directories
				System.IO.Directory.Delete(packageOutDir, true);
				//
				var packageMeta = new Dictionary<string, object>();
				packageMeta["packageOutDir"] = packageOutDir;
				packageMeta["avatarSpecs"] = importAsset.avatarSpecTexts;
				packageMeta["lodLevelList"] = importAsset.lodLevels;

				if (_rmiObject == null)
				{
					callback(NativeResult.BadProgram,
						"LoadPackageAssets can not be invoked when AvatarManager does not be working.");
					return;
				}

				if (_lastLoadingPackageCmd >= 0)
				{
					callback(NativeResult.BadProgram, "LoadPackageAssets can be started after previous one finished!");
					return;
				}

				var curLoadingPackageCmd = ++_lastLoadingPackageCmd;
				//
				_rmiObject.LoadPackageAssets(Newtonsoft.Json.JsonConvert.SerializeObject(packageMeta),
					(resultCode, errorText) =>
					{
						if (_lastLoadingPackageCmd == curLoadingPackageCmd)
						{
							// rename all files and add ".bytes"
							try
							{
								var allFiles =
									System.IO.Directory.GetFiles(packageOutDir, "*", SearchOption.AllDirectories);
								foreach (var x in allFiles)
								{
									System.IO.File.Move(x, x + ".bytes");
								}
							}
							catch (System.Exception ex)
							{
								resultCode = NativeResult.FatalError;
								errorText = "Failed to rename files to .bytes";
								AvatarEnv.Log(DebugLogMask.GeneralError,
									"Failed to rename outputed files. reason:" + ex.Message);
							}

							_lastLoadingPackageCmd = -1;
							if (callback != null)
							{
								callback(resultCode, errorText);
							}
						}
						else
						{
							callback(NativeResult.Unknown, "Operation Expired");
						}
					});

				//let packageOutDir = loadPackageAssetMeta["packageOutDir"];//
				///let avatarSpecList = loadPackageAssetMeta["avatarSpecs"];
				//let lodLevelList = loadPackageAssetMeta["lodLevelList"];
			}

			#endregion


			#region Private Fields

			private static AvatarPreloadAssetPackageSet _intance = null;
			private NativeCall_AvatarPreloadAssetPackageSet _rmiObject = null;
			private Dictionary<string, AvatarPreloadAssetPackage> _packages = null;
			private int _lastLoadingPackageCmd = -1;

			#endregion


			#region Private Methods

			// Hide constructor.
			private AvatarPreloadAssetPackageSet()
			{
			}

			#endregion
		}
	}
}