matrix World;
matrix View;
matrix Projection;
float3 CameraPosition;
float3 LightDirection = float3(1, 1, 1);

bool TextureEnabled = false;

float3 DiffuseColor = float3(0, 1, 0);
float3 AmbientColor = float3(0.2, 0.2, 0.2);
float3 LightColor = float3(0.9, 0.9, 0.9);
float SpecularPower = 32;
float3 SpecularColor = float3(1, 1, 1);

struct VertexShaderInput
{
	float4 Position : POSITION;
	float3 Normal : NORMAL;
};

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float3 Normal : TEXCOORD1;
	float3 ViewDirection : TEXCOORD2;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
	VertexShaderOutput output;
	float4 worldPosition = mul(input.Position, World);
	matrix viewProjection = mul(View, Projection);

	output.Position = mul(worldPosition, viewProjection);
	output.Normal = mul(input.Normal, World);
	output.ViewDirection = worldPosition - CameraPosition;

	return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET
{
	// Start with diffuse color
	float3 color = DiffuseColor;

	// Start with ambient lighting
	float3 lighting = AmbientColor;

	float3 lightDir = normalize(LightDirection);
	float3 normal = normalize(input.Normal);

	// Add lambertian lighting
	lighting += saturate(dot(lightDir, normal)) * LightColor;

	float3 refl = reflect(lightDir, normal);
	float3 view = normalize(input.ViewDirection);

	// Add specular highlights
	lighting += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;

	// Calculate final color
	float3 output = saturate(lighting) * color;

	return float4(output, 1);
}

RasterizerState WireframeState
{
	FillMode = Solid;
	CullMode = None;
	FrontCounterClockwise = false;
};

technique10 Render
{
	pass P0
	{
		SetVertexShader(CompileShader(vs_4_0, VertexShaderFunction()));
		SetGeometryShader(NULL);
		SetPixelShader(CompileShader(ps_4_0, PixelShaderFunction()));
		SetRasterizerState(WireframeState);
	}
}



