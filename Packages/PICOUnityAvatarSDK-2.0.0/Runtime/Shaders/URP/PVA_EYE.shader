Shader "PAV/URP/PVA_EYE"
{
    Properties
    {
        [MainTexture]_BaseMap("Albedo", 2D) = "white" {}
    	 _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
    	_UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
//        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
//        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
//        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) // (H,S,V,alpha) 0~1, 0~2, 0~2, 0~1
       // _BumpMap("Normal Map", 2D) = "bump" {}

        _MatCapMap("MatCapMap", 2D) = "white" {}
    	_Cubemap("CubemapMap", Cube) = "_Skybox" {}
    	 _CubemapMip ("CubemapMip", Range(0, 7)) = 0
    	_CubemapLighting ("Cubemap-Lighting", Range(0, 1)) = 0
        _MatcapLighting("Matcap-Lighting", Range( 0 , 8)) = 3
        
        // BlendMode
        [HideInInspector] _Surface("__surface", Float) = 0.0
        [HideInInspector] _Blend("__blend", Float) = 0.0
        [HideInInspector] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("Src", Float) = 1.0
        [HideInInspector] _DstBlend("Dst", Float) = 0.0
        [HideInInspector] _ZWrite("ZWrite", Float) = 1.0
        [HideInInspector] _Cull("__cull", Float) = 2.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalRenderPipeline" "Queue"="Geometry"}

        Pass
        {
            Cull Back
            HLSLPROGRAM
            #pragma target 4.5
			//#pragma multi_compile _ PAV_VERTEX_FROM_BUFFER
			//#pragma multi_compile _ PAV_MERGED_TEXTURE
			
            #pragma vertex vert
            #pragma fragment frag

           
            #include "Universal/ShaderLibrary/Core.hlsl"
            #include "Universal/ShaderLibrary/Lighting.hlsl"
            #include "Universal/ShaderLibrary/SurfaceInput.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS      : NORMAL;
                float4 tangentOS     : TANGENT;
                float2 uv:TEXCOORD0;
            	PAV_VERTEX_ID
            	
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float2 uv:TEXCOORD0;
                float3 normalWS : TEXCOORD1;    
                float3 tangentWS : TEXCOORD2;   
                float3 bitangentWS : TEXCOORD3; 
                float3 viewWS : TEXCOORD4; 
                float3 viewTS : TEXCOORD5;
            	float3 normalVS : TEXCOORD6;
                
            };

            CBUFFER_START(UnityPerMaterial)
            float _SizeEyes;
			float _MatcapLighting;
			float _PuilSoft;
		    float4 _BaseMap_ST;
           // float _BumpScale;
			float4 _MainTex_ST;
            float4 _MainTexW_ST;
			float _PuilDepthScale;
			float _SizePupil;
			float4 _EyesColor;
			float _Smooth;
            float _CubemapMip;
            float _CubemapLighting;

            half _AdditiveGI;
            half4 _ColorRegion1;
			half4 _ColorRegion2;
			half4 _ColorRegion3;
			half4 _ColorRegion4;
            half _UsingAlbedoHue;
            half _ShaderType;
            CBUFFER_END

            TEXTURE2D(_MatCapMap);SAMPLER(sampler_MatCapMap);
            samplerCUBE _Cubemap;


            
            Varyings vert(Attributes IN)
            {
                Varyings OUT;

            	PAV_GET_VERTEX_PNT(IN.vid, IN.positionOS, IN.normalOS, IN.tangentOS);
                
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(positionWS);

                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);

                OUT.viewWS = GetWorldSpaceViewDir( positionWS);

                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);
                OUT.normalWS = normalInput.normalWS;
                OUT.tangentWS = normalInput.tangentWS;
                OUT.bitangentWS = normalInput.bitangentWS;

                float3x3 tangentSpaceTransform =  float3x3(normalInput.tangentWS,normalInput.bitangentWS,normalInput.normalWS);
                OUT.viewTS = mul(tangentSpaceTransform,  OUT.viewWS);

            	OUT.normalVS = TransformWorldToViewDir( OUT.normalWS);
            	OUT.normalVS = OUT.normalVS * 0.5 + 0.5;

            	 

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {

            	PAV_FLIP_UV_Y(IN.uv.xy);
            	half4 albedoAlpha = PAV_SAMPLE_ALBEDO_ALPHA(IN.uv);
				half4 albedoMaskAlpha = PAV_SAMPLE_COLOR_REGIONS(IN.uv);
            	
            	
            	
            	PAV_GET_SHADER_TYPE(shaderType);
            	PAV_GET_USING_ALBEDO_HUE(usingAlbedoHue);
			    PAV_GET_BASE_COLOR_MASK1(colorMask1);
			    PAV_GET_BASE_COLOR_MASK2(colorMask2);
			    PAV_GET_BASE_COLOR_MASK3(colorMask3);
			    PAV_GET_BASE_COLOR_MASK4(colorMask4);



            	//float4 MainTex = SAMPLE_TEXTURE2D( _BaseMap,sampler_BaseMap,IN.uv );
            	
            	
            	half3 viewWS = SafeNormalize(IN.viewWS);
            	float3 vrDirWS = reflect(-viewWS, IN.normalWS);
            	
            	float3 cubemap = texCUBElod(_Cubemap, float4(vrDirWS, _CubemapMip)).xyz;
            	
            	float3 matcap =  SAMPLE_TEXTURE2D( _MatCapMap,sampler_MatCapMap, IN.normalVS.xy) * _MatcapLighting;
            	
            	half3 totalColor =albedoAlpha + matcap + cubemap *_CubemapLighting ;
            	

            	half3 Color = ApplyAlbedo(totalColor,half4(1,1,1,1),shaderType,albedoMaskAlpha, colorMask1, colorMask2, colorMask3, colorMask4, usingAlbedoHue);
            	
                return half4(Color,1) ;

            	//return float4(totalColor,1);
            }

            ENDHLSL
        }
    }
	       CustomEditor "Pico.Avatar.URPShaderInspector"
}
