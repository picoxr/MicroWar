using System.Runtime.InteropServices;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using System;
using Unity.Collections;

public static class ASTCEncodeHelper
{
	private const uint MAGIC_FILE_CONSTANT = 0x5CA1AB13;
	private const int THREAD_NUM_X = 8;
	private const int THREAD_NUM_Y = 8;
	private const int BLOCK_BYTES = 16;

	private const string sampleName = "ASTC-Encode";

	private static class ShaderProperty
	{
		public static int OutBuffer = Shader.PropertyToID("OutBuffer");
		public static int InTexture = Shader.PropertyToID("InTexture");

		public static int TexelHeight = Shader.PropertyToID("InTexelHeight");
		public static int TexelWidth = Shader.PropertyToID("InTexelWidth");
		public static int GroupNumX = Shader.PropertyToID("InGroupNumX");
		public static int Lod = Shader.PropertyToID("InLod");
		public static int Offset = Shader.PropertyToID("InOffset");
	}

	public struct EncodeOption
	{
		public bool is4x4;
		public bool is6x6;
		public bool is_normal_map;
		public bool has_alpha;
		public bool srgb;
		public bool general_mip;
	}

	private struct astc_header
	{
		public uint magic;
		public byte blockdim_x;
		public byte blockdim_y;
		public byte blockdim_z;
		public byte xsize0;
		public byte xsize1;
		public byte xsize2;
		public byte ysize0;
		public byte ysize1;
		public byte ysize2;
		public byte zsize0;
		public byte zsize1;
		public byte zsize2;
	};

	private struct ktx_header
	{
		public uint magic0;
		public uint magic1;
		public uint magic2;
		public uint endianness;                // should be 0x04030201; if it is instead 0x01020304, then the endianness of everything must be switched.
		public uint gl_type;                   // 0 for compressed textures, otherwise value from table 3.2 (page 162) of OpenGL 4.0 spec
		public uint gl_type_size;              // size of data elements to do endianness swap on (1=endian-neutral data)
		public uint gl_format;                 // 0 for compressed textures, otherwise value from table 3.3 (page 163) of OpenGL spec
		public uint gl_internal_format;        // sized-internal-format, corresponding to table 3.12 to 3.14 (pages 182-185) of OpenGL spec
		public uint gl_base_internal_format;   // unsized-internal-format: corresponding to table 3.11 (page 179) of OpenGL spec
		public uint pixel_width;               // texture dimensions; not rounded up to block size for compressed.
		public uint pixel_height;              // must be 0 for 1D textures.
		public uint pixel_depth;               // must be 0 for 1D, 2D and cubemap textures.
		public uint number_of_array_elements;  // 0 if not a texture array
		public uint number_of_faces;           // 6 for cubemaps, 1 for non-cubemaps
		public uint number_of_mipmap_levels;   // 0 or 1 for non-mipmapped textures; 0 indicates that auto-mipmap-gen should be done at load time.
		public uint bytes_of_key_value_data;   // size in bytes of the key-and-value area immediately following the header.
	};


	private static byte[] StructToByteArray<T>(T struct_)
	{
		int size = Marshal.SizeOf<T>();
		byte[] array = new byte[size];
		GCHandle handle = GCHandle.Alloc(struct_, GCHandleType.Pinned);
		Marshal.Copy(handle.AddrOfPinnedObject(), array, 0, size);
		handle.Free();
		return array;
	}

	public static void SaveKTX(string ktx_path, int xdim, int ydim, int xsize, int ysize, bool sRGB, ref NativeArray<byte> buffer, List<int> bufferSizeList)
	{
		ktx_header hdr = new ktx_header();
		hdr.magic0 = 0x58544BAB;
		hdr.magic1 = 0xBB313120;
		hdr.magic2 = 0x0A1A0A0D;

		hdr.endianness = 0x04030201;
		hdr.gl_type = 0;
		hdr.gl_type_size = 1;
		hdr.gl_format = 0;
		hdr.gl_internal_format = (uint)(sRGB ? 0x93D0 : 0x93B0);
		hdr.gl_base_internal_format = 0x1908;
		hdr.pixel_width = (uint)xsize;
		hdr.pixel_height = (uint)ysize;
		hdr.pixel_depth = 0;
		hdr.number_of_array_elements = 0;
		hdr.number_of_faces = 1;
		hdr.number_of_mipmap_levels = (uint)bufferSizeList.Count;
		hdr.bytes_of_key_value_data = 0;

		string filePath = Path.Combine(Application.persistentDataPath, ktx_path);
		using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
		{
			var head = StructToByteArray(hdr);
			fileStream.Write(head, 0, head.Length);
			int offset = 0;
			var bytes = buffer.ToArray();
			foreach (var bufferSize in bufferSizeList)
			{
				var bufferLen = StructToByteArray(bufferSize);
				fileStream.Write(bufferLen, 0, bufferLen.Length);
				fileStream.Write(bytes, offset, bufferSize);
				offset += bufferSize;
			}
		}
	}

	public static void SaveASTC(string astc_path, int xdim, int ydim, int xsize, int ysize, ref NativeArray<byte> buffer, List<int> bufferSizeList)
	{
		byte[] astc_header = new byte[16];

		astc_header[0] = (byte)(MAGIC_FILE_CONSTANT & 0xFF);
		astc_header[1] = (byte)((MAGIC_FILE_CONSTANT >> 8) & 0xFF);
		astc_header[2] = (byte)((MAGIC_FILE_CONSTANT >> 16) & 0xFF);
		astc_header[3] = (byte)((MAGIC_FILE_CONSTANT >> 24) & 0xFF);

		astc_header[4] = (byte)(xdim);
		astc_header[5] = (byte)(ydim);
		astc_header[6] = 1;

		astc_header[7] = (byte)(xsize & 0xFF);
		astc_header[8] = (byte)((xsize >> 8) & 0xFF);
		astc_header[9] = (byte)((xsize >> 16) & 0xFF);
		astc_header[10] = (byte)(ysize & 0xFF);
		astc_header[11] = (byte)((ysize >> 8) & 0xFF);
		astc_header[12] = (byte)((ysize >> 16) & 0xFF);
		astc_header[13] = 1;
		astc_header[14] = 0;
		astc_header[15] = 0;

		string filePath = Path.Combine(Application.persistentDataPath, astc_path);
		using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
		{
			fileStream.Write(astc_header, 0, astc_header.Length);
			fileStream.Write(buffer.ToArray(), 0, bufferSizeList[0] * BLOCK_BYTES);
		}
	}

	public static ComputeBuffer FetchASTCResult()
	{
		return ComputeBufferPoolManager.Fetch("ASTC_TEX");
	}

	public static List<int> Encode(CommandBuffer cmd ,EncodeOption option, RenderTargetIdentifier rtid, int texWidth, int texHeight,  ComputeShader astcEncoderShader)
	{
		int TexMaxWidth = texWidth;
		int TexMaxHeight = texHeight;
		int DimSize = option.is4x4 ? 4 : 6;

		int mipCount = 0;
		int TotalBlockNumMips = 0;
		for (int i = 0; ; ++i)
		{
			if ((TexMaxWidth >> i) == 0 || (TexMaxHeight >> i) == 0) break;
			int TexWidth = Math.Max(TexMaxWidth >> i , 4) ;
			int TexHeight = Math.Max(TexMaxHeight >> i , 4);
			int xBlockNum = (TexWidth + DimSize - 1) / DimSize;
			int yBlockNum = (TexHeight + DimSize - 1) / DimSize;
			if (xBlockNum == 0 || yBlockNum == 0) break;

			++mipCount;
			TotalBlockNumMips += xBlockNum * yBlockNum;

			if (!option.general_mip) break;
		}
		
		cmd.BeginSample(sampleName);

		var mipSizeList = new List<int>(mipCount);
		var outputBuffer = ComputeBufferPoolManager.Get("ASTC_TEX",TotalBlockNumMips, BLOCK_BYTES, ComputeBufferType.Structured);
		
		for (int i = 0, BlockOffset = 0; i < mipCount; ++i)
		{
			int TexWidth = Math.Max(TexMaxWidth >> i, 4);
			int TexHeight = Math.Max(TexMaxHeight >> i, 4);
			int xBlockNum = (TexWidth + DimSize - 1) / DimSize;
			int yBlockNum = (TexHeight + DimSize - 1) / DimSize;
			int TotalBlockNum = xBlockNum * yBlockNum;

			int GroupSize = THREAD_NUM_X * THREAD_NUM_Y;
			int GroupNum = (TotalBlockNum + GroupSize - 1) / GroupSize;
			int GroupNumX = xBlockNum;
			int GroupNumY = (GroupNum + GroupNumX - 1) / GroupNumX;

			cmd.SetComputeIntParam(astcEncoderShader, ShaderProperty.TexelHeight, TexHeight);
			cmd.SetComputeIntParam(astcEncoderShader, ShaderProperty.TexelWidth, TexWidth);
			cmd.SetComputeIntParam(astcEncoderShader, ShaderProperty.GroupNumX, GroupNumX);
			cmd.SetComputeIntParam(astcEncoderShader, ShaderProperty.Lod, i);
			cmd.SetComputeIntParam(astcEncoderShader, ShaderProperty.Offset, BlockOffset);

			BlockOffset += TotalBlockNum;

			mipSizeList.Add(TotalBlockNum * BLOCK_BYTES);

			cmd.SetComputeBufferParam(astcEncoderShader, 0, ShaderProperty.OutBuffer, outputBuffer);
			cmd.SetComputeTextureParam(astcEncoderShader, 0, ShaderProperty.InTexture, rtid);
			cmd.DispatchCompute(astcEncoderShader, 0, GroupNumX, GroupNumY, 1);
		}

		cmd.EndSample(sampleName);

		return mipSizeList;
	}
}