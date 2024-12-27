Shader "Lava Flowing Shader/Unlit/Distort_URP" {
Properties {
    _DistortX("Distortion in X", Range(0,2)) = 1
    _DistortY("Distortion in Y", Range(0,2)) = 0
    _MainTex("_MainTex RGBA", 2D) = "white" {}
    _Distort("_Distort A", 2D) = "white" {}
    _LavaTex("_LavaTex RGB", 2D) = "white" {}
    _FlowSpeed("Lava Flow Speed", float) = 1.0
    _Granularity("Granularity Factor", Float) = 1.0
}

SubShader {
    Tags { "RenderType"="Opaque" "Queue"="Geometry" }

    Pass {
        Name "ForwardLit"
        Tags { "LightMode"="UniversalForward" }

        HLSLPROGRAM
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D(_Distort);
        SAMPLER(sampler_Distort);

        TEXTURE2D(_LavaTex);
        SAMPLER(sampler_LavaTex);

        float _DistortX;
        float _DistortY;
        float _FlowSpeed;
        float _Granularity;

        struct Attributes {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings {
            float4 positionHCS : SV_POSITION;
            float2 uv_MainTex : TEXCOORD0;
            float2 uv_LavaTex : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings vert(Attributes v) {
            Varyings o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);

            o.positionHCS = TransformObjectToHClip(v.positionOS);

            // Add sinusoidal distortion and granularity to UVs
            float2 sineDistortion = sin(v.uv * _Granularity + float2(_Time.y * _FlowSpeed, _Time.y * _FlowSpeed)) * 0.1;
            o.uv_MainTex = v.uv + sineDistortion;
            o.uv_LavaTex = v.uv + sineDistortion;

            return o;
        }

        half4 frag(Varyings i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            half4 tex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_MainTex);
            half distort = SAMPLE_TEXTURE2D(_Distort, sampler_Distort, i.uv_MainTex).a;

            float2 uv_scroll = float2(i.uv_LavaTex.x - distort * _DistortX, i.uv_LavaTex.y - distort * _DistortY);
            half4 tex2 = SAMPLE_TEXTURE2D(_LavaTex, sampler_LavaTex, uv_scroll);

            return lerp(tex2, tex, tex.a);
        }

        ENDHLSL
    }
}

FallBack "Hidden/InternalErrorShader"
}
