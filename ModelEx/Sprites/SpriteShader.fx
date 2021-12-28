Texture2D Tex;
SamplerState sam
{
	Filter = MIN_MAG_MIP_LINEAR;
	AddressU  = Wrap;
	AddressV  = Wrap;
};
							
struct VS_IN
{
	float2 TexCoord		: TEXCOORD;
	float4 Color		: COLOR;
	float2 Position		: POSITION;
	float2 TexCoordSize	: TEXCOORDSIZE;
	float2 Size			: SIZE;
};

struct GS_OUT
{
	float2 TexCoord : TEXCOORD;
	float4 Color	: COLOR;
	float4 Position : SV_POSITION;
};

struct PS_IN
{
	float2 TexCoord : TEXCOORD;
	float4 Color	: COLOR;
};

DepthStencilState DisableDepth
{
    DepthEnable = FALSE;
    DepthWriteMask = ZERO;
};

[maxvertexcount(4)]
void mainGS( point VS_IN input[1], inout TriangleStream<GS_OUT> TriStream )
{
	/*

	0 -- 1
	|  / |
	| /  |
	2 -- 3

	*/
	GS_OUT v;
	v.Color = input[0].Color;
	v.Position = float4(input[0].Position, 0, 1);
	v.TexCoord = input[0].TexCoord;
	TriStream.Append(v);

	v.Position.x += input[0].Size.x;
	v.TexCoord.x += input[0].TexCoordSize.x;
	TriStream.Append(v);

	v.Position.x = input[0].Position.x;
	v.Position.y -= input[0].Size.y;
	v.TexCoord.x = input[0].TexCoord.x;
	v.TexCoord.y += input[0].TexCoordSize.y;
	TriStream.Append(v);

	v.Position.x += input[0].Size.x;
	v.TexCoord.x += input[0].TexCoordSize.x;
	TriStream.Append(v);

	TriStream.RestartStrip();
}

VS_IN mainVS(VS_IN vs_in){
	return vs_in;
}			

float4 mainPS(PS_IN ps_in) : SV_TARGET {
	return Tex.Sample(sam, ps_in.TexCoord) * ps_in.Color;
}

technique10 Render 
{
	pass p0 
	{	
		SetVertexShader		( CompileShader( vs_4_0 , mainVS() ) );
		SetGeometryShader	( CompileShader( gs_4_0 , mainGS() ) );
		SetPixelShader		( CompileShader( ps_4_0 , mainPS() ) );

		//SetDepthStencilState( DisableDepth, 0 );
	}
}