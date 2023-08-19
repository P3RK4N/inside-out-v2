Shader "Example/DrawableShader"
{
    // The _BaseColor variable is visible in the Material's Inspector, as a field
    // called Base Color. You can use it to select a custom color. This variable
    // has the default value (1, 1, 1, 1).
    Properties
    {
        [MainTexture] _BaseMap("Main Texture", 2D) = "gray"
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM
            #pragma target 5.0
            #pragma vertex vert
            #pragma fragment frag

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 UV : TEXCOORD0;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 UV : TEXCOORD2;
                float4 positionCS : SV_POSITION;
            };


            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
               Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.UV = TRANSFORM_TEX(IN.UV, _BaseMap);
                OUT.normalWS = TransformObjectToWorld(IN.normalOS);

               return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light light = GetMainLight();
                float3 cameraWS = GetCameraPositionWS();     
                float3 halfVector = normalize(cameraWS - IN.positionWS);

                float4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.UV);

                float ndotl = dot(IN.normalWS, light.direction);
                float ndoth = dot(halfVector, IN.normalWS);
                float4 litCoefficient = lit(ndotl, ndoth, 32);

                return half4(texSample.xyz, 1.0);
            }
            ENDHLSL
        }
    }
}