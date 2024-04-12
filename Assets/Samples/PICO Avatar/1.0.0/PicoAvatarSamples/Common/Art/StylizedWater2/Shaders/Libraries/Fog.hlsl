/* Configuration: UnityFog */

//Set this to value to 1 through Shader.SetGlobalFloat to temporarily disable fog for water
float _WaterFogDisabled;

//Authors of third-party fog solutions can reach out to have their method integrated here

/* start UnityFog */
#define UnityFog
/* end UnityFog */

/* start Colorful */
//#define Colorful
/* end Colorful */

/* start Enviro */
//#define Enviro
/* end Enviro */

/* start Azure */
//#define Azure
/* end Azure */

/* start AtmosphericHeightFog */
//#define AtmosphericHeightFog
/* end AtmosphericHeightFog */

/* start SCPostEffects */
//#define SCPostEffects
/* end SCPostEffects */

/* start COZY */
//#define COZY
/* end COZY */

#ifdef Colorful
/* include Colorful */
#include "Assets/ColorfulSky/Shaders/Libraries/Fog.hlsl"
#endif

#ifdef Enviro
/* include Enviro */
#include "Assets/Enviro - Sky and Weather/Core/Resources/Shaders/Core/EnviroFogCore.hlsl"
#endif

#ifdef Azure
/* include Azure */
#include "Assets/Azure[Sky] Dynamic Skybox/Shaders/Transparent/AzureFogCore.cginc"
#endif

#ifdef AtmosphericHeightFog
/* include AtmosphericHeightFog */
#include "Assets/BOXOPHOBIC/Atmospheric Height Fog/Core/Includes/AtmosphericHeightFog.cginc"
#endif

#ifdef SCPostEffects

#ifndef VERSION_2_2_1 //These macros are no longer needed. Kept in place for backwards compatibility
#define DECLARE_TEX(textureName) TEXTURE2D(textureName);
#define DECLARE_RT(textureName) TEXTURE2D_X(textureName);
SAMPLER(sampler_LinearClamp);
SAMPLER(sampler_LinearRepeat);
#define Clamp sampler_LinearClamp
#define Repeat sampler_LinearRepeat
#define SAMPLE_TEX(textureName, samplerName, uv) SAMPLE_TEXTURE2D_LOD(textureName, samplerName, uv, 0)
#define SAMPLE_RT_LOD(textureName, samplerName, uv, mip) SAMPLE_TEXTURE2D_X_LOD(textureName, samplerName, uv, mip)
#endif

/* include SCPostEffects */
#include "Assets/SC Post Effects/Runtime/Fog/Fog.hlsl"
#endif

#ifdef COZY
/* include COZY */
#include "Assets/Distant Lands/Cozy Weather/Contents/Materials/Shaders/Includes/StylizedFogIncludes.cginc"
#endif

//Executed in vertex stage
float CalculateFogFactor(float3 positionCS) {
	return ComputeFogFactor(positionCS.z);
}

//Fragment stage. Note: Screen position passed here is not normalized (divided by w-component)
void ApplyFog(inout float3 color, float fogFactor, float4 screenPos, float3 wPos, float vFace) 
{
	float3 foggedColor = 0;
	
#ifdef UnityFog
	foggedColor = MixFog(color.rgb, fogFactor);
#endif

#ifdef Colorful
	foggedColor.rgb = ApplyFog(color.rgb, fogFactor, wPos, screenPos.xy / screenPos.w);
#endif
	
#ifdef Enviro
	foggedColor.rgb = TransparentFog(float4(color.rgb, 1.0), wPos, screenPos.xy / screenPos.w, fogFactor).rgb;
#endif
	
#ifdef Azure
	foggedColor.rgb = ApplyAzureFog(float4(color.rgb, 1.0), wPos).rgb;
#endif

#ifdef AtmosphericHeightFog
	float4 fogParams = GetAtmosphericHeightFog(wPos.xyz);
	foggedColor.rgb = lerp(color.rgb, fogParams.rgb, fogParams.a);
#endif

#ifdef SCPostEffects
	float4 dummy;
	screenPos.xy /= screenPos.w;
	ApplyFog_float(wPos, float3(0,1,0), screenPos, _TimeParameters.x, 0, color.rgb, dummy, foggedColor.rgb);
#endif

#ifdef COZY
	foggedColor = BlendStylizedFog(wPos, float4(color.rgb, 1.0)).rgb;
#endif

	#ifndef UnityFog
	//Allow fog to be disabled for water globally by setting the value through script
	foggedColor = lerp(foggedColor, color, _WaterFogDisabled);
	#endif
	
	//Fog only applies to the front faces, otherwise affects underwater rendering
	color.rgb = lerp(color.rgb, foggedColor.rgb, vFace);
}
