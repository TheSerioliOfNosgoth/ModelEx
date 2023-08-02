#include "Constants.fx"

struct VertexShaderInput
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
};

struct GeometryShaderInput
{
	float3 Position : POSITION;
	float3 Normal : NORMAL;
};

struct PixelShaderInput
{
	float4 Position : SV_POSITION;
};

struct MorphingUnitShaderInput
{
	float4 Position0 : POSITION0;
	float4 Position1 : POSITION1;
	float3 Normal0 : NORMAL0;
	float3 Normal1 : NORMAL1;
};

GeometryShaderInput VShader(VertexShaderInput input)
{
	GeometryShaderInput output;
	output.Position = input.Position.xyz;
	output.Normal = input.Normal;
	return output;
}

[maxvertexcount(2)]
void GShader(point GeometryShaderInput input[1], inout LineStream<PixelShaderInput> output)
{
	float lineLength = 1.0f;
	matrix viewProjection = mul(View, Projection);

	float3 pointPosition = input[0].Position;
	float3 pointNormal = input[0].Normal;
	float3 lineEndpoint = pointPosition + pointNormal * lineLength;

	float4 worldPosition1 = mul(float4(pointPosition, 1.0f), World);
	PixelShaderInput vertex1;
	vertex1.Position = mul(worldPosition1, viewProjection);
	vertex1.Position.z -= DepthBias;
	output.Append(vertex1);

	float4 worldPosition2 = mul(float4(lineEndpoint, 1.0f), World);
	PixelShaderInput vertex2;
	vertex2.Position = mul(worldPosition2, viewProjection);
	vertex2.Position.z -= DepthBias;
	output.Append(vertex2);
}

float4 PShader(PixelShaderInput input) : SV_TARGET
{
	return float4(1, 1, 1, 1);
}

GeometryShaderInput MorphingUnitVShader(MorphingUnitShaderInput muInput)
{
	VertexShaderInput vsInput;
	vsInput.Position = muInput.Position0 - ((muInput.Position0 - muInput.Position1) * RealmBlend);
	vsInput.Normal = muInput.Normal0 - ((muInput.Normal0 - muInput.Normal1) * RealmBlend);

	GeometryShaderInput output = VShader(vsInput);
	return output;
}