#ifndef HAIRKAJIYA
#define HAIRKAJIYA

struct SurfaceOutputHair
// Upgrade NOTE: excluded shader from DX11, OpenGL ES 2.0 because it uses unsized arrays
// #pragma exclude_renderers d3d11 gles
{
	half3 Albedo;
	half3 Normal;//Tangent actually
	half3 VNormal;//vertext normal
	half Eccentric;
	half Alpha;
	half Roughness;
	half3 Emission;
	half Specular;
};


#define PI 3.1415926

inline float square(float x) {
	return x * x;
}

float acosFast(float inX)
{
	float x = abs(inX);
	float res = -0.156583f * x + (0.5 * PI);
	res *= sqrt(1.0f - x);
	return (inX >= 0) ? res : PI - res;
}


#define SQRT2PI 2.50663

//Gaussian Distribution for M term
inline float Hair_G(float B, float Theta)
{
	return exp(-0.5 * square(Theta) / (B*B)) / (SQRT2PI * B);
}

float HairIOF(float Eccentric) {
	float n = 1.55;
	float a = 1 - Eccentric;
	float ior1 = 2 * (n - 1) * (a * a) - n + 2;
	float ior2 = 2 * (n - 1) / (a * a) - n + 2;
	return 0.5f * ((ior1 + ior2) + 0.5f * (ior1 - ior2)); //assume cos2PhiH = 0.5f 
}

inline float3 SpecularFresnel(float3 F0, float vDotH) {
	return F0 + (1.0f - F0) * pow(1 - vDotH, 5);
}

inline float3 SpecularFresnelLayer(float3 F0, float vDotH, float layer) {
	float3 fresnel = SpecularFresnel(F0,  vDotH);
    return (fresnel * layer) / (1 + (layer-1) * fresnel);
}

float3 HairDiffuseKajiyaUE(SurfaceOutputHair s, float3 L, float3 V, half3 N, half Shadow, float Backlit, float Area) {
	float3 S = 0;
	float KajiyaDiffuse = 1 - abs(dot(N, L));

	float3 FakeNormal = normalize(V - N * dot(V, N));
	N = FakeNormal;

	// Hack approximation for multiple scattering.
	float Wrap = 1;
	float NoL = saturate((dot(N, L) + Wrap) / square(1 + Wrap));
	float DiffuseScatter = (1 / PI) * lerp(NoL, KajiyaDiffuse, 0.33);// *s.Metallic;
	float Luma = Luminance(s.Albedo);
	float3 ScatterTint = pow(s.Albedo / Luma, 1 - Shadow);
	S = sqrt(s.Albedo) * DiffuseScatter * ScatterTint;
	return S;
}

float3 HairSpecularKajiya(SurfaceOutputHair s, float3 tangent1, float3 tangent2, float3 V, float3 L)
{
	float3 H = normalize(L + V);

	float TdotH1 = dot(tangent1, H);
	float TdotH2 = dot(tangent2, H);

	float sinTH1 = sqrt(1.0f - saturate(TdotH1 * TdotH1));
	float sinTH2 = sqrt(1.0f - saturate(TdotH2 * TdotH2));

	// Attenuate the primary highlight by the fresnel term to give some room for the secondary highlight
	float3 fresnel = SpecularFresnel(s.Albedo, saturate(dot(H, V)));

	float3 specular = 0;
	specular += fresnel * s.Albedo * pow(sinTH1, 1);
	specular += (1.0 - fresnel) * s.Albedo * pow(sinTH2, (1 - s.Roughness) * 100);
	return specular;
}

float3 HairShading(SurfaceOutputHair s, float3 L, float3 V, half3 N, float Shadow, float Backlit, float Area)
{
	float3 S = float3(0, 0, 0);
	S = HairSpecularKajiya(s, N, N, V, L); 
	S = -min(-S, 0.0);
	return S;
}

float3 HairBxDF(SurfaceOutputHair s, half3 N, half3 V, half3 L, float Shadow, float Backlit, float Area)
{
	float3 S = float3(0, 0, 0);
	S = HairSpecularKajiya(s, N, N, V, L); 
	S += HairDiffuseKajiyaUE(s, L, V, N, Shadow, Backlit, Area);
	S = -min(-S, 0.0);

	return S;
}

#endif