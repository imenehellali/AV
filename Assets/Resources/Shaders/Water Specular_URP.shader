Shader "Lava Flowing Shader/Water Specular_URP" {

Properties {
    _BaseMap("Base Texture", 2D) = "white" {}
    _MainTex("Main Texture", 2D) = "white" {}
    _AlbedoTex1("Albedo Texture 1", 2D) = "white" {}
    _AlbedoColor("Albedo Color", Color) = (0.15, 0.161, 0.16, 1)
    _NormalMap("Normal Map", 2D) = "bump" {}
    _NormalStrength("Normal Strength", Range(0.1, 5)) = 1
    _ReflectionStrength("Reflection Strength", Range(0.0, 1.0)) = 0.5
    _ReflectionColor("Reflection Color", Color) = (1, 1, 1, 1)
    _WaveSpeed("Wave Speed", Float) = 1.0
    _WaveScale("Wave Scale", Float) = 0.5
    _WaveDirection("Wave Direction", Vector) = (1, 0, 0, 0)
    _AlbedoFlowSpeed("Albedo Flow Speed", Float) = 0.5
}

SubShader {
    Tags { "RenderType"="Transparent" "Queue"="Transparent" }
    LOD 200
    Cull Off
    ZWrite Off
    Blend SrcAlpha OneMinusSrcAlpha

    Pass {
        Name "ForwardLit"
        Tags { "LightMode"="UniversalForward" }

        HLSLPROGRAM
        #pragma target 3.0
        #pragma vertex vert
        #pragma fragment frag

        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

        TEXTURE2D(_BaseMap);
        SAMPLER(sampler_BaseMap);
        
        TEXTURE2D(_MainTex);
        SAMPLER(sampler_MainTex);

        TEXTURE2D(_AlbedoTex1);
        SAMPLER(sampler_AlbedoTex1);

        TEXTURE2D(_NormalMap);
        SAMPLER(sampler_NormalMap);

        float _NormalStrength;
        float _ReflectionStrength;
        float4 _ReflectionColor;
        float _WaveSpeed;
        float _WaveScale;
        float4 _WaveDirection;
        float _AlbedoFlowSpeed;

        struct Attributes {
            float4 positionOS : POSITION;
            float2 uv : TEXCOORD0;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        struct Varyings {
            float4 positionHCS : SV_POSITION;
            float2 uv : TEXCOORD0;
            float3 viewDirWS : TEXCOORD1;
            UNITY_VERTEX_INPUT_INSTANCE_ID
            UNITY_VERTEX_OUTPUT_STEREO
        };

        Varyings vert(Attributes v) {
            Varyings o;
            UNITY_SETUP_INSTANCE_ID(v);
            UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
            UNITY_TRANSFER_INSTANCE_ID(v, o);

            o.positionHCS = TransformObjectToHClip(v.positionOS);
            o.uv = v.uv + float2(_Time.y * _AlbedoFlowSpeed, _Time.y * _AlbedoFlowSpeed);

            float3 worldPos = TransformObjectToWorld(v.positionOS);
            o.viewDirWS = normalize(GetCameraPositionWS() - worldPos);

            return o;
        }

        half4 frag(Varyings i) : SV_Target {
            UNITY_SETUP_INSTANCE_ID(i);
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

            half4 baseColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv);
            half4 albedo = SAMPLE_TEXTURE2D(_AlbedoTex1, sampler_AlbedoTex1, i.uv) * baseColor;
            half3 normal = UnpackNormal(SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, i.uv)) * _NormalStrength;

            // Adjust normal to prevent direct color reflection
            normal = normalize(normal * 2.0 - 1.0);

            // Simulate water movement using sine wave distortion in both directions
            float wave = sin(_Time.y * _WaveSpeed + dot(i.uv, _WaveDirection.xy) * _WaveScale) * 0.1;
            normal.xy += wave;
            normal = normalize(normal);

            // Calculate reflection
            half3 reflected = reflect(normalize(i.viewDirWS), normal);
            reflected = saturate(reflected); // Clamp reflected color to valid range

            // Ensure reflection uses accurate lighting and tint
            half3 reflectionColor = lerp(normalize(reflected), _ReflectionColor.rgb, _ReflectionStrength);

            half4 reflection = half4(reflectionColor, 1.0) * _ReflectionStrength;

            // Combine base, albedo, and reflection
            half4 finalColor = lerp(albedo, reflection, _ReflectionStrength);

            return saturate(finalColor);
        }

        ENDHLSL
    }
}

FallBack "Hidden/InternalErrorShader"
}
