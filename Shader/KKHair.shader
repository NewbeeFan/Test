Shader "Unlit/KKHair"
{
	Properties
	{
		_Diffuse("Diffuse", Color) = (1, 1, 1, 1)
		_Specular("Specular", Color) = (1, 1, 1, 1)
		_Shadow("Shadow", float) = 0.5
		_Backlit("Backlit", float) = 2
		_Area("Area", float) = 0.5
		_Gloss1("Gloss1", Range(8.0, 256)) = 20
		_Gloss2("Gloss2", Range(8.0, 256)) = 20
		_Shift1("Shift1", float) = 0
		_Shift2("Shift2", float) = 0
		_NoiseTex("NoiseTex" , 2D) = "white"{}
		_AlphaTex("Alpha Tex", 2D) = "white" {}
		_MaskTex("Mask Tex", 2D) = "white" {}

		_MedulaScatter("MedulaScatter", float) = 0.3
		_MedulaAbsorb("MedulaAbsorb", float) = 0.2
		_Layer("Layer", float) = 0.2
		_Kappa("Kappa", float) = 0.2

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
			#include "HairBxDF.cginc"

			half4 _Diffuse;
			half4 _Specular;
			float _Shadow;
			float _Backlit;
			float _Area;
			float _Shift1;
			float _Shift2;
			float _Gloss1;
			float _Gloss2;
			sampler2D _NoiseTex;
			float4 _NoiseTex_ST;
			float4 _AlphaTex_ST;
			sampler2D _AlphaTex;
			sampler2D _MaskTex;
			float4 _MaskTex_ST;

			float _MedulaScatter;
			float _MedulaAbsorb;
			float _Layer;
			float _Kappa;

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
				float4 uv : TEXCOORD4;
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
				o.worldNormal = worldNormal;
				half3 worldTangent = TransformObjectToWorldDir(v.tangent.xyz);
				o.worldTangent = worldTangent;
				o.worldBinormal = cross(worldTangent, worldNormal);
				o.uv.xy = v.texcoord.xy * _NoiseTex_ST.xy + _NoiseTex_ST.zw;
				o.uv.zw = v.texcoord.xy * _AlphaTex_ST.xy + _AlphaTex_ST.zw;
				return o;
			}

			//片元着色器中的计算
			half4 frag(v2f i) : SV_Target
			{
				
				half Alpha = tex2D(_AlphaTex, i.uv.zw).r;
				clip(Alpha -0.5);

				//获取环境光
				half3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz;
				VertexPositionInputs positionInputs = GetVertexPositionInputs(i.pos.xyz);
				//VertexNormalInputs normalInputs = GetVertexNormalInputs(float4(i.worldNormal,1.0), float4(i.worldTangent,1.0));
				
				//half3 vertexLight = VertexLighting(positionInputs.positionWS, normalInputs.normalWS);
				Light light = GetMainLight(float4(positionInputs.positionWS,1.0));

				//half3 lightDir = normalize(GetWorldSpaceLightDir(i.worldPos));
				half3 viewDir = normalize(GetWorldSpaceViewDir(i.worldPos));
				half3 reflectDir = normalize(light.direction + viewDir);

				float shift = tex2D(_NoiseTex, i.uv.xy).r - 0.5;
				float shift1 = shift - _Shift1;
				float shift2 = shift - _Shift2;
				float3 worldBinormal = i.worldBinormal;
				float3 worldNormal = i.worldNormal;
				float3 worldBinormal1 = normalize(worldBinormal + shift1 * worldNormal);
				float3 worldBinormal2 = normalize(worldBinormal + shift2 * worldNormal);

				//计算第高光  
				float3 H1 = normalize(light.direction + viewDir);
				float dotTH1 = dot(worldBinormal1, H1);
				float sinTH1 = sqrt(1.0 - dotTH1 * dotTH1);
				float dirAtten1 = smoothstep(-1, 0, dotTH1);
				float S1 = dirAtten1 * pow(sinTH1, _Gloss1);

				

				half3 specular = _MainLightColor.rgb * _Specular.rgb * S1;
				//Lanbert光照
				half3 diffuse = _MainLightColor.rgb * _Diffuse.rgb * saturate(dot(i.worldNormal, light.direction));
				//half3 diffuse = _MainLightColor.rgb * _Diffuse.rgb;
				//对高光范围进行遮罩
				specular *= saturate(diffuse * 2);

				//fixed4 maskTex = tex2D(_MaskTex, i.uv.xy);
				float3 mywnormal = i.worldNormal;
                SurfaceOutputHair mys;
                mys.Albedo = half3(_Diffuse[0],_Diffuse[1],_Diffuse[2]);
                mys.Normal = half3(0.5,0,0.3);
                mys.VNormal = half3(0.5,0,0.3);
                mys.Eccentric = half(0.3);
                mys.Alpha = half(0.5);
                mys.Roughness = half(0.2);

				SurfaceOutputFur myfur;
				myfur.Albedo = half3(_Diffuse[0],_Diffuse[1],_Diffuse[2]);
                myfur.Normal = half3(0,0,0);
                myfur.VNormal = half3(0,0,0);
                myfur.Emission = half(0.3);
                myfur.Alpha = half(0.5);
                myfur.Roughness = half(0.2);
				myfur.MedulaScatter = _MedulaScatter;
				myfur.MedulaAbsorb = _MedulaAbsorb;
				myfur.Layer = _Layer;
				myfur.Kappa = _Kappa;
                
				float3 color = HairBxDF(mys, mywnormal, viewDir, light.direction, _Shadow, _Backlit, _Area);

				return half4(diffuse+ specular, 1.0);
				//return fixed4(0.5,0,0.6,1.0);

			}
			ENDHLSL
		}
	}
	FallBack "Diffuse"
}
