//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

#if VERSION_GREATER_EQUAL(12,0)
#define bakedLightmapUV staticLightmapUV
#else
#define bakedLightmapUV lightmapUV
#endif

struct Attributes
{
	float4 positionOS 	: POSITION;
	float4 uv 			: TEXCOORD0;
	float4 normalOS 	: NORMAL;
	float4 tangentOS 	: TANGENT;
	float4 color 		: COLOR0;

	float2 bakedLightmapUV   : TEXCOORD1;
	float2 dynamicLightmapUV  : TEXCOORD2;
	
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{	
	float4 uv 			: TEXCOORD0;
	DECLARE_LIGHTMAP_OR_SH(bakedLightmapUV, vertexSH, 8); //Called staticLightmapUV in URP12+

	half4 fogFactorAndVertexLight : TEXCOORD2; // x: fogFactor, yzw: vertex light
	
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) //No shadow cascades
	float4 shadowCoord 	: TEXCOORD3;
	#endif
	
	//wPos.x in w-component
	float4 normalWS 	: NORMAL;
	#if _NORMALMAP
	//wPos.y in w-component
	float4 tangent 		: TANGENT;
	//wPos.z in w-component
	float4 bitangent 	: TEXCOORD4;
	#else
	float3 positionWS 	: TEXCOORD4;
	#endif

	#if defined(SCREEN_POS)
	float4 screenPos 	: TEXCOORD5;
	#endif
	
	#ifdef DYNAMICLIGHTMAP_ON
	float2  dynamicLightmapUV : TEXCOORD9; // Dynamic lightmap UVs
	#endif

	float4 positionCS 	: SV_POSITION;
	float4 color 		: COLOR0;	
	
	UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

Varyings LitPassVertex(Attributes input)
{
	Varyings output = (Varyings)0;

	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_TRANSFER_INSTANCE_ID(input, output);
	UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

#if defined(CURVEDWORLD_IS_INSTALLED) && !defined(CURVEDWORLD_DISABLED_ON) 
#if defined(CURVEDWORLD_NORMAL_TRANSFORMATION_ON)
	CURVEDWORLD_TRANSFORM_VERTEX_AND_NORMAL(input.positionOS, input.normalOS.xyz, input.tangentOS)
#else
    CURVEDWORLD_TRANSFORM_VERTEX(input.positionOS)
#endif
#endif

	output.uv.xy = input.uv.xy;
	output.uv.z = _TimeParameters.x;
	output.uv.w = 0;

	float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
	float3 offset = 0;
	
	#if MODIFIERS_ENABLED
	offset += GetDisplacementOffset(positionWS);
	#endif

	VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS.xyz, input.tangentOS);
	
	float4 vertexColor = GetVertexColor(input.color.rgba, _VertexColorMask.rgba);
	
#if _WAVES && !defined(TESSELLATION_ON)
	float2 uv = GetSourceUV(input.uv.xy, positionWS.xz, _WorldSpaceUV);

	//Vertex animation
	WaveInfo waves = GetWaveInfo(uv, TIME_VERTEX * _WaveSpeed, _WaveHeight, lerp(1, 0, vertexColor.b), _WaveFadeDistance.x, _WaveFadeDistance.y);
	//Offset in direction of normals (only when using mesh uv)
	if(_WorldSpaceUV == 0) waves.position *= normalInput.normalWS.xyz;
	
	offset += waves.position.xyz;
#endif

	//SampleWaveSimulationVertex(positionWS, positionWS.y);

	//Apply vertex displacements
	positionWS += offset;

	output.positionCS = TransformWorldToHClip(positionWS);
	half fogFactor = CalculateFogFactor(output.positionCS.xyz);

#ifdef SCREEN_POS
	output.screenPos = ComputeScreenPos(output.positionCS);
#endif
	
	output.normalWS = float4(normalInput.normalWS, positionWS.x);
#if _NORMALMAP
	output.tangent = float4(normalInput.tangentWS, positionWS.y);
	output.bitangent = float4(normalInput.bitangentWS, positionWS.z);
#else
	output.positionWS = positionWS.xyz;
#endif

	//Lambert shading
	half3 vertexLight = 0;
#ifdef _ADDITIONAL_LIGHTS_VERTEX
	vertexLight = VertexLighting(positionWS, normalInput.normalWS);
#endif

	output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);
	output.color = vertexColor;

	//"bakedLightmapUV" resolves to "staticLightmapUV" in URP12+
	OUTPUT_LIGHTMAP_UV(input.bakedLightmapUV, unity_LightmapST, output.bakedLightmapUV);
	OUTPUT_SH(output.normalWS.xyz, output.vertexSH);
	
	#ifdef DYNAMICLIGHTMAP_ON
	output.dynamicLightmapUV = input.dynamicLightmapUV.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
	#endif

#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR)
	VertexPositionInputs vertexInput = (VertexPositionInputs)0;
	vertexInput.positionWS = positionWS;
	vertexInput.positionCS = output.positionCS;
	output.shadowCoord = GetShadowCoord(vertexInput);
#endif
	
	return output;
}