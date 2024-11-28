using UnityEngine;


namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Helper Group of avatr scene light envs.
		/// </summary>
		public class PicoAvatarSceneLightEnvGroup : MonoBehaviour
		{
			// light env prefabs.
			public PicoAvatarSceneLightEnv[] sceneLightEnvsPrefabs;

			// Update is called once per frame
			void OnDestroy()
			{
				// destroy last one
				if (_curSceneGO != null)
				{
					GameObject.Destroy(_curSceneGO);
					_curSceneGO = null;
				}
			}

			public void SwitchScene(string sceneName)
			{
				if (sceneLightEnvsPrefabs == null)
				{
					return;
				}

				PicoAvatarSceneLightEnv sceneLightPrefab = null;

				foreach (var x in sceneLightEnvsPrefabs)
				{
					if (x.sceneName == sceneName)
					{
						sceneLightPrefab = x;
						break;
					}
				}

				// destroy last one
				if (_curSceneGO != null)
				{
					GameObject.Destroy(_curSceneGO);
					_curSceneGO = null;
				}

				//
				if (sceneLightPrefab != null)
				{
					_curSceneGO = GameObject.Instantiate(sceneLightPrefab.gameObject);
					//
					var lightEnvCmp = _curSceneGO.GetComponentInChildren<PicoAvatarSceneLightEnv>(true);
					if (lightEnvCmp != null)
					{
						lightEnvCmp.Apply();
					}
				}
			}

			// current scene.
			private GameObject _curSceneGO = null;
		}
	}
}