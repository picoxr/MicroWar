using System.Diagnostics;
using UnityEngine;
using UnityEngine.Rendering;

public class MergeMaterialAndBakeColorRegionEntity
{
	public Mesh mesh;
	public Material[] materials;
	public Renderer renderer;
	
	public Shader mergeTextureShader;
	public ComputeShader astcEncoderShader;
	public ComputeShader blendTexturesShader;

	private Texture2D astcTex;
	private Material material;

	private Stopwatch stopwatch;

	private static int s_ColorShift = Shader.PropertyToID("_ColorShift");
	void OnComplete(AsyncGPUReadbackRequest request)
	{
		if (request.done)
		{
			astcTex.Apply(false, true);
			material.SetTexture("_BaseMap", astcTex);
			SetMaterialIsBaked();
			var m = new Material[1];
			m[0] = material;
			renderer.sharedMaterials = m;

			stopwatch.Stop();
			float elapsedMs = stopwatch.ElapsedMilliseconds;
			UnityEngine.Debug.Log($"MergeMaterialAndBakeColorRegionEntity execution time: {elapsedMs} ms");
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

	public bool Execute()
	{
		if (mesh.subMeshCount == materials.Length)
		{
			return false;
		}

		stopwatch = new Stopwatch();
		stopwatch.Start();
		var BaseMap = new Texture[materials.Length];
		var ColorRegion = new Texture[materials.Length];
		var Region1 = new Vector4[materials.Length];
		var Region2 = new Vector4[materials.Length];
		var Region3 = new Vector4[materials.Length];
		var Region4 = new Vector4[materials.Length];
		var albedoHue = new float[materials.Length];
		for (var i = 0; i < materials.Length; ++i)
		{
			var mat = materials[i];
			BaseMap[i] = mat.GetTexture("_BaseMap");
			ColorRegion[i] = mat.GetTexture("_ColorRegionMap");
			Region1[i] = mat.GetVector("_ColorRegion1");
			Region2[i] = mat.GetVector("_ColorRegion2");
			Region3[i] = mat.GetVector("_ColorRegion3");
			Region4[i] = mat.GetVector("_ColorRegion4");
			albedoHue[i] = mat.GetFloat("_UsingAlbedoHue");
		}
		material = new Material(materials[0]);
		var cmd = new CommandBuffer();
		cmd.name = "Merge Materials  Command";
		var mipCount = BlendTextureHelper.MergeBaseMaps(cmd, new Material(mergeTextureShader), blendTexturesShader, astcEncoderShader, BaseMap, ColorRegion, Region1,Region2,Region3,Region4, albedoHue, true, 1.0f);
		astcTex = new Texture2D(BaseMap[0].width, BaseMap[0].height, TextureFormat.ASTC_4x4, mipCount, true);
		var outputDatas = astcTex.GetRawTextureData<byte>();
		cmd.RequestAsyncReadbackIntoNativeArray(ref outputDatas, ComputeBufferPoolManager.Fetch("ASTC_TEX"), OnComplete);
		Graphics.ExecuteCommandBuffer(cmd);
		cmd.Release();
		ComputeBufferPoolManager.RemoveAll();

		return true;
	}
}