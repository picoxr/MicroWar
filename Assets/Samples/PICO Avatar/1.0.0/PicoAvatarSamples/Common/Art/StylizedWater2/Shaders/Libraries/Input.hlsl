//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

TEXTURE2D(_IntersectionNoise);
SAMPLER(sampler_IntersectionNoise);
TEXTURE2D(_DepthTex);


CBUFFER_START(UnityPerMaterial)
	float4 _ShallowColor;
	float4 _BaseColor;

	//float _Smoothness;
	//float _Metallic;

	float4 _IntersectionColor;
	float _DepthVertical;
	float _DepthHorizontal;
	float _DepthExp;
	float _WorldSpaceUV;
	float _NormalTiling;
	float _NormalSpeed;
	half4 _DistanceNormalParams;
	half _NormalStrength;
	half4 _TranslucencyParams;
//X: Strength
//Y: Exponent
	half _TranslucencyReflectionMask;
	half _EdgeFade;
	float _WaveSpeed;
	float4 _HorizonColor;
	half _HorizonDistance;
	float _SparkleIntensity;
	half _SparkleSize;
	float _SunReflectionDistortion;
	float _SunReflectionSize;
	float _SunReflectionStrength;
	float _PointSpotLightReflectionExp;
	float _ReflectionDistortion;
	float _ReflectionBlur;
	float _ReflectionFresnel;
	float _ReflectionStrength;
	half _ReflectionLighting;
	half _PlanarReflectionsEnabled;
	half _ShadowStrength;
	float4 _AnimationParams;
	float4 _SlopeParams;
	half _SlopeThreshold;

	//Foam
	float _FoamTiling;
	float4 _FoamColor;
	float _FoamSpeed;
	half _FoamSize;
	half _FoamWaveMask;
	half _FoamWaveMaskExp;

	//Intersection
	half _IntersectionSource;
	half _IntersectionLength;
	half _IntersectionFalloff;
	half _IntersectionTiling;
	half _IntersectionRippleDist;
	half _IntersectionRippleStrength;
	half _IntersectionClipping;
	float _IntersectionSpeed;

	//Waves
	half _WaveHeight;
	half _WaveNormalStr;
	float _WaveDistance;
	half4 _WaveFadeDistance;
	float _WaveSteepness;
	uint _WaveCount;
	half4 _WaveDirection;

	half _ShoreLineWaveStr;
	half _ShoreLineWaveDistance;
	half _ShoreLineLength;

	//Underwater
	half _CausticsBrightness;
	float _CausticsTiling;
	half _CausticsSpeed;
	half _RefractionStrength;
	half _CausticsDistortion;

	half4 _VertexColorMask;
	half _WaveTint;
#ifdef TESSELLATION_ON	
	float _TessValue;
	float _TessMin;
	float _TessMax;
#endif
CBUFFER_END