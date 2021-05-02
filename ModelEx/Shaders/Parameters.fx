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

float RealmBlend = 0.0f;