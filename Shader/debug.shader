Shader "Unlit/debug"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_Specular("Specular", Color) = (1, 1, 1, 1)
		_Gloss1("Gloss1", Range(8.0, 256)) = 20
		_Shift1("Shift1", float) = 0
        _Shininess("Shininess", float) = 20
        _Ks("Ks", float) = 0.5
		_UsePhong("UsePhong", int) = 0
	}

	SubShader
	{
		Tags{ "Queue"="Geometry" "RenderPipeline" = "UniversalPipeline"  "LightMode" = "UniversalForward"}
		LOD 100
		Pass
		{
			Tags {"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
			//#include "HairBxDF.cginc"

			half4 _Diffuse;
			half4 _Specular;
			float _Shift1;
			float _Gloss1;
			float _Shininess;
            float _Ks;
			bool _UsePhong;

			struct a2v
			{
				float4 texcoord : TEXCOORD;
				float4 vertex : POSITION;
				float3 normal : NORMAL;
				float4 tangent : TANGENT;
			};

			struct v2f
			{
				float4 pos : SV_POSITION;
				float3 worldNormal : TEXCOORD0;
				float3 worldPos : TEXCOORD1;
				float3 worldBinormal : TEXCOORD2;
				float3 worldTangent : TEXCOORD3;
			};

			//顶点着色器当中的计算
			v2f vert(a2v v)
			{
				v2f o;
				//转换顶点空间：模型=>投影
				o.pos = TransformObjectToHClip(v.vertex.xyz);
				//转换顶点空间：模型=>世界
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				//转换法线空间：模型=>世界
				half3 worldNormal = TransformObjectToWorldNormal(v.normal, true);
				half3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
                o.worldNormal = worldNormal;
				o.worldTangent = worldTangent;
				o.worldBinormal = cross(worldTangent, worldNormal);
				return o;
			}

			//片元着色器中的计算
			half4 frag(v2f i) : SV_Target
			{
			
				VertexPositionInputs positionInputs = GetVertexPositionInputs(i.pos.xyz);
				
                //Phong specular
                Light light = GetMainLight(float4(positionInputs.positionWS,1.0));
                half3 viewDir = normalize(GetWorldSpaceViewDir(i.worldPos));
                float3 H = normalize(light.direction + viewDir);
                half3 Is_Phong = _Ks * pow(saturate(dot(i.worldNormal, H)), _Shininess);
				
				//获取环境光
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;

                //计算shift后的切线
				float3 worldBinormal1 = normalize(i.worldBinormal + _Shift1 * i.worldNormal);

				//计算第高光  
				float dotTH = dot(worldBinormal1, H);
				float sinTH = sqrt(1.0 - dotTH * dotTH);
				float dirAtten = smoothstep(-1, 0, dotTH);
				float Is_Kajiya = dirAtten * pow(sinTH, _Gloss1);

				//计算output
				if (_UsePhong)
				{
				    half3 specular = _MainLightColor.rgb * _Specular.rgb * Is_Phong;
					half3 diffuse = (1 - _Ks) * _MainLightColor.rgb * _Diffuse.rgb * saturate(dot(i.worldNormal, light.direction));
					//specular *= saturate(diffuse * 2);
					return half4(specular+diffuse, 1.0);
				}
				else
				{
					half3 specular = _MainLightColor.rgb * _Specular.rgb * Is_Kajiya;
					half3 diffuse = _MainLightColor.rgb * _Diffuse.rgb * saturate(dot(i.worldNormal, light.direction));
					specular *= saturate(diffuse * 2);
					return half4(ambient + diffuse + specular, 1.0);
				}
				
				
				//return half4(1.0, 0.0, 0.0, 1.0);
			}
			ENDHLSL
		}
	}
	FallBack "Diffuse"
}

