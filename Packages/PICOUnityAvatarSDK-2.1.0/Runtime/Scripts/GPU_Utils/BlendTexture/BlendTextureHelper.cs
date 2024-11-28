using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
public static class BlendTextureHelper 
{
    private const string bakeColorRegionName = "Bake Color Region Texture";
    private const string mergeBaseMapName = "Merge Base Map Texture";

    private static class ShaderProperty
    {
        public static int RWTex = Shader.PropertyToID("RWTex");
        public static int region1 = Shader.PropertyToID("region1");
        public static int region2 = Shader.PropertyToID("region2");
        public static int region3 = Shader.PropertyToID("region3");
        public static int region4 = Shader.PropertyToID("region4");
        public static int userData = Shader.PropertyToID("userData");

        public static int outTex = Shader.PropertyToID("OutTex");
        public static int baseTex = Shader.PropertyToID("BaseTex");
        public static int colorRegionTex = Shader.PropertyToID("ColorRegionTex");
    }

    private static class MergeTextureShaderProperty 
    {
        public static int _ScaleBias = Shader.PropertyToID("_ScaleBias");
        public static int _SourceTex = Shader.PropertyToID("_SourceTex");
        public static int _MergeRenderTex = Shader.PropertyToID("_MergeRenderTex");
        public static int _Weight = Shader.PropertyToID("_Weight");
    }

    private static class MipShaderProperty
    {
        public static int InTex = Shader.PropertyToID("InTex");
        public static int OuTex = Shader.PropertyToID("OuTex");
        public static int invTexelSize = Shader.PropertyToID("invTexelSize");
        public static int mipLevel = Shader.PropertyToID("mipLevel");
    }

    struct ConstantBuffer 
    {
        public Vector4 region1;
        public Vector4 region2;
        public Vector4 region3;
        public Vector4 region4;
        public Vector4 userData;  // x: albedoHue , y: isNewShaderTheme, zw : BaseTex texelSize
    }

    public static int MergeBaseMaps(CommandBuffer cmd,  Material mergeTexShaderMat,  ComputeShader blendCompute, ComputeShader astcCompressCompute, Texture[] baseTex, Texture[] colorRegionTex, Vector4[] region1, Vector4[] region2, Vector4[] region3, Vector4[] region4, float[] albedoHue, bool isNewShaderTheme, float weight)
    {
        var rtDesc = new RenderTextureDescriptor(baseTex[0].width, baseTex[0].height);
        rtDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
        rtDesc.mipCount = baseTex[0].mipmapCount;
        rtDesc.msaaSamples = 1;
        rtDesc.sRGB = false;
        rtDesc.useMipMap = true;
        rtDesc.autoGenerateMips = false;
        rtDesc.dimension = TextureDimension.Tex2D;
        rtDesc.depthBufferBits = 0;

        cmd.GetTemporaryRT(MergeTextureShaderProperty._MergeRenderTex, rtDesc);

        for (var i = 0; i < baseTex.Length; ++i)
        {
            MergeTexture(cmd, MergeTextureShaderProperty._MergeRenderTex, "MergeBaseMap_Const_Buff_" + i, mergeTexShaderMat, 0, blendCompute, baseTex[i], colorRegionTex[i], region1[i], region2[i], region3[i], region4[i], albedoHue[i], isNewShaderTheme, false, weight);
        }

        cmd.GenerateMips(MergeTextureShaderProperty._MergeRenderTex);

        ASTCEncodeHelper.EncodeOption option;
        option.general_mip = true;
        option.has_alpha = true;
        option.is4x4 = true;
        option.is6x6 = false;
        option.is_normal_map = false;
        option.srgb = true;

        var mipSizeList = ASTCEncodeHelper.Encode(cmd, option, MergeTextureShaderProperty._MergeRenderTex, rtDesc.width, rtDesc.height, astcCompressCompute);

        cmd.ReleaseTemporaryRT(MergeTextureShaderProperty._MergeRenderTex);

        return mipSizeList.Count;
    }

    static void MergeTexture(CommandBuffer cmd, RenderTargetIdentifier rt ,string constBufferName, Material mergeTexShaderMat, int pass, ComputeShader blendCompute,Texture baseTex, Texture colorRegionTex, Vector4 region1, Vector4 region2, Vector4 region3, Vector4 region4, float albedoHue, bool isNewShaderTheme, bool needBakeColorRegion, float weight)
    {
        if (needBakeColorRegion)
        {
            // alloc buffers
            var rtDesc = new RenderTextureDescriptor(baseTex.width, baseTex.height);
            rtDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
            rtDesc.mipCount = baseTex.mipmapCount;
            rtDesc.msaaSamples = 1;
            rtDesc.sRGB = false;
            rtDesc.useMipMap = true;
            rtDesc.autoGenerateMips = false;
            rtDesc.dimension = TextureDimension.Tex2D;
            rtDesc.depthBufferBits = 0;
            rtDesc.enableRandomWrite = true;
            cmd.GetTemporaryRT(ShaderProperty.RWTex, rtDesc);

            //exec gpu actions 
            cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region1, region1);
            cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region2, region2);
            cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region3, region3);
            cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region4, region4);
            cmd.SetComputeVectorParam(blendCompute, ShaderProperty.userData, new Vector4(albedoHue, isNewShaderTheme ? 1f : 0f, 1.0f / baseTex.width, 1.0f / baseTex.height));
            cmd.SetComputeTextureParam(blendCompute, 0, ShaderProperty.baseTex, baseTex);
            cmd.SetComputeTextureParam(blendCompute, 0, ShaderProperty.colorRegionTex, colorRegionTex);
            cmd.SetComputeTextureParam(blendCompute, 0, ShaderProperty.outTex, ShaderProperty.RWTex, 0, RenderTextureSubElement.Color);
            blendCompute.GetKernelThreadGroupSizes(0, out uint threadGroupX, out uint threadGroupY, out uint unused);
            cmd.DispatchCompute(blendCompute, 0, (int)((baseTex.width + (threadGroupX - 1)) / threadGroupX), (int)((baseTex.height + (threadGroupY - 1)) / threadGroupY), 1);

            bool yflip = false;
            float flipSign = yflip ? -1.0f : 1.0f;
            Vector4 scaleBiasRt = (flipSign < 0.0f)
                ? new Vector4(flipSign, 1.0f, -1.0f, 1.0f)
                : new Vector4(flipSign, 0.0f, 1.0f, 1.0f);
            cmd.SetGlobalVector(MergeTextureShaderProperty._ScaleBias, scaleBiasRt);
            cmd.SetGlobalFloat(MergeTextureShaderProperty._Weight, weight);
            cmd.SetGlobalTexture(MergeTextureShaderProperty._SourceTex, ShaderProperty.RWTex);
            cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawProcedural(Matrix4x4.identity, mergeTexShaderMat, pass, MeshTopology.Quads, 4, 1, null);

            cmd.ReleaseTemporaryRT(ShaderProperty.RWTex);
        }
         else
		{
            bool yflip = false;
            float flipSign = yflip ? -1.0f : 1.0f;
            Vector4 scaleBiasRt = (flipSign < 0.0f)
                ? new Vector4(flipSign, 1.0f, -1.0f, 1.0f)
                : new Vector4(flipSign, 0.0f, 1.0f, 1.0f);
            cmd.SetGlobalVector(MergeTextureShaderProperty._ScaleBias, scaleBiasRt);
            cmd.SetGlobalFloat(MergeTextureShaderProperty._Weight, weight);
            cmd.SetGlobalTexture(MergeTextureShaderProperty._SourceTex, baseTex);
            cmd.SetRenderTarget(new RenderTargetIdentifier(rt, 0, CubemapFace.Unknown, -1),
                    RenderBufferLoadAction.Load, RenderBufferStoreAction.Store, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.DontCare);
            cmd.DrawProcedural(Matrix4x4.identity, mergeTexShaderMat, pass, MeshTopology.Quads, 4, 1, null);
        }
    }

    public static int BlendRegions(CommandBuffer cmd, ComputeShader blendCompute, ComputeShader astcCompressCompute, Texture baseTex, Texture colorRegionTex, Vector4 region1, Vector4 region2, Vector4 region3, Vector4 region4, float albedoHue, bool isNewShaderTheme, bool srgb)
    {
       
        cmd.BeginSample(bakeColorRegionName);

        // alloc buffers
        var rtDesc = new RenderTextureDescriptor(baseTex.width, baseTex.height);
        rtDesc.graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB;
        rtDesc.mipCount = baseTex.mipmapCount;
        rtDesc.msaaSamples = 1;
        rtDesc.sRGB = false;
        rtDesc.useMipMap = true;
        rtDesc.autoGenerateMips = false;
        rtDesc.dimension = TextureDimension.Tex2D;
        rtDesc.depthBufferBits = 0;
        rtDesc.enableRandomWrite = true;
        cmd.GetTemporaryRT(ShaderProperty.RWTex, rtDesc);

        //exec gpu actions 
        cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region1, region1);
        cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region2, region2);
        cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region3, region3);
        cmd.SetComputeVectorParam(blendCompute, ShaderProperty.region4, region4);
        // cmd.SetComputeVectorParam(blendCompute, ShaderProperty.userData, new Vector4(albedoHue, isNewShaderTheme ? 1f : 0f, 1.0f / baseTex.width, 1.0f / baseTex.height));
        cmd.SetComputeVectorParam(blendCompute, ShaderProperty.userData, new Vector4(albedoHue, srgb ? 1.0f : 2.2f, 1.0f / baseTex.width, 1.0f / baseTex.height));

        cmd.SetComputeTextureParam(blendCompute, 0, ShaderProperty.baseTex, baseTex);
        cmd.SetComputeTextureParam(blendCompute, 0, ShaderProperty.colorRegionTex, colorRegionTex);
        cmd.SetComputeTextureParam(blendCompute, 0, ShaderProperty.outTex, ShaderProperty.RWTex, 0, RenderTextureSubElement.Color);
        blendCompute.GetKernelThreadGroupSizes(0, out uint threadGroupX, out uint threadGroupY, out uint unused);
        cmd.DispatchCompute(blendCompute, 0, (int)((baseTex.width + (threadGroupX - 1)) / threadGroupX), (int)((baseTex.height + (threadGroupY - 1)) / threadGroupY), 1);

        cmd.GenerateMips(ShaderProperty.RWTex);

        ASTCEncodeHelper.EncodeOption option;
        option.general_mip = true;
        option.has_alpha = true;
        option.is4x4 = true;
        option.is6x6 = false;
        option.is_normal_map = false;
        option.srgb = true;

        var mipSizeList = ASTCEncodeHelper.Encode(cmd, option, ShaderProperty.RWTex, rtDesc.width, rtDesc.height, astcCompressCompute);

        cmd.ReleaseTemporaryRT(ShaderProperty.RWTex);
        cmd.EndSample(bakeColorRegionName);

        return mipSizeList.Count;
    }
}
