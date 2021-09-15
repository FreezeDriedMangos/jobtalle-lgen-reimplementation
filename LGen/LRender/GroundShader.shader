Shader "Custom/GroundShader"
{
    Properties
    {
        _FertilityMap ("FertilityMap (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _FertilityMap;

        struct Input
        {
            float2 uv_MainTex;
            float3 objPos;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)


        /*void vert(inout appdata_full v) {
            v.vertex.xyz += v.normal * 2;
        }*/

        void vert(inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            float c = tex2D(_FertilityMap, v.texcoord).r;
            v.vertex.xyz += v.normal * c;

        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float fertility = tex2D(_FertilityMap, IN.uv_MainTex).r;

            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, fixed2(1-fertility, 0)) * _Color;
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
