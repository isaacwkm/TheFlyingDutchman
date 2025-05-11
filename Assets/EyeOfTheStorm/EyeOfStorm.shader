Shader "Hidden/EyeOfStormFog_Working"
{
    Properties
    {
        _FogColor ("Fog Color", Color) = (0.4, 0.4, 0.4, 1)
        _FogStart ("Fog Start Radius", Float) = 30
        _FogEnd ("Fog End Radius", Float) = 100
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Overlay" }

        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            float4 _FogColor;
            float _FogStart;
            float _FogEnd;

            v2f vert(appdata v)
            {
                v2f o;

                float4 worldPos = TransformObjectToWorld(v.vertex);
                o.vertex = TransformWorldToHClip(worldPos);
                o.uv = v.uv;
                return o;
            }

            float4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv * 2.0 - 1.0;

                float3 rayDir = normalize(float3(uv.x, 0, 1));
                float3 worldPos = _WorldSpaceCameraPos + rayDir * 50;

                float2 offsetXZ = worldPos.xz - _WorldSpaceCameraPos.xz;
                float dist = length(offsetXZ);

                float fogFactor = saturate((dist - _FogStart) / (_FogEnd - _FogStart));
                return float4(_FogColor.rgb, fogFactor);
            }
            ENDHLSL
        }
    }
}
