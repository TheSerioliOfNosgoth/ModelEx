using System;
using System.IO;
using SlimDX.D3DCompiler;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using System.Runtime.InteropServices;

namespace ModelEx
{
    public class IncludeFX : Include
    {
        string _includeDirectory = "";

        public IncludeFX(string includeDirectory)
        {
            _includeDirectory = includeDirectory;
        }

        public void Close(Stream stream)
        {
            stream.Close();
            stream.Dispose();
        }
        public void Open(IncludeType type, string fileName, Stream parentStream, out Stream stream)
        {
            stream = new FileStream(Path.Combine(_includeDirectory, fileName), FileMode.Open);
        }
    }


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
        public float VertexColorFactor;

        // Rasterizer
        [FieldOffset(0x12C)]
        public float DepthBias;

        // Realm
        [FieldOffset(0x130)]
        public float RealmBlend;
    }

    public abstract class Effect
    {
        protected RasterizerState rasterizerStateDefault;
        protected RasterizerState rasterizerStateDefaultNC;
        protected RasterizerState rasterizerStateDecal;
        protected RasterizerState rasterizerStateDecalNC;
        protected RasterizerState rasterizerStateWireframe;

        protected BlendState blendStateDefault;
        protected BlendState blendStateAddColour;
        protected BlendState blendStateSubtractColour;

        protected DepthStencilState depthStencilStateSolid;
        protected DepthStencilState depthStencilStateTranslucent;
        protected DepthStencilState depthStencilStateNoDepth;

        protected bool useBackfaceCulling = true;

        // Render modes
        public bool IsDecal;
        public int BlendMode;

        public virtual void Dispose()
        {
            rasterizerStateDefault?.Dispose();
            rasterizerStateDefaultNC?.Dispose();
            rasterizerStateDecal?.Dispose();
            rasterizerStateDecalNC?.Dispose();
            rasterizerStateWireframe?.Dispose();

            blendStateDefault?.Dispose();
            blendStateAddColour?.Dispose();
            blendStateSubtractColour?.Dispose();

            depthStencilStateSolid?.Dispose();
            depthStencilStateTranslucent?.Dispose();
            depthStencilStateNoDepth?.Dispose();
        }

        public virtual void Initialize()
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

                RasterizerStateDescription rStateDecal = new RasterizerStateDescription()
                {
                    FillMode = FillMode.Solid,
                    CullMode = useBackfaceCulling ? CullMode.Back : CullMode.None,
                    IsFrontCounterclockwise = true,
                    DepthBias = -10,
                    DepthBiasClamp = 0,
                    //SlopeScaledDepthBias = -0.5f,
                    //IsDepthClipEnabled = true
                };

                rasterizerStateDecal = RasterizerState.FromDescription(DeviceManager.Instance.device, rStateDecal);

                RasterizerStateDescription rStateDecalNC = rStateDecal;
                rStateDecalNC.CullMode = CullMode.None;
                rasterizerStateDecalNC = RasterizerState.FromDescription(DeviceManager.Instance.device, rStateDecalNC);

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

                #endregion

                #region Depth Stencils

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
                dsStateTranslucent.DepthWriteMask = DepthWriteMask.Zero;
                dsStateTranslucent.IsDepthEnabled = true;
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

                DepthStencilStateDescription dsStateNoDepth = new DepthStencilStateDescription();

                dsStateNoDepth.DepthComparison = Comparison.Always;
                dsStateNoDepth.DepthWriteMask = DepthWriteMask.Zero;
                dsStateNoDepth.IsDepthEnabled = true;
                dsStateNoDepth.IsStencilEnabled = false;

                depthStencilStateNoDepth = DepthStencilState.FromDescription(DeviceManager.Instance.device, dsStateNoDepth);

                #endregion
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public virtual void ApplyBlendState()
        {
            switch (BlendMode)
            {
                case 1:
                    DeviceManager.Instance.context.OutputMerger.BlendState = blendStateAddColour;
                    break;
                case 2:
                    DeviceManager.Instance.context.OutputMerger.BlendState = blendStateSubtractColour;
                    break;
                default:
                    DeviceManager.Instance.context.OutputMerger.BlendState = blendStateDefault;
                    break;
            }

            if (BlendMode == 0)
            {
                if (!IsDecal)
                {
                    DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateSolid;
                }
                else
                {
                    DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateSolid;
                }
            }
            else
            {
                DeviceManager.Instance.context.OutputMerger.DepthStencilState = depthStencilStateTranslucent;
            }

            if (RenderManager.Instance.Wireframe)
            {
                DeviceManager.Instance.context.Rasterizer.State = rasterizerStateWireframe;
            }
            else if (!IsDecal)
            {
                DeviceManager.Instance.context.Rasterizer.State = useBackfaceCulling ? rasterizerStateDefault : rasterizerStateDefaultNC;
            }
            else
            {
                DeviceManager.Instance.context.Rasterizer.State = useBackfaceCulling ? rasterizerStateDecal : rasterizerStateDecalNC;
            }
        }
    }
}
