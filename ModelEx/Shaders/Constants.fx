cbuffer Constants : register(b0)
{
	matrix World;
	matrix View;
	matrix Projection;
	float3 CameraPosition;
	float3 LightDirection;
	float4 LightColor; // LightDiffuseColor;
	// float4 LightAmbientColor;
	float4 AmbientColor;
	float4 DiffuseColor;
	float4 SpecularColor;
	float SpecularPower;
	bool UseTexture;
	float VertexColorFactor;
	float DepthBias;
	float RealmBlend;
};

Texture2D Texture : register(t0);
SamplerState Sampler : register(s0);