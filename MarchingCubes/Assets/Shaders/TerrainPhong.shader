Shader "Universal Render Pipeline/TerrainPhongURP"
{
    Properties
    {
        _Ambient ("Ambient Intensity", Range(0,1)) = 0.25
        _Diffuse ("Diffuse Intensity", Range(0,2)) = 1.0
    }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" "Queue"="Geometry" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }
            Cull Back ZWrite On ZTest LEqual

            HLSLPROGRAM
            #pragma target 3.0
            #pragma vertex   vert
            #pragma fragment frag

            // URP feature varijante glavnog svetla
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS_VERTEX _ADDITIONAL_LIGHTS
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 posH   : SV_POSITION;
                float3 posWS  : TEXCOORD0;
                float3 nrmWS  : TEXCOORD1;
                float4 color  : COLOR;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Ambient;
                float _Diffuse;
            CBUFFER_END

            v2f vert(appdata v)
            {
                v2f o;
                float3 posWS = TransformObjectToWorld(v.vertex.xyz);
                o.posH  = TransformWorldToHClip(posWS);
                o.posWS = posWS;
                o.nrmWS = TransformObjectToWorldNormal(v.normal);
                o.color = v.color;
                return o;
            }

            half4 frag(v2f i) : SV_Target
            {
                float3 N = normalize(i.nrmWS);

                // Glavno svetlo u URP
                Light mainLight = GetMainLight(TransformWorldToShadowCoord(i.posWS));
                float3 L = normalize(mainLight.direction);
                float NdL = saturate(dot(N, L));

                float3 lightColor = mainLight.color;

                float3 ambient = _Ambient * lightColor;
                float3 diffuse = _Diffuse * NdL * lightColor;

                float3 albedo = i.color.rgb;
                float3 col = (ambient + diffuse) * albedo;

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
