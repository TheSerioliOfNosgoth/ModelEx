#include "Parameters.fx"

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

SamplerState stateLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Color = input.Color;
	output.ViewDirection = worldPosition - CameraPosition;
	output.TexCoord = input.TexCoord;

	return output;
}

VertexShaderOutput Gex3VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Color = input.Color;
	output.ViewDirection = worldPosition - CameraPosition;
	output.TexCoord = input.TexCoord;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET
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

technique10 DefaultRender
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction()));
	}
}