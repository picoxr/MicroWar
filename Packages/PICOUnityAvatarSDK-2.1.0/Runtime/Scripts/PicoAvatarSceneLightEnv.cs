using UnityEngine;


namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Scene light env.
		/// </summary>
		public class PicoAvatarSceneLightEnv : MonoBehaviour
		{
			// scene name used to search in a table.
			public string sceneName;

			// Additive gi.
			public float additiveGI = 0.0f;

			// Start is called before the first frame update
			void Start()
			{
				Apply();
			}

			// Update is called once per frame
			void OnDestroy()
			{
				if (PicoAvatarApp.instance != null && PicoAvatarApp.instance.curAvatarSceneLightEnv == this)
				{
					PicoAvatarApp.instance.SetAvatarSceneLightEnv(null);
				}
			}

			/// <summary>
			/// Apply the light env to all avatars.
			/// </summary>
			public void Apply()
			{
				if (!_applyed && PicoAvatarApp.instance != null)
				{
					_applyed = true;
					PicoAvatarApp.instance.SetAvatarSceneLightEnv(this);
				}
			}

			void Update()
			{
				if (!_applyed && PicoAvatarApp.instance != null)
				{
					_applyed = true;
					PicoAvatarApp.instance.SetAvatarSceneLightEnv(this);
				}
			}

			// whether has applied.
			private bool _applyed = false;
		}
	}
}