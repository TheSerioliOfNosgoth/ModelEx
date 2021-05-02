cbuffer Constants : register(b0)
	matrix World;
	matrix View;
	matrix Projection;
	float3 CameraPosition;
	float3 LightDirection;
	float4 LightColor; // LightDiffuseColor;
	// float4 LightAmbientColor;
	bool UseTexture;
	float DepthBias;
	float4 DiffuseColor;
	float4 AmbientColor;
	float SpecularPower;
	float4 SpecularColor;
	float RealmBlend;
};

Texture2D Texture : register(t0);