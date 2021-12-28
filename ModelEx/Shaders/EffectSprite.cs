using System;
using System.IO;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	public class EffectSprite : Effect
	{
		public GeometryShader geometryShader;
		public VertexShader vertexShader;
		public PixelShader pixelShader;
		public ShaderSignature inputSignature;
		public InputLayout layout;

		public ShaderResourceView Texture;

		/*InputElement[] elements = new[] {
			new InputElement("TEXCOORD",     0, Format.R32G32_Float,    0, 0),
			new InputElement("COLOR",        0, Format.R32G32B32_Float, 8, 0),
			new InputElement("POSITION",     0, Format.R32G32B32_Float, 24,  0),
			new InputElement("TEXCOORDSIZE", 0, Format.R32G32B32_Float, 32, 0),
			new InputElement("SIZE",         0, Format.R32G32B32_Float, 40, 0),
		};*/

		InputElement[] elements = new[] {
			new InputElement("TEXCOORD",     0, SlimDX.DXGI.Format.R32G32_Float, 0, 0),
			new InputElement("TEXCOORDSIZE", 0, SlimDX.DXGI.Format.R32G32_Float, 8, 0),
			new InputElement("POSITION",     0, SlimDX.DXGI.Format.R32G32_Float, 16, 0),
			new InputElement("SIZE",         0, SlimDX.DXGI.Format.R32G32_Float, 24, 0),
			new InputElement("COLOR",        0, SlimDX.DXGI.Format.B8G8R8A8_UNorm, 32, 0)
		};

		public override void Initialize()
		{
			try
			{
				using (ShaderBytecode geometryShaderByteCode = ShaderBytecode.CompileFromFile(
					"Shaders/Sprite.fx",
					"mainGS",
					"gs_4_0",
					ShaderFlags.None,
					EffectFlags.None,
					null,
					new IncludeFX("Shaders")))
				{
					geometryShader = new GeometryShader(DeviceManager.Instance.device, geometryShaderByteCode);
					inputSignature = ShaderSignature.GetInputSignature(geometryShaderByteCode);
				}

				using (ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(
					"Shaders/Sprite.fx",
					"mainVS",
					"vs_4_0",
					ShaderFlags.None,
					EffectFlags.None,
					null,
					new IncludeFX("Shaders")))
				{
					vertexShader = new VertexShader(DeviceManager.Instance.device, vertexShaderByteCode);
				}

				using (ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(
					"Shaders/Sprite.fx",
					"mainPS",
					"ps_4_0",
					ShaderFlags.None,
					EffectFlags.None,
					null,
					new IncludeFX("Shaders")))
				{
					pixelShader = new PixelShader(DeviceManager.Instance.device, pixelShaderByteCode);
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
			geometryShader?.Dispose();
			vertexShader?.Dispose();
			pixelShader?.Dispose();
			layout?.Dispose();
			Texture?.Dispose();
		}

		public override void Apply(int pass)
		{
			DeviceManager.Instance.context.GeometryShader.Set(geometryShader);
			DeviceManager.Instance.context.VertexShader.Set(vertexShader);
			DeviceManager.Instance.context.PixelShader.Set(pixelShader);
			DeviceManager.Instance.context.PixelShader.SetShaderResource(Texture, 0);
		}
	}
}