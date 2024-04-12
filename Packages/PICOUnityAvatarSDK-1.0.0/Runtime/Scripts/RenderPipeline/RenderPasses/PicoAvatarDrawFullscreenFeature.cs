using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		public enum BufferType
		{
			CameraColor,
			Custom
		}

		// screen feature type.
		public enum CommonFullscreenFeatureType
		{
			ToonLine = 1 << 0,
			ColorTemperature = 1 << 1,
		}

		public class PicoAvatarDrawFullscreenFeature : UnityEngine.Rendering.Universal.ScriptableRendererFeature
		{
			[System.Serializable]
			public class Settings
			{
				public UnityEngine.Rendering.Universal.RenderPassEvent renderPassEvent =
					UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingOpaques;

				public Material blitMaterial = null;
				public int blitMaterialPassIndex = -1;
				public BufferType sourceType = BufferType.CameraColor;
				public BufferType destinationType = BufferType.CameraColor;
				public string sourceTextureId = "_SourceTexture";
				public string destinationTextureId = "_DestinationTexture";
				public CommonFullscreenFeatureType featureType; //
			}

			public Settings settings = new Settings();
			PicoAvatarDrawFullscreenPass blitPass;


			// Test whether can skip the feature pass.
			bool shouldSkipped()
			{
				//
				// set color temperature
				if (settings.featureType == CommonFullscreenFeatureType.ColorTemperature)
				{
#if UNITY_EDITOR
					return true;
#endif
				}

				return false;
			}

			public override void Create()
			{
				blitPass = new PicoAvatarDrawFullscreenPass(name);
			}

			public override void AddRenderPasses(UnityEngine.Rendering.Universal.ScriptableRenderer renderer,
				ref UnityEngine.Rendering.Universal.RenderingData renderingData)
			{
				//
				if (PicoAvatarApp.instance != null &&
				    ((uint)PicoAvatarApp.instance.appSettings.enabledFullscreenFeatures & (uint)settings.featureType) ==
				    0)
				{
					return;
				}

				//
				if (shouldSkipped())
				{
					return;
				}

				if (settings.blitMaterial == null && PicoAvatarApp.instance != null
				                                  && PicoAvatarApp.instance.renderSettings.materialConfiguration !=
				                                  null)
				{
					settings.blitMaterial = PicoAvatarApp.instance.renderSettings.materialConfiguration
						.colorTemperatureMaterial;
				}

				//
				if (settings.blitMaterial == null)
				{
					Debug.LogWarningFormat(
						"Missing Blit Material. {0} blit pass will not execute. Check for missing reference in the assigned renderer.",
						GetType().Name);
					return;
				}

				//
				blitPass.renderPassEvent = settings.renderPassEvent;
				blitPass.settings = settings;
				renderer.EnqueuePass(blitPass);
			}
		}
	}
}