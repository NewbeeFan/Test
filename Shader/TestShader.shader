Shader "Custom/TestShader"
{
    Properties{
        _BaseColor1("First Color",Color) = (1.0,1.0,1.0,1.0)
        _BaseColor2("Second Color",Color) = (1.0,1.0,1.0,1.0)
        _Weight("Lerp Weight",Range(0,1)) = 0.5
    }

    SubShader{
        pass{
            //Tags{"LightMode"="UniversalForward"}
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Pixel
            //#include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct vertexOutput{

                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
            };

            fixed4 _BaseColor1;
            fixed4 _BaseColor2;
            fixed _Weight;

            vertexOutput Vertex(appdata_base v){

                vertexOutput o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                o.worldPos = mul(unity_ObjectToWorld,v.vertex).xyz;

                return o;
            }

            fixed4 Pixel(vertexOutput i):SV_TARGET
            {
                
                fixed3 lightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
                fixed3 worldNormal = normalize(i.worldNormal);

                fixed3 albedo = lerp(_BaseColor1.xyz,_BaseColor2.xyz,_Weight);
                //在颜色1和颜色2之间以weight过度

                fixed3 ambient = UNITY_LIGHTMODEL_AMBIENT.xyz * albedo;
                fixed3 diffuse = _LightColor0.xyz * albedo * saturate(dot(worldNormal,lightDir));

                return fixed4( albedo,1.0);
            }

            ENDCG
        }
    }
}