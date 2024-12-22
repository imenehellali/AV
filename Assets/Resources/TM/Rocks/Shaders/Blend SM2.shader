 Shader "Enviro/BumpedDiffuseOverlaySM2_URP" {
    Properties {
        _Color("Main Color", Color) = (1,1,1,1)
        _Opacity("Color over opacity", Range(0, 1)) = 1
        _MainTex("Color over (RGBA)", 2D) = "white" {}
        _BumpMap("Normalmap over", 2D) = "bump" {}
        _MainTex2("Color under (RGBA)", 2D) = "white" {}
        _BumpMap2("Normalmap under", 2D) = "bump" {}
    }

    SubShader {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 400

        Pass {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            TEXTURE2D(_MainTex2);
            SAMPLER(sampler_MainTex2);

            TEXTURE2D(_BumpMap2);
            SAMPLER(sampler_BumpMap2);

            float4 _Color;
            float _Opacity;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv_MainTex : TEXCOORD0;
                float2 uv_BumpMap : TEXCOORD1;
                float2 uv_MainTex2 : TEXCOORD2;
                float2 uv_BumpMap2 : TEXCOORD3;
                float3 normalWS : TEXCOORD4;
                float3 tangentWS : TEXCOORD5;
                float3 bitangentWS : TEXCOORD6;
                float3 viewDirWS : TEXCOORD7;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv_MainTex = v.uv;
                o.uv_BumpMap = v.uv;
                o.uv_MainTex2 = v.uv;
                o.uv_BumpMap2 = v.uv;

                o.normalWS = TransformObjectToWorldNormal(v.normalOS);
                o.tangentWS = TransformObjectToWorldDir(v.tangentOS.xyz);
                o.bitangentWS = cross(o.normalWS, o.tangentWS) * v.tangentOS.w;

                float3 worldPos = TransformObjectToWorld(v.positionOS);
                o.viewDirWS = normalize(GetCameraPositionWS() - worldPos);

                return o;
            }

            half4 frag(Varyings i) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex);
                half4 tex2 = SAMPLE_TEXTURE2D(_MainTex2, sampler_MainTex2, i.uv_MainTex2);

                float4 dest;
                float opacity = _Opacity * tex.a;
                dest.rgb = tex2.rgb <= 0.5 ? 2 * tex.rgb * tex2.rgb : 1 - 2 * (1 - tex.rgb) * (1 - tex2.rgb);
                dest.rgb = lerp(tex2.rgb, dest.rgb, opacity);
                dest.rgb *= _Color.rgb;

                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv_BumpMap));
                half3 normalTS2 = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap2, sampler_BumpMap2, i.uv_BumpMap2));
                half3 finalNormal = lerp(normalTS2, normalTS, opacity);

                half3x3 TBN;
                TBN[0] = normalize(i.tangentWS);
                TBN[1] = normalize(i.bitangentWS);
                TBN[2] = normalize(i.normalWS);
                half3 normalWS = mul(finalNormal, TBN);

                dest.a = tex2.a * _Color.a;

                // Combine albedo and normal
                return half4(dest.rgb, dest.a);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
