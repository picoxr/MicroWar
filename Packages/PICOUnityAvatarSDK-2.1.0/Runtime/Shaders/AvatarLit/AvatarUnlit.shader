Shader "PicoAvatar/Bake"
{
    Properties
    {
        [MainTexture] _BaseMap("Texture", 2D) = "white" {}
        [MainColor] _BaseColor("Color", Color) = (1, 1, 1, 1)
        
        [Toggle(_COLOR_SHIFT)]
        _ColorShift                 ("Color Shift", Float) = 0
        [Toggle]_UsingAlbedoHue("UsingAlbedoHue", Float) = 0
        [NoScaleOffset]
        _ColorRegionMap("ColorRegionMap", 2D) = "black" {}
        _ColorRegion1("ColorRegion1", Vector) = (0,0,0,0) 
        _ColorRegion2("ColorRegion2", Vector) = (0,0,0,0) 
        _ColorRegion3("ColorRegion3", Vector) = (0,0,0,0) 
        _ColorRegion4("ColorRegion4", Vector) = (0,0,0,0) 
        _BodyMaskMap("Body Mask Map (not used in shader)", 2D) = "black" {}
    }

    SubShader
    {
        Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel"="4.5"}
        LOD 100
        
        Cull Back

        Pass
        {
            Name "AvatarUnlit"

            HLSLPROGRAM
            #pragma target 4.5

            #pragma multi_compile _ _COLOR_SHIFT
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #pragma multi_compile _ _ENABLE_STATIC_MESH_BATCHING

            #ifndef _COLOR_SHIFT
            #define PAV_COLOR_REGION_BAKED
            #endif

            #pragma vertex UnlitPassVertex
            #pragma fragment UnlitPassFragment

            #include "Dependency/URP/ShaderLibrary/SurfaceInput.hlsl"
            #include "Dependency/URP/ShaderLibrary/Unlit.hlsl"
            #include "Dependency/URP/ShaderLibrary/Lighting.hlsl"
            #include "Common.hlsl"
            
            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                half4 _BaseColor;

                half _UsingAlbedoHue;
                half4 _ColorRegion1;
                half4 _ColorRegion2;
                half4 _ColorRegion3;
                half4 _ColorRegion4;
            CBUFFER_END
            
            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            #if defined(_ENABLE_STATIC_MESH_BATCHING)
                float2 mtlIndex     : TEXCOORD4;
            #endif

                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float2 uv : TEXCOORD0;
                #if defined(_ENABLE_STATIC_MESH_BATCHING)
                nointerpolation float2 mtlIndex : TEXCOORD1;
                #endif
                float4 positionCS : SV_POSITION;

                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };
            

            Varyings UnlitPassVertex(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);

                output.positionCS = vertexInput.positionCS;
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);

            #if defined(_ENABLE_STATIC_MESH_BATCHING)
                output.mtlIndex = input.mtlIndex;
            #endif

                return output;
            }



            half4 UnlitPassFragment(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

                half2 uv = input.uv;
            #if defined(_ENABLE_STATIC_MESH_BATCHING)
                uint index = (uint)input.mtlIndex.x;
                half4 lightingTex = SampleTexArray(0, index, uv, _BaseMapArray, sampler_BaseMapArray, half4(1.0, 1.0, 1.0, 1.0));
                half3 color = ShiftTextureColor(lightingTex.rgb, _MtlData[index].baseColor.rgb, uv, _MtlData[index].colorRegion1, _MtlData[index].colorRegion2, _MtlData[index].colorRegion3, _MtlData[index].colorRegion4, _MtlData[index].uniform1.x);
            #else
                half4 lightingTex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv);
                half3 color = ShiftTextureColor(lightingTex.rgb, _BaseColor.rgb,uv, _ColorRegion1, _ColorRegion2, _ColorRegion3, _ColorRegion4, _UsingAlbedoHue);
            #endif
                
                return half4(color,1.0);
            }
            ENDHLSL
        }

 
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
