Shader "Example/DrawableShader"
{
    // The _BaseColor variable is visible in the Material's Inspector, as a field
    // called Base Color. You can use it to select a custom color. This variable
    // has the default value (1, 1, 1, 1).
    Properties
    {
        [MainTexture] _BaseMap("Main Texture", 2D) = "gray"
        _SpecularMap("Specular Map", 2D) = "black"
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
                float2 BaseMapUV : TEXCOORD2;
                float2 SpecularMapUV : TEXCOORD3;
                float4 positionCS : SV_POSITION;
            };


            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            TEXTURE2D(_SpecularMap); SAMPLER(sampler_SpecularMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _SpecularMap_ST;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
               Varyings OUT;

                OUT.positionCS = TransformObjectToHClip(IN.positionOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS);
                OUT.BaseMapUV = TRANSFORM_TEX(IN.UV, _BaseMap);
                OUT.SpecularMapUV = TRANSFORM_TEX(IN.UV, _SpecularMap);
                OUT.normalWS = TransformObjectToWorldDir(IN.normalOS);
                
               return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light light = GetMainLight();

                float3 diffuseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.BaseMapUV);
                float3 specularColor = SAMPLE_TEXTURE2D(_SpecularMap, sampler_SpecularMap, IN.SpecularMapUV);
                specularColor = specularColor.r * light.color;

                float shininess = 32.0;

                float3 cameraPos = GetCameraPositionWS();
                float3 lightDir = light.direction;

                // Calculate normalized view direction
                float3 viewDir = normalize(cameraPos - IN.positionWS);

                // Calculate halfway vector
                float3 halfVector = normalize(lightDir + viewDir);

                // Calculate diffuse term
                float diffuseFactor = max(0.0, dot(IN.normalWS, lightDir));
                float3 diffuse = diffuseColor * diffuseFactor;

                // Calculate specular term
                float specularFactor = pow(max(0.0, dot(IN.normalWS, halfVector)), shininess);
                float3 specular = specularColor * specularFactor;

                // Final lighting calculation
                float3 finalColor = diffuse + specular;

                // Optionally, add ambient term
                float3 ambientColor = float3(0.1, 0.1, 0.1); // Adjust as needed
                finalColor += ambientColor;

                return half4(finalColor, 1.0);
            }
            ENDHLSL
        }
    }
}