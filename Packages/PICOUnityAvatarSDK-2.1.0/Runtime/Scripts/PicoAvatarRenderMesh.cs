//#define PAV_MATERIAL_DATA_TEXTURE

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
// using UnityEditor.Graphs;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR;

namespace Pico
{
	namespace Avatar
	{
		/// <summary>
		/// AvatarRenderMesh extracts mesh data from native library and create Unity MeshRenderer.
		/// </summary>
		public class PicoAvatarRenderMesh : MonoBehaviour
		{
			#region Public Properties

			// Unity mesh renderer used to display mesh.
			public MeshRenderer meshRenderer
			{
				get => _staticMeshRenderer;
			}

			// Unity skin mesh renderer used to display mesh.
			public SkinnedMeshRenderer skinMeshRenderer
			{
				get => _skinMeshRenderer;
			}

			//
			public delegate void OnMaterialsUpdate(PicoAvatarRenderMesh renderMesh);

			public OnMaterialsUpdate onMaterialsUpdate = null;

			//
			public delegate void OnMeshUpdate(PicoAvatarRenderMesh renderMesh);

			public OnMeshUpdate onMeshUpdate = null;

			// whether the object has been destroyed.
			public bool isDestroyed
			{
				get => _destroyed;
			}

			// has tangent. MaterialConfiguration configurate the property.
			public bool hasTangent
			{
				get => _HasTangent;
			}

			public bool materialNeedTangent
			{
				get => _materialNeedTangent;
			}

			// cached Lod level from AvatarLod/AvatarPrimitive.
			public AvatarLodLevel lodLevel
			{
				get => _CachedLodLevel;
			}

			// render material.
			public PicoAvatarRenderMaterial[] officialRenderMaterials { get; protected set; }
			public AvatarCustomMaterial[] customRenderMaterials { get; protected set; }

			// whether show as outline.
			public AvatarEffectKind avatarEffectKind { get; protected set; } = AvatarEffectKind.None;

			// whether data is ready.
			public bool isRenderDataReady
			{
				get => _isRenderDataReady;
			}

			// whether mesh data is ready. Means all gpu buffer are ready.
			public bool isRenderMeshDataReady
			{
				get => _isRenderMeshDataReady;
			}

			// whether all runtime material data is ready. means all textures are created.
			public bool isRenderMaterialDataReady
			{
				get => _isRenderMaterialDataReady;
			}

			// shared mesh buffer. holds mesh + morph + joints
			public AvatarMeshBuffer avatarMeshBuffer
			{
				get => _avatarMeshBuffer;
			}

			// 
			public float[] blendshapeWeights
			{
				get => _blendshapeWeights;
			}

			// whether need updated each frame.
			public bool needUpdateSimulation { get; private set; } = false;

			// native render mesh handler.
			internal System.IntPtr nativeRenderMeshHandler
			{
				get => _nativeRenderMeshHandler;
			}

			// avatar skeleton the mesh used.
			public AvatarSkeleton avatarSkeleton { get; private set; }

			#endregion


			#region Public Methods

			internal PicoAvatarRenderMesh()
			{
				//
				if (PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.IncreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarRenderMesh);
				}
			}
			
            //Manually destroy
            internal virtual void Destroy()
			{
				// avoid duplicated decrease.
				if (!_destroyed && PicoAvatarStats.instance != null)
				{
					PicoAvatarStats.instance.DecreaseInstanceCount(PicoAvatarStats.InstanceType.AvatarRenderMesh);
				}

				_destroyed = true;
				//
				_isRenderDataReady = false;
				_isRenderMeshDataReady = false;

				//
				if (_staticMeshFilter != null)
				{
					_staticMeshFilter.sharedMesh = null;
					_staticMeshFilter = null;
				}

				if (_staticMeshRenderer)
				{
					_staticMeshRenderer.sharedMaterial = null;
					_staticMeshRenderer = null;
				}

				if (_skinMeshRenderer != null)
				{
					_skinMeshRenderer.sharedMesh = null;
					_skinMeshRenderer.sharedMaterial = null;
					_skinMeshRenderer = null;
				}

				if (_bakeColorRegionEntities != null)
				{
					foreach (var bcrEntity in _bakeColorRegionEntities)
					{
						bcrEntity.ReleaseManual();
					}
					_bakeColorRegionEntities.Clear();
					_bakeColorRegionEntities = null;
				}

				// release render material.
				DestroyAvatarRenderMaterials();

				//
				ReferencedObject.ReleaseField(ref _avatarMeshBuffer);

				// dispose NativeArrary for _DynamicBuffer.
				if (_DynamicCpuData.IsCreated)
				{
					_DynamicCpuData.Dispose();
				}

				// not a reference counted object.
				NativeObject.ReleaseNative(ref _nativeRenderMeshHandler);

				// clear the field at last.
				_materialConfiguration = null;
			}

			// Unity framework invoke the method when scene object destroyed. 
			protected virtual void OnDestroy()
			{
				Destroy();
			}
			
            //Update Unity render data from native simulation data.
            //Derived class SHOULD override the method to do actual update.
            internal virtual void UpdateSimulationRenderData()
			{
			}
            
            //Destroy AvatarRenderMaterials created from native AvatarRenderMaterial.
            private void DestroyAvatarRenderMaterials()
			{
				if (officialRenderMaterials != null)
				{
					for (int i = 0; i < officialRenderMaterials.Length; ++i)
					{
						officialRenderMaterials[i]?.Release();
						officialRenderMaterials[i] = null;
					}
				}

				officialRenderMaterials = null;
				
				if (customRenderMaterials != null)
				{
					for (int i = 0; i < customRenderMaterials.Length; ++i)
					{
						customRenderMaterials[i]?.Release();
						customRenderMaterials[i] = null;
					}
				}

				customRenderMaterials = null;
			}
			
			internal Material[] GetOfficialRuntimeMaterial(PicoAvatarRenderMaterial[] mats)
			{
				var runtimeMaterials = new Material[mats.Length];
				for (int i = 0; i < mats.Length; ++i)
				{
					//
					var runtimeMaterial = mats[i].GetRuntimeMaterial(this);
					//
					if (runtimeMaterial != null)
					{
						FillRuntimeMaterialWithAvatarRenderMaterial(mats[i], runtimeMaterial);
						runtimeMaterials[i] = runtimeMaterial;
					}
					else
					{
						AvatarEnv.Log(DebugLogMask.GeneralError, "Failed to get runtime material.");
						return null;
					}
				}
				return runtimeMaterials;
			}
			
            protected Material[] GetCustomRuntimeMaterial(AvatarCustomMaterial[] mats)
            {
	            var customMaterialDB = PicoAvatarApp.instance.renderSettings.customMaterialDatabase;
	            if (!customMaterialDB)
	            {
		            AvatarEnv.Log(DebugLogMask.GeneralError, "PicoAvatarApp.instance.renderSettings.customMaterialDatabase is null.");
		            return null;
	            }
	            var runtimeCustomMaterials = new Material[mats.Length];
	            for (int i = 0; i < mats.Length; ++i)
	            {
		            if(customMaterialDB.Lookup(mats[i].Guid, out Material material))
		            {
			            if (material != null)
			            {
				            runtimeCustomMaterials[i] = material;
			            }
			            else
			            {
				            AvatarEnv.Log(DebugLogMask.GeneralError, "Failed to though guid to get runtime custom material from customMaterialDB.");
				            AvatarEnv.Log(DebugLogMask.GeneralError, string.Format("The current project is missing the required material (map key: {0}), If you want to preview it normally, please import it.",mats[i].Guid));
				            return null;
			            } 
		            }
		            else
		            {
			            AvatarEnv.Log(DebugLogMask.GeneralError, "Failed to though guid to get runtime custom material from customMaterialDB.");
			            AvatarEnv.Log(DebugLogMask.GeneralError, string.Format("The current project is missing the required material (map key: {0}), If you want to preview it normally, please import it.",mats[i].Guid));
			            return null;
		            }
	            }
	            return runtimeCustomMaterials;
            }
            
            //Fill properties and textures of runtime unity material with AvatarRenderMaterial.
            protected void FillRuntimeMaterialWithAvatarRenderMaterial(PicoAvatarRenderMaterial mat, Material material)
			{
				mat.UpdateMaterial(material);

				////
				//material.SetBuffer("_staticBuffer", _staticMeshBuffer.runtimeBuffer);
				//material.SetBuffer("_dynamicBuffer", _DynamicBuffer);
				//material.SetBuffer("_outputBuffer", _OutPositionBuffer);
				//material.SetInt("_staticBufferOffset", (int) _staticMeshBuffer.morphAndSkinDataInfo.staticBufferOffset);
				//material.SetInt("_dynamicBufferOffset", (int) _staticMeshBuffer.morphAndSkinDataInfo.dynamicBufferOffset);
				//material.SetInt("_vertexIndexOffset", 0);

				//if (_MaterialDataTexture)
				//{
				//    material.SetTexture("_materialDataTexture", _MaterialDataTexture);
				//    material.SetVector("_materialDataTextureSize", new Vector4(_MaterialDataTexture.width, _MaterialDataTexture.height, 0, 0));
				//}
				//else if (_MaterialDataBuffer != null)
				//{
				//    material.SetBuffer("_materialDataBuffer", _MaterialDataBuffer);
				//}

				// whether disable shadow casting.
				if (PicoAvatarApp.instance.renderSettings.forceDisableReceiveShadow)
				{
					material.DisableKeyword("_MAIN_LIGHT_SHADOWS");
					material.DisableKeyword("_MAIN_LIGHT_SHADOWS_CASCADE");
					material.DisableKeyword("SHADOWS_SHADOWMASK");
					//
					material.EnableKeyword("_RECEIVE_SHADOWS_OFF");
					material.SetFloat("_ReceiveShadows", 0.0f);
				}

				//
				if (this.avatarEffectKind != AvatarEffectKind.None)
				{
					if (avatarEffectKind == AvatarEffectKind.SimpleOutline)
					{
						material.EnableKeyword("PAV_AVATAR_LOD_OUTLINE");
						//
						{
							material.SetFloat("_Surface", (float)PicoAvatarRenderMaterial.SurfaceType.Opaque);
							material.SetFloat("_ColorMask", (float)0.0);
						}
					}
				}

				// whether need tangent.
				this._materialNeedTangent = mat.has_BumpMap;
				//
				if (!material.HasProperty(PicoAvatarApp.instance.renderSettings.materialConfiguration.unityID_BumpMap))
				{
					this._materialNeedTangent = false;
				}

				if (this.lodLevel >= AvatarLodLevel.Lod2)
				{
					material.SetFloat("_BaseColorAmplify", 0.8f);
				}
#if DEBUG
				if (false)
				{
					var keywords = material.shaderKeywords;
					var sb = new System.Text.StringBuilder();
					sb.Append("material ");
					sb.Append(material.shader.name);
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

			//Fill a runtime unity material with AvatarRenderMaterial. Usually used to update material runtimly and manually by avatar sdk developer.
			public void FillRuntimeMaterial(Material mat, int materialIndex = 0)
			{
				if (officialRenderMaterials != null && (uint)materialIndex <= (uint)officialRenderMaterials.Length)
				{
					FillRuntimeMaterialWithAvatarRenderMaterial(officialRenderMaterials[materialIndex], mat);
				}
			}

			// When shader changed outside, should update material properties.
			public void OnShaderChanged()
			{
				if ((meshRenderer == null && skinMeshRenderer == null) || officialRenderMaterials == null || !isRenderDataReady)
				{
					return;
				}

				var sharedMaterials = skinMeshRenderer != null
					? skinMeshRenderer.sharedMaterials
					: meshRenderer.sharedMaterials;
				if (sharedMaterials != null && sharedMaterials.Length == officialRenderMaterials.Length)
				{
					for (int i = 0; i < sharedMaterials.Length; ++i)
					{
						FillRuntimeMaterialWithAvatarRenderMaterial(officialRenderMaterials[i], sharedMaterials[i]);
					}
				}

				if (skinMeshRenderer != null)
				{
					skinMeshRenderer.sharedMaterials = sharedMaterials;
				}
				else
				{
					meshRenderer.sharedMaterials = sharedMaterials;
				}
			}

			internal void SetAvatarEffectKind(AvatarEffectKind avatarEffectKind_)
			{
				if (this.avatarEffectKind != avatarEffectKind_ && (skinMeshRenderer != null || meshRenderer != null))
				{
					this.avatarEffectKind = avatarEffectKind_;

					var sharedMaterials = skinMeshRenderer != null
						? skinMeshRenderer.sharedMaterials
						: meshRenderer.sharedMaterials;
					if (sharedMaterials != null && sharedMaterials.Length == officialRenderMaterials.Length)
					{
						for (int i = 0; i < sharedMaterials.Length; ++i)
						{
							FillRuntimeMaterialWithAvatarRenderMaterial(officialRenderMaterials[i], sharedMaterials[i]);
						}
					}

					if (skinMeshRenderer != null)
					{
						skinMeshRenderer.sharedMaterials = sharedMaterials;
					}
					else
					{
						meshRenderer.sharedMaterials = sharedMaterials;
					}
				}
			}

			internal void Notify_AvatarSceneLightEnvChanged(PicoAvatarSceneLightEnv lightEnv)
			{
				if (officialRenderMaterials != null)
				{
					for (int i = 0; i < officialRenderMaterials.Length; ++i)
					{
						officialRenderMaterials[i]?.OnAvatarSceneLightEnvChanged(lightEnv);
					}
				}
			}

			internal bool BuildCustomMaterialsFromNative(System.IntPtr[] renderMaterialHandles)
			{
                customRenderMaterials = new AvatarCustomMaterial[renderMaterialHandles.Length];
                bool success = true;
                for (int i = 0; i < customRenderMaterials.Length; ++i)
                {
	                customRenderMaterials[i] = new AvatarCustomMaterial(_AvatarLod);
	                customRenderMaterials[i].Retain();
                    
                    // try to load render material.
                    if (!customRenderMaterials[i].GetGuidFromNativeMaterial(renderMaterialHandles[i]))
                    {
                        success = false;
                        break;
                    }
                }
                if (!success)
                {
                    for (int i = 0; i < customRenderMaterials.Length; ++i)
                    {
	                    customRenderMaterials[i]?.Release();
	                    customRenderMaterials[i] = null;
                    }
                    customRenderMaterials = null;
                    return false;
                }

                return true;
			}

            //Build material from native AvatarRenderMaterial and apply to the renderer.
            //renderMaterialHandles list of native AvatarRenderMaterial. Reference count has been added from invoker.
            internal bool BuildOfficialMaterialsFromNative(System.IntPtr[] renderMaterialHandles,
				AvatarLodLevel lodLevel, bool allowEdit, bool merged)
			{
				
				// create material with native material data. pass lifetime management of renderMaterialHandles to AvatarRenderMaterials.
				officialRenderMaterials = CreateRenderMaterials(renderMaterialHandles, lodLevel, allowEdit, merged);
				if (officialRenderMaterials == null)
				{
					Destroy();
					return false;
				}

				{
					for (int i = 0; i < officialRenderMaterials.Length; ++i)
					{
						var renderMaterial = officialRenderMaterials[i];
						if (renderMaterial != null)
						{
							if (!renderMaterial.LoadTexturesFromNativeMaterial(lodLevel, allowEdit))
							{
								Destroy();
								return false;
							}
						}
					}
				}
				return true;
			}

			#endregion


			#region Protected Fields

			internal AvatarLod _AvatarLod;

			#endregion


			#region Private Fields

			// whether has been destroyed.
			private bool _destroyed = false;

			// has tangent. MaterialConfiguration configurate the property.
			private bool _HasTangent = true;

			// cached lod level from AvatarLod/AvatarPrimitive
			public AvatarLodLevel _CachedLodLevel = 0;

			// The handle need be reference counted.
			private System.IntPtr _nativeRenderMeshHandler;

			// sets by derived class object.
			private bool _isRenderDataReady = false;
			private bool _isRenderMeshDataReady = false;
			private bool _isRenderMaterialDataReady = false;

			// shared mesh buffer. holds mesh + morph + joints
			private AvatarMeshBuffer _avatarMeshBuffer;

			// global shared material configuration.
			private PicoMaterialConfiguration _materialConfiguration;

			// cpu data.
			private NativeArray<int> _DynamicCpuData;

			//
			private MorphAndSkinSimulationGpuData _DynamicGpuDataInfo;

			// Unity static mesh filter.
			private MeshFilter _staticMeshFilter;

			// Unity mesh renderer to hold material.
			private MeshRenderer _staticMeshRenderer;

			// Unity Skinned MeshRenderer
			private SkinnedMeshRenderer _skinMeshRenderer;

			// whether material need tangent.
			private bool _materialNeedTangent = false;

			// last active morph channels, used to turn of active morph channels.
			private int[] _lastMorphChannelActiveFrames = null;

			// last frame that update morph and skin simulation
			private int _lastUpdateMorphAndSkinSimulationBufferFrame = 0;

			// whether cache the morphChannelWeights
			private Boolean _cacheMorphChannelWeights = false;

			// cached morphChannelWeights
			private float[] _blendshapeWeights;
			
			//
			protected bool _useCustomMaterial = false;
			protected List<BakeColorRegionEntity> _bakeColorRegionEntities = new List<BakeColorRegionEntity>();
			#endregion


			#region Protected Methods

			//Build mesh object.
			//renderMeshHandle native handle to AvatarRenderMesh. Reference count has been added.
			//allowGpuDataCompressd whether allow compress gpu data.
			protected bool CreateUnityRenderMesh(System.IntPtr renderMeshHandle, AvatarSkeleton avatarSkeleton_,
				AvatarLodLevel lodLevel_,
				System.IntPtr owner, bool allowGpuDataCompressd,
				AvatarShaderType mainShaderType = AvatarShaderType.Invalid,
				bool cacheMorphChannelWeights = false,
				bool depressSkin = false)
			{
				
				avatarSkeleton = avatarSkeleton_;

				// check
				if (_nativeRenderMeshHandler != System.IntPtr.Zero || _staticMeshFilter != null ||
				    _skinMeshRenderer != null)
				{
					NativeObject.ReleaseNative(ref renderMeshHandle);
					//
					AvatarEnv.Log(DebugLogMask.GeneralError, "PicoAvatarRenderMesh has been created!");
					return false;
				}

				// keep native render mesh.
				_nativeRenderMeshHandler = renderMeshHandle;

				_materialConfiguration = PicoAvatarRenderMaterial.materialConfiguration;
				//
				{
					// _staticMeshBuffer.Retain() has been invoked.
					_avatarMeshBuffer = AvatarMeshBuffer.CreateAndRefMeshBuffer(renderMeshHandle,
						_materialConfiguration.needTangent,
						allowGpuDataCompressd, depressSkin, this._useCustomMaterial);
					if (_avatarMeshBuffer == null)
					{
						return false;
					}
				}
				//
				_CachedLodLevel = lodLevel_;

				//
				_cacheMorphChannelWeights = cacheMorphChannelWeights;

				// whether has tangent data.
				_HasTangent = _avatarMeshBuffer.hasTangent;

				// add mesh filter.
				{
					if (avatarSkeleton != null && !Utility.IsNullOrEmpty(_avatarMeshBuffer.boneNameHashes))
					{
						var boneNameHahes = _avatarMeshBuffer.boneNameHashes;
						var bones = new Transform[boneNameHahes.Length];
						//
						for (int i = 0; i < boneNameHahes.Length; ++i)
						{
							var trans = avatarSkeleton.GetTransform(boneNameHahes[i]);
							if (trans == null)
							{
								AvatarEnv.Log(DebugLogMask.GeneralError, "transform for a bone not found!");
							}

							bones[i] = trans;
						}

						var rootBoneTrans = avatarSkeleton.GetTransform(_avatarMeshBuffer.rootBoneNameHash);

						_skinMeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>();
						_skinMeshRenderer.sharedMesh = _avatarMeshBuffer.mesh;
						_skinMeshRenderer.bones = bones;
						_skinMeshRenderer.rootBone =
							rootBoneTrans == null ? avatarSkeleton.rootTransform : rootBoneTrans;
						_skinMeshRenderer.localBounds = _avatarMeshBuffer.mesh.bounds;

						// whether disable shadow casting.
						if (PicoAvatarApp.instance.renderSettings.forceDisableCastShadow ||
						    mainShaderType == AvatarShaderType.Eyelash_Base)
						{
							_skinMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
						}

						// whether disable shadow casting.
						if (PicoAvatarApp.instance.renderSettings.forceDisableReceiveShadow ||
						    lodLevel_ > AvatarLodLevel.Lod2)
						{
							_skinMeshRenderer.receiveShadows = false;
						}
					}
					else
					{
						_staticMeshFilter = this.gameObject.AddComponent<MeshFilter>();
						_staticMeshFilter.sharedMesh = _avatarMeshBuffer.mesh;

						// add mesh renderer.
						_staticMeshRenderer = this.gameObject.AddComponent<MeshRenderer>();

						// whether disable shadow casting.
						if (PicoAvatarApp.instance.renderSettings.forceDisableCastShadow ||
						    mainShaderType == AvatarShaderType.Eyelash_Base)
						{
							_staticMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
						}

						// whether disable shadow casting.
						if (PicoAvatarApp.instance.renderSettings.forceDisableReceiveShadow ||
						    lodLevel_ > AvatarLodLevel.Lod2)
						{
							_staticMeshRenderer.receiveShadows = false;
						}
					}
				}
				//
				{
					InitializeSimulationGpuData(owner);
				}
				//
				_isRenderMeshDataReady = true;

				//if (AvatarEnv.NeedLog(DebugLogMask.AvatarLoad))
				//{
				//    AvatarEnv.Log(DebugLogMask.AvatarLoad, "AvatarRenderMesh.CreateUnityRenderMesh.end");
				//}
				return true;
			}
            
            //Initialize gpu data.
            protected void InitializeSimulationGpuData(System.IntPtr owner)
			{
				//
				needUpdateSimulation = false;

				// Dynamic Buffer
				if (_avatarMeshBuffer == null)
				{
					return;
				}

				// currently on morph data needed for each frame updation.
				if (_avatarMeshBuffer.meshInfo.blendShapeCount == 0)
				{
					return;
				}

				var dynamicBufferByteSize = _avatarMeshBuffer.morphAndSkinDataInfo.dynamicBufferByteSize;

				_DynamicCpuData = new NativeArray<int>((int)dynamicBufferByteSize / 4, Allocator.Persistent,
					NativeArrayOptions.UninitializedMemory);

				_DynamicGpuDataInfo = new MorphAndSkinSimulationGpuData();
				// currently only morph weights need updated.
				_DynamicGpuDataInfo.flags = (uint)RenderResourceDataFlags.Dynamic_HasMorphWeights;
				_DynamicGpuDataInfo.version = 0;
				_DynamicGpuDataInfo.dynamicBufferByteSize = dynamicBufferByteSize;
				unsafe
				{
					_DynamicGpuDataInfo.dataBuffer = (System.IntPtr)_DynamicCpuData.GetUnsafePtr();
				}

				_DynamicGpuDataInfo.materialData = System.IntPtr.Zero;

				// update at startup.
				UpdateMorphAndSkinSimulationBuffer(owner);

				// need update simulation.
				needUpdateSimulation = true;
			}
            
            //Invoked to update simulation buffer.
            protected bool UpdateMorphAndSkinSimulationBuffer(System.IntPtr meshOwner,
				RecordBodyAnimLevel level = RecordBodyAnimLevel.Count, bool expressionPlaybackEnabled = false)
			{
				if (!Utility.EnableRenderObject || !Utility.EnableSDKUpdate || _avatarMeshBuffer == null ||
				    _nativeRenderMeshHandler == System.IntPtr.Zero)
				{
					return false;
				}

				var morphChannelCount = _avatarMeshBuffer.meshInfo.blendShapeCount;

				// if has no morph data and only need update morph data, do nothing.
				if (!expressionPlaybackEnabled)
				{
					if (_DynamicGpuDataInfo.flags == (uint)RenderResourceDataFlags.Dynamic_HasMorphWeights)
					{
						// if has no bs or currently skip bs, do nothing.
						if (morphChannelCount == 0 || (level < RecordBodyAnimLevel.BasicBlendShape &&
						                               level != RecordBodyAnimLevel.DeviceInput))
						{
							//TODO: fadeout to empty bs weights
							return false;
						}
					}
				}

				var curFrameCount = Time.frameCount;
				if (_lastUpdateMorphAndSkinSimulationBufferFrame == curFrameCount)
				{
					return false;
				}

				_lastUpdateMorphAndSkinSimulationBufferFrame = curFrameCount;

				// check last active morph channels.
				if (_lastMorphChannelActiveFrames == null || _lastMorphChannelActiveFrames.Length != morphChannelCount)
				{
					_lastMorphChannelActiveFrames = new int[morphChannelCount];
				}

				if (_blendshapeWeights == null || _blendshapeWeights.Length != morphChannelCount)
				{
					_blendshapeWeights = new float[morphChannelCount];
				}

				UnityEngine.Profiling.Profiler.BeginSample("Update GPU Skin DynamicBuffer");

				// set current owner.
				_DynamicGpuDataInfo.owner = meshOwner;
				//
				var curFrameIndex = Time.frameCount;
				//
				if (pav_AvatarRenderMesh_FillMorphAndSkinSimulationGpuData(_nativeRenderMeshHandler,
					    ref _DynamicGpuDataInfo) == NativeResult.Success)
				{
					if (_skinMeshRenderer != null && _skinMeshRenderer.sharedMesh != null)
					{
						// update blendshapes
						var dynamicBuffer = _DynamicGpuDataInfo.dataBuffer;
						int offset = (int)_avatarMeshBuffer.morphAndSkinDataInfo.dynamicBufferOffset;
						int morphCount = Marshal.ReadInt32(dynamicBuffer, offset + 0);
						int morphIndexDataOffset = Marshal.ReadInt32(dynamicBuffer, offset + 4);
						int morphWeightDataOffset = Marshal.ReadInt32(dynamicBuffer, offset + 8);
						int blendShapeCount = _skinMeshRenderer.sharedMesh.blendShapeCount;
						unsafe
						{
							for (int i = 0; i < morphCount; ++i)
							{
								const int stride = 4;
								int index = Marshal.ReadInt32(dynamicBuffer, morphIndexDataOffset + stride * i);
								int weighti = Marshal.ReadInt32(dynamicBuffer, morphWeightDataOffset + stride * i);
								float weight = *(float*)&weighti;
								_lastMorphChannelActiveFrames[index] = curFrameIndex;
								// set current value.
								_skinMeshRenderer.SetBlendShapeWeight(index, weight * 100);
								// cache blendshape weights
								if (_cacheMorphChannelWeights)
								{
									_blendshapeWeights[index] = weight * 100;
								}
							}
						}

						// turn off not used channels.
						for (int i = 0; i < morphChannelCount; ++i)
						{
							if (_lastMorphChannelActiveFrames[i] != 0 &&
							    _lastMorphChannelActiveFrames[i] != curFrameIndex)
							{
								// clear active flag
								_lastMorphChannelActiveFrames[i] = 0;
								// clear deactived channel.
								_skinMeshRenderer.SetBlendShapeWeight(i, 0);
								// cache blendshape weights
								if (_cacheMorphChannelWeights)
								{
									_blendshapeWeights[i] = 0;
								}
							}
						}
					}
				}

				UnityEngine.Profiling.Profiler.EndSample();

				return true;
			}
            
            //Sets render material. 
            //renderMaterialHandles list of handle to native AvatarRenderMaterial. Reference count has been added from invoker.
            protected PicoAvatarRenderMaterial[] CreateRenderMaterials(System.IntPtr[] renderMaterialHandles,
				AvatarLodLevel lodLevel, bool allowEdit, bool merged)
			{
				PicoAvatarRenderMaterial[] materials = new PicoAvatarRenderMaterial[renderMaterialHandles.Length];
				bool success = true;
				for (int i = 0; i < materials.Length; ++i)
				{
					materials[i] = new PicoAvatarRenderMaterial(merged, _AvatarLod);
					materials[i].Retain();

					// try to load render material.
					if (!materials[i].LoadPropertiesFromNativeMaterial(renderMaterialHandles[i], lodLevel, allowEdit))
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

			#endregion


			#region Update Primitive Dirty Data
			
            //Rebuild gpu skin and morph data in work thread. invoke before UpdateMorphAndSkinResourceGpuData
            internal NativeResult RebuildGpuSkinAndMorphDataT()
			{
				return pav_AvatarRenderMesh_RebuildMorphAndSkinGpuDataT(_nativeRenderMeshHandler);
			}
            
            //update gpu skin and morph buffer.
            internal void UpdateMorphAndSkinResourceGpuData()
			{
				if (_avatarMeshBuffer != null)
				{
					if (_avatarMeshBuffer.UpdateMorphAndSkinResourceGpuData(_nativeRenderMeshHandler))
					{
						//// notify that mesh changed.
						//if(onMeshUpdate != null)
						//{
						//    onMeshUpdate(this);
						//}
					}
				}
			}
            
            //Update dirty mesh pnt data.
            internal void UpdateDirtyMeshPNTData()
			{
				if (_avatarMeshBuffer != null)
				{
					var needTangent = _materialNeedTangent && _HasTangent;
					_avatarMeshBuffer.UpdateMeshPNTData(_nativeRenderMeshHandler, _materialConfiguration, needTangent);
				}
			}
            
            //Update dirty material uniforms.
            internal void UpdateDirtyMaterialUniforms()
			{
				if (officialRenderMaterials != null && (meshRenderer != null || skinMeshRenderer != null))
				{
					var unityMaterials = skinMeshRenderer != null
						? skinMeshRenderer.sharedMaterials
						: meshRenderer.sharedMaterials;
					var count = officialRenderMaterials.Length;
					if (count == unityMaterials.Length)
					{
						for (int i = 0; i < count; ++i)
						{
							officialRenderMaterials[i].UpdateDirtyMaterialUniforms();
							_materialConfiguration.UpdateToUniformsMaterial(officialRenderMaterials[i], unityMaterials[i]);
						}
					}
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_GetMorphAndSkinGpuDataInfo(
				System.IntPtr nativeHandle, uint requiredVersion, uint requiredFlags,
				ref MorphAndSkinDataInfo gpuDataInfo);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_RebuildMorphAndSkinGpuDataT(
				System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern NativeResult pav_AvatarRenderMesh_FillMorphAndSkinSimulationGpuData(
				System.IntPtr nativeHandle, ref MorphAndSkinSimulationGpuData gpuData);

			#endregion
		}


		// Render Mesh of an AvatarPrimitive instance.
		public class PicoPrimitiveRenderMesh : PicoAvatarRenderMesh
		{
			// owner primitive object
			public AvatarPrimitive primitive
			{
				get { return _Primitive; }
			}

			[Obsolete("Deprecated, use primitive instead!")]
			public AvatarPrimitive Primitive
			{
				get { return _Primitive; }
			}

			#region Framework Methods
			
            //Sets primitive and build mesh.
            internal void AttachPrimitive(AvatarPrimitive primitive)
			{
				_Primitive = primitive;
				_AvatarLod = _Primitive?.owner;
				//
				avatarEffectKind = primitive.owner.owner.owner.avatarEffectKind;
			}

			// enabled to make sure correct first frame rendering
			private void OnWillRenderObject()
			{
				if (isRenderDataReady && _Primitive != null && _Primitive.nativeHandle != System.IntPtr.Zero &&
				    needUpdateSimulation
				    && !_Primitive.owner.owner.isNativeUpdateSkippedThisFrame)
				{
					UpdateMorphAndSkinSimulationBuffer(_Primitive.nativeHandle,
						_Primitive.owner.owner.lastAnimationPlaybackLevel,
						_Primitive.owner.owner.expressionPlaybackEnabled);
				}
			}
			
            //Invoked when the scene object destroyed by Unity.
            internal override void Destroy()
			{
				if (_Primitive != null)
				{
					_Primitive.OnRenderMeshDestroy(this);
					_Primitive = null;
				}

				//
				base.Destroy();
			}

			#endregion

			#region BakeColorRegion
			void OfficialMaterialBakeColorRegionOnGPUAndAsyncReadback(Material mat)
			{
				var materailConfig = PicoAvatarApp.instance.renderSettings.materialConfiguration;
				var bte = new BakeColorRegionEntity();
				_bakeColorRegionEntities.Add(bte);
				if (bte == null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "BakeColorRegionEntity is null.");
					return;
				}
				if (!materailConfig)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "PicoAvatarApp.instance.renderSettings.materialConfiguration is null.");
					return;
				}
				bte.astcEncoderShader = materailConfig.astcEncodeShader;
				bte.blendTexturesShader = materailConfig.blendTextureShader;
				bte.material = mat;
				bte.BaseMap = mat.GetTexture("_BaseMap");
				bte.ColorRegion = mat.GetTexture("_ColorRegionMap");
				bte.Region1 = mat.GetVector("_ColorRegion1");
				bte.Region2 = mat.GetVector("_ColorRegion2");
				bte.Region3 = mat.GetVector("_ColorRegion3");
				bte.Region4 = mat.GetVector("_ColorRegion4");
				bte.albedoHue = mat.GetFloat("_UsingAlbedoHue");
				bte.Execute();
			}


			bool OfficialMergeMaterialsOnGPUAndAsyncReadback(Mesh mesh, Renderer render ,Material[] mats)
			{
				var materailConfig = PicoAvatarApp.instance.renderSettings.materialConfiguration;
				var bte = new MergeMaterialAndBakeColorRegionEntity();
				if (bte == null)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "MergeMaterialAndBakeColorRegionEntity is null.");
					return false;
				}
				if (!materailConfig)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "PicoAvatarApp.instance.renderSettings.materialConfiguration is null.");
					return false;
				}
				bte.astcEncoderShader = materailConfig.astcEncodeShader;
				bte.blendTexturesShader = materailConfig.blendTextureShader;
				bte.mergeTextureShader = materailConfig.mergeTextureShader;
				bte.materials = mats;
				bte.renderer = render;
				bte.mesh = mesh;
				return bte.Execute();
			}
			#endregion
			#region Build Mesh/Material

			//Build renderer with native AvatarRenderMesh and AvatarRenderMaterial.
			//renderMeshHandle handle to native AvatarRenderMesh. Reference count has been added from invoker.
			//renderMaterialHandles handle to native AvatarRenderMaterial. Reference count has been added from invoker.
			internal bool BuildFromNativeRenderMeshAndMaterial(System.IntPtr renderMeshHandle,
				System.IntPtr[] renderMaterialHandles,
				AvatarSkeleton avatarSkeleton_, bool allowGpuDataCompressd, bool cacheMorphChannelWeights = false,
				bool depressSkin = false)
			{
				if (_Primitive == null || meshRenderer != null || skinMeshRenderer != null)
				{
					throw new System.Exception("BuildFromNativeRenderMesh invoked wrongly.");
				}

				// if native render mesh or render material not created, can not show unity mesh.
				if (renderMeshHandle == System.IntPtr.Zero || renderMaterialHandles == null)
				{
					return false;
				}


				Material[] customMaterials = null;
				Material[] officialMaterials = null;

				// get Custom Material info.
				// get customRenderMaterials --> AvatarMaterial.
				// get customMaterials --> Unity Runtime Material.
				if (PicoAvatarApp.instance.renderSettings.useCustomMaterial)
				{
					if (BuildCustomMaterialsFromNative(renderMaterialHandles))
					{
						customMaterials = GetCustomRuntimeMaterial(customRenderMaterials);
						if (customMaterials != null)
						{
							this._useCustomMaterial = true;
						}
					}
				}

				// build mesh with native mesh data for a lod level second.
				// using _useCustomMaterial to flip uv.
				{
					if (!CreateUnityRenderMesh(renderMeshHandle, avatarSkeleton_, _Primitive.lodLevel,
							_Primitive.nativeHandle,
							allowGpuDataCompressd, _Primitive.mainShaderType, cacheMorphChannelWeights, depressSkin))
					{
						return false;
					}
				}

				// get Official material info.
				// get officialRenderMaterials --> AvatarMaterial.
				// get officialMaterials --> Unity Runtime Material.
				{
					if (!BuildOfficialMaterialsFromNative(renderMaterialHandles, _Primitive.lodLevel, _Primitive.owner.owner.owner.capabilities.allowEdit, false))
					{
						return false;
					}
					officialMaterials = GetOfficialRuntimeMaterial(officialRenderMaterials);
				}

				// set official Material first.
				if (officialMaterials != null)
				{
					if (!_Primitive.owner.owner.owner.capabilities.allowEdit)
					{
						for (int i = 0; i < officialRenderMaterials.Length; ++i)
						{
							var renderMaterial = officialRenderMaterials[i]; 
							var runtimeMaterial = officialMaterials[i];
							if (!renderMaterial.mat_shaderColorRegionBaked)
							{
								OfficialMaterialBakeColorRegionOnGPUAndAsyncReadback(runtimeMaterial);
							}
						}
					}
					if (skinMeshRenderer)
					{
						skinMeshRenderer.sharedMaterials = officialMaterials;
					}
					else if (meshRenderer)
					{
						meshRenderer.sharedMaterials = officialMaterials;
						meshRenderer.SetPropertyBlock(new MaterialPropertyBlock());
					}
				}

					// set CustomMaterial Second.
					if (customMaterials != null)
					{
						if (skinMeshRenderer)
						{
							skinMeshRenderer.sharedMaterials = customMaterials;
						}
						else if (meshRenderer)
						{
							meshRenderer.sharedMaterials = customMaterials;
							meshRenderer.SetPropertyBlock(new MaterialPropertyBlock());
						}
					}

					//
					if (needUpdateSimulation && _Primitive.owner != null)
					{
						_Primitive.owner.AddSimulationNeededAvatarPrimitive(_Primitive);
					}

					return true;
			}

			#endregion

			#region Update Simulation Data

			//Update morph and skin each frame.
			internal override void UpdateSimulationRenderData()
			{
				if (_Primitive != null && _Primitive.nativeHandle != System.IntPtr.Zero && needUpdateSimulation)
				{
					UpdateMorphAndSkinSimulationBuffer(_Primitive.nativeHandle,
						_Primitive.owner.owner.lastAnimationPlaybackLevel,
						_Primitive.owner.owner.expressionPlaybackEnabled);
				}
			}

			#endregion


			#region Private Fields

			// owner primitive.
			private AvatarPrimitive _Primitive;

			#endregion
		}

		internal struct MergedMeshData
		{
			public NativeArray<Vector3> positions;
			public NativeArray<Vector3> normals;
			public NativeArray<Vector4> tangents;
			public NativeArray<Color32> colors;
			public NativeArray<Vector2> uv1;
			public NativeArray<Vector2> uv2;
			public NativeArray<Vector2> uv3;
			public NativeArray<Vector2> uv4;
			public NativeArray<Vector2> materialIndices;
			public NativeArray<int> boneNameHashes;
			public NativeArray<Matrix4x4> invBindPoses;
			public NativeArray<BoneWeight> boneWeights;
			public NativeArray<uint> indices;
        }

		public class PicoAvatarMergedRenderMesh : MonoBehaviour
		{
			private SkinnedMeshRenderer _skinnedMeshRenderer;
			public SkinnedMeshRenderer skinnedMeshRenderer { get => _skinnedMeshRenderer; }

			private AvatarMergedMeshBuffer _meshBuffer;
			public AvatarMergedMeshBuffer meshBuffer { get => _meshBuffer; }

			private System.IntPtr _nativeHandle;
			internal System.IntPtr nativeHandle { get => _nativeHandle; set { _nativeHandle = value; } }

            internal void OnDestroy()
			{
                if (_skinnedMeshRenderer != null)
                {
                    _skinnedMeshRenderer.sharedMesh = null;
                    _skinnedMeshRenderer.sharedMaterial = null;
                    _skinnedMeshRenderer = null;
                }

				// Decreasing mesh buffer refcount
				ReferencedObject.ReleaseField(ref _meshBuffer);
                // Release the added ref count of nativeHandle when calling pav_AvatarLod_GetMergedRenderMesh
                NativeObject.ReleaseNative(ref _nativeHandle);
            }

			private void ProcessMesh(AvatarSkeleton skeleton)
			{
				_skinnedMeshRenderer.sharedMesh = _meshBuffer.mesh;

				// Update bones data
                var bones = new Transform[_meshBuffer.boneNameHashes.Length];
				for (int i = 0; i < _meshBuffer.boneNameHashes.Length; i++)
				{
					var bone = skeleton.GetTransform(_meshBuffer.boneNameHashes[i]);
					if (bone == null)
					{
                        AvatarEnv.Log(DebugLogMask.GeneralError, String.Format("Transform for a bone {0} not found!", _meshBuffer.boneNameHashes[i]));
					}
					bones[i] = bone;
				}
				_skinnedMeshRenderer.bones = bones;
				var rootBone = skeleton.GetTransform((int)_meshBuffer.rootBoneNameHash);
				_skinnedMeshRenderer.rootBone = rootBone != null ? rootBone : skeleton.rootTransform;

				// Update bounding box
				_skinnedMeshRenderer.localBounds = _meshBuffer.mesh.bounds;
			}

			internal bool Build(AvatarLod lod, AvatarMergedMeshBuffer buffer)
			{
				if (_skinnedMeshRenderer == null)
					_skinnedMeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>();

				ReferencedObject.Replace(ref _meshBuffer, buffer);
				ProcessMesh(lod.avatarSkeleton);

				// _skinnedMeshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                // whether disable shadow casting.
                if (PicoAvatarApp.instance.renderSettings.forceDisableCastShadow)
                {
                    _skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                // whether disable shadow casting.
                if (PicoAvatarApp.instance.renderSettings.forceDisableReceiveShadow || lod.lodLevel > AvatarLodLevel.Lod2)
                {
                    _skinnedMeshRenderer.receiveShadows = false;
                }
                return true;
            }

			internal bool Build(AvatarLod lod, int hashCode, ref MergedMeshInfo meshInfo, ref MergedMeshData meshData)
			{
				if (_skinnedMeshRenderer == null)
					_skinnedMeshRenderer = this.gameObject.AddComponent<SkinnedMeshRenderer>();

				var nativeBuffer = pav_AvatarMergedRenderMesh_GetBuffer(nativeHandle);
				_meshBuffer = AvatarMergedMeshBuffer.Create(hashCode, nativeBuffer, ref meshInfo, ref meshData);
				ProcessMesh(lod.avatarSkeleton);

				// _skinnedMeshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));

                // whether disable shadow casting.
                if (PicoAvatarApp.instance.renderSettings.forceDisableCastShadow)
                {
                    _skinnedMeshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                }

                // whether disable shadow casting.
                if (PicoAvatarApp.instance.renderSettings.forceDisableReceiveShadow || lod.lodLevel > AvatarLodLevel.Lod2)
                {
                    _skinnedMeshRenderer.receiveShadows = false;
                }
                return true;
			}

			const string PavDLLName = DllLoaderHelper.PavDLLName;
			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarMergedRenderMesh_GetBuffer(System.IntPtr nativeHandle);
		}
	}
}