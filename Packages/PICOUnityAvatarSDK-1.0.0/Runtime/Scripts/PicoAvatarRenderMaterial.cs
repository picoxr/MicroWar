using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		public enum RenderPipelineType
		{
			None = 0,
			UnityBuitIn = 1,
			UnityURP = 2
		}

		public enum AvatarTextureSemantic
		{
			None = 0,
			BaseMap = 1,
			ToonShadowMap = 2,
			MetallicGlossMap = 3,
			SpecGlossMap = 4,
			BumpMap = 5,
			ParallaxMap = 6,
			DetailNormalMap = 7,
			EmissionMap = 8,
			DetailMask = 9,
			DetailAlbedoMap = 10,
			ColorRegionMap = 11,
			SecondBaseMap = 12,
			SecondBumpMap = 13,
			SecondMetallicSpecGlossMap = 14,
		}

		public enum AvatarFloatSemantic
		{
			None = 0,
			Metallic = 1,
			Smoothness = 2,
		}

		public enum AvatarColorSemantic
		{
			None = 0,
			SpecColor = 1,
			BaseColor = 2,
		}

		public enum AvatarVectorSemantic
		{
			None = 0,
		}

		// read content from c++
		[StructLayout(LayoutKind.Sequential, Pack = 8)]
		public struct MaterialPropertyItemRaw
		{
			public uint id;
			public uint semantic;
			public System.IntPtr aeName;
			public System.IntPtr unityName;
		}

		public class MaterialPropertyItem
		{
			public uint id;
			public int unityID;
			public uint semantic;
			public string unityName;

			public MaterialPropertyItem()
			{
			}

			public MaterialPropertyItem(MaterialPropertyItemRaw item)
			{
				id = item.id;
				unityName = Marshal.PtrToStringAnsi(item.unityName);
				semantic = item.semantic;
			}

			public MaterialPropertyItem(MaterialPropertyItem item)
			{
				id = item.id;
				unityName = item.unityName;
				semantic = item.semantic;
			}
		}

		public class MaterialFloatPropertyItem : MaterialPropertyItem
		{
			public MaterialFloatPropertyItem(MaterialPropertyItemRaw item) : base(item)
			{
			}

			public MaterialFloatPropertyItem(MaterialPropertyItem item) : base(item)
			{
			}

			public float value;
			public bool has_value;
		}

		public class MaterialColorPropertyItem : MaterialPropertyItem
		{
			public MaterialColorPropertyItem(MaterialPropertyItemRaw item) : base(item)
			{
			}

			public MaterialColorPropertyItem(MaterialPropertyItem item) : base(item)
			{
			}

			public Color value;
			public bool has_value;
		}

		public class MaterialVectorPropertyItem : MaterialPropertyItem
		{
			public MaterialVectorPropertyItem(MaterialPropertyItemRaw item) : base(item)
			{
			}

			public MaterialVectorPropertyItem(MaterialPropertyItem item) : base(item)
			{
			}

			public Vector4 value;
			public bool has_value;
		}

		public class MaterialTexturePropertyItem : MaterialPropertyItem
		{
			public MaterialTexturePropertyItem(MaterialPropertyItemRaw item) : base(item)
			{
				unityArrayName = $"{unityName}Array";
			}

			public MaterialTexturePropertyItem(MaterialPropertyItem item) : base(item)
			{
				unityArrayName = $"{unityName}Array";
			}

			public AvatarTexture value;
			public bool has_value;
			public int unityArrayID;
			public string unityArrayName;
		}

		public class AvatarRenderMaterialDef : NativeObject
		{
			public AvatarRenderMaterialDef(AvatarShaderType shaderType, RenderPipelineType pipelineType)
			{
				floatPropertyItems = new List<MaterialPropertyItem>();
				colorPropertyItems = new List<MaterialPropertyItem>();
				vectorPropertyItems = new List<MaterialPropertyItem>();
				texturePropertyItems = new List<MaterialPropertyItem>();

				string key = $"{(uint)shaderType}_{(uint)pipelineType}";
				System.IntPtr materialDefHandle = pav_AvatarRenderMaterial_GetRenderMaterialDef(key);
				if (materialDefHandle == System.IntPtr.Zero)
				{
					UnityEngine.Debug.LogError(string.Format(
						"Failed to load avatar material. shaderType:{0} pipelineType:{1}", shaderType, pipelineType));
					return;
				}

				floatPropertyCount = pav_AvatarRenderMaterial_GetFloatPropertyCount(materialDefHandle);
				colorPropertyCount = pav_AvatarRenderMaterial_GetColorPropertyCount(materialDefHandle);
				vectorPropertyCount = pav_AvatarRenderMaterial_GetVectorPropertyCount(materialDefHandle);
				texturePropertyCount = pav_AvatarRenderMaterial_GetTexturePropertyCount(materialDefHandle);

				MaterialPropertyItemRaw itemRaw = new MaterialPropertyItemRaw();
				for (int idx = 0; idx < floatPropertyCount; ++idx)
				{
					pav_AvatarRenderMaterial_GetFloatPropertyItem(materialDefHandle, idx, ref itemRaw);
					MaterialPropertyItem item = new MaterialPropertyItem(itemRaw);
					floatPropertyItems.Add(item);
				}

				for (int idx = 0; idx < colorPropertyCount; ++idx)
				{
					pav_AvatarRenderMaterial_GetColorPropertyItem(materialDefHandle, idx, ref itemRaw);
					MaterialPropertyItem item = new MaterialPropertyItem(itemRaw);
					colorPropertyItems.Add(item);
				}

				for (int idx = 0; idx < vectorPropertyCount; ++idx)
				{
					pav_AvatarRenderMaterial_GetVectorPropertyItem(materialDefHandle, idx, ref itemRaw);
					MaterialPropertyItem item = new MaterialPropertyItem(itemRaw);
					vectorPropertyItems.Add(item);
				}

				for (int idx = 0; idx < texturePropertyCount; ++idx)
				{
					pav_AvatarRenderMaterial_GetTexturePropertyItem(materialDefHandle, idx, ref itemRaw);
					MaterialPropertyItem item = new MaterialPropertyItem(itemRaw);
					texturePropertyItems.Add(item);
				}
			}

			public int floatPropertyCount;
			public int colorPropertyCount;
			public int vectorPropertyCount;
			public int texturePropertyCount;

			public List<MaterialPropertyItem> floatPropertyItems;
			public List<MaterialPropertyItem> colorPropertyItems;
			public List<MaterialPropertyItem> vectorPropertyItems;
			public List<MaterialPropertyItem> texturePropertyItems;

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarRenderMaterial_GetRenderMaterialDef(string key);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarRenderMaterial_GetFloatPropertyCount(System.IntPtr tableHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarRenderMaterial_GetColorPropertyCount(System.IntPtr tableHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarRenderMaterial_GetVectorPropertyCount(System.IntPtr tableHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarRenderMaterial_GetTexturePropertyCount(System.IntPtr tableHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarRenderMaterial_GetFloatPropertyItem(System.IntPtr tableHandle, int idx,
				ref MaterialPropertyItemRaw item);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarRenderMaterial_GetColorPropertyItem(System.IntPtr tableHandle, int idx,
				ref MaterialPropertyItemRaw item);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarRenderMaterial_GetVectorPropertyItem(System.IntPtr tableHandle,
				int idx, ref MaterialPropertyItemRaw item);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarRenderMaterial_GetTexturePropertyItem(System.IntPtr tableHandle,
				int idx, ref MaterialPropertyItemRaw item);
		}

		/// <summary>
		/// AvatarRenderMaterial reads material data from native library and create Unity Material.
		/// </summary>
		public class PicoAvatarRenderMaterial : NativeObject
		{
			#region Public Typesd

			// Unity surface type
			public enum SurfaceType // UnityEditor.BaseShaderGUI.SurfaceType
			{
				Opaque,
				Transparent
			}

			// Unity BlendMode
			public enum BlendMode // UnityEditor.BaseShaderGUI.SurfaceType.BlendMode
			{
				Alpha, // Old school alpha-blending mode, fresnel does not affect amount of transparency
				Premultiply, // Physically plausible transparency mode, implemented as alpha pre-multiply
				Additive,
				Multiply
			}

			// Unity Smoothness Source.
			public enum SmoothnessSource // UnityEditor.BaseShaderGUI.SurfaceType.SmoothnessSource
			{
				BaseAlpha,
				SpecularAlpha
			}

			// Unity cull mode.
			public enum RenderFace // UnityEditor.BaseShaderGUI.SurfaceType.RenderFace
			{
				Front = 2,
				Back = 1,
				Both = 0
			}

			public enum BlendModeOpt //UnityEngine.Rendering.BlendMode;
			{
				//
				Zero = 0,
				One = 1,
				DstColor = 2,
				SrcColor = 3,
				OneMinusDstColor = 4,
				SrcAlpha = 5,
				OneMinusSrcColor = 6,
				DstAlpha = 7,
				OneMinusDstAlpha = 8,
				SrcAlphaSaturate = 9,
				OneMinusSrcAlpha = 10
			}

			#endregion


			#region Public Properties

			public AvatarRenderMaterialDef avatarRenderMaterialDef;
			public List<MaterialFloatPropertyItem> mat_FloatPropertyItems;
			public List<MaterialColorPropertyItem> mat_ColorPropertyItems;
			public List<MaterialVectorPropertyItem> mat_VectorPropertyItems;
			public List<MaterialTexturePropertyItem> mat_TexturePropertyItems;

			/// <summary>
			/// Get active material configuration. The configuration in AmazAvatarManager is the top priority.
			/// </summary>
			public static PicoMaterialConfiguration materialConfiguration
			{
				get => GetMaterialConfiguration();
			}

			// hint should cull back. local actor should cull back.
			public bool hintCullBack = false;

			#region Material properties

			// AvatarShaderType "_ShaderType"; default : 0.0
			public AvatarShaderType mat_ShaderType { get; private set; }

			// RenderPipelineType; default : 0.0
			public RenderPipelineType mat_RenderPipelineType { get; private set; }

			// render queue; default : -2
			public int mat_RenderQueue { get; private set; }

			// AvatarSceneBlendType "_SceneBlendType"; default: 0.0
			internal AvatarSceneBlendType mat_SceneBlendType { get; private set; }

			// AvatarRenderFace "_RenderFace"; default: 2.0
			internal AvatarRenderFace mat_RenderFace { get; private set; }

			// AvatarRenderFace "_ZWrite"; default 1.0
			public bool mat_ZWrite { get; private set; }

			// AvatarRenderFace "_ZTest"; default 1.0
			public bool mat_ZTest { get; private set; }

			// AvatarPBRWorkflow "_Workflow"; default: 1.0
			internal AvatarPBRWorkflow mat_Workflow => AvatarPBRWorkflow.Metallic;

			// AvatarPBRWorkflow "_SmoothSource"; default: 1.0
			internal AvatarPBRSmoothnessSource mat_SmoothSource
			{
				get { return AvatarPBRSmoothnessSource.SpecularMetallicAlpha; }
			}

			// mat color mask rgba channel masks
			public byte mat_ColorMask { get; private set; }

			public Vector4 mat_BaseMap_ST
			{
				get => matPV_BaseMap_ST;
			}

			public AvatarTexture mat_BumpMap
			{
				get => matPV_BumpMap;
			}

			public AvatarTexture mat_EmissionMap
			{
				get => matPV_EmissionMap;
			}

			#endregion

			#region Has Material properties

			public bool has_BaseMap_ST;
			public bool has_BumpMap;
			public bool has_ColorRegionBaked;

			#endregion

			#endregion


			#region Public Methods

			internal PicoAvatarRenderMaterial(bool merged, AvatarLod avatarLod)
			{
				_Merged = merged;
				_AvatarLod = avatarLod;
			}

			// Get runtime unity material.
			internal Material GetRuntimeMaterial(PicoAvatarRenderMesh renderMesh)
			{
				if (_RuntimeMaterial != null)
				{
					return _RuntimeMaterial;
				}

				//
				if (_MaterialConfig == null)
				{
					return null;
				}

				// set material
				_RuntimeMaterial = _MaterialConfig.ApplyToMaterial(this, renderMesh.lodLevel);

				if (_AvatarLod.owner.owner.materialProvider != null)
				{
					Material materialOverride =
						_AvatarLod.owner.owner.materialProvider(renderMesh.lodLevel, mat_ShaderType, renderMesh);
					if (materialOverride)
					{
						_RuntimeMaterial = materialOverride;
					}
				}

				if (!(renderMesh.materialNeedTangent && renderMesh.hasTangent))
				{
					_RuntimeMaterial.EnableKeyword("PAV_NO_TANGENTS");
				}

				//
				return _RuntimeMaterial;
			}

			// Update avatar scene light env.
			internal void OnAvatarSceneLightEnvChanged(PicoAvatarSceneLightEnv lightEnv)
			{
				if (lightEnv != null && _RuntimeMaterial != null)
				{
					_RuntimeMaterial.SetFloat("_AdditiveGI", lightEnv.additiveGI);
				}
			}

			private MaterialPropertyBlock _propertyBlock = new MaterialPropertyBlock();

			internal void UpdateMaterial(Material material)
			{
				_MaterialConfig.ApplyToMaterial(this, material);

				//https://docs.unity3d.com/Manual/SL-Blend.html
				//if (mat_ShaderType != AvatarShaderType.Body_Base)
				{
					material.SetFloat("_ShaderType", (float)mat_ShaderType);

					// the way will be deprecated! use mat_SceneBlendType and AvatarSceneBlendType instead.
					if (mat_ShaderType == AvatarShaderType.Eyelash_Base)
					{
						// for builtin.
						material.EnableKeyword("_ALPHABLEND_ON");
						// force  turn off z write
						mat_ZWrite = false;
					}
				}

				// check add additive gi.
				{
					float additiveGI = PicoAvatarApp.instance.appSettings.additiveGI;
					//
					if (PicoAvatarApp.instance.curAvatarSceneLightEnv != null)
					{
						additiveGI = PicoAvatarApp.instance.curAvatarSceneLightEnv.additiveGI;
					}

					material.SetFloat("_AdditiveGI", additiveGI);
				}

				// if tooth, only accept main light.
				if (_MaterialConfig.enableMultiLights)
				{
					material.EnableKeyword("_ADDITIONAL_LIGHTS");
				}

				// set macros.
				if (mat_BumpMap != null)
				{
					material.EnableKeyword("_NORMALMAP");
					material.EnableKeyword("_METALLICSPECGLOSSMAP");
					material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
					material.DisableKeyword("_SPECULAR_SETUP");
					material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
#if UNITY_EDITOR
					material.SetFloat("_SpecularHighlights", 0.0f);
					material.SetFloat("_EnvironmentReflections", 1.0f);
#endif
					//UnityEngine.Debug.Log("AvatarSDK metallic tex set.");
				}

				if (mat_EmissionMap != null)
				{
					material.EnableKeyword("_EMISSION");
				}

				//
				if (mat_SceneBlendType == AvatarSceneBlendType.SrcAlpha_OneMinusSrcAlpha)
				{
					material.SetFloat("_Surface", (float)SurfaceType.Transparent);
					material.SetFloat("_Blend", (float)BlendMode.Alpha);
					material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
					material.SetFloat("_DstBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
					material.SetFloat("_SrcAlphaBlend", (float)UnityEngine.Rendering.BlendMode.One);
					material.SetFloat("_DstAlphaBlend", (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);

					// for builtin.
					material.EnableKeyword("_ALPHABLEND_ON");

					// set queue to transparent + 500.
					material.renderQueue = (int)PicoAvatarApp.instance.renderSettings.avatarTranspQueueStart;
					//
					material.SetOverrideTag("RenderType", "Transparent");
				}
				else if (PicoAvatarApp.instance.renderSettings.avatarOpaqueQueueStart != 2000)
				{
					material.renderQueue = (int)PicoAvatarApp.instance.renderSettings.avatarOpaqueQueueStart;
				}
				else if (PicoAvatarApp.instance.renderSettings.avatarOpaqueQueueStart != 2000)
				{
					material.renderQueue = (int)PicoAvatarApp.instance.renderSettings.avatarOpaqueQueueStart;
				}

				if (mat_RenderQueue >= 0)
				{
					material.renderQueue = mat_RenderQueue;
				}

				// color mask
				if (mat_ColorMask != 0x0f)
				{
					material.SetFloat("_ColorMask", (float)mat_ColorMask);
				}

				{
					float cullValue = 0.0f; // close cull.
					material.SetFloat("_Cull", cullValue);
				}


				if (!mat_ZWrite)
				{
					material.SetFloat("_ZWrite", (float)0.0f);
				}

				if (!mat_ZTest)
				{
					material.SetFloat("_ZTest", (float)0.0f);
				}

				if (PicoAvatarApp.instance != null && PicoAvatarApp.instance.renderSettings.enableRimProfile)
				{
					material.EnableKeyword("PAV_RIM_PROFILE");
				}

				// PAV_COLOR_REGION_BAKED
				{
					if (matPV_ColorRegionBaked > 0.5f)
					{
						material.EnableKeyword("PAV_COLOR_REGION_BAKED");
					}
				}
			}
			
            //Load material data from native render material.
            //@param nativeHandle_ native AvatarRenderMaterial. Reference count has been added by invoker.
            internal bool LoadPropertiesFromNativeMaterial(System.IntPtr nativeHandle_, AvatarLodLevel lodLevel)
			{
				if (nativeHandle != System.IntPtr.Zero)
				{
					throw new System.Exception("BadProgram!");
				}

				// no need to add reference count since invoker has added.
				SetNativeHandle(nativeHandle_, false);

				// TODO(tianshengcai): to promote performance?
				// config
				{
					var renderState = new RenderStates();
					renderState.version = 0;
					pav_AvatarRenderMaterial_GetRenderStates(nativeHandle_, ref renderState);

					//
					mat_ShaderType = (AvatarShaderType)renderState.shaderType;
					mat_RenderQueue = renderState.renderQueue;
					// reset render queue id.
					if (mat_RenderQueue >= 3000)
					{
						mat_RenderQueue = PicoAvatarApp.instance.renderSettings.avatarTranspQueueStart +
							mat_RenderQueue - 3000;
					}

					mat_SceneBlendType = (AvatarSceneBlendType)renderState.sceneBlendType;
					mat_RenderFace = (AvatarRenderFace)renderState.renderFace;
					mat_ZWrite = renderState.zWrite != 0;
					mat_ZTest = renderState.zTest != 0;
					//mat_Workflow = (AvatarPBRWorkflow)renderState.workflow;
					//mat_SmoothSource = (AvatarPBRSmoothnessSource)renderState.smoothSource;
					mat_ColorMask = (byte)renderState.colorMask;
				}
				var _MaterialConfig = GetMaterialConfiguration();
				mat_RenderPipelineType = _MaterialConfig.renderPipelineType == RenderPipelineType.None
					? RenderPipelineType.UnityURP
					: _MaterialConfig.renderPipelineType;
				avatarRenderMaterialDef = new AvatarRenderMaterialDef(mat_ShaderType, mat_RenderPipelineType);
				avatarRenderMaterialDef.Retain();
				mat_FloatPropertyItems = new List<MaterialFloatPropertyItem>();
				mat_ColorPropertyItems = new List<MaterialColorPropertyItem>();
				mat_VectorPropertyItems = new List<MaterialVectorPropertyItem>();
				mat_TexturePropertyItems = new List<MaterialTexturePropertyItem>();

				CheckInitialize();

				// load floats
				UpdateDirtyMaterialUniforms();

				// properties process
				ConvertMergedMaterialColorsToLinear();

				// set smoothness.
				foreach (MaterialFloatPropertyItem floatPropertyItem in mat_FloatPropertyItems)
				{
					switch ((AvatarFloatSemantic)floatPropertyItem.semantic)
					{
						case AvatarFloatSemantic.Smoothness:
							if (_MaterialConfig.need_BumpMap
							    && ((lodLevel == AvatarLodLevel.Lod2 && !_MaterialConfig.isLod2NeedBumpMap) ||
							        (lodLevel == (AvatarLodLevel)3 && !_MaterialConfig.isLod3EnablePBR)))
							{
								//TODO: temporarily set smooth to 1.
								floatPropertyItem.value = 0.4f;
							}
							else
							{
								//TODO: temporarily set smooth value to 0.8.
								if (floatPropertyItem.value == 0.0f)
								{
									floatPropertyItem.value = 0.8f;
								}
							}

							floatPropertyItem.has_value = true;
							SetMergedFloat(-1, floatPropertyItem.id, floatPropertyItem.value);
							break;
						case AvatarFloatSemantic.Metallic:
							floatPropertyItem.has_value = true;
							//TODO: temporarily set  metallic value to 0.8.
							if (floatPropertyItem.value == 0.0f)
							{
								floatPropertyItem.value = 0.8f;
							}

							SetMergedFloat(-1, floatPropertyItem.id, floatPropertyItem.value);
							break;
						default:
							break;
					}
				}

				foreach (MaterialColorPropertyItem colorPropertyItem in mat_ColorPropertyItems)
				{
					switch ((AvatarColorSemantic)colorPropertyItem.semantic)
					{
						case AvatarColorSemantic.SpecColor:
							if (colorPropertyItem.value.r < 0.001f && colorPropertyItem.value.g < 0.001f &&
							    colorPropertyItem.value.b < 0.001f)
							{
								colorPropertyItem.has_value = true;
								colorPropertyItem.value = new Color(0.05f, 0.05f, 0.05f, 0.5f);
							}

							int mergedCount = GetMergedCount();
							for (int i = 0; i < mergedCount; ++i)
							{
								Color specColor = Color.white;
								if (GetMergedColor(i, colorPropertyItem.id, ref specColor, true))
								{
									if (specColor.r < 0.001f &&
									    specColor.g < 0.001f &&
									    specColor.b < 0.001f)
									{
										specColor = new Color(0.05f, 0.05f, 0.05f, 0.5f);
										SetMergedColor(i, colorPropertyItem.id, specColor);
									}
								}
							}

							break;
						case AvatarColorSemantic.BaseColor:
							colorPropertyItem.value = Color.white;
							colorPropertyItem.has_value = true;
							break;
						default:
							break;
					}
				}

				return true;
			}

			// When  uniforms in native material changed, should synchronized to Unity material.
			internal void UpdateDirtyMaterialUniforms()
			{
				for (int idx = 0; idx < avatarRenderMaterialDef.floatPropertyCount; ++idx)
				{
					mat_FloatPropertyItems[idx].has_value = LoadFloat(nativeHandle, mat_FloatPropertyItems[idx].id,
						ref mat_FloatPropertyItems[idx].value);
				}

				for (int idx = 0; idx < avatarRenderMaterialDef.colorPropertyCount; ++idx)
				{
					mat_ColorPropertyItems[idx].has_value = LoadColor(nativeHandle, mat_ColorPropertyItems[idx].id,
						ref mat_ColorPropertyItems[idx].value);
				}

				for (int idx = 0; idx < avatarRenderMaterialDef.vectorPropertyCount; ++idx)
				{
					mat_VectorPropertyItems[idx].has_value = LoadVector(nativeHandle, mat_VectorPropertyItems[idx].id,
						ref mat_VectorPropertyItems[idx].value);
				}

				// TODO: move into js
				{
					has_ColorRegionBaked = LoadFloat(nativeHandle, id_ColorRegionBaked, ref matPV_ColorRegionBaked);
					has_BaseMap_ST = LoadVector(nativeHandle, id_BaseMap_ST, ref matPV_BaseMap_ST);
				}

				LoadCustomVecs();
			}

			private void ConvertMergedMaterialColorsToLinear()
			{
				if (!_Merged)
				{
					return;
				}

				int mergedCount = GetMergedCount();
				for (int i = 0; i < mergedCount; ++i)
				{
					Color color = Color.white;
					for (int idx = 0; idx < avatarRenderMaterialDef.colorPropertyCount; ++idx)
					{
						if (GetMergedColor(i, mat_ColorPropertyItems[idx].id, ref color, false))
						{
							SetMergedColor(i, mat_ColorPropertyItems[idx].id, color);
						}
					}
				}
			}

			internal bool LoadTexturesFromNativeMaterial(AvatarLodLevel lodLevel)
			{
				// clear map flags.
				has_BumpMap = false;

				foreach (MaterialTexturePropertyItem texturePropertyItem in mat_TexturePropertyItems)
				{
					switch ((AvatarTextureSemantic)texturePropertyItem.semantic)
					{
						case AvatarTextureSemantic.BaseMap:
							texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
								ref texturePropertyItem.value, false);
							break;
						case AvatarTextureSemantic.ToonShadowMap:
							if (_MaterialConfig.need_ToonShadowMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, false);
							}

							break;
						case AvatarTextureSemantic.MetallicGlossMap:
							if ((lodLevel < (AvatarLodLevel)3 || _MaterialConfig.isLod3EnablePBR) &&
							    _MaterialConfig.need_MetallicGlossMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, false);
							}

							break;
						case AvatarTextureSemantic.SpecGlossMap:
							if ((lodLevel < (AvatarLodLevel)3 || _MaterialConfig.isLod3EnablePBR) &&
							    _MaterialConfig.need_SpecGlossMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, false);
							}

							break;
						case AvatarTextureSemantic.BumpMap:
							if (_MaterialConfig.need_BumpMap &&
							    (lodLevel < AvatarLodLevel.Lod2 || _MaterialConfig.isLod2NeedBumpMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
								matPV_BumpMap = texturePropertyItem.value;
								has_BumpMap = texturePropertyItem.has_value;
							}

							break;
						case AvatarTextureSemantic.ParallaxMap:
							if (_MaterialConfig.need_BumpMap && _MaterialConfig.need_ParallaxMap &&
							    (lodLevel < AvatarLodLevel.Lod2 || _MaterialConfig.isLod2NeedBumpMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.DetailNormalMap:
							if (_MaterialConfig.need_BumpMap && _MaterialConfig.need_DetailNormalMap &&
							    (lodLevel < AvatarLodLevel.Lod2 || _MaterialConfig.isLod2NeedBumpMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.EmissionMap:
							if (_MaterialConfig.need_EmissionMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
								matPV_EmissionMap = texturePropertyItem.value;
							}

							break;
						case AvatarTextureSemantic.DetailMask:
							if (_MaterialConfig.need_DetailMask)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.DetailAlbedoMap:
							if (_MaterialConfig.need_DetailAlbedoMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.ColorRegionMap:
							if (_MaterialConfig.need_ColorRegionMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.SecondBaseMap:
							if (_MaterialConfig.need_SecondMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.SecondBumpMap:
							if (_MaterialConfig.need_SecondMap && _MaterialConfig.need_BumpMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						case AvatarTextureSemantic.SecondMetallicSpecGlossMap:
							if (_MaterialConfig.need_SecondMap && (_MaterialConfig.need_MetallicGlossMap ||
							                                       _MaterialConfig.need_SpecGlossMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value);
							}

							break;
						default:
							break;
					}
				}

				return true;
			}

			#region Custom Vectors

			private static readonly int PAV_CUSTOM_PROPERTY_KEY_COUNT = 10;
			private static uint[] _id_CustomVecs = null;
			private Vector4[] _customVecs = new Vector4[PAV_CUSTOM_PROPERTY_KEY_COUNT];

			private void CheckCustomVecsID()
			{
				if (_id_CustomVecs == null)
				{
					_id_CustomVecs = new uint[PAV_CUSTOM_PROPERTY_KEY_COUNT];
					for (int i = 0; i < PAV_CUSTOM_PROPERTY_KEY_COUNT; ++i)
					{
						_id_CustomVecs[i] = Pico.Avatar.Utility.AddNameToIDNameTable("_CustomVec_" + i);
					}
				}
			}

			internal void LoadCustomVecs()
			{
				CheckCustomVecsID();

				for (int i = 0; i < PAV_CUSTOM_PROPERTY_KEY_COUNT; ++i)
				{
					LoadVector(nativeHandle, _id_CustomVecs[i], ref _customVecs[i]);
				}
			}

			internal void SetCustomVecs()
			{
				for (int i = 0; i < PAV_CUSTOM_PROPERTY_KEY_COUNT; ++i)
				{
					_RuntimeMaterial.SetVector("_CustomVec_" + i, _customVecs[i]);
				}
			}

			#endregion

			internal int GetMergedCount()
			{
				if (!_Merged)
				{
					return 0;
				}

				int materialCount = 0;
				pav_LodMergedAvatarRenderMaterial_getMergedCount(nativeHandle, ref materialCount);
				return materialCount;
			}

			internal bool SetMergedFloat(int materialIndex, uint propertyID, float val)
			{
				if (!_Merged)
				{
					return false;
				}

				return NativeResult.Success ==
				       pav_LodMergedAvatarRenderMaterial_setFloat(nativeHandle, materialIndex, propertyID, ref val);
			}

			internal bool SetMergedColor(int materialIndex, uint propertyID, Color val)
			{
				if (!_Merged)
				{
					return false;
				}

				// TODO: check color space
				// if current color space is linear
				// color property apply to shader need convert from srgb to linear
				val = val.linear;
				return NativeResult.Success ==
				       pav_LodMergedAvatarRenderMaterial_setVector4(nativeHandle, materialIndex, propertyID, ref val);
			}

			internal bool GetMergedFloat(int materialIndex, uint propertyID, ref float val)
			{
				if (!_Merged)
				{
					return false;
				}

				return NativeResult.Success ==
				       pav_LodMergedAvatarRenderMaterial_getFloat(nativeHandle, materialIndex, propertyID, ref val);
			}

			internal bool GetMergedColor(int materialIndex, uint propertyID, ref Color val, bool toGamma)
			{
				if (!_Merged)
				{
					return false;
				}

				// TODO: check color space
				// if current color space is linear
				// color property apply to shader need convert from srgb to linear
				if (NativeResult.Success ==
				    pav_LodMergedAvatarRenderMaterial_getVector4(nativeHandle, materialIndex, propertyID, ref val))
				{
					if (toGamma)
					{
						val.r = Mathf.LinearToGammaSpace(val.r);
						val.g = Mathf.LinearToGammaSpace(val.g);
						val.b = Mathf.LinearToGammaSpace(val.b);
					}

					return true;
				}

				return false;
			}

			#endregion


			#region Protected Methods
			
            //Derived class can override the method to release resources when the object will be destroyed.
            protected override void OnDestroy()
			{
				for (int i = 0; i < avatarRenderMaterialDef.texturePropertyCount; ++i)
				{
					ReferencedObject.ReleaseField(ref mat_TexturePropertyItems[i].value);
				}

				matPV_BumpMap = null;
				matPV_EmissionMap = null;
				//
				if (_RuntimeMaterial != null)
				{
					UnityEngine.Object.DestroyImmediate(_RuntimeMaterial);
					_RuntimeMaterial = null;
				}

				// let base method release native handler.
				base.OnDestroy();


				if (avatarRenderMaterialDef != null)
				{
					avatarRenderMaterialDef.Release();
					avatarRenderMaterialDef = null;
				}
			}
            
            //Load texture field.
            //@param isLinear whether texture is linear or srgb.
            //@return false if can not load the property.
            private bool LoadTexture(System.IntPtr renderMaterialHandle, uint propertyID, ref AvatarTexture avatarTex,
				bool isLinear = true)
			{
				//if(AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
				//{
				//    AvatarEnv.Log(DebugLogMask.AssetTrivial, string.Format("LoadTexture id:{0:D} frame:{1:D}", propertyID, Time.frameCount));
				//}
				//
				var ti = new TextureInfo();
				ti.version = 0;
				if (pav_AvatarRenderMaterial_GetTexture(renderMaterialHandle, propertyID, ref ti) ==
				    NativeResult.Success)
				{
					avatarTex = AvatarTexture.CreateAndRefTexture(ref ti, isLinear);
					return true;
				}

				return false;
			}

			private bool LoadInt(System.IntPtr renderMaterialHandle, uint propertyID, ref int val)
			{
				return NativeResult.Success ==
				       pav_AvatarRenderMaterial_GetInt(renderMaterialHandle, propertyID, ref val);
			}
			
            //Load float field.
            //@return false if can not load the property.
            private bool LoadFloat(System.IntPtr renderMaterialHandle, uint propertyID, ref float val)
			{
				return NativeResult.Success ==
				       pav_AvatarRenderMaterial_GetFloat(renderMaterialHandle, propertyID, ref val);
			}
            
            //Load float field.
            //@return false if can not load the property.
            private bool LoadColor(System.IntPtr renderMaterialHandle, uint propertyID, ref Color val)
			{
				return NativeResult.Success ==
				       pav_AvatarRenderMaterial_GetVector4(renderMaterialHandle, propertyID, ref val);
			}

			internal bool LoadVector(System.IntPtr renderMaterialHandle, uint propertyID, ref Vector4 val)
			{
				return NativeResult.Success ==
				       pav_AvatarRenderMaterial_GetVector4(renderMaterialHandle, propertyID, ref val);
			}

			#endregion


			#region Private Fields

			// runtimely instantiated material.
			private Material _RuntimeMaterial = null;

			// Whether is a merged material
			private bool _Merged = false;

			private AvatarLod _AvatarLod = null;

			// application configurated material configuration.
			private static PicoMaterialConfiguration _MaterialConfig = null;

			#endregion


			#region Private Methods

			
            //Get active material configuration. The configuration in AmazAvatarManager is the top priority.
            private static PicoMaterialConfiguration GetMaterialConfiguration()
			{
				if (_MaterialConfig != null)
				{
					return _MaterialConfig;
				}

				//
				if (PicoAvatarApp.instance != null)
				{
					_MaterialConfig = PicoAvatarApp.instance.renderSettings.materialConfiguration;
					if (_MaterialConfig != null)
					{
						return _MaterialConfig;
					}
				}

				// get default one.
				if (_MaterialConfig == null)
				{
					_MaterialConfig = ScriptableObject.CreateInstance<PicoMaterialConfiguration>();
				}

				return _MaterialConfig;
			}

			#endregion


			#region Material Property Values.

			private float matPV_ColorRegionBaked;
			private Vector4 matPV_BaseMap_ST;
			private AvatarTexture matPV_BumpMap;
			private AvatarTexture matPV_EmissionMap;

			#endregion


			#region Prepare Property ID

			private static bool _initialized = false;
			private static uint id_ShaderType;
			private static uint id_SceneBlendType;
			private static uint id_BaseMap_ST;
			private static uint id_ColorRegionBaked;

			// gets property ids.
			private void CheckInitialize()
			{
				if (!_initialized)
				{
					id_ShaderType = Pico.Avatar.Utility.AddNameToIDNameTable("_ShaderType");
					id_SceneBlendType = Pico.Avatar.Utility.AddNameToIDNameTable("_SceneBlendType");
					id_BaseMap_ST = Pico.Avatar.Utility.AddNameToIDNameTable("_BaseMap_ST");
					id_ColorRegionBaked = Pico.Avatar.Utility.AddNameToIDNameTable("_ColorRegionBaked");
				}

				_initialized = true;

				for (int idx = 0; idx < avatarRenderMaterialDef.floatPropertyCount; ++idx)
				{
					MaterialPropertyItem item = avatarRenderMaterialDef.floatPropertyItems[idx];
					mat_FloatPropertyItems.Add(new MaterialFloatPropertyItem(item));
				}

				for (int idx = 0; idx < avatarRenderMaterialDef.colorPropertyCount; ++idx)
				{
					MaterialPropertyItem item = avatarRenderMaterialDef.colorPropertyItems[idx];
					mat_ColorPropertyItems.Add(new MaterialColorPropertyItem(item));
				}

				for (int idx = 0; idx < avatarRenderMaterialDef.vectorPropertyCount; ++idx)
				{
					MaterialPropertyItem item = avatarRenderMaterialDef.vectorPropertyItems[idx];
					mat_VectorPropertyItems.Add(new MaterialVectorPropertyItem(item));
				}

				for (int idx = 0; idx < avatarRenderMaterialDef.texturePropertyCount; ++idx)
				{
					MaterialPropertyItem item = avatarRenderMaterialDef.texturePropertyItems[idx];
					mat_TexturePropertyItems.Add(new MaterialTexturePropertyItem(item));
				}
			}

			#endregion

			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[StructLayout(LayoutKind.Sequential, Pack = 8)]
			struct RenderStates
			{
				// version
				[MarshalAs(UnmanagedType.I1)] public byte version;

				// AvatarPBRWorkflow "_Workflow"; default: 1.0
				[MarshalAs(UnmanagedType.I1)] public byte workflow;

				// AvatarRenderFace "_ZWrite"; default 1.0
				[MarshalAs(UnmanagedType.I1)] public byte zWrite;

				// AvatarRenderFace "_ZTest"; default 1.0
				[MarshalAs(UnmanagedType.I1)] public byte zTest;

				// AvatarShaderType "_ShaderType"; default : 0.0
				[MarshalAs(UnmanagedType.I4)] public uint shaderType;

				// render queue; default : -1
				[MarshalAs(UnmanagedType.I4)] public int renderQueue;

				// AvatarSceneBlendType "_SceneBlendType"; default: 0.0
				[MarshalAs(UnmanagedType.I1)] public byte sceneBlendType;

				// AvatarRenderFace "_RenderFace"; default: 2.0
				[MarshalAs(UnmanagedType.I1)] public byte renderFace;

				// AvatarPBRSmoothnessSource "_PBRSmoothSource"; default: 0.0
				[MarshalAs(UnmanagedType.I1)] public byte smoothSource;

				[MarshalAs(UnmanagedType.I1)] public byte colorMask;

				// reserved
				[MarshalAs(UnmanagedType.I4)] uint reserved0;

				// reserved
				[MarshalAs(UnmanagedType.I4)] uint reserved1;

				// reserved
				[MarshalAs(UnmanagedType.I4)] uint reserved2;
			}

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetRenderStates(System.IntPtr nativeHandle,
				ref RenderStates renderStates);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetBool(System.IntPtr nativeHandle,
				uint propertyID, ref bool val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetFloat(System.IntPtr nativeHandle,
				uint propertyID, ref float val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetInt(System.IntPtr nativeHandle,
				uint propertyID, ref int val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetVector3(System.IntPtr nativeHandle,
				uint propertyID, ref Vector3 val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetVector4(System.IntPtr nativeHandle,
				uint propertyID, ref Vector4 val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetVector4(System.IntPtr nativeHandle,
				uint propertyID, ref Color val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMaterial_GetTexture(System.IntPtr nativeHandle,
				uint propertyID, ref TextureInfo val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_LodMergedAvatarRenderMaterial_getMergedCount(
				System.IntPtr nativeHandle, ref int materialCount);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_LodMergedAvatarRenderMaterial_setFloat(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref float val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_LodMergedAvatarRenderMaterial_setVector4(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref Color val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_LodMergedAvatarRenderMaterial_getFloat(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref float val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_LodMergedAvatarRenderMaterial_getVector4(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref Color val);

			#endregion
		}
	}
}