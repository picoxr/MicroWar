using Pico.Avatar;
using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class BatchingGPUTask
{
    private Texture BaseMap;
    private Texture ColorRegionMap;
    private Vector4 RegionColor0;
    private Vector4 RegionColor1;
    private Vector4 RegionColor2;
    private Vector4 RegionColor3;
    private float UsingAlbedoHue;
    private ComputeShader AstcEncodeShader;
    private ComputeShader TextureBlendingShader;
    private Texture2D AstcTexture;
    private PicoAvatarMergedRenderMaterial MergedMtl;
    private System.IntPtr NativeMtl;

    public BatchingGPUTask(Texture _BaseMap, 
        Texture _ColorRegionMap, 
        Vector4 _RegionColor0, 
        Vector4 _RegionColor1, 
        Vector4 _RegionColor2, 
        Vector4 _RegionColor3, 
        float _UsingAlbedoHue,
        ComputeShader _Encoder, 
        ComputeShader _Blender, 
        PicoAvatarMergedRenderMaterial _Mtl,
        System.IntPtr _NativeMtl) 
    {
        this.BaseMap = _BaseMap;
        this.ColorRegionMap = _ColorRegionMap;
        this.RegionColor0 = _RegionColor0;
        this.RegionColor1 = _RegionColor1; 
        this.RegionColor2 = _RegionColor2;
        this.RegionColor3 = _RegionColor3;
        this.UsingAlbedoHue = _UsingAlbedoHue;
        this.AstcEncodeShader = _Encoder;
        this.TextureBlendingShader = _Blender;
        this.MergedMtl = _Mtl;
        this.NativeMtl = _NativeMtl;
    }

    internal void Dispose()
    {
        // BaseMap and ColorRegionMap will be destroyed along with RenderMesh in MergedRenderMaterial
        BaseMap = null;
        ColorRegionMap = null;
        if (AstcTexture != null)
        {
            GameObject.Destroy(AstcTexture);
        }
    }

    internal void Execute()
    {
        if (AstcEncodeShader == null || TextureBlendingShader == null) 
        {
            MergedMtl.OnGpuTaskComplete(false, NativeMtl, AstcTexture);
            return;
        }
        if (ColorRegionMap == null)
        {
            ColorRegionMap = Texture2D.blackTexture;
            RegionColor0.w = 0;
            RegionColor1.w = 0;
            RegionColor2.w = 0;
            RegionColor3.w = 0;
        }

		var cmd = new CommandBuffer();
		cmd.name = "Mesh batching: Bake ColorRegion to BaseMap Command";	
		var mipCount = BlendTextureHelper.BlendRegions(cmd, 
            TextureBlendingShader, 
            AstcEncodeShader, 
            BaseMap, 
            ColorRegionMap, 
            RegionColor0,
            RegionColor1,
            RegionColor2,
            RegionColor3, 
            UsingAlbedoHue, 
            true,
            true);
        AstcTexture = new Texture2D(BaseMap.width, BaseMap.height, TextureFormat.ASTC_4x4, mipCount, true);
        var buffer = AstcTexture.GetRawTextureData<byte>();
        cmd.RequestAsyncReadbackIntoNativeArray(ref buffer,
            ComputeBufferPoolManager.Fetch("ASTC_TEX"),
            (request) => MergedMtl.OnGpuTaskComplete(request.done, NativeMtl, AstcTexture));
		Graphics.ExecuteCommandBuffer(cmd);
		cmd.Release();
		ComputeBufferPoolManager.RemoveAll();
    }
}