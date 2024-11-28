using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using System.Collections;
using System;

namespace Pico
{
	namespace Avatar
	{
        public class AvatarCustomMaterial : NativeObject
        {
            public string Guid;

            protected CommonMaterialPropertyData[] propertyInfos;
            
            // refactor: rename the function name.
            internal unsafe bool GetGuidFromNativeMaterial(System.IntPtr nativeHandle_)
            {
                if (nativeHandle != System.IntPtr.Zero)
                {
                    throw new System.Exception("BadProgram!");
                }
                
                NativeResult result;
                System.Text.StringBuilder guid = new System.Text.StringBuilder("", 128);
                result = pav_AvatarRenderMaterial_GetCustomMaterialGuid(nativeHandle_, guid);
                if (result != NativeResult.Success) 
                    return false;
                Guid = guid.ToString();
                return true;
            } 

            internal AvatarCustomMaterial(AvatarLod avatarLod)
            {
                _AvatarLod = avatarLod;
            }

            AvatarLod _AvatarLod;


            [StructLayout(LayoutKind.Sequential, Pack = 1)]
            public struct CommonMaterialPropertyInfo
            {
                public uint type;
                public uint id;
                public System.IntPtr name;
            };

            public struct CommonMaterialPropertyData
            {
                public uint type;
                public uint id;
                public string name;
            };
            
            const string PavDLLName = DllLoaderHelper.PavDLLName;
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
            private static extern NativeResult pav_AvatarRenderMaterial_GetCustomMaterialGuid(System.IntPtr nativeHandle, System.Text.StringBuilder guid);
        }
        /// <summary>
        /// /////////////////////
        /// </summary>

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
			Cutoff = 3,
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
			public MaterialFloatPropertyItem()
            {
            }

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
			public MaterialColorPropertyItem()
			{
			}

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
			public MaterialVectorPropertyItem()
            {
            }

			public MaterialVectorPropertyItem(MaterialPropertyItemRaw item) : base(item)
			{
			}

			public MaterialVectorPropertyItem(MaterialPropertyItem item) : base(item)
			{
			}

			public Vector4 value;
			public bool has_value;
			public bool isTetxureST;
		}

		public class MaterialTexturePropertyItem : MaterialPropertyItem
		{
			public MaterialTexturePropertyItem()
			{
			}

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
			public bool has_ST = false;
			public Vector4 value_ST;
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

			internal bool mat_shaderColorRegionBaked { get { return matPV_ColorRegionBaked > 0.5; } }

			// AvatarShaderType "_ShaderType"; default : 0.0
			public AvatarShaderType mat_ShaderType { get; private set; }
			
			// OfficialShaderTheme "_ShaderTheme"; default : 0.0
			public OfficialShaderTheme mat_ShaderTheme { get; private set; }

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

			public bool mat_alphaTest { get; private set; }

			#endregion

			#region Has Material properties

			public bool has_BaseMap_ST;
			public bool has_BumpMap;
			public bool has_ColorRegionBaked;

            #endregion

            #endregion


            #region Public Methods

            public static PicoAvatarRenderMaterial[] CreateRenderMaterials(System.IntPtr[] renderMaterialHandles,
                AvatarLod lod, bool merged)
            {
                PicoAvatarRenderMaterial[] materials = new PicoAvatarRenderMaterial[renderMaterialHandles.Length];
                bool success = true;
                for (int i = 0; i < materials.Length; ++i)
                {
                    materials[i] = new PicoAvatarRenderMaterial(merged, lod);
                    materials[i].Retain();

                    // try to load render material.
                    if (!materials[i].LoadPropertiesFromNativeMaterial(renderMaterialHandles[i], lod.lodLevel, lod.owner.owner.capabilities.allowEdit))
                    {
                        success = false;
                    }
                }

                if (!success)
                {
                    for (int i = 0; i < materials.Length; ++i)
                    {
                        materials[i]?.Release();
                        materials[i] = null;
                    }

                    materials = null;
                }

                return materials;
            }

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
				_RuntimeMaterial = _MaterialConfig.CreateRuntimeMaterialFromNativeMaterial(this, renderMesh.lodLevel);

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
				_RuntimeMaterial.EnableKeyword("FLIP_Y");

				//
				return _RuntimeMaterial;
			}

			internal Material GetRuntimeMaterial(AvatarLodLevel level)
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
                _RuntimeMaterial = _MaterialConfig.CreateRuntimeMaterialFromNativeMaterial(this, level);
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

				if (mat_alphaTest && mat_ShaderTheme == OfficialShaderTheme.PicoNPR)
				{
					material.EnableKeyword("_ALPHATEST_ON");
				}

				// if tooth, only accept main light.
				if (_MaterialConfig.enableMultiLights)
				{
					material.EnableKeyword("_ADDITIONAL_LIGHTS");
				}

// 				// set macros.
// 				if (mat_BumpMap != null)
// 				{
// 					material.EnableKeyword("_NORMALMAP");
// 					material.EnableKeyword("_METALLICSPECGLOSSMAP");
// 					material.DisableKeyword("_SPECULARHIGHLIGHTS_OFF");
// 					material.DisableKeyword("_SPECULAR_SETUP");
// 					material.DisableKeyword("_ENVIRONMENTREFLECTIONS_OFF");
// #if UNITY_EDITOR
// 					material.SetFloat("_SpecularHighlights", 0.0f);
// 					material.SetFloat("_EnvironmentReflections", 1.0f);
// #endif
// 					//UnityEngine.Debug.Log("AvatarSDK metallic tex set.");
// 				}

				// if (mat_EmissionMap != null)
				// {
				// 	material.EnableKeyword("_EMISSION");
				// }

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
				// if (mat_ColorMask != 0x0f)
				// {
				// 	material.SetFloat("_ColorMask", (float)mat_ColorMask);
				// }

				// {
				// 	float cullValue = 0.0f; // close cull.
				// 	material.SetFloat("_Cull", cullValue);
				// }


				// if (!mat_ZWrite)
				// {
				// 	material.SetFloat("_ZWrite", (float)0.0f);
				// }

				// if (!mat_ZTest)
				// {
				// 	material.SetFloat("_ZTest", (float)0.0f);
				// }

				// if (PicoAvatarApp.instance != null && PicoAvatarApp.instance.renderSettings.enableRimProfile)
				// {
				// 	material.EnableKeyword("PAV_RIM_PROFILE");
				// }

                // PAV_COLOR_REGION_BAKED
                {
					// use gpu bake for realtime, so here is not setting
					
					if (mat_shaderColorRegionBaked)
                    {
#if UNITY_2021_1_OR_NEWER
						if (material.HasFloat(s_ColorShift))
#else
						if (material.HasProperty(s_ColorShift))
#endif
						{
                            material.SetFloat(s_ColorShift, 0.0f);
                            material.DisableKeyword("_COLOR_SHIFT");
                        }
                        material.EnableKeyword("PAV_COLOR_REGION_BAKED");
                    }
					else
                    {
#if UNITY_2021_1_OR_NEWER
						if (material.HasFloat(s_ColorShift))
#else
                        if (material.HasProperty(s_ColorShift))
#endif
                        {
                            if (material.GetFloat(s_ColorShift) == 1.0f)
                            {
                                material.EnableKeyword("_COLOR_SHIFT");
                            }
                            else
                            {
                                material.DisableKeyword("_COLOR_SHIFT");
                            }
                        }
					}
				}
			}

			//Load material data from native render material.
			//@param nativeHandle_ native AvatarRenderMaterial. Reference count has been added by invoker.
			internal bool LoadPropertiesFromNativeMaterial(System.IntPtr nativeHandle_, AvatarLodLevel lodLevel, bool allowEdit)
			{
				if (nativeHandle == System.IntPtr.Zero)
				{
					// throw new System.Exception("BadProgram!");
                    // no need to add reference count since invoker has added.
                    SetNativeHandle(nativeHandle_, false);
				}


				// TODO(tianshengcai): to promote performance?
				// config
				{
					var renderState = new RenderStates();
					renderState.version = 0;
					pav_AvatarRenderMaterial_GetRenderStates(nativeHandle_, ref renderState);

					//
					mat_ShaderType = (AvatarShaderType)renderState.shaderType;
					mat_ShaderTheme = (OfficialShaderTheme)renderState.shaderTheme;
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
				mat_FloatPropertyItems = new List<MaterialFloatPropertyItem>();
				mat_ColorPropertyItems = new List<MaterialColorPropertyItem>();
				mat_VectorPropertyItems = new List<MaterialVectorPropertyItem>();
				mat_TexturePropertyItems = new List<MaterialTexturePropertyItem>();

				if (AvatarManager.IsAvatarLitShader(mat_ShaderTheme))
				{
					avatarRenderMaterialDef = null;

                    if (!_initialized)
                    {
                        // id_ShaderType = Pico.Avatar.Utility.AddNameToIDNameTable("_ShaderType");
                        // id_SceneBlendType = Pico.Avatar.Utility.AddNameToIDNameTable("_SceneBlendType");
                        id_BaseMap_ST = Pico.Avatar.Utility.AddNameToIDNameTable("_BaseMap_ST");
                        id_ColorRegionBaked = Pico.Avatar.Utility.AddNameToIDNameTable("_ColorRegionBaked");
                    }

                    _initialized = true;

					var material = _MaterialConfig.FetchMaterial(mat_ShaderTheme);
					var shader = material.shader;

					if (shader != null)
					{
						var propertyCount = shader.GetPropertyCount();
						for (int i = 0; i < propertyCount; ++i)
						{
							var name = shader.GetPropertyName(i);
							var type = shader.GetPropertyType(i);
							if (type == UnityEngine.Rendering.ShaderPropertyType.Float
								|| type == UnityEngine.Rendering.ShaderPropertyType.Range
#if UNITY_2021_1_OR_NEWER
								|| type == UnityEngine.Rendering.ShaderPropertyType.Int
#endif
							)
							{
								var item = new MaterialFloatPropertyItem();
								item.id = Pico.Avatar.Utility.AddNameToIDNameTable(name);
                                item.unityName = name;
								item.unityID = Shader.PropertyToID(item.unityName);
								item.semantic = (uint)AvatarFloatSemantic.None;
								item.has_value = LoadFloat(nativeHandle, item.id, ref item.value);
		
								//Debug.Log($"Loading float from shader name {name}, {item.has_value} {item.value}");

								mat_FloatPropertyItems.Add(item);
							}
							else if (type == UnityEngine.Rendering.ShaderPropertyType.Color)
                            {
								var item = new MaterialColorPropertyItem();
								item.id = Pico.Avatar.Utility.AddNameToIDNameTable(name);
								item.unityName = name;
								item.unityID = Shader.PropertyToID(item.unityName);
								item.semantic = (uint)AvatarColorSemantic.None;
								item.has_value = LoadColor(nativeHandle, item.id, ref item.value);
								//Debug.Log($"Loading color from shader name {name}, {item.has_value} {item.value}");

								mat_ColorPropertyItems.Add(item);
							}
							else if (type == UnityEngine.Rendering.ShaderPropertyType.Vector)
							{
								var item = new MaterialVectorPropertyItem();
								item.id = Pico.Avatar.Utility.AddNameToIDNameTable(name);
								item.unityName = name;
								item.unityID = Shader.PropertyToID(item.unityName);
								item.semantic = (uint)AvatarVectorSemantic.None;
								item.has_value = LoadVector(nativeHandle, item.id, ref item.value);

								//Debug.Log($" Loading vector from shader name {name}, {item.has_value} {item.value}");

								mat_VectorPropertyItems.Add(item);
							}
							else if (type == UnityEngine.Rendering.ShaderPropertyType.Texture)
                            {
								var item = new MaterialTexturePropertyItem();
								item.id = Pico.Avatar.Utility.AddNameToIDNameTable(name);
								item.unityName = name;
								item.unityID = Shader.PropertyToID(item.unityName);
								item.unityArrayName = $"{item.unityName}Array";
								item.unityArrayID = Shader.PropertyToID(item.unityArrayName);
								item.semantic = (uint)AvatarTextureSemantic.None;
								item.has_value = LoadTexture(nativeHandle, item.id, ref item.value, allowEdit, name != "_BaseMap");

								item.value_ST = new Vector4(1, 1, 0, 0);
								var id_ST = Pico.Avatar.Utility.AddNameToIDNameTable(name + "_ST");
								item.has_ST = LoadVector(nativeHandle, id_ST, ref item.value_ST);

								//Debug.Log($" Loading texture from shader name {name}, {item.has_value} {item.value} {item.has_ST} {item.value_ST}");

								//has_BumpMap set to materialNeedTangent and PAV_NO_TANGENTS is discarded in new shader

								mat_TexturePropertyItems.Add(item);
							}
						}
					}
					else
					{
						Debug.LogError("PicoAvatarRenderMaterial Ceating Material Error: cannot find Shader on configuration！");
						return false;
					}

					//legacy states
                    {
						//TODO //TODO: move into js
						{
							has_ColorRegionBaked = LoadFloat(nativeHandle, id_ColorRegionBaked, ref matPV_ColorRegionBaked);
							has_BaseMap_ST = LoadVector(nativeHandle, id_BaseMap_ST, ref matPV_BaseMap_ST);
						}

						LoadCustomVecs();

						//TODO set alpah test state using dictionary
						mat_alphaTest = false;
						foreach (var item in mat_FloatPropertyItems)
                        {
							if (item.unityName == "_CutOff" && item.has_value && item.value > 0.0f)
							{
								mat_alphaTest = true;
								break;
							}
                        }
					}
				}
				else
				{
					avatarRenderMaterialDef = new AvatarRenderMaterialDef(mat_ShaderType, mat_RenderPipelineType);
					avatarRenderMaterialDef.Retain();

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
							case AvatarFloatSemantic.Cutoff:
								floatPropertyItem.has_value = true;
								if (floatPropertyItem.value > 0.0)
								{
									mat_alphaTest = true;
								}
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
				}

				return true;
			}

			// When  uniforms in native material changed, should synchronized to Unity material.
			internal void UpdateDirtyMaterialUniforms()
			{
                for (int idx = 0; idx < mat_FloatPropertyItems.Count; ++idx)
                {
                    mat_FloatPropertyItems[idx].has_value = LoadFloat(nativeHandle, mat_FloatPropertyItems[idx].id,
                        ref mat_FloatPropertyItems[idx].value);
					//Debug.Log($" Loading float from shader name {mat_FloatPropertyItems[idx].unityName} {mat_FloatPropertyItems[idx].value}");
				}


                for (int idx = 0; idx < mat_ColorPropertyItems.Count; ++idx)
                {
                    mat_ColorPropertyItems[idx].has_value = LoadColor(nativeHandle, mat_ColorPropertyItems[idx].id,
                        ref mat_ColorPropertyItems[idx].value);
					//Debug.Log($" Loading color from shader name {mat_ColorPropertyItems[idx].unityName} {mat_ColorPropertyItems[idx].value}");
				}

                for (int idx = 0; idx < mat_VectorPropertyItems.Count; ++idx)
                {
                    mat_VectorPropertyItems[idx].has_value = LoadVector(nativeHandle, mat_VectorPropertyItems[idx].id,
                        ref mat_VectorPropertyItems[idx].value);
                    //Debug.Log($" Loading vector from shader name {mat_VectorPropertyItems[idx].unityName} {mat_VectorPropertyItems[idx].value}");
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

			internal bool LoadTexturesFromNativeMaterial(AvatarLodLevel lodLevel, bool allowEdit)
			{
				if (AvatarManager.IsAvatarLitShader(mat_ShaderTheme)) return true;

				// clear map flags.
				has_BumpMap = false;

				foreach (MaterialTexturePropertyItem texturePropertyItem in mat_TexturePropertyItems)
				{
					switch ((AvatarTextureSemantic)texturePropertyItem.semantic)
					{
						case AvatarTextureSemantic.BaseMap:
							texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
								ref texturePropertyItem.value, allowEdit, false);
							break;
						case AvatarTextureSemantic.ToonShadowMap:
							if (_MaterialConfig.need_ToonShadowMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit, false);
							}

							break;
						case AvatarTextureSemantic.MetallicGlossMap:
							if ((lodLevel < (AvatarLodLevel)3 || _MaterialConfig.isLod3EnablePBR) &&
							    _MaterialConfig.need_MetallicGlossMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit, false);
							}

							break;
						case AvatarTextureSemantic.SpecGlossMap:
							if ((lodLevel < (AvatarLodLevel)3 || _MaterialConfig.isLod3EnablePBR) &&
							    _MaterialConfig.need_SpecGlossMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit, false);
							}

							break;
						case AvatarTextureSemantic.BumpMap:
							if (_MaterialConfig.need_BumpMap &&
							    (lodLevel < AvatarLodLevel.Lod2 || _MaterialConfig.isLod2NeedBumpMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
								matPV_BumpMap = texturePropertyItem.value;
								has_BumpMap = texturePropertyItem.has_value;
							}

							break;
						case AvatarTextureSemantic.ParallaxMap:
							if (_MaterialConfig.need_BumpMap && _MaterialConfig.need_ParallaxMap &&
							    (lodLevel < AvatarLodLevel.Lod2 || _MaterialConfig.isLod2NeedBumpMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.DetailNormalMap:
							if (_MaterialConfig.need_BumpMap && _MaterialConfig.need_DetailNormalMap &&
							    (lodLevel < AvatarLodLevel.Lod2 || _MaterialConfig.isLod2NeedBumpMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.EmissionMap:
							if (_MaterialConfig.need_EmissionMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
								matPV_EmissionMap = texturePropertyItem.value;
							}

							break;
						case AvatarTextureSemantic.DetailMask:
							if (_MaterialConfig.need_DetailMask)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.DetailAlbedoMap:
							if (_MaterialConfig.need_DetailAlbedoMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.ColorRegionMap:
							if (_MaterialConfig.need_ColorRegionMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.SecondBaseMap:
							if (_MaterialConfig.need_SecondMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.SecondBumpMap:
							if (_MaterialConfig.need_SecondMap && _MaterialConfig.need_BumpMap)
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
							}

							break;
						case AvatarTextureSemantic.SecondMetallicSpecGlossMap:
							if (_MaterialConfig.need_SecondMap && (_MaterialConfig.need_MetallicGlossMap ||
							                                       _MaterialConfig.need_SpecGlossMap))
							{
								texturePropertyItem.has_value = LoadTexture(nativeHandle, texturePropertyItem.id,
									ref texturePropertyItem.value, allowEdit);
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
				pav_AvatarMergedRenderMaterial_getMergedCount(nativeHandle, ref materialCount);
				return materialCount;
			}

			internal bool SetMergedFloat(int materialIndex, uint propertyID, float val)
			{
				if (!_Merged)
				{
					return false;
				}

				return NativeResult.Success ==
				       pav_AvatarMergedRenderMaterial_setFloat(nativeHandle, materialIndex, propertyID, ref val);
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
				       pav_AvatarMergedRenderMaterial_setVector4(nativeHandle, materialIndex, propertyID, ref val);
			}

			internal bool GetMergedFloat(int materialIndex, uint propertyID, ref float val)
			{
				if (!_Merged)
				{
					return false;
				}

				val = 0;
				return NativeResult.Success ==
				       pav_AvatarMergedRenderMaterial_getFloat(nativeHandle, materialIndex, propertyID, ref val);
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
				    pav_AvatarMergedRenderMaterial_getVector4(nativeHandle, materialIndex, propertyID, ref val))
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
				if (mat_TexturePropertyItems != null)
				{
                    foreach (MaterialTexturePropertyItem texturePropertyItem in mat_TexturePropertyItems)
                    {
                        ReferencedObject.ReleaseField(ref texturePropertyItem.value);
                    }
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
            private bool LoadTexture(System.IntPtr renderMaterialHandle, uint propertyID, ref AvatarTexture avatarTex, bool allowEdit,
				bool isLinear = true)
			{
				//if(AvatarEnv.NeedLog(DebugLogMask.AssetTrivial))
				//{
				//    AvatarEnv.Log(DebugLogMask.AssetTrivial, string.Format("LoadTexture id:{0:D} frame:{1:D}", propertyID, Time.frameCount));
				//}
				//
				var ti = new TextureInfo();
				ti.version = 0;
				// Used as md5 flag
				ti.reserveByte2 = 1;
				if (pav_AvatarRenderMaterial_GetTexture(renderMaterialHandle, propertyID, ref ti) ==
				    NativeResult.Success)
				{
					avatarTex = AvatarTexture.CreateAndRefTexture(ref ti, isLinear, allowEdit);
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

			// add by lingzhaojun
            internal bool LoadVector(System.IntPtr renderMaterialHandle, uint propertyID, ref Vector3 val)
            {
                return NativeResult.Success == pav_AvatarRenderMaterial_GetVector3(renderMaterialHandle, propertyID, ref val);
            }
            // add by lingzhaojun
            internal bool LoadVector(System.IntPtr renderMaterialHandle, uint propertyID, ref Vector2 val)
            {
                return NativeResult.Success == pav_AvatarRenderMaterial_GetVector2(renderMaterialHandle, propertyID, ref val);
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

			protected static int s_ColorShift = Shader.PropertyToID("_ColorShift");

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

				// OfficialShaderTheme "_ShaderTheme"; default : 0.0
				[MarshalAs(UnmanagedType.I4)] public uint shaderTheme;

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
            private static extern NativeResult pav_AvatarRenderMaterial_GetVector2(System.IntPtr nativeHandle, uint propertyID, ref Vector2 val);

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

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_getMergedCount(System.IntPtr nativeHandle, 
				ref int materialCount);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_setFloat(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref float val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_setVector4(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref Color val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_getFloat(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref float val);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_getVector4(System.IntPtr nativeHandle,
				int materialIndex, uint propertyID, ref Color val);

			#endregion
		}

		public class PicoAvatarMergedRenderMaterial : PicoAvatarRenderMaterial
        {
			private GraphicsBuffer _mergedMtlBuffer;
			private int _bakeTaskFinished = 0;
			private int _bakeTaskCount = 0;
			private List<(BatchingGPUTask, PicoPrimitiveRenderMesh)> _bakeTasks = new List<(BatchingGPUTask, PicoPrimitiveRenderMesh)>();

            private bool LoadTextures(AvatarLod lod)
			{
                if (!LoadTexturesFromNativeMaterial(lod.lodLevel, false))
                {
                    return false;
                }

				return true;
			}

			private void UpdateUnityMaterialWithAvatarRenderMaterial(AvatarLod lod, bool meshHasTangent, Material unityMtl)
			{
				UpdateMaterial(unityMtl);

				// whether disable shadow casting.
				if (PicoAvatarApp.instance.renderSettings.forceDisableReceiveShadow)
				{
					unityMtl.DisableKeyword("_MAIN_LIGHT_SHADOWS");
					unityMtl.DisableKeyword("_MAIN_LIGHT_SHADOWS_CASCADE");
					unityMtl.DisableKeyword("SHADOWS_SHADOWMASK");
					//
					unityMtl.EnableKeyword("_RECEIVE_SHADOWS_OFF");
					unityMtl.SetFloat("_ReceiveShadows", 0.0f);
				}

				//
				if (lod.owner.owner.avatarEffectKind != AvatarEffectKind.None)
				{
					if (lod.owner.owner.avatarEffectKind == AvatarEffectKind.SimpleOutline)
					{
						unityMtl.EnableKeyword("PAV_AVATAR_LOD_OUTLINE");
						//
						{
							unityMtl.SetFloat("_Surface", (float)PicoAvatarRenderMaterial.SurfaceType.Opaque);
							unityMtl.SetFloat("_ColorMask", (float)0.0);
						}
					}
				}

				// whether need tangent.
				bool materialNeedTangent = has_BumpMap;
				if (!unityMtl.HasProperty(PicoAvatarApp.instance.renderSettings.materialConfiguration.unityID_BumpMap))
				{
					materialNeedTangent = false;
				}

				if (lod.lodLevel >= AvatarLodLevel.Lod2)
				{
					unityMtl.SetFloat("_BaseColorAmplify", 0.8f);
				}
#if DEBUG
				if (false)
				{
					var keywords = unityMtl.shaderKeywords;
					var sb = new System.Text.StringBuilder();
					sb.Append("material ");
					sb.Append(unityMtl.shader.name);
					sb.Append(" keywords:");
					foreach (var x in keywords)
					{
						sb.Append(x);
						sb.Append("|");
					}

					AvatarEnv.Log(DebugLogMask.GeneralInfo, sb.ToString());
				}
#endif
			}

			private Material CreateUnityMaterialFromAvatarRenderMaterial(AvatarLod lod, bool meshHasTangent)
			{
                if (!LoadTextures(lod))
                    return null;

                Material unityMtl = GetRuntimeMaterial(lod.lodLevel);
                if (unityMtl != null)
                {
                    {
                        unityMtl.SetFloat(s_ColorShift, 0.0f);
                        unityMtl.DisableKeyword("_COLOR_SHIFT");
                    }
                    unityMtl.EnableKeyword("PAV_COLOR_REGION_BAKED");
					unityMtl.EnableKeyword("_ENABLE_STATIC_MESH_BATCHING");


					uint strideCount = 0, mergedCount = 0, mergedTextureSize = 0;
					pav_AvatarMergedRenderMaterial_GetMaterialInfo(nativeHandle, ref strideCount, ref mergedCount, ref mergedTextureSize);
					// UnityEngine.Debug.Log(string.Format("pav: CreateUnityMaterial Merged primitive: {0}, {1}", mergedCount, strideCount));

					var bufferData = new NativeArray<float>((int)(strideCount * mergedCount), Allocator.Temp);
					System.IntPtr bufferDataPointer;
					unsafe
					{
						bufferDataPointer = (System.IntPtr)bufferData.GetUnsafePtr();
					}
					pav_AvatarMergedRenderMaterial_GetMaterialInfoData(nativeHandle, bufferDataPointer);
                    _mergedMtlBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, (int)mergedCount, (int)(strideCount * sizeof(float)));
					_mergedMtlBuffer.SetData(bufferData);
					unityMtl.SetBuffer("_MtlData", _mergedMtlBuffer);
					bufferData.Dispose();
                    UpdateUnityMaterialWithAvatarRenderMaterial(lod, meshHasTangent, unityMtl);
                }
                else
                {
                    AvatarEnv.Log(DebugLogMask.GeneralError, "Failed to get runtime material.");
                    return null;
                }
				return unityMtl;
			}

			internal void TryUpdateMergedMaterialInfo(AvatarLod lod)
			{
				bool isNeedUpdate = false;
				if (pav_AvatarMergedRenderMaterial_IsMaterialInfoTextureDirty(nativeHandle, ref isNeedUpdate) != NativeResult.Success)
					return;
				if (isNeedUpdate && _mergedMtlBuffer != null)
				{
					uint strideCount = 0, mergedCount = 0, mergedTextureSize = 0;
					pav_AvatarMergedRenderMaterial_GetMaterialInfo(nativeHandle, ref strideCount, ref mergedCount, ref mergedTextureSize);
					// UnityEngine.Debug.Log(string.Format("pav: CreateUnityMaterial Merged primitive: {0}, {1}", mergedCount, strideCount));

                    var bufferData = new NativeArray<float>((int)(mergedCount * strideCount), Allocator.Temp);
                    System.IntPtr bufferDataPointer;
                    unsafe
                    {
                        bufferDataPointer = (System.IntPtr)bufferData.GetUnsafePtr();
                    }
                    pav_AvatarMergedRenderMaterial_GetMaterialInfoData(nativeHandle, bufferDataPointer);
                    _mergedMtlBuffer.SetData(bufferData);
					bufferData.Dispose();
                }
			}

			public void FillRuntimeMaterial(AvatarLod lod, Material material)
			{
                material.EnableKeyword("_ENABLE_STATIC_MESH_BATCHING");
                material.SetBuffer("_MtlData", _mergedMtlBuffer);
				bool meshHasTangent = lod.mergedRenderMesh.skinnedMeshRenderer.sharedMesh.tangents.Length > 0;
                UpdateUnityMaterialWithAvatarRenderMaterial(lod, meshHasTangent, material);
            }

            internal PicoAvatarMergedRenderMaterial(bool merged, AvatarLod avatarLod) : base(merged, avatarLod)
			{

			}

            protected override void OnDestroy()
            {
				_mergedMtlBuffer.Release();
				ClearBakeTasks();
                // Added ref count of nativeHandle when calling pav_AvatarLod_GetMergedRenderMaterial is released in base class
                base.OnDestroy();
            }

            public Material Build(System.IntPtr mtlNativeHandle, AvatarLod lod, bool hasTangent)
			{
				if (!LoadPropertiesFromNativeMaterial(mtlNativeHandle, lod.lodLevel, false))
					return null;

				foreach (var item in mat_TexturePropertyItems)
				{
					if (item.unityName == "_ColorRegionMap" && item.value != null && item.value.runtimeTexture != null)
					{
						ReferencedObject.ReleaseField(ref item.value);
						item.has_value = false;
						break;
					}
				}

				return CreateUnityMaterialFromAvatarRenderMaterial(lod, hasTangent);
			}

			internal void ConditionalDispatchGPUTasks(AvatarLod lod, Dictionary<uint, AvatarPrimitive> primitives, System.IntPtr nativeMergedMtl)
			{
				ClearBakeTasks();
				if (!lod.owner.owner.capabilities.allowEdit)
				{
					SetNativeHandle(nativeMergedMtl, false);
					_bakeTaskFinished = 0;
					var materialConfig = PicoAvatarApp.instance.renderSettings.materialConfiguration;
					foreach (var primitive in primitives.Values)
					{
						if (primitive != null && primitive.isMergedToAvatarLod)
						{
							var renderMtls = primitive.GetRenderMaterialHandles();
							if (renderMtls.Length == 0)
								continue;
							var tempMesh = new PicoPrimitiveRenderMesh();
							tempMesh.AttachPrimitive(primitive);
							if (tempMesh.BuildOfficialMaterialsFromNative(renderMtls, lod.lodLevel, false, false))
							{
								var runtimeMtls = tempMesh.GetOfficialRuntimeMaterial(tempMesh.officialRenderMaterials);
								for (int n = 0; n < runtimeMtls.Length; ++n)
								{
									var officialRenderMaterial = tempMesh.officialRenderMaterials[n];
									var material = runtimeMtls[n];
									if (officialRenderMaterial.mat_shaderColorRegionBaked)
										continue;

									// Create gpu task
									var baseMap = material.GetTexture("_BaseMap");
									if (baseMap == null)
										continue;
									if (baseMap.dimension != TextureDimension.Tex2D)
										continue;
									_bakeTasks.Add((new BatchingGPUTask(
										baseMap,
                                        material.GetTexture("_ColorRegionMap"),
                                        material.GetVector("_ColorRegion1"),
                                        material.GetVector("_ColorRegion2"),
                                        material.GetVector("_ColorRegion3"),
                                        material.GetVector("_ColorRegion4"),
										material.GetFloat("_UsingAlbedoHue"),
										materialConfig.astcEncodeShader,
										materialConfig.blendTextureShader,
										this,
										officialRenderMaterial.nativeHandle), tempMesh));
									// Increase total task count
									++_bakeTaskCount;
								}
							}
						}
					}
				}

				// Dispatch all tasks if any
				if (_bakeTaskCount > 0)
				{
                    foreach (var task in _bakeTasks)
					{
                        task.Item1.Execute();
					}
				}
			}

			internal void BakeTaskFinished()
			{
				_bakeTaskFinished++;
			}

			internal bool CheckAllBaskTasksFinished()
			{
				return _bakeTaskFinished == _bakeTaskCount;
			}

			private void ClearBakeTasks()
			{
				if (_bakeTasks.Count > 0)
				{
                    foreach (var task in _bakeTasks)
                    {
                        task.Item1.Dispose();
                        task.Item2.Destroy();
						Component.Destroy(task.Item2);
                    }
                    _bakeTaskFinished = 0;
                    _bakeTaskCount = 0;
                    _bakeTasks.Clear();
				}
			}

			internal void OnGpuTaskComplete(bool succeed, System.IntPtr nativeMtlHandle, Texture2D baseMap)
			{
				if (succeed)
				{
					unsafe
					{
						// Write buffer back to C++ texture
						var buffer = baseMap.GetRawTextureData<byte>();
						pav_AvatarMergedRenderMaterial_UpdateBakedTextureData(nativeMtlHandle, (System.IntPtr)buffer.GetUnsafeReadOnlyPtr<byte>(), (uint)buffer.Length, baseMap.width, baseMap.height, baseMap.mipmapCount);
					}
				}

				BakeTaskFinished();
            }

			internal bool ConditionalContinueMerging()
			{
				if (CheckAllBaskTasksFinished())
				{
					ClearBakeTasks();
					return true;
				}
				else { return false; }
			}

			const string PavDLLName = DllLoaderHelper.PavDLLName;
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarMergedRenderMaterial_GetMaterialInfo(System.IntPtr nativeHandle, ref uint stride, ref uint count, ref uint mergedTextureSize);
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_GetMaterialInfoData(System.IntPtr nativeHandle, System.IntPtr val);
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarMergedRenderMaterial_IsMaterialInfoTextureDirty(System.IntPtr nativeHandle, ref bool val);
            [DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarMergedRenderMaterial_UpdateBakedTextureData(System.IntPtr nativeHandle, System.IntPtr buffer, uint size, int width, int height, int mips);
        }
	}
}