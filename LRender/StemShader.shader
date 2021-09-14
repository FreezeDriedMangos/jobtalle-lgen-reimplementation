﻿Shader "Custom/StemShader"
{
    Properties
    {
        [PerRendererData] _Fertility("Fertility", Range(0,1)) = 0
        [PerRendererData] _StemLoad("StemLoad", Range(0,1)) = 0
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "ReplaceWithExposureMaterial"="True" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        #pragma multi_compile_instancing

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Fertility;
        float _StemLoad;


        float4x4 _Rotation;
        float topRadius;
        float bottomRadius;
        float length;
        float3 position;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v) {
            // this shader should be applied to a cylinder with radius 1 and height 1, with its top at the origin and its bottom at (0, -1, 0)
            v.vertex.xz *= lerp(-v.vertex.y, topRadius, bottomRadius);
            v.vertex.y *= length;

            // https://docs.unity3d.com/ScriptReference/Material.SetMatrix.html
            v.vertex.xyz = mul(_Rotation, float4(v.vertex.xyz, 1)).xyz;
            v.vertex.xyz += position;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {


            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, fixed2(_StemLoad, 1-_Fertility)) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
