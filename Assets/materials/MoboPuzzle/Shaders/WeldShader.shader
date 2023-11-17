Shader "Example/WeldShader"
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
            #pragma hull hull
            #pragma domain domain
            #pragma fragment frag

            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 UV : TEXCOORD;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessellationControlPoint 
            {
                float3 positionWS : INTERNALTESSPOS;
                float2 UV : TEXCOORD;
                float3 normalWS : NORMAL;
                float3 tangentWS : TANGENT;
                float3 bitangentWS : BINORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct TessellationFactors
            {
                float edge[3] : SV_TessFactor;
                float inside : SV_InsideTessFactor;
            };

            struct Varyings
            {
                float3 normalWS : TEXCOORD0;
                float3 positionWS : TEXCOORD1;
                float2 UV : TEXCOORD2;
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
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

            TessellationControlPoint vert(Attributes IN)
            {
               TessellationControlPoint OUT;

               UNITY_SETUP_INSTANCE_ID(IN);
               UNITY_TRANSFER_INSTANCE_ID(IN, OUT);

               OUT.positionWS = TransformObjectToWorld(IN.positionOS);
               OUT.UV = TRANSFORM_TEX(IN.UV, _BaseMap);

               VertexNormalInputs VNI = GetVertexNormalInputs(float3(0.0,1.0,0.0), float4(1.0,0.0,0.0,1.0));

               OUT.normalWS = VNI.normalWS;
               OUT.tangentWS = VNI.tangentWS;
               OUT.bitangentWS = VNI.bitangentWS;

               return OUT;
            }

            TessellationFactors PatchConstantFunction
            (
                InputPatch<TessellationControlPoint, 3> patch
            )
            {
                UNITY_SETUP_INSTANCE_ID(patch[0]);
                TessellationFactors f;
                float dist = clamp(_PlayerDistance, 3.0, 5.0);
                float val = 1.0 - (dist-3.0)/2.0;
                float edgeFactor = lerp(_MinEdgeFactor, _MaxEdgeFactor, val);
                float insideFactor = lerp(_MinInsideFactor, _MaxInsideFactor, val);
                f.edge[0] = edgeFactor;
                f.edge[1] = edgeFactor;
                f.edge[2] = edgeFactor;
                f.inside = insideFactor;
                return f;
            }

            [domain("tri")]
            [outputcontrolpoints(3)]
            [outputtopology("triangle_cw")]
            [patchconstantfunc("PatchConstantFunction")]
            [partitioning("integer")]
            TessellationControlPoint hull
            (
                InputPatch<TessellationControlPoint, 3> patch,
                uint id : SV_OutputControlPointID
            )
            {
                return patch[id];
            }


            #define BARYCENTRIC_INTERPOLATE(fieldName) \
                patch[0].fieldName * barycentricCoordinates.x + \
                patch[1].fieldName * barycentricCoordinates.y + \
                patch[2].fieldName * barycentricCoordinates.z

            [domain("tri")]
            Varyings domain
            (
                TessellationFactors factors,
                OutputPatch<TessellationControlPoint, 3> patch,
                float3 barycentricCoordinates : SV_DomainLocation
            )
            {
               Varyings OUT;

               UNITY_SETUP_INSTANCE_ID(patch[0]);
               UNITY_TRANSFER_INSTANCE_ID(patch[0], OUT);
               UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(OUT);

               float2 UV = BARYCENTRIC_INTERPOLATE(UV);
               float3 normalWS = patch[0].normalWS;
               float3 tangentWS = patch[0].tangentWS;
               float3 bitangentWS = patch[0].bitangentWS;

               float3 offsetY = _ScaleFactor * normalWS * 0.5;
               float offsetX = _TexelWidth * _ScaleFactor;
               float offsetZ = _TexelHeight * _ScaleFactor;

               float d = 1.0;
               float3 height = offsetY * SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, UV, 0).g;
               float3 heightx = offsetY * SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, UV - float2(_TexelWidth*d, 0), 0).g;
               float3 heightz = offsetY * SAMPLE_TEXTURE2D_LOD(_BaseMap, sampler_BaseMap, UV + float2(0,_TexelHeight*d), 0).g;

               float3 pos = BARYCENTRIC_INTERPOLATE(positionWS);
               float3 positionWS =  pos + height;
               float3 positionWSx = pos + tangentWS * offsetX + heightx;
               float3 positionWSz = pos + bitangentWS * offsetZ + heightz;

               float3 tangent = - positionWS + positionWSx; 
               float3 bitangent = - positionWS + positionWSz; 

               float3 newNormal = cross(tangent, bitangent);

               OUT.positionCS = TransformWorldToHClip(positionWS);
               OUT.normalWS = normalize(newNormal);
               OUT.positionWS = positionWS;
               OUT.UV = UV;

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