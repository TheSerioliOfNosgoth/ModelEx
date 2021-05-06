#include "Constants.fx"

struct VertexShaderInput
{
	float4 Position : POSITION;
	float3 Color : COLOR;
	float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Color : TEXCOORD0;
	float3 ViewDirection : TEXCOORD1;
	float2 TexCoord : TEXCOORD2;
};

struct MorphingUnitShaderInput
{
	float4 Position0 : POSITION0;
	float4 Position1 : POSITION1;
	float3 Color0 : COLOR0;
	float3 Color1 : COLOR1;
	float2 TexCoord : TEXCOORD;
};

SamplerState stateLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

VertexShaderOutput VShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Color = input.Color;
	output.Color *= VertexColorFactor;
	output.ViewDirection = worldPosition - CameraPosition;
	output.TexCoord = input.TexCoord;

	output.Position.z -= DepthBias;

	return output;
}

float4 PShader(VertexShaderOutput input) : SV_TARGET
{
	// Start with diffuse color
	float4 color = UseTexture == false ? DiffuseColor : Texture.Sample(stateLinear, input.TexCoord);
	if (color.a < 0.5)
	{
		clip(-1);
	}
	else
	{
		color.a = 1;
	}

	// Calculate final color
	float3 output = input.Color * color.rgb;

	return float4(output, color.a);
}

VertexShaderOutput MorphingUnitVShader(MorphingUnitShaderInput muInput)
{
	VertexShaderInput vsInput;

	vsInput.Position = muInput.Position0 - ((muInput.Position0 - muInput.Position1) * RealmBlend);
	vsInput.Color = muInput.Color0 - ((muInput.Color0 - muInput.Color1) * RealmBlend);
	vsInput.TexCoord = muInput.TexCoord;

	VertexShaderOutput output = VShader(vsInput);

	return output;
}