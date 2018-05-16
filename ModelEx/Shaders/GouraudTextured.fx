matrix World;
matrix View;
matrix Projection;
float3 CameraPosition;
float3 LightDirection = float3(1, 1, 1);

bool TextureEnabled = false;
Texture2D Texture;

SamplerState stateLinear
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU = Wrap;
	AddressV = Wrap;
};

float4 DiffuseColor = float4(0, 1, 0, 1);
float4 AmbientColor = float4(0.2, 0.2, 0.2, 1);
float4 LightColor = float4(0.9, 0.9, 0.9, 1);
float SpecularPower = 32;
float4 SpecularColor = float4(1, 1, 1, 1);

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

float4 PixelShaderFunction(VertexShaderOutput input) : SV_TARGET
{
	// Start with diffuse color
	float4 color = DiffuseColor;

	color *= Texture.Sample(stateLinear, input.TexCoord);

	// Calculate final color
	float4 output = float4(input.Color, 1) * color; // * float3(4, 4, 4, 1);

	return output;
}

BlendState AlphaBlend
{
	BlendEnable[0] = TRUE;
	SrcBlend = SRC_ALPHA;
	DestBlend = INV_SRC_ALPHA;
	BlendOp = ADD;
	RenderTargetWriteMask[0] = 0x0F;
};

RasterizerState WireframeState
{
	FillMode = Solid;
	CullMode = Front; // Is Front for Soul Reaver 2. Normally None.
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
		//SetBlendState(AlphaBlend, float4(0.0f, 0.0f, 0.0f, 0.0f), 0xFFFFFFFF);
	}
}



