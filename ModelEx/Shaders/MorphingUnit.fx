#include "Parameters.fx"

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

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	//float4 position = RealmBlend < 0.5 ? input.Position0 : input.Position1;
	//float3 color = RealmBlend < 0.5 ? input.Color0 : input.Color1;

	float4 position = input.Position0 - ((input.Position0 - input.Position1) * RealmBlend);
	/*float4 color = float4(
		input.Color0.b + ((input.Color0.b - input.Color1.b) * RealmBlend),
		input.Color0.g + ((input.Color0.g - input.Color1.g) * RealmBlend),
		input.Color0.r + ((input.Color0.r - input.Color1.r) * RealmBlend),
		input.Color0.a + ((input.Color0.a - input.Color1.a) * RealmBlend));
		);*/
		/*float3 color = float3(
			input.Color0.b + ((input.Color0.b - input.Color1.b) * RealmBlend),
			input.Color0.g + ((input.Color0.g - input.Color1.g) * RealmBlend),
			input.Color0.r + ((input.Color0.r - input.Color1.r) * RealmBlend));*/

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

VertexShaderOutput Gex3VertexShaderFunction(VertexShaderInput input)
{
	//float4 position = RealmBlend < 0.5 ? input.Position0 : input.Position1;
	//float3 color = RealmBlend < 0.5 ? input.Color0 : input.Color1;

	float4 position = input.Position0 - ((input.Position0 - input.Position1) * RealmBlend);
	/*float4 color = float4(
		input.Color0.b + ((input.Color0.b - input.Color1.b) * RealmBlend),
		input.Color0.g + ((input.Color0.g - input.Color1.g) * RealmBlend),
		input.Color0.r + ((input.Color0.r - input.Color1.r) * RealmBlend),
		input.Color0.a + ((input.Color0.a - input.Color1.a) * RealmBlend));
		);*/
		/*float3 color = float3(
			input.Color0.b + ((input.Color0.b - input.Color1.b) * RealmBlend),
			input.Color0.g + ((input.Color0.g - input.Color1.g) * RealmBlend),
			input.Color0.r + ((input.Color0.r - input.Color1.r) * RealmBlend));*/

	float4 color;
	color.r = input.Color0.r - ((input.Color0.r - input.Color1.r) * RealmBlend);
	color.g = input.Color0.g - ((input.Color0.g - input.Color1.g) * RealmBlend);
	color.b = input.Color0.b - ((input.Color0.b - input.Color1.b) * RealmBlend);
	//color.a = input.Color0.a + ((input.Color0.a - input.Color1.a) * RealmBlend);
	color.a = 1.0;
	color.rbg *= 2.0;

	VertexShaderOutput output;
	float4 worldPosition = mul(position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Color = color;
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