using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Pico
{
	namespace Avatar
	{

		[CreateAssetMenu(fileName = "DefaultAvatarCustomMaterialDataBase", menuName = "Pico/Avatar/SDK/AvatarCustomMaterialDataBase", order = 1)]
		public class AvatarCustomMaterialDataBase : ScriptableObject
		{
#if UNITY_EDITOR
			public static void CheckDirectory(string assetPath)
			{
				string dirPath;
				// check path root.
				if (assetPath.LastIndexOf(".") > assetPath.LastIndexOf("/"))
				{
					dirPath = assetPath.Substring(0, assetPath.LastIndexOf("/"));
				}
				else
				{
					dirPath = assetPath;
				}

				if (!System.IO.Directory.Exists(dirPath))
				{
					int startIndex = assetPath.IndexOf(Application.dataPath);
					if (startIndex == 0)
					{
						dirPath = dirPath.Replace(Application.dataPath, "Assets");
					}

					var dirNames = dirPath.Split('/');
					string parentPathName = "Assets";
					for (int i = 1; i < dirNames.Length; ++i)
					{
						string newPathName = parentPathName + "/" + dirNames[i];
						if (!System.IO.Directory.Exists(newPathName))
						{
							UnityEditor.AssetDatabase.CreateFolder(parentPathName, dirNames[i]);
						}

						parentPathName = newPathName;
					}
				}
			}

			/**
			 * @brief Load asset at asset path. if failed, create new one and save to the path.
			 */
			public static T LoadOrCreateAsset<T>(string assetPathName) where T : ScriptableObject
			{
				var assetObj = AssetDatabase.LoadAssetAtPath<T>(assetPathName);
				if (assetObj == null)
				{
					assetObj = ScriptableObject.CreateInstance<T>();
					CheckDirectory(assetPathName);
					AssetDatabase.CreateAsset(assetObj, assetPathName);
					EditorUtility.SetDirty(assetObj);
					AssetDatabase.SaveAssetIfDirty(assetObj);
				}

				return assetObj;
			}

			private static AvatarCustomMaterialDataBase _instance;
			public static AvatarCustomMaterialDataBase instance
			{
				get
				{
					if (_instance == null)
					{
						_instance = LoadOrCreateAsset<AvatarCustomMaterialDataBase>("Packages/org.byted.avatar.sdk/Runtime/Settings/URPAvatarCustomMaterialDataBase.asset");
					}
					return _instance;
				}
			}
#endif
			
			[SerializeField]
			private List<string> keys = new List<string>();
			[SerializeField]
			private List<Material> values = new List<Material>();

			private Dictionary<string, Material> _materialLookUpTable = new Dictionary<string, Material>();


			public void Load() 
			{
				_materialLookUpTable.Clear();
				if (keys.Count != values.Count)
				{
					Debug.LogError("Keys and values count does not match!");
					return;
				}

				for (int i = 0; i < keys.Count; i++)
				{
					_materialLookUpTable.Add(keys[i], values[i]);
				}
			}

			public void Save()
			{
				keys.Clear();
				values.Clear();

				foreach (var kvp in _materialLookUpTable)
				{
					keys.Add(kvp.Key);
					values.Add(kvp.Value);
				}
#if UNITY_EDITOR
				EditorUtility.SetDirty(this);
				AssetDatabase.SaveAssetIfDirty(this);
#endif
			}

			public bool Lookup(string guid, out Material material)
			{
				return _materialLookUpTable.TryGetValue(guid, out material);
			}

			public bool Add(string guid, Material material)
			{
				if (!_materialLookUpTable.ContainsKey(guid))
				{
					_materialLookUpTable.Add(guid, material);
					return true;
				}
				return false;
			}
		}
	}
}