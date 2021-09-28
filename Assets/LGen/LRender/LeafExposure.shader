// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Shader "Unlit/LeafExposure"
//{
//    Properties
//    {
//        _MainTex ("Texture", 2D) = "white" {}
//        [PerRendererData] 
//        _LeafExposureColor("Color", Color) = (1,1,1,1)
//        [PerRendererData] 
//        _Seed("Seed", Int) = 0
//        [PerRendererData] 
//        _Opacity("Opacity", Range(0,1)) = 0
//    }
//    SubShader
//    {
//        Tags { "RenderType"="Opaque" }
//        LOD 100
//
//        Pass
//        {
//            CGPROGRAM
//            #pragma vertex vert
//            #pragma fragment frag
//            // make fog work
//            #pragma multi_compile_fog
//
//            #include "UnityCG.cginc"
//
//            struct appdata
//            {
//                float4 vertex : POSITION;
//                float2 uv : TEXCOORD0;
//            };
//
//            struct v2f
//            {
//                float2 uv : TEXCOORD0;
//                UNITY_FOG_COORDS(1)
//                float4 vertex : SV_POSITION;
//            };
//
//            sampler2D _MainTex;
//            float4 _MainTex_ST;
//
//            v2f vert (appdata v)
//            {
//                v2f o;
//                o.vertex = UnityObjectToClipPos(v.vertex);
//                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
//                UNITY_TRANSFER_FOG(o,o.vertex);
//                return o;
//            }
//
//            fixed4 frag (v2f i) : SV_Target
//            {
//                // sample the texture
//                fixed4 col = tex2D(_MainTex, i.uv);
//                // apply fog
//                UNITY_APPLY_FOG(i.fogCoord, col);
//                return col;
//            }
//            ENDCG
//        }
//    }
//}


// shader modified from https://forum.unity.com/threads/unlit-single-color-shader.180833/

Shader "Unlit/LeafExposure" {
    Properties{
        [PerRendererData] _LeafExposureColor("Color", Color) = (1,1,1,1)
        [PerRendererData] _Seed("Seed", Int) = 0
        [PerRendererData] _Opacity("Opacity", Range(0,1)) = 0
    }
    SubShader{
        Tags { "RenderType" = "Opaque" "ExposureReplace" = "leaf"}
        LOD 200

        Pass {
            Tags { "LightMode" = "Always" }

            Fog { Mode Off }
            ZWrite On
            ZTest LEqual
            Cull Back
            Lighting Off

            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #pragma fragmentoption ARB_precision_hint_fastest

                fixed4 _LeafExposureColor;
                float _Opacity;
                int _Seed;

                #pragma multi_compile_instancing

                struct appdata {
                    float4 vertex : POSITION;
                };

                struct v2f {
                    float2 uv : TEXCOORD0;
                    float4 vertex : POSITION;
                };

                v2f vert(appdata v) {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    return o;
                }

                // function from https://www.shadertoy.com/view/4sfGzS
                float hash(float3 p)
                {
                    p = frac(p * 0.3183099 + .1);
                    p *= 17.0;
                    return frac(p.x * p.y * p.z * (p.x + p.y + p.z));
                }

                fixed4 frag(v2f i) : COLOR{
                    if (hash(i.vertex + _Seed/ 0x00FFFFFE) > _Opacity) discard;
                    return _LeafExposureColor;
                }
            ENDCG

        }
    }
    SubShader
    {
        // unlit surface shader option from http://answers.unity.com/answers/1510374/view.html

        Tags { "RenderType"="Opaque" "ExposureReplace"="seed" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf NoLighting noambient vertex:vert 

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

        float4x4 _Rotation;
        float radius;
        float4 position;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v) {
            // this shader should be applied to a sphere approximation with radius 1, with its center at the origin
            v.vertex.xyz *= radius;

            // https://docs.unity3d.com/ScriptReference/Material.SetMatrix.html
            v.vertex.xyz = mul(_Rotation, float4(v.vertex.xyz, 1)).xyz;
            v.vertex.xyz += position;
        }
        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = fixed4(1, 1, 1, 1);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
            return fixed4(s.Albedo, s.Alpha);
        }
        ENDCG
    }
    SubShader
    {
        // unlit surface shader option from http://answers.unity.com/answers/1510374/view.html

        Tags { "RenderType"="Opaque" "ExposureReplace"="stem" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf NoLighting noambient vertex:vert

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
        float4 position;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        //UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        //UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v) {
            // this shader should be applied to a cylinder with radius 1 and height 1, with its top at the origin and its bottom at (0, 0, -1)
            v.vertex.xy *= lerp(topRadius, bottomRadius, -v.vertex.z);
            v.vertex.z *= length;

            // https://docs.unity3d.com/ScriptReference/Material.SetMatrix.html
            v.vertex.xyz = mul(_Rotation, float4(v.vertex.xyz, 1)).xyz;
            v.vertex.xyz += position;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = fixed4(1, 1, 1, 1);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }

        fixed4 LightingNoLighting(SurfaceOutput s, fixed3 lightDir, fixed atten) {
            return fixed4(s.Albedo, s.Alpha);
        }

        ENDCG
    }
}
