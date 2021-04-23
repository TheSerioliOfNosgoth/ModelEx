using System;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
    public abstract class Effect
    {
        protected BlendState blendStateDefault;
        protected BlendState blendStateSubtractColour;
        protected BlendState blendStateAddColour;

        protected DepthStencilState depthStencilStateSolid;
        protected DepthStencilState depthStencilStateTranslucent;

        // Render modes
        public int BlendMode;

        public virtual void Dispose()
        {
            blendStateDefault?.Dispose();
            blendStateSubtractColour?.Dispose();
            blendStateAddColour?.Dispose();

            depthStencilStateSolid?.Dispose();
            depthStencilStateTranslucent?.Dispose();
        }

        public virtual void Initialize()
        {
            try
            {
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

                RenderTargetBlendDescription rtBlendSubtractColour = new RenderTargetBlendDescription()
                {
                    BlendEnable = true,
                    BlendOperation = BlendOperation.Add,
                    RenderTargetWriteMask = ColorWriteMaskFlags.All,
                    SourceBlend = BlendOption.SourceColor,
                    DestinationBlend = BlendOption.InverseSourceColor,
                    BlendOperationAlpha = BlendOperation.Add,
                    SourceBlendAlpha = BlendOption.One,
                    DestinationBlendAlpha = BlendOption.Zero
                };

                BlendStateDescription bBlendStateSubtractColor = new BlendStateDescription();
                bBlendStateSubtractColor.AlphaToCoverageEnable = false;
                bBlendStateSubtractColor.IndependentBlendEnable = false;
                bBlendStateSubtractColor.RenderTargets[0] = rtBlendSubtractColour;

                blendStateSubtractColour = BlendState.FromDescription(DeviceManager.Instance.device, bBlendStateSubtractColor);

                RenderTargetBlendDescription rtBlendAddColour = new RenderTargetBlendDescription()
                {
                    BlendEnable = true,
                    BlendOperation = BlendOperation.Add,
                    RenderTargetWriteMask = ColorWriteMaskFlags.All,
                    SourceBlend = BlendOption.SourceColor,
                    DestinationBlend = BlendOption.InverseSourceColor,
                    BlendOperationAlpha = BlendOperation.Add,
                    SourceBlendAlpha = BlendOption.One,
                    DestinationBlendAlpha = BlendOption.Zero
                };

                BlendStateDescription bBlendStateAddColour = new BlendStateDescription();
                bBlendStateAddColour.AlphaToCoverageEnable = false;
                bBlendStateAddColour.IndependentBlendEnable = false;
                bBlendStateAddColour.RenderTargets[0] = rtBlendAddColour;

                blendStateAddColour = BlendState.FromDescription(DeviceManager.Instance.device, bBlendStateAddColour);

                DepthStencilStateDescription dsStateSolid = new DepthStencilStateDescription();

                dsStateSolid.DepthComparison = Comparison.Less;
                dsStateSolid.DepthWriteMask = DepthWriteMask.All;
                dsStateSolid.IsDepthEnabled = true;
                dsStateSolid.IsStencilEnabled = false;
                dsStateSolid.StencilReadMask = 0xFF;
                dsStateSolid.StencilWriteMask = 0xFF;

                dsStateSolid.FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                };

                dsStateSolid.BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                };

                depthStencilStateSolid = DepthStencilState.FromDescription(DeviceManager.Instance.device, dsStateSolid);

                DepthStencilStateDescription dsStateTranslucent = new DepthStencilStateDescription();

                dsStateTranslucent.DepthComparison = Comparison.Less;
                dsStateTranslucent.DepthWriteMask = DepthWriteMask.All;
                dsStateTranslucent.IsDepthEnabled = false;
                dsStateTranslucent.IsStencilEnabled = false;
                dsStateTranslucent.StencilReadMask = 0xFF;
                dsStateTranslucent.StencilWriteMask = 0xFF;

                dsStateTranslucent.FrontFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Increment,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                };

                dsStateTranslucent.BackFace = new DepthStencilOperationDescription()
                {
                    FailOperation = StencilOperation.Keep,
                    DepthFailOperation = StencilOperation.Decrement,
                    PassOperation = StencilOperation.Keep,
                    Comparison = Comparison.Always
                };

                depthStencilStateTranslucent = DepthStencilState.FromDescription(DeviceManager.Instance.device, dsStateTranslucent);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void ApplyBlendState()
        {
            switch (BlendMode)
            {
                case 1:
                    DeviceManager.Instance.context.OutputMerger.BlendState = blendStateSubtractColour;
                    break;
                case 2:
                    DeviceManager.Instance.context.OutputMerger.BlendState = blendStateAddColour;
                    break;
                default:
                    DeviceManager.Instance.context.OutputMerger.BlendState = blendStateDefault;
                    break;
            }

            if (BlendMode == 0)
            {
                DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateSolid;
            }
            else
            {
                DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateTranslucent;
            }
        }
    }
}
