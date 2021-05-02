using System;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ModelEx
{
    public class EffectMorphingUnit2 : Effect
    {
        public VertexShader vertexShader;
        public PixelShader pixelShader;
        public ShaderSignature inputSignature;
        public InputLayout layout;

        public Matrix World;
        public Matrix View;
        public Matrix Projection;
        public Vector3 CameraPosition;

        // Lights
        public Vector3 LightDirection;

        // Colors
        public Vector4 AmbientColor;
        public Vector4 DiffuseColor;
        public Vector4 SpecularColor;
        public Vector4 LightColor;

        public float SpecularPower;

        // Texture
        public bool UseTexture;
        public ShaderResourceView Texture;

        // Rasterizer
        public float DepthBias;

        // Realm
        public float RealmBlend;

        InputElement[] elements = new[] { 
                new InputElement("POSITION", 0, Format.R32G32B32_Float, 0,  0),
                new InputElement("POSITION", 1, Format.R32G32B32_Float, 12, 0),
                new InputElement("COLOR",    0, Format.R32G32B32_Float, 24, 0),
                new InputElement("COLOR",    1, Format.R32G32B32_Float, 36, 0),
                new InputElement("TEXCOORD", 0, Format.R32G32_Float,    48, 0) 
            };

        public override void Initialize()
        {
            base.Initialize();

            try
            {
                using (ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(
                    "Shaders/MorphingUnitVS.fx",
                    "VShader",
                    "vs_4_0",
                    ShaderFlags.None,
                    EffectFlags.None,
                    null,
                    new IncludeFX("Shaders")))
                {
                    vertexShader = new VertexShader(DeviceManager.Instance.device, vertexShaderByteCode);
                    inputSignature = ShaderSignature.GetInputSignature(vertexShaderByteCode);
                }

                using (ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(
                    "Shaders/MorphingUnitPS.fx",
                    "PShader",
                    "ps_4_0",
                    ShaderFlags.None,
                    EffectFlags.None,
                    null,
                    new IncludeFX("Shaders")))
                {
                    pixelShader = new PixelShader(DeviceManager.Instance.device, vertexShaderByteCode);
                }

                layout = new InputLayout(DeviceManager.Instance.device, inputSignature, elements);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public override void Dispose()
        {
            vertexShader?.Dispose();
            pixelShader?.Dispose();
            layout?.Dispose();
        }

        public override void ApplyBlendState()
        {
            base.ApplyBlendState();

            DeviceManager.Instance.context.PixelShader.SetShaderResource(Texture, 0);
            //DeviceManager.Instance.context.PixelShader.SetSampler(SamplerState, 0);
            DeviceManager.Instance.context.VertexShader.Set(vertexShader);
            DeviceManager.Instance.context.PixelShader.Set(pixelShader);
        }
    }
}
