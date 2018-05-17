using System.Drawing;
using System.Threading;
using Resource = SlimDX.Direct3D11.Resource;
using Device = SlimDX.Direct3D11.Device;
using SlimDX;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;

namespace ModelEx
{
    public class RenderManager
    {
        protected Thread renderThread;

        private static RenderManager instance = null;
        public static RenderManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new RenderManager();
                }
                return instance;
            }
        }

        private RenderManager() { }
        public void Initialize()
        {
            renderThread = new Thread(new ThreadStart(RenderScene));
            renderThread.Name = "RenderThread";
            renderThread.Start();
        }

        public void ShutDown()
        {
            renderThread.Abort();
        }

        int syncInterval = 1;

        public void SwitchSyncInterval()
        {
            if (syncInterval == 0)
            {
                syncInterval = 1;
            }
            else if (syncInterval == 1)
            {
                syncInterval = 0;
            }
        }

        FrameCounter fc = FrameCounter.Instance;

        public bool resize = false;

        protected void RenderScene()
        {
            while (true)
            {
                Timer.Instance.Tick();

                if (resize)
                {
                    DeviceManager.Instance.Resize();
                    resize = false;
                }

                fc.Count();

                DeviceManager dm = DeviceManager.Instance;
                dm.context.ClearDepthStencilView(dm.depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
                dm.context.ClearRenderTargetView(dm.renderTarget, new Color4(Color.Gray));

                CameraManager.Instance.UpdateFrameCamera();

                Scene.Instance.Render();

                // syncInterval can be 0
                dm.swapChain.Present(syncInterval, PresentFlags.None);
            }
        }
    }
}
