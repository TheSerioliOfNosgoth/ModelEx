#include "Constants.fx"

struct VertexShaderInput
{
	float4 Position0 : POSITION0;
	float4 Position1 : POSITION1;
	float3 Color0 : COLOR0;
	float3 Color1 : COLOR1;
	float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Color : TEXCOORD0;
	float3 ViewDirection : TEXCOORD1;
	float2 TexCoord : TEXCOORD2;
};

SamplerState stateLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

float4 PShader(VertexShaderOutput input) : SV_TARGET
{
	// Start with diffuse color
	float4 color = UseTexture == false ? DiffuseColor : Texture.Sample(stateLinear, input.TexCoord);
	if (color.a < 0.75)
	{
		clip(-1);
	}

	// Calculate final color
	float3 output = input.Color * color.rgb;

	return float4(output, color.a);
}