using UnityEngine;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// MaterialConfiguration is a unity asset to specify avatar materials the SDK App actually employed.
		/// </summary>
		[CreateAssetMenu(fileName = "DefaultMaterialConfiguration",
			menuName = "Pico/Avatar/SDK/AvatarMaterialConfiguration", order = 1)]
		public class PicoMaterialConfiguration : ScriptableObject
		{
			#region Public Properties

			/// <summary>
			/// unity bump map id.
			/// </summary>
			public int unityID_BumpMap
			{
				get => id_BumpMap;
			}

			#endregion

			#region Public Fields

			public RenderPipelineType renderPipelineType;

			/// <summary>
			/// lod materials. application maybe need add some more properties.
			/// </summary>
			public Material lodMaterial0;

			public Material lodMaterial1;
			public Material lodMaterial2;
			public Material lodMaterial3;
			public Material lodMaterial4;
			public Shader avatarBunchShader;
			public Material hairKKMaterial0;

			public ShaderVariantCollection shaderVariantCollection;

			/// <summary>
			/// Whether need tangent data.  If need pbr/normal map, should set true.
			/// </summary>
			public bool needTangent = true;

			/// <summary>
			/// Whether enable multiple lights
			/// </summary>
			public bool enableMultiLights = true;

			/// <summary>
			/// Morph and skinning shader.
			/// </summary>
			public ComputeShader morphAndSkinningShader;

			/// <summary>
			/// Compute shader to transfer mesh data in avatar shaping state to static mesh buffer.
			/// </summary>
			public ComputeShader transferShapingMeshShader;

			/// <summary>
			/// Material
			/// </summary>
			public Material colorTemperatureMaterial;

			/// <summary>
			/// avatarRenderMaterialDefTable
			/// </summary>
			public AvatarRenderMaterialDef avatarRenderMaterialDefTable;


			/// <summary>
			/// whether need texture.
			/// </summary>
			public bool need_ToonShadowMap = true;

			public bool need_MetallicGlossMap = true;
			public bool need_SpecGlossMap = true;
			public bool need_BumpMap = true;
			public bool need_ParallaxMap = false;
			public bool need_EmissionMap = true;
			public bool need_DetailMask = false;
			public bool need_DetailAlbedoMap = false;
			public bool need_DetailNormalMap = false;
			public bool need_SecondMap = false;
			public bool need_ColorRegionMap = true;

			/// <summary>
			/// whether lod2 remove bump map. lod2 maybe pbr.
			/// </summary>
			public bool isLod2NeedBumpMap = false;

			/// <summary>
			/// whether lod3 need pbr shader.
			/// </summary>
			public bool isLod3EnablePBR = false;

			public string name_BaseMap = "_BaseMap";
			public string name_BumpMap = "_BumpMap";

			#endregion


			#region Public Methods
			/// <summary>
			/// Apply property of nativeMaterial to material
			/// </summary>
			/// <param name="nativeMaterial">Source material</param>
			/// <param name="material">Target material</param>
			public void ApplyToMaterial(PicoAvatarRenderMaterial nativeMaterial, Material material)
			{
				// set float properties
				UpdateToUniformsMaterial(nativeMaterial, material);
				// set vector properties.
				{
					foreach (MaterialVectorPropertyItem item in nativeMaterial.mat_VectorPropertyItems)
					{
						if (item.has_value)
						{
							material.SetTextureScale(item.unityID, new Vector2(item.value.x, item.value.y));
							material.SetTextureOffset(item.unityID,
								new Vector2(item.value.z, 1 - item.value.w - item.value.y));
						}
					}

					if (nativeMaterial.has_BaseMap_ST)
					{
						material.SetTextureScale(id_BaseMap,
							new Vector2(nativeMaterial.mat_BaseMap_ST.x, nativeMaterial.mat_BaseMap_ST.y));
						material.SetTextureOffset(id_BaseMap,
							new Vector2(nativeMaterial.mat_BaseMap_ST.z,
								1 - nativeMaterial.mat_BaseMap_ST.w - nativeMaterial.mat_BaseMap_ST.y));
					}
				}

				// set textures properties.
				foreach (MaterialTexturePropertyItem item in nativeMaterial.mat_TexturePropertyItems)
				{
					setTextureProperty(material, item.value, item.unityID, item.unityArrayID);
				}
			}


			/// <summary>
			/// Sets properties to unity runtime material, derived class can override the method as filter to modify material.
			/// </summary>
			/// <param name="nativeMaterial">Source material</param>
			/// <param name="lodLevel">current lodLevel</param>
			/// <returns>unity material</returns>
			public virtual Material ApplyToMaterial(PicoAvatarRenderMaterial nativeMaterial, AvatarLodLevel lodLevel)
			{
				CheckPrepareConfiguration(nativeMaterial);
				//
				var material = new Material(SelectMaterial(nativeMaterial.mat_ShaderType, lodLevel));

				return material;
			}

			/// <summary>
			/// Dynamically update uniforms in native material to unity material.
			/// </summary>
			/// <param name="nativeMaterial">Source material</param>
			/// <param name="material">Target material</param>
			public void UpdateToUniformsMaterial(PicoAvatarRenderMaterial nativeMaterial, Material material)
			{
				// set float properties
				foreach (MaterialFloatPropertyItem item in nativeMaterial.mat_FloatPropertyItems)
				{
					if (item.has_value)
					{
						material.SetFloat(item.unityID, item.value);
					}
				}

				// set colors properties.
				foreach (MaterialColorPropertyItem item in nativeMaterial.mat_ColorPropertyItems)
				{
					if (item.has_value)
					{
						material.SetColor(item.unityID, item.value);
					}
				}

				nativeMaterial.SetCustomVecs();
			}

			#endregion


			#region Private Methods

			private static void setTextureProperty(Material material, AvatarTexture avatarTexture, int texPropertyId,
				int arrayPropertyId)
			{
				if (avatarTexture != null)
				{
					if (avatarTexture.runtimeTexture != null)
					{
						material.SetTexture(texPropertyId, avatarTexture.runtimeTexture);
					}
					else if (avatarTexture.runtimeMergedTexture != null)
					{
						material.SetTexture(arrayPropertyId, avatarTexture.runtimeMergedTexture);
					}
				}
			}

			// Select material.
			protected virtual Material SelectMaterial(AvatarShaderType shaderType, AvatarLodLevel lodLevel)
			{
				switch (shaderType)
				{
					case AvatarShaderType.Hair_KK:
					{
						if (hairKKMaterial0 != null)
						{
							return hairKKMaterial0;
						}
					}
						break;
					default:
						break;
				}

				if (lodMaterial4 != null && (int)AvatarLodLevel.Count > 3 && lodLevel > (AvatarLodLevel)3)
				{
					return lodMaterial4;
				}
				else if (lodMaterial3 != null && lodLevel > AvatarLodLevel.Lod2)
				{
					return lodMaterial3;
				}
				else if (lodMaterial2 != null && lodLevel > AvatarLodLevel.Lod1)
				{
					return lodMaterial2;
				}
				else if (lodMaterial1 != null && lodLevel > AvatarLodLevel.Lod0)
				{
					return lodMaterial1;
				}

				// at least material0 should be configured.
				return lodMaterial0;
			}

			protected void CheckPrepareConfiguration(PicoAvatarRenderMaterial nativeMaterial)
			{
				if (lodMaterial0 == null)
				{
					lodMaterial0 = new Material(Shader.Find("PAV/URP/DiffuseSpec"));
				}

				CheckInitialize(nativeMaterial);
			}

			#endregion


			#region Private Fields

			private int id_BumpMap;
			private int id_BaseMap = 0;

			#endregion


			#region Private Methods

			/// <summary>
			/// gets property ids.
			/// </summary>
			/// <param name="nativeMaterial"></param>
			private void CheckInitialize(PicoAvatarRenderMaterial nativeMaterial)
			{
				// check compute shader.
				if (morphAndSkinningShader == null)
				{
					morphAndSkinningShader = Resources.Load<ComputeShader>("MorphAndSkinning");
				}

				// check compute shader.
				if (transferShapingMeshShader == null)
				{
					transferShapingMeshShader = Resources.Load<ComputeShader>("PavTransferShapingMesh");
				}

				// float property
				for (int i = 0; i < nativeMaterial.avatarRenderMaterialDef.floatPropertyCount; ++i)
				{
					nativeMaterial.mat_FloatPropertyItems[i].unityID =
						Shader.PropertyToID(nativeMaterial.mat_FloatPropertyItems[i].unityName);
				}

				// color property
				for (int i = 0; i < nativeMaterial.avatarRenderMaterialDef.colorPropertyCount; ++i)
				{
					nativeMaterial.mat_ColorPropertyItems[i].unityID =
						Shader.PropertyToID(nativeMaterial.mat_ColorPropertyItems[i].unityName);
				}

				// vector property
				for (int i = 0; i < nativeMaterial.avatarRenderMaterialDef.vectorPropertyCount; ++i)
				{
					nativeMaterial.mat_VectorPropertyItems[i].unityID =
						Shader.PropertyToID(nativeMaterial.mat_VectorPropertyItems[i].unityName);
				}

				// texture property
				for (int i = 0; i < nativeMaterial.avatarRenderMaterialDef.texturePropertyCount; ++i)
				{
					nativeMaterial.mat_TexturePropertyItems[i].unityID =
						Shader.PropertyToID(nativeMaterial.mat_TexturePropertyItems[i].unityName);
					nativeMaterial.mat_TexturePropertyItems[i].unityArrayID =
						Shader.PropertyToID(nativeMaterial.mat_TexturePropertyItems[i].unityArrayName);
				}

				id_BumpMap = Shader.PropertyToID(name_BumpMap);
				id_BaseMap = Shader.PropertyToID(name_BaseMap);
			}

			#endregion
		}
	}
}