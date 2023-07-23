#include "Constants.fx"

struct VertexShaderInput
{
	float4 Position : POSITION;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 ViewDirection : TEXCOORD0;
};

struct MorphingUnitShaderInput
{
	float4 Position0 : POSITION0;
	float4 Position1 : POSITION1;
};

VertexShaderOutput VShader(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.ViewDirection = worldPosition - CameraPosition;

	output.Position.z -= DepthBias;

	return output;
}

float4 PShader(VertexShaderOutput input) : SV_TARGET
{
	return DiffuseColor;
}

VertexShaderOutput MorphingUnitVShader(MorphingUnitShaderInput muInput)
{
	VertexShaderInput vsInput;

	vsInput.Position = muInput.Position0 - ((muInput.Position0 - muInput.Position1) * RealmBlend);

	VertexShaderOutput output = VShader(vsInput);

	return output;
}