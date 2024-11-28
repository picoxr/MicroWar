using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// The ScriptObject class used to generate 
		/// </summary>
		public class PicoAvatarPreloadAssetPackageImportAsset : ScriptableObject
		{
			// default scene path for building package.
			public const string DefaultBuildPackageScenePath = "PicoAvatar/Scenes/ImportPreloadAssetPackage.unity";

			[Tooltip(
				"path of the scene needed to build preload asset package. normally located in \"Packages/org.byted.avatar.sdk/Scenes/ImportPreloadAssetPackage.unity\"")]
			[HideInInspector]
			public UnityEngine.Object buildPackageScene = null;

			// package name
			public string packageName;

			// package directory path name
			[HideInInspector] [SerializeField] public string packagePathName;

			// lod levels to package. usually all lod level added.
			//[HideInInspector]
			[SerializeField] public int[] lodLevels;

			// avatar specification texts
			[SerializeField] public string[] avatarSpecTexts;
		}
	}
}