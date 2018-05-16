matrix gWVP;
float4 colorSolid;
float4 colorWireframe;

float4 VShader(float4 position : POSITION) : SV_POSITION
{
	return mul( position, gWVP);
}

float4 PShader(float4 position : SV_POSITION) : SV_Target
{
	return colorSolid;
}

float4 PShaderWireframe(float4 position : SV_POSITION) : SV_Target
{
	return colorWireframe;
}

RasterizerState SolidState
{
	FillMode = Solid;
};

RasterizerState WireframeState
{
    FillMode = Wireframe;
    SlopeScaledDepthBias = -0.5f;
};

technique10 Render
{
	pass P0
	{
		SetVertexShader( CompileShader( vs_4_0, VShader() ));
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PShader() ));
		SetRasterizerState(SolidState);
	}

	pass P1
	{
		SetVertexShader( CompileShader( vs_4_0, VShader() ));
		SetGeometryShader( NULL );
		SetPixelShader( CompileShader( ps_4_0, PShaderWireframe() ));
		SetRasterizerState(WireframeState);
	}
}