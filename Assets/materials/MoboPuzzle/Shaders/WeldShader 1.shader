Shader "Example/WeldShader1"
{
    // The _BaseColor variable is visible in the Material's Inspector, as a field
    // called Base Color. You can use it to select a custom color. This variable
    // has the default value (1, 1, 1, 1).
    Properties
    {
        [MainTexture] _BaseMap("Main Texture", 2D) = "black"
        _MetalMap("Metal Texture", 2D) = "white"
        _SpecularMap("Specular Texture", 2D) = "white"
        _TexelWidth("Texel width", float) = 0.00125
        _TexelHeight("Texel height", float) = 0.0125
        _ScaleFactor("Scale factor", float) = 1.0
        _MinEdgeFactor("Min Edge Factor", float) = 1.0
        _MinInsideFactor("Min Inside Factor", float ) = 1.0
        _MaxEdgeFactor("Max Edge Factor", float) = 2.0
        _MaxInsideFactor("Max Inside Factor", float) = 2.0
        _PlayerDistance("Player Distance", float ) = 5.0
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
                float2 UV : TEXCOORD;
            };

            struct Varyings
            {
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 UV : TEXCOORD2;
                float4 positionCS : SV_POSITION;
            };


            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_MetalMap); SAMPLER(sampler_MetalMap);
            TEXTURE2D(_SpecularMap); SAMPLER(sampler_SpecularMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _MetalMap_ST;
                float4 _SpecularMap_ST;
                float _ScaleFactor;
                float _TexelWidth;
                float _TexelHeight;
                float _MinInsideFactor;
                float _MinEdgeFactor;
                float _MaxInsideFactor;
                float _MaxEdgeFactor;
                float _PlayerDistance;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
               Varyings OUT;

               OUT.positionWS = TransformObjectToWorld(IN.positionOS);
               OUT.positionCS = TransformObjectToHClip(IN.positionOS);
               OUT.UV = TRANSFORM_TEX(IN.UV, _BaseMap);

               VertexNormalInputs VNI = GetVertexNormalInputs(float3(0.0,1.0,0.0), float4(1.0,0.0,0.0,1.0));

               OUT.normalWS = VNI.normalWS;

               return OUT;
            }

            static half4 colors[] = 
            {
                5*half4(1.0,1.0,1.0,1.0),
                20*half4(1.0,0.0,0.0,1.0),
                20*half4(0.0,1.0,0.0,1.0),
                20*half4(0.0,0.0,1.0,1.0),
                15*half4(3.0,1.0,0.0,1.0),
                20*half4(0.0,1.0,3.0,1.0),
                20*half4(1.0,0.0,1.3,1.0)
            };

            half4 valToColor(float val)
            {
                int index = round(val*10.0f);

                switch (index)
                {
                    case 0: return colors[0];
                    case 1: return colors[1];
                    case 2: return colors[2];
                    case 3: return colors[3];
                    case 4: return colors[4];
                    case 5: return colors[5];
                }
    
                return colors[clamp(index, 0, 6)];
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light light = GetMainLight();
                float3 cameraWS = GetCameraPositionWS();     
                float3 halfVector = normalize(cameraWS - IN.positionWS);

                float4 texSample = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.UV);
                float4 metalSample = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, IN.UV);
                float specularSample = SAMPLE_TEXTURE2D(_MetalMap, sampler_MetalMap, IN.UV).r;
                specularSample = texSample.g < 0.01 ? specularSample : 1.0;

                float ndotl = dot(IN.normalWS, light.direction);
                float ndoth = dot(halfVector, IN.normalWS);
                float4 litCoefficient = lit(ndotl, ndoth, 32);

                half4 metallicColor = texSample.g < 0.01 ? half4(0.15,0.6,0.12,1.0) : half4(0.4333, 0.46, 0.5, 1.0);
                half4 specularColor = texSample.g == 0.0 ? half4(0.08,0.07,0.06,1.0) : half4(0.8,0.7,0.67,1.0);
                float ambient = 0.05;
                half4 tempColor = half4(texSample.r,0,0,0.5);

                half4 pointColor = valToColor(texSample.b);
                metallicColor *= pointColor * metalSample;
                half4 outColor = metallicColor * litCoefficient.y * 0.25 + ambient * half4(light.color, 1.0) * metallicColor + specularColor * litCoefficient.z * specularSample;
    
                return tempColor.r > 0.01 ? lerp(outColor, tempColor, clamp((tempColor.r-0.01)*20, 0, 1)) : outColor;
                // return valToColor(SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.UV).b);
                // return SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.UV).bbbb;
                // return valToColor(0.125);
                // return half4(IN.UV,0.0,1.0);
            }
            ENDHLSL
        }
    }
}