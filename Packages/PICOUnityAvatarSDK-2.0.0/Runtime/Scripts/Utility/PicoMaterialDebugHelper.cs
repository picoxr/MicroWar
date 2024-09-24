using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		// Helper for debug Material.
		[ExecuteInEditMode]
		public class PicoMaterialDebugHelper : MonoBehaviour
		{
			public Material[] materials;

			//
			[Tooltip("Additive GI."), Range(0.0f, 1.0f)]
			public float additiveGI = 0.0f;

			// scene light
			public Pico.Avatar.PicoAvatarSceneLightEnv avatarSceneLightEnv;

			//
			private float _lastAdditiveGI = -1.0f;

			// Start is called before the first frame update
			void Start()
			{
			}

			// Update is called once per frame
			void Update()
			{
				//
				if (avatarSceneLightEnv == null)
				{
					if (_lastAdditiveGI != additiveGI &&
					    materials != null &&
					    materials.Length > 0)
					{
						_lastAdditiveGI = additiveGI;
						//
						foreach (var x in materials)
						{
							if (x != null)
							{
								x.SetFloat("_AdditiveGI", additiveGI);
							}
						}
					}
				}
				else
				{
					if (_lastAdditiveGI != additiveGI &&
					    materials != null &&
					    materials.Length > 0)
					{
						_lastAdditiveGI = additiveGI;
						//
						foreach (var x in materials)
						{
							if (x != null)
							{
								x.SetFloat("_AdditiveGI", additiveGI);
							}
						}

						//
						if (PicoAvatarApp.instance != null)
						{
							avatarSceneLightEnv.additiveGI = additiveGI;
							//
							PicoAvatarApp.instance.SetAvatarSceneLightEnv(avatarSceneLightEnv);
						}
					}
				}
			}
		}
	}
}