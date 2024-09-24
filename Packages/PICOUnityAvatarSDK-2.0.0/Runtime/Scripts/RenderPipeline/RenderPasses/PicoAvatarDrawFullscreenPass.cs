using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// Draws full screen mesh using given material and pass and reading from source target.
		/// </summary>
		internal class PicoAvatarDrawFullscreenPass : ScriptableRenderPass
		{
			public FilterMode filterMode { get; set; }
			public PicoAvatarDrawFullscreenFeature.Settings settings;

			RenderTargetIdentifier source;
			RenderTargetIdentifier destination;
			int temporaryRTId = Shader.PropertyToID("_TempRT");

			int sourceId;
			int destinationId;
			bool isSourceAndDestinationSameTarget;

			string m_ProfilerTag;

			public PicoAvatarDrawFullscreenPass(string tag)
			{
				m_ProfilerTag = tag;
			}

			public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
			{
				base.Configure(cmd, cameraTextureDescriptor);
			}

			public void Prepare(CommandBuffer cmd, ref RenderingData renderingData)
			{
				RenderTextureDescriptor blitTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;
				blitTargetDescriptor.depthBufferBits = 0;

				//
				isSourceAndDestinationSameTarget = settings.sourceType == settings.destinationType &&
				                                   (settings.sourceType == BufferType.CameraColor ||
				                                    settings.sourceTextureId == settings.destinationTextureId);

				//
				if (settings.sourceType == BufferType.CameraColor)
				{
					sourceId = -1;
					source = this.colorAttachment;
				}
				else
				{
					sourceId = Shader.PropertyToID(settings.sourceTextureId);
					cmd.GetTemporaryRT(sourceId, blitTargetDescriptor, filterMode);
					source = new RenderTargetIdentifier(sourceId);
				}

				if (isSourceAndDestinationSameTarget)
				{
					destinationId = temporaryRTId;
					cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
					destination = new RenderTargetIdentifier(destinationId);
				}
				else if (settings.destinationType == BufferType.CameraColor)
				{
					destinationId = -1;
					destination = this.colorAttachment;
				}
				else
				{
					destinationId = Shader.PropertyToID(settings.destinationTextureId);
					cmd.GetTemporaryRT(destinationId, blitTargetDescriptor, filterMode);
					destination = new RenderTargetIdentifier(destinationId);
				}
			}

			/// <inheritdoc/>
			public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
			{
				CommandBuffer cmd = CommandBufferPool.Get(m_ProfilerTag);

#if UNITY_EDITOR
				// TODO: temporary keep only game view, cause source is flipY, uv.y flip will be disturb scene view
				if (!renderingData.cameraData.isSceneViewCamera)
				{
#endif
					//
					Prepare(cmd, ref renderingData);

					// Can't read and write to same color target, create a temp render target to blit. 
					if (isSourceAndDestinationSameTarget)
					{
						Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
						Blit(cmd, destination, source);
					}
					else
					{
						Blit(cmd, source, destination, settings.blitMaterial, settings.blitMaterialPassIndex);
					}

					context.ExecuteCommandBuffer(cmd);
					CommandBufferPool.Release(cmd);
#if UNITY_EDITOR
				}
#endif
			}

			/// <inheritdoc/>
			public override void FrameCleanup(CommandBuffer cmd)
			{
				if (destinationId != -1)
					cmd.ReleaseTemporaryRT(destinationId);

				if (source == destination && sourceId != -1)
					cmd.ReleaseTemporaryRT(sourceId);
			}
		}
	}
}