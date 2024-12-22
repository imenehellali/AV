Shader "Custom/BumpedSpecularRim_URP" {
    Properties {
        _Color("Main Color", Color) = (1,1,1,1)
        _SpecColorTexture("Specular Color", 2D) = "black" {}
        _Shininess("Shininess", Range(0.03, 1)) = 0.078125
        _MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
        _BumpMap("Normalmap", 2D) = "bump" {}
        _RimColor("Rim Color", Color) = (0.26,0.19,0.16,0.0)
        _RimPower("Rim Power", Range(0.5, 8.0)) = 3.0
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

            TEXTURE2D(_SpecColorTexture);
            SAMPLER(sampler_SpecColorTexture);

            float4 _Color;
            float _Shininess;
            float4 _RimColor;
            float _RimPower;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 normalOS : NORMAL;
                float4 tangentOS : TANGENT;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float3 tangentWS : TEXCOORD2;
                float3 bitangentWS : TEXCOORD3;
                float3 viewDirWS : TEXCOORD4;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes v) {
                Varyings o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                UNITY_TRANSFER_INSTANCE_ID(v, o);

                o.positionHCS = TransformObjectToHClip(v.positionOS);
                o.uv = v.uv;

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

                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv);
                half4 specTex = SAMPLE_TEXTURE2D(_SpecColorTexture, sampler_SpecColorTexture, i.uv);
                half3 normalTS = UnpackNormal(SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, i.uv));

                half3x3 TBN;
                TBN[0] = normalize(i.tangentWS);
                TBN[1] = normalize(i.bitangentWS);
                TBN[2] = normalize(i.normalWS);
                half3 normalWS = mul(normalTS, TBN);

                half3 albedo = mainTex.rgb * _Color.rgb;
                half gloss = _Shininess;
                half3 specColor = specTex.rgb;

                half rim = 1.0 - saturate(dot(normalize(i.viewDirWS), normalWS));
                half3 rimLight = _RimColor.rgb * pow(rim, _RimPower);

                half3 lighting = albedo + rimLight;

                return half4(lighting, mainTex.a * _Color.a);
            }

            ENDHLSL
        }
    }
    FallBack "Hidden/InternalErrorShader"
}
