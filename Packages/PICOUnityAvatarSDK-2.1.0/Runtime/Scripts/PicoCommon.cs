using System;
using UnityEngine;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace Pico
{
	namespace Avatar
	{
		public static class AvatarConstants
		{
			static AvatarConstants()
			{
				s_officialShaderNameThemes = new Dictionary<string, OfficialShaderTheme>()
				{
					{ "PAV/URP/PicoPBR", OfficialShaderTheme.PicoPBR },
					{ "PAV/URP/PicoNPR", OfficialShaderTheme.PicoNPR },
					{ "PicoAvatar/AvatarLit", OfficialShaderTheme.PicoAvatarLit },
					{ "PicoAvatar/Skin", OfficialShaderTheme.PicoAvatarSkin },
					{ "PicoAvatar/Hair", OfficialShaderTheme.PicoAvatarHair },
					{ "PicoAvatar/Eye", OfficialShaderTheme.PicoAvatarEye },
					{ "PicoAvatar/SimpleLit", OfficialShaderTheme.PicoAvatarSimpleLit },
					{ "PicoAvatar/Bake", OfficialShaderTheme.PicoAvatarBake },
				};
				s_officialShaderThemeNames = new Dictionary<OfficialShaderTheme, string>();
				foreach (var pair in s_officialShaderNameThemes)
				{
					s_officialShaderThemeNames.Add(pair.Value, pair.Key);
				}

				s_shaderNames = new Dictionary<string, string>()
				{
					{ "PBR", "PAV/URP/PicoPBR" },
					{ "NPR", "PAV/URP/PicoNPR" },
					{ "Unlit", "PAV/URP/Unlit" },
					{ "AvatarLit", "PicoAvatar/AvatarLit" },
					{ "AvatarSkin", "PicoAvatar/Skin" },
					{ "AvatarHair", "PicoAvatar/Hair" },
					{ "AvatarEye", "PicoAvatar/Eye" },
					{ "AvatarSimpleLit", "PicoAvatar/SimpleLit" },
					{ "AvatarBake", "PicoAvatar/Bake" },
				};
				s_shaderType1s = new Dictionary<string, string>();
				foreach (var pair in s_shaderNames)
				{
					s_shaderType1s.Add(pair.Value, pair.Key);
				}
				s_officialMaterialThemePostfixes = new Dictionary<OfficialShaderTheme, string>()
				{
					{ OfficialShaderTheme.PicoPBR, "_officialPBR" },
					{ OfficialShaderTheme.PicoNPR, "_officialNPR" },
					{ OfficialShaderTheme.PicoAvatarLit, "_Official_AvatarLit" },
					{ OfficialShaderTheme.PicoAvatarSkin, "_Official_AvatarSkin" },
					{ OfficialShaderTheme.PicoAvatarHair, "_Official_AvatarHair" },
					{ OfficialShaderTheme.PicoAvatarEye, "_Official_AvatarEye" },
					{ OfficialShaderTheme.PicoAvatarSimpleLit, "_Official_AvatarSimpleLit" },
					{ OfficialShaderTheme.PicoAvatarBake, "_Official_AvatarBake" },
				};
				s_officialShaderTextures = new Dictionary<string, List<string>>()
				{
					{ "AvatarLit", new List<string>() { "BaseTex" } },
					{ "AvatarSkin", new List<string>() { "BaseTex" } },
					{ "AvatarHair", new List<string>() { "BaseTex" } },
					{ "AvatarEye", new List<string>() { "BaseTex" } },
					{ "AvatarSimpleLit", new List<string>() { "BaseTex" } },
					{ "AvatarBake", new List<string>() { "BaseTex" } },
				};
				s_keywordPair = new Dictionary<string, string>()
				{
					{ "_Tonemapping", "_ACES" },
				};
			}

			public static Dictionary<string, OfficialShaderTheme> s_officialShaderNameThemes = null;
			public static Dictionary<OfficialShaderTheme, string> s_officialShaderThemeNames = null;

			public static Dictionary<string, string> s_shaderNames = null;

			public static Dictionary<string, string> s_shaderType1s = null;

			public static Dictionary<OfficialShaderTheme, string> s_officialMaterialThemePostfixes = null;

			public static Dictionary<string, List<string>> s_officialShaderTextures = null;

			public static Dictionary<string, string> s_keywordPair = null;
		};

		public static class AvatarManager
		{
			public static bool IsAvatarLitShader(OfficialShaderTheme theme)
            {
				return theme >= OfficialShaderTheme.PicoAvatarLit && theme != OfficialShaderTheme.Invalid;
            }
		}


		// All const number placed here.
		internal enum Consts
		{
			// Maximum submesh count.
			kMaxSubmeshCount = 8,
		}

		/// <summary>
		/// Operation error code from native call
		/// </summary>
		public enum NativeResult
		{

			Success = 0,
		    Failed = -1,

		    //CommonError
		    NotInitialized = 1000,
		    AlreadyInitialized,
		    MismatchVersion,
		    CreateFailed,
		    NullReferenceObject,
		    LoadObjectFailed,
		    ParameterError,
		    NotImplemented,
		    OperationPending,
		    OperationCanceled,
		    InvalidType,
		    IndexOutOfRange,
		    Unsupported,
		    ParameterNotFound,
		    MaterialPropertyNotFound,
		    TransformNotFound,
		    FileNotFound,
		    InvalidState,
		    InvalidData,
		    FatalError,
		    NativeControllerNotFound,
		    SkeletonNotTPose,
		    DataNotFound,
		    ObjectNotExist,
		    ObjectAlreadyDeleted,
		    DuplicatedObject,
		    ObjectStillInUse,
		    SaveDataFailed,
		    //Shader/Material Error
		    MaterialAlreadyExist = 1501,
		    PresetTraitAlreadyExist,
		    //BodyTrackingError
		    DeviceInputReaderNotFound= 1701,
		    //LoadAvatarError
		    OnlyOneMainAvatar = 2301,
		    AvatarAlreadyExist,
		    AvatarLodAlreadyExist,
		    AvatarEntityAlreadyExist,
		    WrongBakingType,
		    LoadedIdNotSameWithParamId,
		    BakedAvatarConfigNotFound,
		    AvatarConfigNotFound,
		    BothConfigNotFound,
		    MainObjectAlreadyDestroyed,
		    AvatarLodDataLoadFailed,
		    SpecGraphIsEmpty,
		    SpecGraphNodeIsEmpty,
		    SkeletonNotReady,
		    SkeletonLoadFailed,
		    SkeletonDataNotFound,
		    SocketJointNameIsNull,
		    SocketProtoNotFound,
		    CreateJointLinkerFailed,
		    CachedClipedBodyDataCorrupted,
		    ClipMeshWithTextureFailed,
		    LoadAvatarFailed,
		    AvatarAlreadyUnloaded,
		    UnloadAvatarFailed,
		    AvatarIdIsInvalid,
		    AvatarLoadContextExist,
		    LastDressUpNotFinished,
		    AvatarNotFound,
		    ConfigJsonNotFound,
		    BasebodyNotFound,
		    MeshesNotFound,
		    StyleSchemaNotFound,
		    BakedAvatarLodDataNotExist,
		    NeedEnabledAllowEdit,
		    AvatarMaskLoadFailed,
		    //AnimationError
		    AnimationSetAssetNotReady = 2501,
		    AnimationSetDataNotFound,
		    AnimatorIsNull,
		    AnimazLoadFailed,
		    AddAnimationSetFailed,
		    ClipNotFound,
		    ClipStillInUse,
		    LoadAnimationFailed,
		    AddAnimationFailed,
		    //NetworkError
		    NetworkRequestFailed = 2701,
		    NetworkRequestCanceled,
		    NetworkServiceNotWorking,
		    //LoadAssetError
		    LoadAssetMetasFailed = 2901,
		    RepeatAddAssets,
		    RepeatAddStyleAssetSet,
		    AssetBundleNotExist,
		    LodLevelInvalid,
		    LodMetaNotFound,
		    AssetLodLevelNotExist,
		    LoadLodLevelDataFailed,
		    LoadAssetBundleFailed,
		    LocalDataFileCorrupted,
		    LoadAssetFailed,
		    LoadZipFileFailed,
		    WriteAssetBundleCacheFailed,
		    AssetDataConfigIsWrong,
		    AvatarVersionAndLodVersionMismatch,
		    LoadManifestFailed,
		    LoadSubAssetFailed,
		    ManifestAsJsonIsNull,
		    MakeupMaterialNotFound,
		    LodLevelDataAlreadyExist,
		    BatchAssetIdFailed,
		    //MemoryError
		    OutOfMemory = 3101,
		    BufferTooSmall,
		}

		// target engine type. Unity/Unreal/AmazingEngine./...
		public enum TargetEngineType : uint
		{
			/// AmazingEngine
			AmazingEngine = 0,

			/// Unity
			Unity = 1,

			/// Unreal
			Unreal = 2,
		}


		/// <summary>
		/// Root of resource or where to a resource file located.
		/// </summary>
		public enum ResourceRootType : uint
		{
			/// <summary>
			/// Located in binary part
			/// </summary>
			Bin = 0,

			/// <summary>
			/// Located in StreamingAssets
			/// </summary>
			ReadOnlyAssets = 1,

			/// <summary>
			/// Located in local disk
			/// </summary>
			LocalFileSystem = 2,

			/// <summary>
			/// Located in server
			/// </summary>
			AssetServer = 3,
		};

		/// <summary>
		/// Avatar Sex Type
		/// </summary>
		public enum AvatarSexType
		{
			/// <summary>
			/// Female
			/// </summary>
			Female = 0,

			/// <summary>
			/// Male
			/// </summary>
			Male = 1,
			Both = 3,
			Invalid = -1,
		}

		/// <summary>
		/// How avatar viewed
		/// </summary>
		public enum AvatarViewType : uint
		{
			None = 0,

			/// <summary>
			/// First person view
			/// </summary>
			FirstPerson = 1 << 0,

			/// <summary>
			/// Third person view
			/// </summary>
			ThirdPerson = 1 << 1,
			All = FirstPerson | ThirdPerson
		}

		/**
         * Avatar Node types.
         */
		/// <summary>
		/// Avatar Node types.
		/// </summary>
		public enum AvatarNodeTypes : uint
		{
			/// <summary>
			/// Any
			/// </summary>
			Any = 0,

			/// <summary>
			/// Head, all types in head
			/// </summary>
			Head = 1 << 0,

			/// <summary>
			/// Hand
			/// </summary>
			Hand = 1 << 1,

			/// <summary>
			/// Body
			/// </summary>
			Body = 1 << 2,

			/// <summary>
			/// Face, always companied with Head.
			/// </summary>
			Face = 1 << 3, // always companied with Head

			/// <summary>
			/// Eyebrow, always companied with Head.
			/// </summary>
			Eyebrow = 1 << 4, // always companied with Head

			/// <summary>
			/// Eyeball, always companied with Head.
			/// </summary>
			Eyeball = 1 << 5, // always companied with Head

			/// <summary>
			/// Eyelash, always companied with Head.
			/// </summary>
			Eyelash = 1 << 6, // always companied with Head

			/// <summary>
			/// Hair, always companied with Head.
			/// </summary>
			Hair = 1 << 7, // always companied with Head

			/// <summary>
			/// Tongue, always companied with Head.
			/// </summary>
			Tongue = 1 << 8, // always companied with Head

			/// <summary>
			/// Tooth, always companied with Head.
			/// </summary>
			Tooth = 1 << 9, // always companied with Head


			/// <summary>
			/// All types of clothes.
			/// </summary>
			Clothes = 1 << 10,

			/// <summary>
			/// ClothUp, always companied with Clothes
			/// </summary>
			ClothUp = 1 << 11, // always companied with Clothes

			/// <summary>
			/// ClothDown, always companied with Clothes
			/// </summary>
			ClothDown = 1 << 12, // always companied with Clothes

			/// <summary>
			/// ClothHandGlove, always companied with Clothes
			/// </summary>
			ClothHandGlove = 1 << 13, // always companied with Clothes

			/// <summary>
			/// accessory on body
			/// </summary>
			BodyAccessory = 1 << 14,

			/// <summary>
			/// accessory on head 
			/// </summary>
			HeadAccessory = 1 << 15,

			/// <summary>
			/// Shoe, always companied with Clothes
			/// </summary>
			Shoe = 1 << 20, // always companied with Clothes
		}

		// Bit combines of AvatarNodeTypes
		// 先不导出了，工具会乱
		public enum AvatarNodeTypeCombines : uint
		{
			/// <summary>
			/// Combines of HeadNode
			/// </summary>
			HeadNodes = (int)AvatarNodeTypes.Face |
			            (int)AvatarNodeTypes.Eyeball |
			            (int)AvatarNodeTypes.Eyebrow |
			            (int)AvatarNodeTypes.Eyelash |
			            (int)AvatarNodeTypes.Hair |
			            (int)AvatarNodeTypes.Tongue |
			            (int)AvatarNodeTypes.Tooth |
			            (int)AvatarNodeTypes.HeadAccessory,

			/// <summary>
			/// Combines of ClothNode 
			/// </summary>
			ClothNodes = (int)AvatarNodeTypes.ClothUp |
			             (int)AvatarNodeTypes.ClothDown |
			             (int)AvatarNodeTypes.Shoe,
		}

		/// <summary>
		/// Avatar shader type. "_ShaderType", parameter name is "_ShaderType"
		/// </summary>
		public enum AvatarShaderType
		{
			/// <summary>
			/// Invalid
			/// </summary>
			Invalid = -1,

			/// <summary>
			/// Default type
			/// </summary>
			Body_Base = 0,

			/// <summary>
			/// Body toon
			/// </summary>
			Body_Toon = 1,

			/// <summary>
			/// Eyelash base
			/// </summary>
			Eyelash_Base = 200,

			/// <summary>
			/// Hair base shader, transparent, scene blend
			/// </summary>
			Hair_Base = 300,

			/// <summary>
			/// Kajiya Kay
			/// </summary>
			Hair_KK = 301,

			/// <summary>
			/// Eye base shader
			/// </summary>
			Eye_Base = 400,

			/// <summary>
			/// Cloth base shader
			/// </summary>
			Cloth_Base = 500,

			/// <summary>
			/// Cloth laser shader
			/// </summary>
			Cloth_Laser = 501,

			/// <summary>
			/// Cloth laser shader
			/// </summary>
			Tooth_Base = 600,
		}
		
		public enum OfficialShaderTheme
		{
			PicoPBR = 0,
			PicoNPR,
			PicoAvatarLit = 2,
			PicoAvatarSkin,
			PicoAvatarHair,
			PicoAvatarEye,
			PicoAvatarSimpleLit,
			PicoAvatarBake,
			Invalid = 100
		}

		// Avatar scene blend type, parameter name is "_SceneBlendType"
		internal enum AvatarSceneBlendType
		{
			/// <summary>
			/// Default type
			/// </summary>
			Opaque = 0,

			/// <summary>
			/// Body toon
			/// </summary>
			SrcAlpha_OneMinusSrcAlpha = 1,
			/// Other...
		}

		// Avatar cull mode, parameter name is "_RenderFace"
		internal enum AvatarRenderFace // UnityEditor.BaseShaderGUI.RenderFace
		{
			/// <summary>
			/// Cull back
			/// </summary>
			Front = 2,

			/// <summary>
			/// Cull front
			/// </summary>
			Back = 1,

			/// <summary>
			/// Cull none
			/// </summary>
			Both = 0
		}
		// AvatarZWrite = "_ZWrite";
		// AvatarZTest = "_ZTest";

		//  Avatar pbr smoothness source type, parameter name is "_PBRWorkflow"
		internal enum AvatarPBRWorkflow // UnityEditor.Rendering.Universal.ShaderGUI.LitGUI.WorkflowMode
		{
			/// <summary>
			/// Specular
			/// </summary>
			Specular = 0,

			/// <summary>
			/// Metallic
			/// </summary>
			Metallic = 1,
		}

		// Avatar pbr smoothness source type, parameter name is "_PBRSmoothSource"
		internal enum AvatarPBRSmoothnessSource // UnityEditor.Rendering.Universal.ShaderGUI.LitGUI.SmoothnessMapChannel
		{
			SpecularMetallicAlpha = 0,
			AlbedoAlpha = 1,
		}


		/// <summary>
		/// Bit flags that control animation synchronization.
		/// </summary>
		public enum AvatarAnimationFlags
		{
			/// <summary>
			/// Only broadcast joints over hips joint. enable sdk application to takeover lower body animation, thus sdk can send less joint data to remote sides.
			/// </summary>
			DepressRemoteLowerBodyJoints = 1 << 0,

			/// <summary>
			/// Depress animation for assets such as wings, which has standalone animation.
			/// </summary>
			DepressAdditiveAnimations = 1 << 1,

			/// <summary>
			/// Depress physics for assets such as hairs, which should be driven by physics.
			/// </summary>
			DepressAdditivePhysics = 1 << 2,
		}


		/// <summary>
		/// System level avatar display effects.
		/// </summary>
		public enum AvatarEffectKind
		{
			None = 0,

			/// <summary>
			/// Transparent body with white outline
			/// </summary>
			SimpleOutline = 1,

			/// <summary>
			/// Fully black
			/// </summary>
			Black = 2
		}

		/// <summary>
		/// Avatar memory compact flags.
		/// </summary>
		public enum AvatarMemoryCompactFlags : uint
		{
			/// <summary>
			/// No texture: normal and metallic
			/// </summary>
			NoTex_NormalAndMetal = 1 << 0,

			/// <summary>
			/// Force clear used assets when loaded in editor
			/// </summary>
			ClearUsedAssetsInEditor = 1 << 10,
		};

		// Avatar primitie dirty flags..
		internal enum AvatarPrimitiveDirtyFlags
		{
			MeshVertex = 1,
			MeshTopology = 2,
			MaterialUniforms = 8,
			Textures = 16,
			Morph = 32,
			MeshSocket = 64,
		};

		// Avatar primitie App flags..
		internal enum AvatarPrimitiveControlFlags
		{
			DisableNativeBoneAccumulation = 1 << 0, // 1
		}

		internal enum AvatarTextureWrapMode
		{
			REPEAT,
			CLAMP,
			BORDER,
			MIRROR,
		};

		// Texture information used to create render texture.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct TextureInfo
		{
			/// native texture object handle.
			public System.IntPtr texObject;

			/// native mesh object, need by C# asset manager.
			public uint instanceKey;

			// version.
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte sRGB;
			[MarshalAs(UnmanagedType.I1)] public byte reserveByte2;
			[MarshalAs(UnmanagedType.I1)] public byte reserveByte3;

			/// width.
			public uint width;

			/// height
			public uint height;

			/// AvatarPixelFormat
			public uint format;

			/// AvatarDataType
			public ushort dataType;

			/// AvatarImageType
			public ushort imageType;

			/// mips count
			public ushort mipsCount;

			/// depth count
			public ushort depthCount;

			/// slices count for texture array.
			public ushort slicesCount;

			/// reserved
			public ushort reserved0;

			/// slice face data size including all mip levels.
			public uint sliceFaceByteSize;

			/// sampler
			public sbyte filterMin;

			public sbyte filterMag;
			public sbyte filterMipmap;
			public sbyte reserved1;
			public sbyte wrapModeS;
			public sbyte wrapModeT;
			public sbyte wrapModeR;
			public sbyte maxAnisotropy;

			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
			public byte[] md5;
		};

		// A slice of texture data used to fill content of a texture slice.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct TextureSliceData
		{
			// version.
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserveByte1;
			[MarshalAs(UnmanagedType.I1)] public byte reserveByte2;
			[MarshalAs(UnmanagedType.I1)] public byte reserveByte3;

			/// width.
			public uint width;

			/// height
			public uint height;

			/// format
			public uint format;

			/// which mip layer to fill. if -1, for all.
			public ushort mips;

			/// if cubemap, which face to fill, if depth texture, depth.
			public ushort face;

			/// slice index.
			public ushort slice;

			/// reserved.
			public ushort reserved;

			/// byte size of data.
			public uint dataByteSize;

			// receive data.
			public System.IntPtr data;
		};

		// XForm.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct XForm
		{
			/// positions
			public Vector3 position;

			/// orientations.
			public Quaternion orientation;

			/// scales
			public Vector3 scale;
		};

		// Mesh Information.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MeshInfo
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;

			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			// hash id of root bone name calcualted in native part. default is 0. some asset may has different root bone. bindpose depend on root bone.
			[MarshalAs(UnmanagedType.I4)] public uint rootBoneNameHash;

			/// native mesh object, need by C# asset manager.
			public System.IntPtr meshObject;

			/// native mesh object, need by C# asset manager.
			public uint instanceKey;

			/// vert count
			public uint positionCount;

			/// normal count
			public uint normalCount;

			/// tangent count
			public uint tangentCount;

			/// color count
			public uint colorCount;

			/// uv count
			public uint uvCount;

			/// uv2 count
			public uint uv2Count;

			/// uv3 vertices count
			public uint uv3Count;

			/// uv4 count
			public uint uv4Count;

			/// bone names count
			public uint boneNameCount;

			/// bind poses
			public uint bindPoseBoneCount;

			/// weights
			public uint boneWeightCount;

			/// weights
			public uint boneWeight4_8Count;

			/// var number weights
			public uint bonesPerVertexWeightCount;

			/// sub-mesh count
			public uint subMeshCount;

			/// indicesCount.kMaxSubmeshCount
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
			public uint[] indicesCount;

			public int blendShapeCount;
			public int blendShapeHasVertex;
			public int blendShapeHasNormal;
			public int blendShapeHasTangent;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MergedMeshInfo
		{
            [MarshalAs(UnmanagedType.U1)] public byte version;
            [MarshalAs(UnmanagedType.U1)] public byte weight8;
            [MarshalAs(UnmanagedType.U1)] public byte reserved2;
            [MarshalAs(UnmanagedType.U1)] public byte reserved3;
            public uint positionCount;
            public uint tangentCount;
            public uint colorCount;
            public uint uv1Count;
            public uint uv2Count;
            public uint uv3Count;
            public uint uv4Count;
            public uint indexCount;
			public uint boneCount;
			public uint rootBoneNameHash;
			public Vector3 boundCenter;
			public Vector3 boundSize;
			// public bool boneHas8Weights;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MeshExtraInfo
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			public System.IntPtr blendShapeNames;
		}

		// Bone weight
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct JointWeight
		{
			/// weights[4]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			float[] weights;

			/// indices[4]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
			int[] indices;
		};

		// Mesh data to return.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MeshData
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte useCustomMaterial;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;
			[MarshalAs(UnmanagedType.I4)] public uint reserveInt1;

			/// Vector3*
			public System.IntPtr positions;

			/// Vector3*
			public System.IntPtr normals;

			/// Vector4*
			public System.IntPtr tangents;

			/// Color32*
			public System.IntPtr colors;

			/// Vector2*
			public System.IntPtr uv1;

			/// Vector2*
			public System.IntPtr uv2;

			/// Vector2*
			public System.IntPtr uv3;

			/// Vector2*
			public System.IntPtr uv4;

			/// uint*
			public System.IntPtr boneNameHashes;

			/// uint*
			public System.IntPtr boneNames;

			/// Matrix4x4*
			public System.IntPtr bindPoses;

			/// BoneWeight*
			public System.IntPtr boneWeights;

			/// BoneWeight*
			public System.IntPtr boneWeight4_8s;

			/// uint*
			public System.IntPtr bonesPerVertexList;

			/// BoneWeight1*
			public System.IntPtr bonesPerVertexWeight;

			/// uint**
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U8, SizeConst = 8)]
			public ulong[] indices;

			public System.IntPtr blendShapeDeltaVertices;
			public System.IntPtr blendShapeDeltaNormals;
			public System.IntPtr blendShapeDeltaTangents;
		};

		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MergedMeshRawData
		{
            [MarshalAs(UnmanagedType.U1)] public byte version;
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 3)]
			public byte[] reserved;
            [MarshalAs(UnmanagedType.U4)] public uint reserveInt;
            public System.IntPtr positions;
			public System.IntPtr normals;
			public System.IntPtr tangents;
			public System.IntPtr colors;
			public System.IntPtr uv1;
			public System.IntPtr uv2;
			public System.IntPtr uv3;
			public System.IntPtr uv4;
			public System.IntPtr materialIndices;
			public System.IntPtr invBindPoses;
			public System.IntPtr boneWeights;
			// public System.IntPtr bonesPerVertex;
			public System.IntPtr boneNameHashes;
			public System.IntPtr indices;
		}

		// SkeletonPalette Information.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct JointPaletteData
		{
			/// Matrix4x4f*
			public System.IntPtr jointMatrices;

			/// xform.
			public XForm rootJointXForm;
		};

		/// <summary>
		/// Data format for mesh and morph vertices.
		/// </summary>
		public enum DataFormat
		{
			/// <summary>
			/// Float
			/// </summary>
			Float = 0,

			/// <summary>
			/// Half
			/// </summary>
			Half = 1,

			// 不导出
			Packed_10_10_10_2_snorm = 2,
		}

		// data element types.
		internal enum RenderResourceDataFlags
		{
			Static_HasPosition = 1 << 0,
			Static_HasMorphChunks = 1 << 1,
			Dynamic_HasJointMatrices = 1 << 10,
			Dynamic_HasMorphWeights = 1 << 11,
			Output_HasPosition = 1 << 25,
		};

		// required info to get MorphAndSkinDataInfo.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MorphAndSkinDataRequiredInfo
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			/// required flags
			public uint flags;

			/// specify data format of mesh base vertex + normal + tangent
			public uint meshVertexDataFormat;

			/// specify data format of mesh morph vertex + normal + tangent
			public uint morphVertexDataFormat;
		}

		// layout information of static resource and dynamic simulation data of morph and skin.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MorphAndSkinDataInfo
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			/// specify potential control flags.
			public uint flags;

			/// byte size of static resource buffer. including layout desc + positions + normals + tangents + morph chunks.
			public uint staticBufferByteSize;

			/// byte size of static resource buffer. including layout desc + positions + normals + tangents + morph chunks.
			public uint dynamicBufferByteSize;

			/// byte size of output buffer. including positions + normals + tangents
			public uint outputBufferByteSize;

			/// byte offset of static buffer
			public uint staticBufferOffset;

			/// byte offset of dynamic buffer
			public uint dynamicBufferOffset;

			/// byte size of material data
			public uint materialDataByteSize;
		}

		// bit flags for MorphAndSkinResourceGpuData.flags
		internal enum MorphAndSkinResourceGpuDataFlags
		{
			Default = 0,
			SkinAsTextureRGBAHalf = 1 << 1, // 4 bone weights per vertex
		}

		// static resource data of morph and skin.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MorphAndSkinResourceGpuData
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			/// specify potential control flags.
			public uint flags;

			/// byte size of static resource buffer. including layout desc + positions + normals + tangents + morph chunks.
			public uint staticBufferByteSize;

			/// buffer.
			public System.IntPtr dataBuffer;
		};

		// simulation data of morph and skin data.
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct MorphAndSkinSimulationGpuData
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			/// specify potential control flags.
			public uint flags;

			/// byte size of static resource buffer. including layout desc + positions + normals + tangents + morph chunks.
			public uint dynamicBufferByteSize;

			//
			public System.IntPtr dataBuffer;
			public System.IntPtr owner;
			public System.IntPtr materialData;
			public bool materialDataDirty;
		};


		// ik effector parameters
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct IKEffectorParameter
		{
			/// ik position weight
			public float ikPositionWeight;

			/// bfloat type animation parameters
			public float ikRotationWeight;
		};

		// local avatar animation config
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct LocalAnimationConfig
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			public float ikInterpDelayTime;

			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 16)]
			public byte[] reserved;
		};

		// animation record config
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct AnimationRecordConfig
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			public int recordVersion;
			public float recordInterval;

			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 12)]
			public byte[] reserved;
		};

		// animation playback config
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct AnimationPlaybackConfig
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			public int maxPacketCountPerFrame;
			public float avgDelayTime;
			public float maxPlaybackSpeedRatio;
			public float minPlaybackSpeedRatio;
			public float playbackInterval;
		};

		// face expression record config
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct FaceExpressionRecordConfig
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			public int recordVersion;

			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U1, SizeConst = 8)]
			public byte[] reserved;
		};

		// animation playback config
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct FaceExpressionPlaybackConfig
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			public int maxPacketCountPerFrame;
			public float avgDelayTime;
			public float maxPlaybackSpeedRatio;
			public float minPlaybackSpeedRatio;
			public float playbackInterval;
		};

		// device input data
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		public struct DeviceData
		{
			[MarshalAs(UnmanagedType.I1)] public byte version;
			[MarshalAs(UnmanagedType.I1)] public byte reserve1;
			[MarshalAs(UnmanagedType.I1)] public byte reserve2;
			[MarshalAs(UnmanagedType.I1)] public byte reserve3;

			/// bool[]
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4,
				SizeConst = (int)IDeviceInputReader.DeviceType.Count)]
			public uint[] connectionStatus;

			/// bool[]
			[MarshalAs(UnmanagedType.ByValArray, ArraySubType = UnmanagedType.U4,
				SizeConst = (int)IDeviceInputReader.ControllerButtons.Count)]
			public uint[] controllerButtonStatus;

			/// Vector3[]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)IDeviceInputReader.DeviceType.Count)]
			public Vector3[] positions;

			/// Quaternion[]
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = (int)IDeviceInputReader.DeviceType.Count)]
			public Quaternion[] orientations;
		};

		// to transfer binary pointer
		[StructLayout(LayoutKind.Sequential, Pack = 4)]
		struct BinaryData
		{
			public System.IntPtr data;
			public int len;
		};
	}
}