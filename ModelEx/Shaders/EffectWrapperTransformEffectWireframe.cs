using System;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	public class EffectWrapperTransformEffectWireframe : Effect3D
	{
		public ShaderSignature inputSignature;
		public EffectTechnique technique;
		public EffectPass pass;

		public SlimDX.Direct3D11.Effect effect;

		public InputLayout layout;

		public EffectMatrixVariable tmat;
		public EffectVectorVariable mCol;
		public EffectVectorVariable wfCol;

		public override void Initialize()
		{
			base.Initialize();

			try
			{
				using (ShaderBytecode effectByteCode = ShaderBytecode.CompileFromFile(
					"Shaders/transformEffectWireframe.fx",
					"Render",
					"fx_5_0",
					ShaderFlags.EnableStrictness,
					EffectFlags.None))
				{
					effect = new SlimDX.Direct3D11.Effect(DeviceManager.Instance.device, effectByteCode);
					technique = effect.GetTechniqueByIndex(0);
					pass = technique.GetPassByIndex(0);
					inputSignature = pass.Description.Signature;
				}

				var elements = new[] {
					new InputElement("POSITION", 0, Format.R32G32B32_Float, 0),
				};
				layout = new InputLayout(DeviceManager.Instance.device, inputSignature, elements);

				tmat = effect.GetVariableByName("gWVP").AsMatrix();
				wfCol = effect.GetVariableByName("colorWireframe").AsVector();
				mCol = effect.GetVariableByName("colorSolid").AsVector();
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public override void Dispose()
		{
			effect?.Dispose();
			layout?.Dispose();

			base.Dispose();
		}
	}
}