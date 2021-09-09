﻿// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

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
                uint _Seed;
                uint randindex;

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

                // low bias hash from https://www.shadertoy.com/view/WttXWX
                uint lowbias32(uint x)
                {
                    x ^= x >> 16;
                    x *= 0x7feb352dU;
                    x ^= x >> 15;
                    x *= 0x846ca68bU;
                    x ^= x >> 16;
                    return x;
                }

                float makeRandom() {
                    randindex++;
                    return (float)(lowbias32(_Seed + randindex) % 0x10000000) / (float)0x10000000;
                }

                // function from https://www.shadertoy.com/view/4djSRW
                float hashOld12(fixed2 p)
                {
                    // Two typical hashes...
                    return frac(sin(dot(p, fixed2(12.9898, 78.233))) * 43758.5453);
                }

                // function from https://answers.unity.com/questions/399751/randomity-in-cg-shaders-beginner.html
                float rand(float3 co)
                {
                    return frac(sin(dot(co.xyz, float3(12.9898, 78.233, 45.5432))) * 43758.5453);
                }

                // function from https://www.shadertoy.com/view/4sfGzS
                float hash(float3 p)  // replace this by something better
                {
                    p = frac(p * 0.3183099 + .1 + _Seed);
                    p *= 17.0;
                    return frac(_Seed + p.x * p.y * p.z * (p.x + p.y + p.z));
                }

                fixed4 frag(v2f i) : COLOR{
                    //float opacity = floor(makeRandom() + _Opacity);
                    ////return _Color * opacity + fixed4(0, 0, 0, 0) * (1 - opacity);
                    //return fixed4(1, opacity, opacity, opacity);

                    //randindex = i.vertex.x;
                    //randindex = (i.uv.x + i.uv.y) * (i.uv.x + i.uv.y + 1) / 2 + i.uv.x;
                    //if (makeRandom() > _Opacity) discard;
                    //return _Color;
                    
                    //if (hashOld12(i.uv +_Seed * 1500. + 50.0) > _Opacity) discard;
                    //return _Color;
                    
                    // working:
                    //if (rand(i.vertex) > _Opacity) discard;
                    //return _Color;

                    // working:
                    if (hash(i.vertex) > _Opacity) discard;
                    return _Color;
                }
            ENDCG

        }
    }
}
