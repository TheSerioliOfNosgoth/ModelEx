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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	float4 position = input.Position0 - ((input.Position0 - input.Position1) * RealmBlend);

	float4 color;
	color.r = input.Color0.r - ((input.Color0.r - input.Color1.r) * RealmBlend);
	color.g = input.Color0.g - ((input.Color0.g - input.Color1.g) * RealmBlend);
	color.b = input.Color0.b - ((input.Color0.b - input.Color1.b) * RealmBlend);
	//color.a = input.Color0.a + ((input.Color0.a - input.Color1.a) * RealmBlend);
	color.a = 1.0;

	VertexShaderOutput output;
	float4 worldPosition = mul(position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Color = color;
	output.ViewDirection = worldPosition - CameraPosition;
	output.TexCoord = input.TexCoord;

	output.Position.z -= DepthBias;

	return output;
}