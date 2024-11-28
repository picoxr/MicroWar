using System.Diagnostics;
using System.Security.Cryptography;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using Pico.Avatar;

public class BakeColorRegionEntity
{
	public Material material;
	public Texture BaseMap;
	public Texture ColorRegion;
	public Vector4 Region1;
	public Vector4 Region2;
	public Vector4 Region3;
	public Vector4 Region4;
	public float albedoHue;
	public ComputeShader astcEncoderShader;
	public ComputeShader blendTexturesShader;

	public Texture2D astcTex;
	private AvatarTexture avatarTexture;

	private Stopwatch stopwatch;

	public void ReleaseManual() 
	{
		material = null;
		BaseMap = null;
		ColorRegion = null;
		if (avatarTexture != null)
		{
			ReferencedObject.ReleaseField(ref avatarTexture);
		}
		if (astcTex != null)
		{
			GameObject.Destroy(astcTex);
			astcTex = null;
		}
	}

	private static int s_ColorShift = Shader.PropertyToID("_ColorShift");
	void OnComplete(AsyncGPUReadbackRequest request)
	{
		if (request.done)
		{
			// Calculate md5 for baked texture
            var outputDatas = astcTex.GetRawTextureData<byte>();
			var md5 = MD5.Create();
			var md5Hash = md5.ComputeHash(outputDatas.ToArray());
            string textureCacheKey = "md5+" + System.String.Join("", md5Hash.Select(v=>v.ToString("X2")).ToArray());
			// Check if baked texture already cached
			if (AvatarTexture.CreateAndRefTexture(textureCacheKey, astcTex, (uint)outputDatas.Length, out avatarTexture))
			{
				material.SetTexture("_BaseMap", avatarTexture.runtimeTexture);
				// Baked texture is already cached, we don't need astcTex
				GameObject.Destroy(astcTex);
			}
			else
			{
				// Baked texture is cached when current request done
                astcTex.Apply(false, true);
                material.SetTexture("_BaseMap", astcTex);
			}
            astcTex = null;
			SetMaterialIsBaked();
			stopwatch.Stop();
			float elapsedMs = stopwatch.ElapsedMilliseconds;
			UnityEngine.Debug.Log($"BakeColorRegionEntity execution time: {elapsedMs} ms");
		}
	}

	void SetMaterialIsBaked() 
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

	public void Execute()
	{
		stopwatch = new Stopwatch();
		stopwatch.Start();
		if (BaseMap == null
		|| ColorRegion == null
		|| blendTexturesShader == null
		|| astcEncoderShader == null
		|| (Region1.w < Mathf.Epsilon
		&& Region2.w < Mathf.Epsilon
		&& Region3.w < Mathf.Epsilon
		&& Region4.w < Mathf.Epsilon)) { SetMaterialIsBaked(); return; }
		
		var cmd = new CommandBuffer();
		cmd.name = "Bake ColorRegion to BaseMap Command";	
		var mipCount = BlendTextureHelper.BlendRegions(cmd, blendTexturesShader, astcEncoderShader, BaseMap, ColorRegion, Region1,Region2,Region3,Region4, albedoHue, true, false);
		astcTex = new Texture2D(BaseMap.width, BaseMap.height, TextureFormat.ASTC_4x4, mipCount, true);
		var outputDatas = astcTex.GetRawTextureData<byte>();
		cmd.RequestAsyncReadbackIntoNativeArray(ref outputDatas, ComputeBufferPoolManager.Fetch("ASTC_TEX"), OnComplete);
		Graphics.ExecuteCommandBuffer(cmd);
		cmd.Release();
		ComputeBufferPoolManager.RemoveAll();
	}
}