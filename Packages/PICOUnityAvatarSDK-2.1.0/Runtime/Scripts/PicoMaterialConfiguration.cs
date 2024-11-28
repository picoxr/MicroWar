using UnityEngine;
using UnityEngine.Serialization;
using System.Collections.Generic;

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

			[Header("Avatar Lit Config")]
			public Material AvatarLitMaterial;
			public Material AvatarSkinMaterial;
			public Material AvatarHairMaterial;
			public Material AvatarEyeMaterial;
			public Material AvatarSimpleMaterial;
			public Material AvatarBakeMaterial;

			/// <summary>
			/// Official lod PBR materials. application maybe need add some more properties.
			/// </summary>
			[Header("PBR Material Config")]
			public Material lodPBRMaterial0;
			public Material lodPBRMaterial1;
			public Material lodPBRMaterial2;
			public Material lodPBRMaterial3;
			public Material lodPBRMaterial4;

			/// <summary>
			/// Official lod NPR materials. application maybe need add some more properties.
			/// </summary>
			[Header("NPR Material Config")]
			public Material lodNPRMaterial0;
			public Material lodNPRMaterial1;
			public Material lodNPRMaterial2;
			public Material lodNPRMaterial3;
			public Material lodNPRMaterial4;
			
			[Header("other Material or Shader")]
			public Shader avatarBunchShader;
			public Material hairKKMaterial0;

			
			public ShaderVariantCollection shaderVariantCollection;

			[Header("other property")]
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
			/// astc encode shader.
			/// </summary>
			public ComputeShader astcEncodeShader;

			/// <summary>
			/// blend texture shader
			/// </summary>
			public ComputeShader blendTextureShader;

			/// <summary>
			/// merge shader.
			/// </summary>
			public Shader mergeTextureShader;

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
            public void ApplyToMaterial(PicoAvatarRenderMaterial nativeMaterial, Material material)// fix name
			{
				// set float properties
				UpdateToUniformsMaterial(nativeMaterial, material);
				// set vector properties.
				{
					foreach (MaterialVectorPropertyItem item in nativeMaterial.mat_VectorPropertyItems)
					{
						//Debug.Log($"pico down find texture scale offset? {item.unityName}");
						//todo vector is not used on old shader per MaterialDef, need concern for texture ST?

						if (item.has_value)
						{
							if (AvatarManager.IsAvatarLitShader(nativeMaterial.mat_ShaderTheme))
							{
								material.SetVector(item.unityID, item.value);
							}
							else
							{
								material.SetTextureScale(item.unityID, new Vector2(item.value.x, item.value.y));
								material.SetTextureOffset(item.unityID,
									new Vector2(item.value.z, 1 - item.value.w - item.value.y));
							}
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
					if (AvatarManager.IsAvatarLitShader(nativeMaterial.mat_ShaderTheme) && item.has_ST)
                    {
						material.SetTextureScale(item.unityID, new Vector2(item.value_ST.x, item.value_ST.y));
						material.SetTextureOffset(item.unityID,
							new Vector2(item.value_ST.z, 1 - item.value_ST.w - item.value_ST.y));
					}
				}
#if UNITY_EDITOR
				UnityEditor.AssetDatabase.Refresh();
#endif
			}


			/// <summary>
			/// Sets properties to unity runtime material, derived class can override the method as filter to modify material.
			/// </summary>
			/// <param name="nativeMaterial">Source material</param>
			/// <param name="lodLevel">current lodLevel</param>
			/// <returns>unity material</returns>
			public virtual Material CreateRuntimeMaterialFromNativeMaterial(PicoAvatarRenderMaterial nativeMaterial, AvatarLodLevel lodLevel) // fix name: create material 
			{
				CheckPrepareConfiguration(nativeMaterial);
				//
				var material = new Material(SelectMaterial(nativeMaterial.mat_ShaderType, nativeMaterial.mat_ShaderTheme, lodLevel));

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
						if (AvatarConstants.s_keywordPair.ContainsKey(item.unityName))
                        {
							if (item.value > 0.0f)
                            {
								material.EnableKeyword(AvatarConstants.s_keywordPair[item.unityName]);
                            }
							else
                            {
								material.DisableKeyword(AvatarConstants.s_keywordPair[item.unityName]);
							}
						}
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

				// set colors properties.
				foreach (MaterialVectorPropertyItem item in nativeMaterial.mat_VectorPropertyItems)
                {
                    if (item.has_value)
                    {
                        material.SetVector(item.unityID, item.value);
                    }
                }

                nativeMaterial.SetCustomVecs(); //useless
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
			protected virtual Material SelectMaterial(AvatarShaderType shaderType, OfficialShaderTheme shaderTheme, AvatarLodLevel lodLevel)
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
				if (shaderTheme == OfficialShaderTheme.PicoPBR)
				{
                    if (lodPBRMaterial4 != null && (int)AvatarLodLevel.Count > 3 && lodLevel > (AvatarLodLevel)3)
                    {
                        return lodPBRMaterial4;
                    }
                    else if (lodPBRMaterial3 != null && lodLevel > AvatarLodLevel.Lod2)
                    {
                        return lodPBRMaterial3;
                    }
                    else if (lodPBRMaterial2 != null && lodLevel > AvatarLodLevel.Lod1)
                    {
                        return lodPBRMaterial2;
                    }
                    else if (lodPBRMaterial1 != null && lodLevel > AvatarLodLevel.Lod0)
                    {
                        return lodPBRMaterial1;
                    }
                    else if (lodPBRMaterial0 != null)
                    {
                        return lodPBRMaterial0;
                    }
				}
				else if (shaderTheme == OfficialShaderTheme.PicoNPR)
				{
					if (lodNPRMaterial4 != null && (int)AvatarLodLevel.Count > 3 && lodLevel > (AvatarLodLevel)3)
					{
						return lodNPRMaterial4;
					}
					else if (lodNPRMaterial3 != null && lodLevel > AvatarLodLevel.Lod2)
					{
						return lodNPRMaterial3;
					}
					else if (lodNPRMaterial2 != null && lodLevel > AvatarLodLevel.Lod1)
					{
						return lodNPRMaterial2;
					}
					else if (lodNPRMaterial1 != null && lodLevel > AvatarLodLevel.Lod0)
					{
						return lodNPRMaterial1;
					}
					else if (lodNPRMaterial0 != null)
					{
						return lodNPRMaterial0;
					}
				}
                else
                {
					var material = FetchMaterial(shaderTheme);
					return material;
                }

				// at least material0 should be configured.
				return lodPBRMaterial0;
			}

			private static Dictionary<OfficialShaderTheme, string> s_themeShaderNames = new Dictionary<OfficialShaderTheme, string>()
			{
				{ OfficialShaderTheme.PicoAvatarLit, "AvatarLitMaterial" },
				{ OfficialShaderTheme.PicoAvatarSkin, "AvatarSkinMaterial" },
				{ OfficialShaderTheme.PicoAvatarHair, "AvatarHairMaterial" },
				{ OfficialShaderTheme.PicoAvatarEye, "AvatarEyeMaterial" },
				{ OfficialShaderTheme.PicoAvatarSimpleLit, "AvatarSimpleMaterial" },
				{ OfficialShaderTheme.PicoAvatarBake, "AvatarBakeMaterial" },
			};

			public Material FetchMaterial(OfficialShaderTheme theme)
			{
				if (!s_themeShaderNames.ContainsKey(theme))
                {
					return null;
                }
				string name = s_themeShaderNames[theme];
				Material material = (Material)(typeof(PicoMaterialConfiguration).GetField(name)?.GetValue(this));
				//if (theme == OfficialShaderTheme.PicoAvatarLit) Debug.Log($"pico down is GetValue return the even reference? {material == AvatarLitMaterial}");
				return material;
			}

			protected void CheckPrepareConfiguration(PicoAvatarRenderMaterial nativeMaterial)
			{
				if (lodPBRMaterial0 == null)
				{
					lodPBRMaterial0 = new Material(Shader.Find("PAV/URP/PicoPBR"));
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

				id_BumpMap = Shader.PropertyToID(name_BumpMap);
				id_BaseMap = Shader.PropertyToID(name_BaseMap);

				if (!AvatarManager.IsAvatarLitShader(nativeMaterial.mat_ShaderTheme))
                {
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
                }
			}

			#endregion
		}
	}
}