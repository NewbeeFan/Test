Shader "Custom/NewSurfaceShader"
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
                half3 tspace0 : TEXCOORD2; // tangent.x, bitangent.x, normal.x
                half3 tspace1 : TEXCOORD3; // tangent.y, bitangent.y, normal.y
                half3 tspace2 : TEXCOORD4; // tangent.z, bitangent.z, normal.z
            };

            struct appdata
            {
                float4 vertex : POSITION;
                //float2 uv : TEXCOORD0;
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
                // compute bitangent from cross product of normal and tangent
                half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
                half3 wBitangent = cross(wNormal, wTangent) * tangentSign;
                // output the tangent space matrix
                o.tspace0 = half3(wTangent.x, wBitangent.x, wNormal.x);
                o.tspace1 = half3(wTangent.y, wBitangent.y, wNormal.y);
                o.tspace2 = half3(wTangent.z, wBitangent.z, wNormal.z);
                return o;
            }

            float4 MyFragmentProgram(v2f i) : SV_TARGET
            {
                float3 mywnormal = float3(i.tspace0[2], i.tspace1[2], i.tspace2[2]);
                SurfaceOutputHair mys;
                mys.Albedo = half3(0.5,0.2,0.3);
                mys.Normal = half3(0.5,0,0.3);
                mys.VNormal = half3(0.5,0,0.3);
                mys.Eccentric = half(0.3);
                mys.Alpha = half(0.5);
                mys.Roughness = half(0.2);
                
                float3 LightingDirection = float3(0.5,0.3,0.5);
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
