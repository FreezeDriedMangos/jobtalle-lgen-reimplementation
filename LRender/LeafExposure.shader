// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

//Shader "Unlit/LeafExposure"
//{
//    Properties
//    {
//        _MainTex ("Texture", 2D) = "white" {}
//        [PerRendererData] 
//        _Color("Color", Color) = (1,1,1,1)
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
        [PerRendererData] _Color("Color", Color) = (1,1,1,1)
        [PerRendererData] _Seed("Seed", Int) = 0
        [PerRendererData] _Opacity("Opacity", Range(0,1)) = 0
    }
    SubShader{
        Tags { "RenderType" = "Opaque" }
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

                fixed4 _Color;
                float _Opacity;
                int _Seed;

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
                    return _Color;
                }
            ENDCG

        }
    }
}
