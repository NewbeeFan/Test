Shader "Custom/KajiyaShader"
{
    Properties
    {
        _Tint("Tint", Color) = (1,1,1,1)
        _Postest("Pos", Vector) = (1,2,3,4)
    }
    SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex MyVertexProgram
            #pragma fragment MyFragmentProgram

            #include "UnityCG.cginc"
            #include "Kajiya.cginc"

            float4 _Tint;
            float4 _Postest;

            struct Interpolators {
                float4 position : SV_POSITION;
                float3 localPosition : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                half3 wnormal : TEXCOORD2;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
            };

            v2f MyVertexProgram(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);

                half3 wNormal = UnityObjectToWorldNormal(v.normal);
                half3 wTangent = UnityObjectToWorldDir(v.tangent.xyz);
                o.wnormal = wNormal;
                return o;
            }

            float4 MyFragmentProgram(v2f i) : SV_TARGET
            {
                float3 mywnormal = i.wnormal;
                SurfaceOutputHair mys;
                mys.Albedo = half3(0.5,0.2,0.3);
                mys.Normal = half3(0.5,0,0.3);
                mys.VNormal = half3(0.5,0,0.3);
                mys.Eccentric = half(0.3);
                mys.Alpha = half(0.5);
                mys.Roughness = half(0.2);
                
                float3 LightingDirection = float3(-0.5,0.3,0.5);
                float3 ViewingDirection = float3(-0.5,-0.5,0.5);
                //float3 temp = HairBxDF(mys, direction, mywnormal,direction2,0,0,0);
                float3 color = HairShading(mys, LightingDirection, ViewingDirection, mywnormal, 0, 0, 0);
                return float4(color, 1);
                //return float4(i.tspace1, 1);
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}