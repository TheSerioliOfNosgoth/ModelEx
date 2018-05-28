using System;
using Resource = SlimDX.Direct3D11.Resource;
using Device = SlimDX.Direct3D11.Device;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ModelEx
{
    public class DeviceManager
    {
        public Device device;
        public SwapChain swapChain;
        public RenderTargetView renderTarget;
        public DepthStencilView depthStencil;
        Texture2D depthStencilTexture;
        Texture2D backBufferTexture;
        DepthStencilState depthStencilState;

        public DeviceContext context;

        System.Windows.Forms.Control form;

        private static DeviceManager instance = null;
        public static DeviceManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DeviceManager();
                }
                return instance;
            }
        }

        private DeviceManager() { }

        public void Initialize(System.Windows.Forms.Control form)
        {
            CreateDeviceAndSwapChain(form);
            CreateDepthStencilBuffer(form);
        }

        public void CreateDeviceAndSwapChain(System.Windows.Forms.Control form)
        {
            this.form = form;

            float aspectRatio = (float)form.ClientSize.Width / (float)form.ClientSize.Height;
            CameraManager.Instance.SetPerspective((float)Math.PI / 4, aspectRatio, 0.1f, 10000.0f);

            var description = new SwapChainDescription()
            {
                BufferCount = 1,
                Usage = Usage.RenderTargetOutput,
                OutputHandle = form.Handle,
                IsWindowed = true,
                ModeDescription = new ModeDescription(0, 0, new Rational(60, 1), Format.R8G8B8A8_UNorm),
                SampleDescription = new SampleDescription(1, 0),
                Flags = SwapChainFlags.AllowModeSwitch,
                SwapEffect = SwapEffect.Discard
            };

            Device.CreateWithSwapChain(DriverType.Hardware, DeviceCreationFlags.None, description, out device, out swapChain);

            // create a view of our render target, which is the backbuffer of the swap chain we just created
            backBufferTexture = Resource.FromSwapChain<Texture2D>(swapChain, 0);
            renderTarget = new RenderTargetView(device, backBufferTexture);

            // setting a viewport is required if you want to actually see anything
            context = device.ImmediateContext;
            var viewport = new Viewport(0.0f, 0.0f, form.ClientSize.Width, form.ClientSize.Height);
            context.OutputMerger.SetTargets(renderTarget);
            context.Rasterizer.SetViewports(viewport);

            // prevent DXGI handling of alt+enter, which doesn't work properly with Winforms
            using (var factory = swapChain.GetParent<Factory>())
                factory.SetWindowAssociation(form.Handle, WindowAssociationFlags.IgnoreAltEnter);
        }

        public void CreateDepthStencilBuffer(System.Windows.Forms.Control form)
        {
            depthStencilTexture?.Dispose();
            depthStencilTexture = new Texture2D(
              device,
              new Texture2DDescription()
              {
                  ArraySize = 1,
                  MipLevels = 1,
                  Format = Format.D32_Float,
                  Width = form.ClientSize.Width,
                  Height = form.ClientSize.Height,
                  BindFlags = BindFlags.DepthStencil,
                  CpuAccessFlags = CpuAccessFlags.None,
                  SampleDescription = new SampleDescription(1, 0),
                  Usage = ResourceUsage.Default
              }
            );

            depthStencil?.Dispose();
            depthStencil = new DepthStencilView(
              device,
              depthStencilTexture,
              new DepthStencilViewDescription()
              {
                  ArraySize = 0,
                  FirstArraySlice = 0,
                  MipSlice = 0,
                  Format = Format.D32_Float,
                  Dimension = DepthStencilViewDimension.Texture2D
              }
             );

            depthStencilState?.Dispose();
            depthStencilState = DepthStencilState.FromDescription(
             device,
             new DepthStencilStateDescription()
             {
                 DepthComparison = Comparison.Less,
                 DepthWriteMask = DepthWriteMask.All,
                 IsDepthEnabled = true,
                 IsStencilEnabled = false
             }
           );

            context.OutputMerger.DepthStencilState = depthStencilState;
            context.OutputMerger.SetTargets(depthStencil, renderTarget);
        }

        public void ShutDown()
        {
            depthStencilState.Dispose();
            depthStencil.Dispose();
            depthStencilTexture.Dispose();
            renderTarget.Dispose();
            backBufferTexture.Dispose();
            swapChain.Dispose();
            device.Dispose();
        }

        internal void Resize()
        {
            try
            {
                if (device == null)
                {
                    return;
                }

                float aspectRatio = (float)form.ClientSize.Width / (float)form.ClientSize.Height;
                CameraManager.Instance.SetPerspective((float)Math.PI / 4, aspectRatio, 0.1f, 10000.0f);

                // Dispose before resizing.
                renderTarget?.Dispose();
                backBufferTexture?.Dispose();
                depthStencil?.Dispose();

                swapChain.ResizeBuffers(1,
                  form.ClientSize.Width,
                  form.ClientSize.Height,
                  Format.R8G8B8A8_UNorm,
                  SwapChainFlags.AllowModeSwitch);

                backBufferTexture = Texture2D.FromSwapChain<Texture2D>(swapChain, 0);
                renderTarget = new RenderTargetView(device, backBufferTexture);

                CreateDepthStencilBuffer(form);

                Viewport viewport = new Viewport(0.0f, 0.0f, form.ClientSize.Width, form.ClientSize.Height);
                context.Rasterizer.SetViewports(viewport);
                context.OutputMerger.SetTargets(depthStencil, renderTarget);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}
