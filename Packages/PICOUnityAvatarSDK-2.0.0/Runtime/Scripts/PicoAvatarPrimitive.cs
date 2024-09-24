using UnityEngine;
using System.Runtime.InteropServices;

namespace Pico
{
	namespace Avatar
	{
		// Primitive wraps a render
		public class AvatarPrimitive : ReferencedObject
		{
			#region Public Properteids

			// Whether has been merged to AvatarLod.
			internal bool isMergedToAvatarLod { get; private set; } = false;

			// native handle.
			internal System.IntPtr nativeHandle
			{
				get => _nativeHandle;
			}

			// Object Owner. The AvatarPrimitive created by AvatarLod and managed by AvatarLod.
			public AvatarLod owner
			{
				get => _owner;
			}

			//  Use lod level of owner AvatarLod.
			public AvatarLodLevel lodLevel
			{
				get => _owner.lodLevel;
			}

			// Primitive render mesh.
			public PicoPrimitiveRenderMesh primitiveRenderMesh
			{
				get => _primitiveRenderMesh;
			}

			// flag AvatarNodeTypes.
			public uint nodeTypes
			{
				get
				{
					if (_nodeFlags == 0xffffffff)
					{
						_nodeFlags = pav_AvatarPrimitive_GetNodeTypes(_nativeHandle);
					}

					return _nodeFlags;
				}
			}

			public uint nodeId
			{
				get => _nodeId;
			}

			// main shader type.
			public AvatarShaderType mainShaderType
			{
				get
				{
					if (_mainShaderType == AvatarShaderType.Invalid)
					{
						_mainShaderType = (AvatarShaderType)pav_AvatarPrimitive_GetMainShaderType(_nativeHandle);
					}

					return _mainShaderType;
				}
			}

			// whether need updated each frame.
			internal bool needUpdateSimulation
			{
				get { return _primitiveRenderMesh == null ? false : _primitiveRenderMesh.needUpdateSimulation; }
			}

			// whether need updated each frame. if the primitive need updated each frame, should be added to AvatarEntityLod 
			internal bool needUpdateFrame { get; private set; } = false;

			// mesh socket that derive the primitive.
			internal AvatarMeshSocket meshSocket
			{
				get => _meshSocket;
			}

			#endregion


			#region Public Framework Methods

			// Constructor invoked by AvatarLod.
			internal AvatarPrimitive(System.IntPtr nativeHandle_, uint nodeId, AvatarLod owner_)
			{
				_nativeHandle = nativeHandle_;
				_nodeId = nodeId;
				_owner = owner_;
			}

			~AvatarPrimitive()
			{
				if (_nativeHandle != System.IntPtr.Zero)
				{
					AvatarEnv.Log(DebugLogMask.GeneralError, "AvatarPrimitive not destroyed.");
					//
					NativeObject.ReleaseNative(ref _nativeHandle);
				}
			}

			// Update after amaz
			internal void UpdateSimulationRenderData()
			{
				_primitiveRenderMesh?.UpdateSimulationRenderData();
			}

			// Destroy the primitive object. Only CAN be invoked by AvatarLod.
			internal void Destory()
			{
				if (_meshSocket != null)
				{
					_meshSocket.Release();
					_meshSocket = null;
				}

				if (_nativeHandle != System.IntPtr.Zero)
				{
					_nodeId = 0;
					//
					DestroyPrimitiveRenderMesh();

					// destroy native object.
					NativeObject.ReleaseNative(ref _nativeHandle);
				}
			}

			// Notification from AvatarMeshRenderable when destroyed.
			internal void OnRenderMeshDestroy(PicoAvatarRenderMesh meshRenderable)
			{
				_primitiveRenderMesh = null;
			}

			internal string GetAssetId()
			{
				var binaryData = pav_AvatarPrimitive_GetAssetId(nativeHandle);
				if (binaryData.len <= 0)
				{
					return string.Empty;
				}

				unsafe
				{
					return System.Text.Encoding.ASCII.GetString((byte*)binaryData.data, binaryData.len);
				}
			}

			#endregion


			#region Build Primitive Render Mesh

			// Sets that the primitive has been merged by AvatarLod. Invoked by AvatarLod
			internal void SetMergedByAvatarLod(bool merged)
			{
				this.isMergedToAvatarLod = merged;
			}

			// Build primitive render mesh and material from native object.
			internal bool BuildFromNativeRenderMeshAndMaterial()
			{
				//
				CreatePrimitiveRenderMesh();
				//
				if (_primitiveRenderMesh != null)
				{
					bool depressSkin = _owner.owner.owner.capabilities.isAvatarBunchSource;

					// FIXME: classify the concept of isMainAvatar and isLocalAvatar in AvatarCapability
					if (!_primitiveRenderMesh.BuildFromNativeRenderMeshAndMaterial(
						    GetRenderMeshHandle(), GetRenderMaterialHandles(), GetSkeleton(),
						    _owner.IsAllowGpuDataCompress(false),
						    _owner.owner.owner.capabilities.isMainAvatar, depressSkin))
					{
						return false;
					}
				}
				else
				{
					AvatarEnv.Log(DebugLogMask.GeneralWarn, "No render mesh found in AvatarPrimitive!");
				}

				return true;
			}
			
            //When native texture changed should rebuild material object.
            //Usually invoked during editing avatar when changing texture based asset.
            internal bool RebuildMaterialsFromNative()
			{
				if (_primitiveRenderMesh == null)
				{
					return false;
				}

				//
				return _primitiveRenderMesh.BuildOfficialMaterialsFromNative(GetRenderMaterialHandles(), lodLevel, false);
			}
            
            //Native handle for render material of the primitive.
            //@note Reference count has been added, invoker should release the reference count when DO NOT reference
            //the mesh object any more.
            private System.IntPtr[] GetRenderMaterialHandles()
			{
				System.IntPtr[] handles = null;
				int materialCount = pav_AvatarPrimitive_GetRenderMaterialCount(_nativeHandle);
				if (materialCount > 0)
				{
					handles = new System.IntPtr[materialCount];
					for (int i = 0; i < materialCount; ++i)
					{
						handles[i] = pav_AvatarPrimitive_GetRenderMaterialByIndex(_nativeHandle, i);
					}
				}

				return handles;
			}

            //Native handle for render mesh of the primitive.
            //@note Reference count has been added, invoker should release the reference count when DO NOT reference
            //the mesh object any more.
            private System.IntPtr GetRenderMeshHandle()
			{
				return pav_AvatarPrimitive_GetRenderMesh(_nativeHandle);
			}
            
            //Gets primitive node name.
            private string GetPrimitiveNodeName(int nodeTypes)
			{
				// head
				if (((int)nodeTypes & (int)AvatarNodeTypes.Head) != 0)
				{
					var headNodeType = (AvatarNodeTypes)(nodeTypes & (int)AvatarNodeTypeCombines.HeadNodes);
					if ((int)headNodeType == 0)
					{
						return "head_any";
					}

					if ((int)headNodeType == ((int)AvatarNodeTypes.Tongue | (int)AvatarNodeTypes.Tooth))
					{
						return "tooth_tongue";
					}
					else if ((int)headNodeType == (int)AvatarNodeTypes.HeadAccessory)
					{
						return "head_accessory";
					}

					return headNodeType.ToString();
				} // clothes
				else if (((int)nodeTypes & (int)AvatarNodeTypes.Clothes) != 0)
				{
					var clothNodeType = (AvatarNodeTypes)(nodeTypes & (int)AvatarNodeTypeCombines.ClothNodes);
					if ((int)clothNodeType == 0)
					{
						return "cloth_any";
					}

					return clothNodeType.ToString();
				}
				else if ((nodeTypes & (int)AvatarNodeTypes.BodyAccessory) != 0)
				{
					return "body_accessory";
				}

				// body and others.
				return ((AvatarNodeTypes)nodeTypes).ToString();
			}
            
            //Created primitive render mesh.
            private void CreatePrimitiveRenderMesh()
			{
				if (isMergedToAvatarLod || _primitiveRenderMesh != null
				                        || !NeedCreateRenderMesh())
				{
					return;
				}

				// lod3 hide something.
				if (owner.lodLevel > AvatarLodLevel.Lod2)
				{
					// skip eyelash for lod3
					if (mainShaderType == AvatarShaderType.Eyelash_Base)
					{
						return;
					}
				}

				var primitiveNodeTypes = this.nodeTypes;
				//
				var go = new GameObject(string.Format("PrimitiveMesh{0}_{1}", _nodeId,
					GetPrimitiveNodeName((int)primitiveNodeTypes)));
				var trans = go.transform;
                go.hideFlags = HideFlags.DontSave;
				//
				trans.transform.parent = _owner.transform;
				//
				trans.localPosition = Vector3.zero;
				trans.localRotation = Quaternion.identity;
				trans.localScale = Vector3.one;
				//
				go.layer = owner.gameObject.layer;
				//
				_primitiveRenderMesh = go.AddComponent<PicoPrimitiveRenderMesh>();
				_primitiveRenderMesh.AttachPrimitive(this);

				// if is head, and need hide head,
				if ((primitiveNodeTypes & (uint)AvatarNodeTypes.Head) != 0
				    && owner.owner.owner.capabilities.headShowType == AvatarHeadShowType.Hide)
				{
					//
					if (PicoAvatarApp.instance.extraSettings.mainAvatarHeadSceneLayer < 0)
					{
						go.SetActive(false);
					}
					else
					{
						go.layer = (int)PicoAvatarApp.instance.extraSettings.mainAvatarHeadSceneLayer;
					}
				}

				// if manifestationType is head or HeadHands, and need hide other go except Head.
				if ((primitiveNodeTypes & (uint)AvatarNodeTypes.Head) == 0
				    && (owner.owner.owner.capabilities.manifestationType == AvatarManifestationType.Head ||
				        owner.owner.owner.capabilities.manifestationType == AvatarManifestationType.HeadHands))
				{
					//
					if (PicoAvatarApp.instance.extraSettings.mainAvatarHeadSceneLayer < 0)
					{
						go.SetActive(false);
					}
					else
					{
						go.layer = (int)PicoAvatarApp.instance.extraSettings.mainAvatarHeadSceneLayer;
					}

					if (owner.owner.owner.capabilities.manifestationType == AvatarManifestationType.HeadHands &&
					    (primitiveNodeTypes & (uint)AvatarNodeTypes.ClothHandGlove) != 0)
					{
						go.SetActive(true);
					}
				}

				// check create socket
				var meshSocketHandle = pav_AvatarPrimitive_GetMeshSocket(_nativeHandle);
				if (meshSocketHandle != System.IntPtr.Zero)
				{
					_meshSocket = new AvatarMeshSocket(meshSocketHandle);
					_meshSocket.Retain();

					var socketTransNameHash = _meshSocket.GetNativeSocketTransformNameHash();
					if (socketTransNameHash != 0)
					{
						var socketTrans = this.owner.avatarSkeleton.GetTransform((int)socketTransNameHash);
						if (socketTrans != null)
						{
							trans.parent = socketTrans;
							var xform = _meshSocket.GetLocalXForm();
							trans.localPosition = xform.position;
							trans.localRotation = xform.orientation;
							trans.localScale = xform.scale;
						}
					}
					//// check whether has been added to owner.
					//if (_meshSocket.needUpdateUnitySocketTransform)
					//{
					//    // set target mesh transform.
					//    _meshSocket.targetMeshTransform = go.transform;

					//    if (!needUpdateFrame)
					//    {
					//        needUpdateFrame = true;
					//        owner.AddUpdationNeededAvatarPrimitive(this);
					//    }
					//}
				}
			}

			
            //Invoked from PicoAvatarLod to depress merge the primitive.
            internal void SetDepressMerge()
			{
				pav_AvatarPrimitive_SetDepressMerge(_nativeHandle, true);
			}
         //
            //@brief Sets control flags. Bits from AvatarPrimitiveAppFlags
            internal void SetControlFlags(uint flags)
			{
				pav_AvatarPrimitive_SetControlFlags(nativeHandle, flags);
			}

			#endregion


			#region Update Dirty Primitive Data
//
            //@brief Check and update dirty data.
            internal uint CheckUpdatePrimitiveDrityData()
			{
				var dirtyFlags = pav_AvatarPrimitive_GetAndClearPrimitiveDirtyFlags(_nativeHandle);
				if (_primitiveRenderMesh != null)
				{
					//
					if ((dirtyFlags & (uint)AvatarPrimitiveDirtyFlags.MaterialUniforms) != 0)
					{
						_primitiveRenderMesh.UpdateDirtyMaterialUniforms();
					}

					if ((dirtyFlags & (uint)AvatarPrimitiveDirtyFlags.Morph) != 0)
					{
						if (_primitiveRenderMesh.RebuildGpuSkinAndMorphDataT() == NativeResult.Success)
						{
							_primitiveRenderMesh.UpdateMorphAndSkinResourceGpuData();
						}
					}

					if ((dirtyFlags & (uint)AvatarPrimitiveDirtyFlags.MeshVertex) != 0)
					{
						_primitiveRenderMesh.UpdateDirtyMeshPNTData();
					}

					if ((dirtyFlags & (uint)AvatarPrimitiveDirtyFlags.MeshSocket) != 0)
					{
						UpdateDirtyMeshSocketData();
					}
				}

				return dirtyFlags;
			}
            
            //@brief Do works that need be updated each frame.
            internal void UpdateFrame()
			{
				if (_meshSocket != null && _meshSocket.targetMeshTransform != null)
				{
					_meshSocket.UpdateUnityTargetTransformFromNative();
				}
			}

			#endregion


			#region Protected Methods
			
            //Derived class can override the method to release resources when the object will be destroyed.
            //The method should be invoked by override method.
            protected override void OnDestroy()
			{
				// destroy object.
				Destory();
				//
				base.OnDestroy();
			}

			#endregion


			#region Private Fields

			// node id of the primitive.
			private uint _nodeId;

			// native primitive handle.
			private System.IntPtr _nativeHandle;

			// owner
			private AvatarLod _owner;

			// mesh renderable.
			private PicoPrimitiveRenderMesh _primitiveRenderMesh;

			// main shader type which control shadow/scene blend etc.
			private AvatarShaderType _mainShaderType = AvatarShaderType.Invalid;

			//
			private uint _nodeFlags = 0xffffffff;

			// main avatar skeleton or additive skeleton the primitive used.
			private AvatarSkeleton _avatarSkeleton = null;

			// mesh socket to use.
			private AvatarMeshSocket _meshSocket = null;

			#endregion


			#region Private Methods
			
            //Destroy the mesh created by the primitive.
            protected void DestroyPrimitiveRenderMesh()
			{
				if (_primitiveRenderMesh != null)
				{
					var primitiveGo = _primitiveRenderMesh.gameObject;
					// destroy native resource immediatelly.
					_primitiveRenderMesh.Destroy();
					_primitiveRenderMesh = null;
					//
					Utility.Destroy(primitiveGo);
				}
			}
            
            //Query whether need create render mesh.
            private bool NeedCreateRenderMesh()
			{
				return pav_AvatarPrimitive_needCreateRenderMesh(nativeHandle);
			}
            
            //@brief Gets skeleton the primitive used.
            private AvatarSkeleton GetSkeleton()
			{
				if (_avatarSkeleton == null && this.owner.avatarSkeleton != null)
				{
					var skeletonHandle = pav_AvatarPrimitive_PeekAdditiveSkeleton(nativeHandle);
					_avatarSkeleton = this.owner.avatarSkeleton.GetAdditiveSkeleton(skeletonHandle);
					if (_avatarSkeleton == null)
					{
						_avatarSkeleton = this.owner.avatarSkeleton;
					}
				}

				return _avatarSkeleton;
			}

			/// <summary>
			/// Update mesh socket data.
			/// </summary>
			private void UpdateDirtyMeshSocketData()
			{
				if (_meshSocket != null)
				{
					// clear c# cache
					_meshSocket.OnSocketOrTargetTransformChanged();
					//
					var socketTransNameHash = _meshSocket.GetNativeSocketTransformNameHash();
					if (socketTransNameHash != 0 && _primitiveRenderMesh != null)
					{
						var socketTrans = this.owner.avatarSkeleton.GetTransform((int)socketTransNameHash);
						if (socketTrans != null)
						{
							var trans = this._primitiveRenderMesh.transform;
							trans.parent = socketTrans;
							var xform = _meshSocket.GetLocalXForm();
							trans.localPosition = xform.position;
							trans.localRotation = xform.orientation;
							trans.localScale = xform.scale;
						}
					}
					else
					{
						var targetNameHash = _meshSocket.GetNativeTargetTransformNameHash();
						if (targetNameHash != 0)
						{
							var additiveSkeleton =
								this._owner.avatarSkeleton.GetAdditiveSkeletonContainsTheJoint(targetNameHash);
							if (additiveSkeleton != null)
							{
								additiveSkeleton.OnAdditiveSkeletonParentChanged();
							}
						}
					}
				}
			}

			#endregion


			#region Native Methods

			const string PavDLLName = DllLoaderHelper.PavDLLName;

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarPrimitive_needCreateRenderMesh(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern void pav_AvatarPrimitive_SetControlFlags(System.IntPtr nativeHandle,
				uint controlFlags);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarPrimitive_SetDepressMerge(System.IntPtr nativeHandle,
				bool depressMerge);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern bool pav_AvatarPrimitive_GetMergedToAvatarLod(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarPrimitive_GetNodeTypes(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern BinaryData pav_AvatarPrimitive_GetAssetId(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarPrimitive_GetMainShaderType(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarPrimitive_GetRenderMaterial(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern int pav_AvatarPrimitive_GetRenderMaterialCount(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarPrimitive_GetRenderMaterialByIndex(System.IntPtr nativeHandle,
				int index);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarPrimitive_GetRenderMesh(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern uint pav_AvatarPrimitive_GetAndClearPrimitiveDirtyFlags(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarPrimitive_PeekAdditiveSkeleton(System.IntPtr nativeHandle);

			[DllImport(PavDLLName, CallingConvention = CallingConvention.Cdecl)]
			private static extern System.IntPtr pav_AvatarPrimitive_GetMeshSocket(System.IntPtr nativeHandle);

			#endregion
		}
	}
}