using UnityEngine;
using UnityEngine.Rendering;

namespace Pico
{
	namespace Avatar
	{
		namespace Postprocess
		{
			/// <summary>
			/// color temperature for pico JDI device for builtin pipeline
			/// </summary>
			class PicoColorTemperature : MonoBehaviour
			{
				public Material blitMaterial;

				public void OnRenderImage(RenderTexture srcRT, RenderTexture destRT)
				{
					if (blitMaterial == null)
					{
						return;
					}

					//
					Graphics.Blit(srcRT, destRT, blitMaterial);
				}

				public static void CheckColorTemperature()
				{
					//
					if (GraphicsSettings.currentRenderPipeline != null)
					{
						return;
					}

					if (PicoAvatarApp.instance == null
					    || ((uint)PicoAvatarApp.instance.appSettings.enabledFullscreenFeatures &
					        (uint)CommonFullscreenFeatureType.ColorTemperature) == 0)
					{
						return;
					}

					var camera = PicoAvatarApp.instance.appSettings.mainCamera;
					if (camera == null)
					{
						camera = Camera.main;
					}

					if (camera == null)
					{
						return;
					}

					var materialConfig = PicoAvatarApp.instance.renderSettings.materialConfiguration;
					if (materialConfig == null || materialConfig.colorTemperatureMaterial == null)
					{
						return;
					}

					var cmp = camera.gameObject.AddComponent<PicoColorTemperature>();
					cmp.blitMaterial = materialConfig.colorTemperatureMaterial;
				}
			}
		}
	}
}