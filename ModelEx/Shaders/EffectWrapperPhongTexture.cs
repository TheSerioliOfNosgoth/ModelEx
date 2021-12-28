using System;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	public class EffectWrapperPhongTexture : Effect3D
	{
		public VertexShader vertexShader;
		public PixelShader pixelShader;
		public ShaderSignature inputSignature;
		public InputLayout layout;

		public EffectConstants Constants;
		SlimDX.Direct3D11.Buffer ConstantsBuffer;
		public ShaderResourceView Texture;

		InputElement[] elements = new[] {
				new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
				new InputElement("NORMAL", 0, Format.R32G32B32_Float, 12, 0),
				new InputElement("TEXCOORD", 0, Format.R32G32_Float, 24, 0)
			};

		public override void Initialize()
		{
			base.Initialize();

			try
			{
				using (ShaderBytecode vertexShaderByteCode = ShaderBytecode.CompileFromFile(
					"Shaders/PhongTextured.fx",
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

				using (ShaderBytecode pixelShaderByteCode = ShaderBytecode.CompileFromFile(
					"Shaders/PhongTextured.fx",
					"PShader",
					"ps_4_0",
					ShaderFlags.None,
					EffectFlags.None,
					null,
					new IncludeFX("Shaders")))
				{
					pixelShader = new PixelShader(DeviceManager.Instance.device, pixelShaderByteCode);
				}

				layout = new InputLayout(DeviceManager.Instance.device, inputSignature, elements);

				ConstantsBuffer = new SlimDX.Direct3D11.Buffer(
					DeviceManager.Instance.device,
					0x140,//Marshal.SizeOf(Constants),
					ResourceUsage.Dynamic,
					BindFlags.ConstantBuffer,
					CpuAccessFlags.Write,
					ResourceOptionFlags.None,
					0
				);
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
			inputSignature?.Dispose();
			layout?.Dispose();
			ConstantsBuffer?.Dispose();
			Texture?.Dispose();
		}

		public override void Apply(int pass)
		{
			base.Apply(pass);

			DataBox box = DeviceManager.Instance.context.MapSubresource(ConstantsBuffer, 0, MapMode.WriteDiscard, SlimDX.Direct3D11.MapFlags.None);
			box.Data.Write(Constants);
			box.Data.Position = 0;
			DeviceManager.Instance.context.UnmapSubresource(ConstantsBuffer, 0);

			//DeviceManager.Instance.context.PixelShader.SetSampler(SamplerState, 0);

			DeviceManager.Instance.context.VertexShader.Set(vertexShader);
			DeviceManager.Instance.context.VertexShader.SetConstantBuffer(ConstantsBuffer, 0);

			if (pass == 0)
			{
				DeviceManager.Instance.context.PixelShader.Set(null);
			}
			else
			{
				DeviceManager.Instance.context.PixelShader.Set(pixelShader);
				DeviceManager.Instance.context.PixelShader.SetConstantBuffer(ConstantsBuffer, 0);
				DeviceManager.Instance.context.PixelShader.SetShaderResource(Texture, 0);
			}
		}
	}
}
