using System;
using System.IO;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
	[StructLayout(LayoutKind.Explicit, Size = 0x140)]
	public struct EffectConstants
	{
		[FieldOffset(0)]
		public Matrix World;
		[FieldOffset(0x40)]
		public Matrix View;
		[FieldOffset(0x80)]
		public Matrix Projection;
		[FieldOffset(0xC0)]
		public Vector3 CameraPosition;

		// Lights
		[FieldOffset(0xD0)]
		public Vector3 LightDirection;
		[FieldOffset(0xE0)]
		public Vector4 LightColor;

		// Colors
		[FieldOffset(0xF0)]
		public Vector4 AmbientColor;
		[FieldOffset(0x100)]
		public Vector4 DiffuseColor;
		[FieldOffset(0x110)]
		public Vector4 SpecularColor;

		[FieldOffset(0x120)]
		public float SpecularPower;

		// Texture
		[FieldOffset(0x124)]
		public bool UseTexture;
		[FieldOffset(0x128)]
		public float ColorFactor;

		// Rasterizer
		[FieldOffset(0x12C)]
		public float DepthBias;

		// Realm
		[FieldOffset(0x130)]
		public float RealmBlend;
	}

	public abstract class Effect3D : Effect
	{
		protected RasterizerState rasterizerStateDefault;
		protected RasterizerState rasterizerStateDefaultNC;
		protected RasterizerState rasterizerStateWireframe;

		protected BlendState blendStateDefault;
		protected BlendState blendStateAddColour;
		protected BlendState blendStateDepth;

		protected DepthStencilState depthStencilStateSolid;
		protected DepthStencilState depthStencilStateTranslucent;
		protected DepthStencilState depthStencilStateDepth;

		protected bool useBackfaceCulling = true;

		// Render modes
		public int BlendMode;

		public override void Initialize()
		{
			try
			{
				#region Rasterizer States

				RasterizerStateDescription rStateDefault = new RasterizerStateDescription()
				{
					FillMode = FillMode.Solid,
					CullMode = CullMode.Back,
					IsFrontCounterclockwise = true,
					DepthBias = 0,
					DepthBiasClamp = 0,
					//SlopeScaledDepthBias = 0.0f,
					//IsDepthClipEnabled = true
				};

				rasterizerStateDefault = RasterizerState.FromDescription(DeviceManager.Instance.device, rStateDefault);

				RasterizerStateDescription rStateDefaultNC = rStateDefault;
				rStateDefaultNC.CullMode = CullMode.None;
				rasterizerStateDefaultNC = RasterizerState.FromDescription(DeviceManager.Instance.device, rStateDefaultNC);

				RasterizerStateDescription rStateWireframe = new RasterizerStateDescription()
				{
					FillMode = FillMode.Wireframe,
					CullMode = CullMode.None,
					IsFrontCounterclockwise = true,
					DepthBias = 0,
					DepthBiasClamp = 0,
					//SlopeScaledDepthBias = 0.0f,
					//IsDepthClipEnabled = true
				};

				rasterizerStateWireframe = RasterizerState.FromDescription(DeviceManager.Instance.device, rStateWireframe);

				#endregion

				#region Blend States

				RenderTargetBlendDescription rtBlendDefault = new RenderTargetBlendDescription()
				{
					BlendEnable = true,
					BlendOperation = BlendOperation.Add,
					RenderTargetWriteMask = ColorWriteMaskFlags.All,
					SourceBlend = BlendOption.SourceAlpha,
					DestinationBlend = BlendOption.InverseSourceAlpha,
					BlendOperationAlpha = BlendOperation.Add,
					SourceBlendAlpha = BlendOption.One,
					DestinationBlendAlpha = BlendOption.Zero
				};

				BlendStateDescription bBlendStateDefault = new BlendStateDescription();
				bBlendStateDefault.AlphaToCoverageEnable = false;
				bBlendStateDefault.IndependentBlendEnable = false;
				bBlendStateDefault.RenderTargets[0] = rtBlendDefault;

				blendStateDefault = BlendState.FromDescription(DeviceManager.Instance.device, bBlendStateDefault);

				RenderTargetBlendDescription rtBlendAddColour = new RenderTargetBlendDescription()
				{
					BlendEnable = true,
					BlendOperation = BlendOperation.Add,
					RenderTargetWriteMask = ColorWriteMaskFlags.All,
					SourceBlend = BlendOption.One,
					DestinationBlend = BlendOption.One,
					BlendOperationAlpha = BlendOperation.Add,
					SourceBlendAlpha = BlendOption.One,
					DestinationBlendAlpha = BlendOption.Zero
				};

				BlendStateDescription bBlendStateAddColour = new BlendStateDescription();
				bBlendStateAddColour.AlphaToCoverageEnable = false;
				bBlendStateAddColour.IndependentBlendEnable = false;
				bBlendStateAddColour.RenderTargets[0] = rtBlendAddColour;

				blendStateAddColour = BlendState.FromDescription(DeviceManager.Instance.device, bBlendStateAddColour);

				// UseAlphaMask = textureAttributes & 0x0010
				/*RenderTargetBlendDescription rtBlendSubstactColour = new RenderTargetBlendDescription()
                {
                    BlendEnable = true,
                    BlendOperation = BlendOperation.ReverseSubtract,
                    RenderTargetWriteMask = ColorWriteMaskFlags.All,
                    SourceBlend = BlendOption.SourceColor, // One works too
                    DestinationBlend = BlendOption.InverseSourceColor, // One works too
                    BlendOperationAlpha = BlendOperation.Add,
                    SourceBlendAlpha = BlendOption.One,
                    DestinationBlendAlpha = BlendOption.Zero
                };*/

				RenderTargetBlendDescription rtBlendDepth = new RenderTargetBlendDescription()
				{
					BlendEnable = false,
					RenderTargetWriteMask = ColorWriteMaskFlags.None
				};

				BlendStateDescription bBlendStateDepth = new BlendStateDescription();
				bBlendStateDepth.AlphaToCoverageEnable = false;
				bBlendStateDepth.IndependentBlendEnable = false;
				bBlendStateDepth.RenderTargets[0] = rtBlendDepth;

				blendStateDepth = BlendState.FromDescription(DeviceManager.Instance.device, bBlendStateDepth);

				#endregion

				#region Depth Stencils

				DepthStencilOperationDescription frontFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Increment,
					PassOperation = StencilOperation.Keep,
					Comparison = Comparison.Always
				};

				DepthStencilOperationDescription backFace = new DepthStencilOperationDescription()
				{
					FailOperation = StencilOperation.Keep,
					DepthFailOperation = StencilOperation.Decrement,
					PassOperation = StencilOperation.Keep,
					Comparison = Comparison.Always
				};

				DepthStencilStateDescription dsStateSolid = new DepthStencilStateDescription
				{
					DepthComparison = Comparison.LessEqual,
					DepthWriteMask = DepthWriteMask.All,
					IsDepthEnabled = true,
					IsStencilEnabled = false,
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF,
					FrontFace = frontFace,
					BackFace = backFace
				};

				depthStencilStateSolid = DepthStencilState.FromDescription(DeviceManager.Instance.device, dsStateSolid);

				DepthStencilStateDescription dsStateTranslucent = new DepthStencilStateDescription
				{
					DepthComparison = Comparison.LessEqual,
					DepthWriteMask = DepthWriteMask.Zero,
					IsDepthEnabled = true,
					IsStencilEnabled = false,
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF,
					FrontFace = frontFace,
					BackFace = backFace
				};

				depthStencilStateTranslucent = DepthStencilState.FromDescription(DeviceManager.Instance.device, dsStateTranslucent);

				DepthStencilStateDescription dsStateDepth = new DepthStencilStateDescription
				{
					DepthComparison = Comparison.LessEqual,
					DepthWriteMask = DepthWriteMask.All,
					IsDepthEnabled = true,
					IsStencilEnabled = false,
					StencilReadMask = 0xFF,
					StencilWriteMask = 0xFF,
					FrontFace = frontFace,
					BackFace = backFace
				};

				depthStencilStateDepth = DepthStencilState.FromDescription(DeviceManager.Instance.device, dsStateDepth);

				#endregion
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.ToString());
			}
		}

		public override void Dispose()
		{
			rasterizerStateDefault?.Dispose();
			rasterizerStateDefaultNC?.Dispose();
			rasterizerStateWireframe?.Dispose();

			blendStateDefault?.Dispose();
			blendStateAddColour?.Dispose();
			blendStateDepth?.Dispose();

			depthStencilStateSolid?.Dispose();
			depthStencilStateTranslucent?.Dispose();
			depthStencilStateDepth?.Dispose();
		}

		public override void Apply(int pass)
		{
			if (pass == 0)
			{
				DeviceManager.Instance.context.OutputMerger.BlendState = blendStateDepth;
				DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateDepth;
			}
			else
			{
				switch (BlendMode)
				{
					case 1:
						DeviceManager.Instance.context.OutputMerger.BlendState = blendStateAddColour;
						DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateTranslucent;
						break;
					default:
						DeviceManager.Instance.context.OutputMerger.BlendState = blendStateDefault;
						DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateSolid;
						break;
				}
			}

			if (RenderManager.Instance.Wireframe)
			{
				DeviceManager.Instance.context.Rasterizer.State = rasterizerStateWireframe;
			}
			else
			{
				DeviceManager.Instance.context.Rasterizer.State = useBackfaceCulling ? rasterizerStateDefault : rasterizerStateDefaultNC;
			}
		}
	}
}
