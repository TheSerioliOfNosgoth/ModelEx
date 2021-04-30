matrix World;
matrix View;
matrix Projection;
float3 CameraPosition;
float3 LightDirection = float3(1, 1, 1);

bool UseTexture = false;
Texture2D Texture;

float DepthBias = 0;

float4 DiffuseColor = float4(0, 1, 0, 1);
float4 AmbientColor = float4(0.2, 0.2, 0.2, 1);
float4 LightColor = float4(0.9, 0.9, 0.9, 1);
float SpecularPower = 32;
float4 SpecularColor = float4(1, 1, 1, 1);

struct VertexShaderInput
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
	float2 TexCoord : TEXCOORD;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : TEXCOORD0;
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
	output.Normal = mul(input.Normal, World);
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
	output.Normal = mul(input.Normal, World);
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

	// Start with ambient lighting
	float3 lighting = AmbientColor.rgb;

	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);

	// Add lambertian lighting
	lighting += saturate(dot(lightDir, normal)) * LightColor;

	float3 refl = reflect(lightDir, normal);
	float3 view = normalize(input.ViewDirection);

	// Add specular highlights
	lighting += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;

	// Calculate final color
	float3 output = saturate(lighting) * color.rgb;

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