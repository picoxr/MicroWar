//Stylized Water 2
//Staggart Creations (http://staggart.xyz)
//Copyright protected under Unity Asset Store EULA

//Double sample depth to avoid depth discrepancies 
#if !_DISABLE_DEPTH_TEX && _REFRACTION && _ADVANCED_SHADING
#define RESAMPLE_REFRACTION_DEPTH
#endif

//Mask caustics by shadows cast on scene. Doubles the shadow sampling cost for underwater pixels
#if _CAUSTICS && _ADVANCED_SHADING && defined(MAIN_LIGHT_CALCULATE_SHADOWS)	
#define CAUSTICS_SHADOWMASK
#endif

#define COLLAPSIBLE_GROUP 1

struct SceneData
{
	float4 positionSS;
	float4 pixelOffset;
	float3 positionWS;
	float3 color;

	#ifdef CAUSTICS_SHADOWMASK
	float shadowMask;
	#endif
	
	float viewDepth;
	float verticalDepth;
	
	#ifdef RESAMPLE_REFRACTION_DEPTH
	float viewDepthRefracted;
	float verticalDepthRefracted;
	#endif

	#if UNDERWATER_ENABLED
	float skyMask;
	#endif
};

void PopulateSceneData(inout SceneData scene, Varyings input, WaterSurface water)
{
	#ifdef SCREEN_POS
	scene.positionSS = input.screenPos;
	#endif
	
	#if _REFRACTION || UNDERWATER_ENABLED
	scene.pixelOffset.xy = water.tangentWorldNormal.xz * (_RefractionStrength * lerp(0.1, 0.01,  unity_OrthoParams.w));
	scene.pixelOffset.zw = 0;
	#endif

	//Default for disabled depth texture
	scene.viewDepth = 1;
	scene.verticalDepth = 1;

	#if !_DISABLE_DEPTH_TEX
	SceneDepth depth = SampleDepth(scene.positionSS);
	scene.positionWS = ReconstructViewPos(scene.positionSS, water.viewDir, depth);

	//Invert normal when viewing backfaces
	float normalSign = ceil(dot(SafeNormalize(water.viewDir), water.waveNormal));
	normalSign = normalSign == 0 ? -1 : 1;

	//Z-distance to opaque surface
	scene.viewDepth = SurfaceDepth(depth, input.positionCS);
	//Distance to opaque geometry in normal direction
	scene.verticalDepth = DepthDistance(water.positionWS, scene.positionWS, water.waveNormal * normalSign);

	//Compare position of water to opaque geometry, in order to filter out pixels above the water for refraction
	#if _REFRACTION
		SceneDepth depthRefracted = SampleDepth(scene.positionSS + scene.pixelOffset);
		float3 opaqueWorldPosRefracted = ReconstructViewPos(scene.positionSS + scene.pixelOffset, water.viewDir, depthRefracted);

		//Reject any offset pixels in front of the water surface
		half refractionMask = saturate((water.positionWS.y - opaqueWorldPosRefracted.y));

		#if UNDERWATER_ENABLED
		//Flip mask for backfaces
		refractionMask = lerp(saturate((opaqueWorldPosRefracted.y - water.positionWS.y)), refractionMask, water.vFace);
		#endif

		//Lerp to un-refracted screen-position for pixels above water
		scene.pixelOffset = lerp(0, scene.pixelOffset, refractionMask);
	
		#ifdef RESAMPLE_REFRACTION_DEPTH
		//With the current screen-space UV known, re-compose the water density
		depthRefracted = SampleDepth(scene.positionSS + scene.pixelOffset);
		opaqueWorldPosRefracted = ReconstructViewPos(scene.positionSS + scene.pixelOffset, water.viewDir, depthRefracted);

		//Use this sample as the representation of the underwater geometry position (more accurate)
		//scene.positionWS = lerp(scene.positionWS, opaqueWorldPosRefracted, refractionMask);
	
		scene.viewDepthRefracted = SurfaceDepth(depthRefracted, input.positionCS);
		scene.verticalDepthRefracted = DepthDistance(water.positionWS, opaqueWorldPosRefracted, water.waveNormal * normalSign);
		#endif
	
	#endif

	#if _REFRACTION || UNDERWATER_ENABLED
	scene.color = SampleOpaqueTexture(scene.positionSS + scene.pixelOffset, water.vFace);
	#endif

	#ifdef CAUSTICS_SHADOWMASK
	float4 shadowCoords = TransformWorldToShadowCoord(scene.positionWS);

	#if VERSION_GREATER_EQUAL(10,0)
	Light sceneLight = GetMainLight(shadowCoords, scene.positionWS, 1.0);
	#else
	Light sceneLight = GetMainLight(shadowCoords);
	#endif

	scene.shadowMask = sceneLight.shadowAttenuation;
	#endif

	#if !_RIVER && _ADVANCED_SHADING
		half VdotN = 1.0 - saturate(dot(SafeNormalize(water.viewDir), water.waveNormal));
		float grazingTerm = saturate(pow(VdotN, 64));
	
		//Resort to z-depth at surface edges. Otherwise makes intersection/edge fade visible through the water surface
		scene.verticalDepth = lerp(scene.verticalDepth, scene.viewDepth, grazingTerm);

		#ifdef RESAMPLE_REFRACTION_DEPTH
		scene.verticalDepthRefracted = lerp(scene.verticalDepthRefracted, scene.viewDepthRefracted, grazingTerm);
		#endif
	#endif
	
	#endif

	//Scene mask is used for backface reflections, to blend between refraction and reflection probes
	#if UNDERWATER_ENABLED
		scene.skyMask = 0;

		#if !_DISABLE_DEPTH_TEX
		float depthSource = depth.linear01;
		
		#if _REFRACTION
		//Use depth resampled with refracted screen UV
		depthSource = depthRefracted.linear01;
		#endif
				
		scene.skyMask = depthSource > 0.99 ? 1 : 0;
		#endif
	#endif
}

float GetWaterDensity(SceneData scene, float mask)
{
	//Best default value, otherwise water just turns invisible (infinitely shallow)
	float density = 1.0;
	
	#if !_DISABLE_DEPTH_TEX

	float viewDepth = scene.viewDepth;
	float verticalDepth = scene.verticalDepth;

		#if defined(RESAMPLE_REFRACTION_DEPTH)
		viewDepth = scene.viewDepthRefracted;
		verticalDepth = scene.verticalDepthRefracted;
		#endif

	float depthAttenuation = 1.0 - exp(-viewDepth * _DepthVertical * lerp(0.1, 0.01, unity_OrthoParams.w));
	float heightAttenuation = saturate(lerp(verticalDepth * _DepthHorizontal, 1.0 - exp(-verticalDepth * _DepthHorizontal), _DepthExp));
	
	density = max(depthAttenuation, heightAttenuation);
	
	#endif

	#if !_RIVER
	//Use green vertex color channel to control density
	density *= saturate(density - mask);
	#endif

	return density;
}

//Note: Throws an error about a BLENDWEIGHTS vertex attribute on GLES when VR is enabled (fixed in URP 10+)
//Possibly related to: https://issuetracker.unity3d.com/issues/oculus-a-non-system-generated-input-signature-parameter-blendindices-cannot-appear-after-a-system-generated-value
#if SHADER_API_GLES3 && SHADER_LIBRARY_VERSION_MAJOR < 10
#define FRONT_FACE_SEMANTIC_REAL VFACE
#else
#define FRONT_FACE_SEMANTIC_REAL FRONT_FACE_SEMANTIC
#endif

#if UNDERWATER_ENABLED
half4 ForwardPassFragment(Varyings input, FRONT_FACE_TYPE vertexFace : FRONT_FACE_SEMANTIC_REAL) : SV_Target
#else
half4 ForwardPassFragment(Varyings input) : SV_Target
#endif
{
	UNITY_SETUP_INSTANCE_ID(input);
	UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

	//Initialize with null values. Anything that isn't assigned, shouldn't be used either
	WaterSurface water = (WaterSurface)0;
	SceneData scene = (SceneData)0;

	water.alpha = 1.0;
	water.vFace = 1;

	/* ========
	// GEOMETRY DATA
	=========== */
	#if COLLAPSIBLE_GROUP

	#if UNDERWATER_ENABLED
	water.vFace = IS_FRONT_VFACE(vertexFace, true, false); //0 = back face
	//return float4(lerp(float3(1,0,0), float3(0,1,0), IS_FRONT_VFACE(water.vFace, true, false)), 1.0);
	#endif
	
	float4 vertexColor = input.color; //Mask already applied in vertex shader
	//return float4(vertexColor.rgb, 1);

	//Vertex normal in world-space
	float3 normalWS = normalize(input.normalWS.xyz);
#if _NORMALMAP
	float3 WorldTangent = input.tangent.xyz;
	float3 WorldBiTangent = input.bitangent.xyz;
	float3 positionWS = float3(input.normalWS.w, input.tangent.w, input.bitangent.w);
#else
	float3 positionWS = input.positionWS;
#endif

	water.positionWS = positionWS;
	//Not normalized for depth-pos reconstruction. Normalization required for lighting (otherwise breaks on mobile)
	water.viewDir = GetCurrentViewPosition() - positionWS;
	//water.viewDir = GetWorldSpaceViewDir(positionWS); //Uses the camera's forward vector for orthographic projection, the result isn't as useful
	
	//Note: SafeNormalize() tends to cause issues on mobile when dealing with large numbers
	float3 viewDirNorm = normalize(water.viewDir);
	//return float4(water.viewDir, 1);
	
	half VdotN = 1.0 - saturate(dot(viewDirNorm, normalWS));
	
	#if _FLAT_SHADING
	float3 dpdx = ddx(positionWS.xyz);
	float3 dpdy = ddy(positionWS.xyz);
	normalWS = normalize(cross(dpdy, dpdx));
	#endif

	water.vertexNormal = normalWS;

	#if _RIVER
	water.slope = GetSlope(normalWS, _SlopeThreshold);
	//return float4(water.slope.xxx, 1);
	#endif

	#if MODIFIERS_ENABLED
	float3 positionVS = TransformWorldToView(water.positionWS);
	CascadeInfo cascades = GetCascadeInfo(positionVS, water.positionWS);

	Modifiers modifiers = (Modifiers)0;

	modifiers.albedo = SampleAlbedoModifiers(cascades);
	
	water.offset += -GetDisplacementOffset(cascades);

	#if _ADVANCED_SHADING
	//Effective world position is possibly shifted on the XZ plane. Recalculate the cascade UV's
	positionVS = TransformWorldToView(water.positionWS + water.offset);
	cascades = GetCascadeInfo(positionVS, water.positionWS + water.offset);
	#endif
	//Assume the max amplitude is 10 units. Scale down to normalize for the surface foam wave mask
	//water.offset.y *= 0.1; 
	//return float4(offset.xyz, 1.0);
	#endif
	
	//Returns mesh or world-space UV
	float2 uv = GetSourceUV(input.uv.xy, positionWS.xz, _WorldSpaceUV);;
	#endif

	/* ========
	// WAVES
	=========== */
	#if COLLAPSIBLE_GROUP

	water.waveNormal = normalWS;
	
#if _WAVES
	WaveInfo waves = GetWaveInfo(uv, TIME * _WaveSpeed, _WaveHeight,  lerp(1, 0, vertexColor.b), _WaveFadeDistance.x, _WaveFadeDistance.y);
	#if !_FLAT_SHADING
		//Flatten by blue vertex color weight
		waves.normal = lerp(waves.normal, normalWS, lerp(0, 1, vertexColor.b));
		//Blend wave/vertex normals in world-space
		water.waveNormal = BlendNormalWorldspaceRNM(waves.normal, normalWS, water.vertexNormal);
	#endif
	//return float4(water.waveNormal.xyz, 1);
	//water.height += waves.position.y * 0.5 + 0.5;

	water.offset.y += waves.position.y;
	//For steep waves the horizontal stretching is too extreme, tone it down here
	water.offset.xz += waves.position.xz * 0.5;
#endif

	#endif

	#if _WAVES || MODIFIERS_ENABLED
	//After modifier and/or wave displacement, recalculated world-space UVs
	if(_WorldSpaceUV == 1) uv = GetSourceUV(input.uv.xy, positionWS.xz + water.offset.xz, _WorldSpaceUV);
	//return float4(frac(uv), 0, 1);
	#endif

	/* ========
	// SHADOWS
	=========== */
	#if COLLAPSIBLE_GROUP
	float4 shadowCoords = float4(0, 0, 0, 0);
	#if defined(REQUIRES_VERTEX_SHADOW_COORD_INTERPOLATOR) && !defined(UNLIT)
	shadowCoords = input.shadowCoord;
	#elif defined(MAIN_LIGHT_CALCULATE_SHADOWS) && !defined(UNLIT)
	shadowCoords = TransformWorldToShadowCoord(water.positionWS);
	#endif

	#if VERSION_GREATER_EQUAL(10,0)
	Light mainLight = GetMainLight(shadowCoords, water.positionWS, 1.0);
	#else
	Light mainLight = GetMainLight(shadowCoords);
	#endif

	water.shadowMask = mainLight.shadowAttenuation;
	
	//return float4(water.shadowMask.xxx,1);
	half backfaceShadows = 1;
	
	#if UNDERWATER_ENABLED
	//Separate so shadows applied by Unity's lighting do not appear on backfaces
	backfaceShadows = water.shadowMask;
	water.shadowMask = lerp(1.0, water.shadowMask, water.vFace);
	#endif
	#endif

	/* ========
	// NORMALS
	=========== */
	#if COLLAPSIBLE_GROUP
	water.tangentNormal = float3(0.5, 0.5, 1);
	water.tangentWorldNormal = water.waveNormal;
	
#if _NORMALMAP
	//Tangent-space
	water.tangentNormal = SampleNormals(uv * _NormalTiling, positionWS, TIME, _NormalSpeed, water.slope, water.vFace);
	//return float4(SRGBToLinear(float3(water.tangentNormal.x * 0.5 + 0.5, water.tangentNormal.y * 0.5 + 0.5, 1)), 1.0);

	//Based on wave normal, makes it easier to create blend between the smooth wave normals and high-frequency normal maps
	water.tangentToWorldMatrix = half3x3(WorldTangent, WorldBiTangent, water.waveNormal);

#if MODIFIERS_ENABLED
	modifiers.tangentNormals = SampleNormalModifiers(cascades);

	//Flatten normals on wave crests?
	//water.tangentNormal = lerp(water.tangentNormal, float3(0.5, 0.5, 1.0), saturate(water.offset.y));

	//Blend towards the original normals at the edge of the render range
	modifiers.tangentNormals.xyz = lerp(water.tangentNormal, modifiers.tangentNormals.xyz, cascades.fadeFactor);
	
	water.tangentNormal = BlendTangentNormals(water.tangentNormal, modifiers.tangentNormals.xyz);

	//return float4(SRGBToLinear(float3(modifiers.tangentNormals.x * 0.5 + 0.5, modifiers.tangentNormals.y * 0.5 + 0.5, 1)), 1.0);
	//return float4(SRGBToLinear(float3(water.tangentNormal.x * 0.5 + 0.5, water.tangentNormal.y * 0.5 + 0.5, 1)), 1.0);
	
	float3 modifierWorldNormal = normalize(TransformTangentToWorld(modifiers.tangentNormals.xyz, half3x3(WorldTangent, WorldBiTangent, water.vertexNormal)));
	modifierWorldNormal = lerp(water.waveNormal, modifierWorldNormal, modifiers.tangentNormals.a);

	water.waveNormal = BlendNormalWorldspaceRNM(modifierWorldNormal, water.waveNormal, water.vertexNormal);
#endif

	//World-space
	water.tangentWorldNormal = normalize(TransformTangentToWorld(water.tangentNormal, water.tangentToWorldMatrix));	
	//return float4(water.tangentWorldNormal, 1.0);
#endif
	#endif

	//Normals can perturb the screen coordinates, so needs to be calculated first
	PopulateSceneData(scene, input, water);

	//return float4(scene.positionSS.xy + scene.pixelOffset.xy, 0, 1.0);

	#if UNDERWATER_ENABLED
	ClipSurface(scene.positionSS.xyzw, positionWS, input.positionCS.xyz, water.vFace);
	#endif

	/* =========
	// COLOR + FOG
	============ */
	#if COLLAPSIBLE_GROUP

	water.fog = GetWaterDensity(scene, vertexColor.g);
	//return float4(water.density.xxx, 1.0);

	//Albedo
	float4 baseColor = lerp(_ShallowColor, _BaseColor, water.fog);
	//Avoid color bleeding for foam/intersection on clear water (assumes white foam)
	//baseColor = lerp(1.0, baseColor, baseColor.a);
	
	baseColor.rgb += _WaveTint * water.offset.y;

	water.fog *= baseColor.a;
	water.alpha = baseColor.a;
	
	water.albedo.rgb = baseColor.rgb;	
	#endif

	/* ========
	// INTERSECTION FOAM
	=========== */
	#if COLLAPSIBLE_GROUP

	water.intersection = 0;
#if _SHARP_INERSECTION || _SMOOTH_INTERSECTION

	float interSecGradient = 0;
	
	#if !_DISABLE_DEPTH_TEX
	interSecGradient = 1-saturate(exp(scene.verticalDepth) / _IntersectionLength);	
	#endif
	
	if (_IntersectionSource == 1) interSecGradient = vertexColor.r;
	if (_IntersectionSource == 2) interSecGradient = saturate(interSecGradient + vertexColor.r);

	water.intersection = SampleIntersection(uv.xy, interSecGradient, TIME * _IntersectionSpeed) * _IntersectionColor.a;

	#if UNDERWATER_ENABLED
	//Hide on backfaces
	water.intersection *= water.vFace;
	#endif

	#if _WAVES
	//Prevent from peering through waves when camera is at the water level
	if(positionWS.y < scene.positionWS.y) water.intersection = 0;
	#endif

	//water.density += water.intersection;
	
	//Flatten normals on intersection foam
	water.waveNormal = lerp(water.waveNormal, normalWS, water.intersection);
	//return float4(water.intersection.xxx,1);
#endif
	#endif

	/* ========
	// SURFACE FOAM
	=========== */
	#if COLLAPSIBLE_GROUP
	water.foam = 0;
	
	#if _FOAM

	#if !_RIVER
	//Composed mask for foam caps, based on wave height
	float foamMask = lerp(1, saturate(water.offset.y), _FoamWaveMask);
	foamMask = pow(abs(foamMask), _FoamWaveMaskExp);
	//return float4(foamMask.xxx, 1.0);
	#else
	//Rivers don't have waves
	float foamMask = 1;
	#endif

	water.foam = SampleFoam(uv * _FoamTiling, TIME, _FoamSize, foamMask, water.slope);
	
	//Add foam based on vertex color alpha channel
	#if _RIVER
	water.foam *= saturate(_FoamColor.a + water.slope + vertexColor.a);
	#else
	water.foam *= saturate(_FoamColor.a + vertexColor.a);
	#endif

	#if MODIFIERS_ENABLED
	float4 foamMod = SampleFoamModifiers(cascades);
	foamMod.a *= cascades.fadeFactor;
	water.foam = lerp(water.foam, foamMod.r, foamMod.a);
	#endif
	
	//return float4(water.foam.xxx, 1);
	#endif

	#if WAVE_SIMULATION
	SampleWaveSimulationFoam(positionWS, water.foam);
	#endif
	#endif
	
	/* ========
	// EMISSION (Caustics + Specular)
	=========== */
	#if COLLAPSIBLE_GROUP

	#if _CAUSTICS
	float2 causticsProjection = scene.positionWS.xz;
	//causticsProjection = mul((float4x4)_MainLightWorldToLight, float4(scene.positionWS, 1.0)).xy;
	water.caustics = SampleCaustics(causticsProjection + lerp(water.waveNormal.xz, water.tangentNormal.xy, _CausticsDistortion), TIME * _CausticsSpeed, _CausticsTiling);

	//Caustics based on normals?
	//float3 causticsTangentNormals = SampleNormals(scene.positionWS.xz * _NormalTiling, scene.positionWS, TIME, _NormalSpeed, water.slope, water.vFace);
	//water.caustics = smoothstep(0, 1, 1-causticsTangentNormals.b);
	
	float causticsMask = saturate((1-water.fog) - water.intersection - water.foam) * water.vFace;

	#ifdef CAUSTICS_SHADOWMASK
	causticsMask *= scene.shadowMask;
	#endif
	//return float4(causticsMask.xxx, 1.0);
	
	//Note: not masked by shadows, this occurs in the lighting function
	water.caustics *= causticsMask * _CausticsBrightness;
	//return float4(water.caustics.rgb, 1);
	#endif

#if _NORMALMAP
	//Can piggyback on the tangent normal
	half3 sparkles = mainLight.color * saturate(step(_SparkleSize, (water.tangentNormal.y))) * _SparkleIntensity;
	
	#if !_UNLIT
	//Fade out the effect as the sun approaches the horizon
	half sunAngle = saturate(dot(water.vertexNormal, mainLight.direction));
	half angleMask = saturate(sunAngle * 10); /* 1.0/0.10 = 10 */
	sparkles *= angleMask;
	#endif

	//water.albedo += sparkles.rgb;
	water.specular += sparkles.rgb;
#endif
	
#ifndef _SPECULARHIGHLIGHTS_OFF
	float3 sunReflectionNormals = water.tangentWorldNormal;

	#if _FLAT_SHADING //Use face normals
	sunReflectionNormals = water.waveNormal;
	#endif
	
	half3 sunSpec = SpecularReflection(mainLight, viewDirNorm, sunReflectionNormals, _SunReflectionDistortion, _SunReflectionSize, _SunReflectionStrength);
	sunSpec.rgb *= saturate((1-water.foam) * (1-water.intersection) * water.shadowMask); //Hide
	
	water.specular += sunSpec;
#endif
	//return float4(specular, 1.0);

	//Reflection probe/planar
#ifndef _ENVIRONMENTREFLECTIONS_OFF

	//Blend between smooth surface normal and normal map to control the reflection perturbation (probes only!)
	#if !_FLAT_SHADING 
	float3 refWorldTangentNormal = lerp(water.waveNormal, normalize(water.waveNormal + water.tangentWorldNormal), _ReflectionDistortion);
	#else //Skip, not a good fit
	float3 refWorldTangentNormal = water.waveNormal;
	#endif
	
	float3 reflectionVector = reflect(-viewDirNorm, refWorldTangentNormal);

	#if !_RIVER
	//Ensure only the top hemisphere of the reflection probe is used
	reflectionVector.y = max(0, reflectionVector.y);
	#endif
	
	//Pixel offset for planar reflection, sampled in screen-space
	float2 reflectionPerturbation = lerp(water.waveNormal.xz, water.tangentNormal.xy, _ReflectionDistortion).xy;
	
	water.reflections = SampleReflections(reflectionVector, _ReflectionBlur, _PlanarReflectionsEnabled, scene.positionSS.xyzw, positionWS, refWorldTangentNormal, viewDirNorm, reflectionPerturbation);
	
	half reflectionFresnel = ReflectionFresnel(refWorldTangentNormal, viewDirNorm, _ReflectionFresnel);
	//return float4(reflectionFresnel.xxx, 1.0);
	water.reflectionMask = _ReflectionStrength * reflectionFresnel * water.vFace;
	water.reflectionLighting = 1-_ReflectionLighting;

	#if _UNLIT
	//Nullify, otherwise reflections turn black
	water.reflectionLighting = 1.0;
	#endif
#endif
	#endif

	/* ========
	// COMPOSITION
	=========== */
	#if COLLAPSIBLE_GROUP

	#if MODIFIERS_ENABLED
	//Blend in albedo color from modifiers
	water.albedo = lerp(water.albedo, modifiers.albedo.rgb, modifiers.albedo.a);
	#endif
	
	//Foam application on top of everything up to this point
	#if _FOAM
	water.albedo.rgb = lerp(water.albedo.rgb, _FoamColor.rgb, water.foam);
	#endif

	#if _SHARP_INERSECTION || _SMOOTH_INTERSECTION
	//Layer intersection on top of everything
	water.albedo.rgb = lerp(water.albedo.rgb, _IntersectionColor.rgb, water.intersection);
	#endif

	#if _FOAM || _SHARP_INERSECTION || _SMOOTH_INTERSECTION
	//Sum values to compose alpha
	water.alpha = saturate(water.alpha + water.intersection + water.foam);
	#endif

	#ifndef _ENVIRONMENTREFLECTIONS_OFF
	//Foam complete, use it to mask out the reflection (considering that foam is rough)
	water.reflectionMask = saturate(water.reflectionMask - water.foam - water.intersection) * _ReflectionStrength;
	//return float4(reflectionFresnel.xxx, 1);

	#if !_UNLIT
	//Blend reflection with albedo. Diffuse lighting will affect it
	water.albedo.rgb = lerp(water.albedo, lerp(water.albedo.rgb, water.reflections, water.reflectionMask), _ReflectionLighting);
	//return float4(water.albedo.rgb, 1);
	#endif
	#endif
	//return float4(water.reflections.rgb, 1);

	#if !_UNLIT
	float normalMask = saturate(water.intersection + water.foam);
	//Blend between smooth geometry normal and normal map for diffuse lighting
	water.diffuseNormal = lerp(water.waveNormal, water.tangentWorldNormal, saturate(_NormalStrength - normalMask));
	#endif

	#if _FLAT_SHADING
	//Moving forward, consider the tangent world normal the same as the flat-shaded normals
	water.tangentWorldNormal = water.waveNormal;
	#endif
	
	//Horizon color (note: not using normals, since they are perturbed by waves)
	float fresnel = saturate(pow(VdotN, _HorizonDistance));
	#if UNDERWATER_ENABLED
	fresnel *= water.vFace;
	#endif
	water.albedo.rgb = lerp(water.albedo.rgb, _HorizonColor.rgb, fresnel * _HorizonColor.a);

	#if UNITY_COLORSPACE_GAMMA
	//Gamma-space is likely a choice, enabling this will have the water stand out from non gamma-corrected shaders
	//water.albedo.rgb = LinearToSRGB(water.albedo.rgb);
	#endif
	
	//Final alpha
	water.edgeFade = saturate(scene.verticalDepth / (_EdgeFade * 0.01));

	#if UNDERWATER_ENABLED
	water.edgeFade = lerp(1.0, water.edgeFade, water.vFace);
	#endif

	water.alpha *= water.edgeFade;
	#endif

	/* ========
	// TRANSLUCENCY
	=========== */
	TranslucencyData translucencyData = (TranslucencyData)0;
	#if _TRANSLUCENCY
	//Note: value is subtracted
	float thickness = 1-(saturate(water.shadowMask - (water.foam ) - (1-water.fog) + (water.edgeFade) - (water.reflectionMask * _TranslucencyReflectionMask)));
	//return float4(thickness.xxx, 1);

	translucencyData = PopulateTranslucencyData(_ShallowColor.rgb,mainLight.direction, mainLight.color, viewDirNorm, lerp(UP_VECTOR, water.waveNormal, water.vFace), water.tangentWorldNormal, thickness, _TranslucencyParams);

	#if UNDERWATER_ENABLED
	//Override the strength of the effect for the backfaces, to match the underwater shading post effect
	translucencyData.strength *= lerp(_UnderwaterFogBrightness * _UnderwaterSubsurfaceStrength, 1, water.vFace);
	#endif
	#endif

	/* ========
	// UNITY SURFACE & INPUT DATA
	=========== */
	#if COLLAPSIBLE_GROUP
	SurfaceData surfaceData = (SurfaceData)0;

	surfaceData.albedo = water.albedo.rgb;
	surfaceData.specular = water.specular.rgb;
	surfaceData.metallic = 0;
	surfaceData.smoothness = 0;
	surfaceData.normalTS = water.tangentNormal;
	surfaceData.emission = 0; //To be populated with translucency+caustics
	surfaceData.occlusion = 1.0;
	surfaceData.alpha = water.alpha;

	//https://github.com/Unity-Technologies/Graphics/blob/31106afc882d7d1d7e3c0a51835df39c6f5e3073/com.unity.render-pipelines.universal/ShaderLibrary/Input.hlsl#L34
	InputData inputData = (InputData)0;
	inputData.positionWS = positionWS;
	inputData.viewDirectionWS = viewDirNorm;
	inputData.shadowCoord = shadowCoords;
	#if UNDERWATER_ENABLED
	//Flatten normals for underwater lighting (distracting, peers through the fog)
	inputData.normalWS = lerp(water.waveNormal, water.tangentWorldNormal, water.vFace);
	#else
	inputData.normalWS = water.tangentWorldNormal;
	#endif
	inputData.fogCoord = input.fogFactorAndVertexLight.x;
	inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;

	inputData.bakedGI = 0;
	#if defined(DYNAMICLIGHTMAP_ON) && VERSION_GREATER_EQUAL(12,0)
    inputData.bakedGI = SAMPLE_GI(input.bakedLightmapUV, input.dynamicLightmapUV, input.vertexSH, inputData.normalWS);
    #else
    inputData.bakedGI = SAMPLE_GI(input.bakedLightmapUV, input.vertexSH, inputData.normalWS);
    #endif

	#endif

	float4 finalColor = float4(ApplyLighting(surfaceData, scene.color, mainLight, inputData, water, translucencyData, _ShadowStrength, water.vFace), water.alpha);

	/* ========
	// RENDERING DEBUGGER (URP 12+)
	=========== */
	#if COLLAPSIBLE_GROUP

	#if VERSION_GREATER_EQUAL(12,0) && defined(DEBUG_DISPLAY)
	inputData.positionCS = input.positionCS;
	#if _NORMALMAP
	inputData.tangentToWorld = water.tangentToWorldMatrix;
	#else
	inputData.tangentToWorld = 0;
	#endif
	inputData.normalizedScreenSpaceUV = scene.positionSS.xy / scene.positionSS.w;
	inputData.shadowMask = shadowCoords;
	#if defined(DYNAMICLIGHTMAP_ON)
	inputData.dynamicLightmapUV = input.dynamicLightmapUV;
	#endif
	#if defined(LIGHTMAP_ON)
	inputData.staticLightmapUV = input.bakedLightmapUV;
	#else
	inputData.vertexSH = input.vertexSH;
	#endif

	inputData.brdfDiffuse = surfaceData.albedo;
	inputData.brdfSpecular = surfaceData.specular;
	inputData.uv = uv;
	inputData.mipCount = 0;
	inputData.texelSize = float4(1/uv.x, 1/uv.y, uv.x, uv.y);
	inputData.mipInfo = 0;
	half4 debugColor;

	if (CanDebugOverrideOutputColor(inputData, surfaceData, debugColor))
	{
		return debugColor;
	}
	#endif
	#endif
	
	#if _REFRACTION
	finalColor.rgb = lerp(scene.color.rgb, finalColor.rgb, saturate(water.fog + water.intersection + water.foam));
	//The opaque color texture is now used. The "real" alpha value is solely the edge fade factor
	water.alpha = water.edgeFade;
	#endif

	ApplyFog(finalColor.rgb, inputData.fogCoord, scene.positionSS, positionWS, water.vFace);
	
	#if UNDERWATER_ENABLED	
	float3 underwaterColor = ShadeUnderwaterSurface(surfaceData.albedo.rgb, surfaceData.emission.rgb, surfaceData.specular.rgb, scene.color, scene.skyMask, backfaceShadows, inputData.positionWS, inputData.normalWS, water.tangentWorldNormal, viewDirNorm,  _ShallowColor.rgb, _BaseColor.rgb, water.vFace);
	finalColor.rgb = lerp(underwaterColor, finalColor.rgb, water.vFace);
	water.alpha = lerp(1.0, water.alpha, water.vFace);
	#endif
	
	#ifdef COZY
	water.alpha = max(water.alpha, GetStylizedFogDensity(positionWS));
	#endif
	
	finalColor.a = water.alpha;

	#if _RIVER
	//Vertex color green channel controls real alpha in this case (not the color depth gradient)
	finalColor.a = water.alpha * saturate(water.alpha - vertexColor.g);
	#endif

	return finalColor;
}