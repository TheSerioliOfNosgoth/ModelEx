using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Resource = SlimDX.Direct3D11.Resource;
using Device = SlimDX.Direct3D11.Device;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Game = CDC.Game;
using ExportOptions = CDC.Objects.ExportOptions;
using SRFile = CDC.Objects.SRFile;

namespace ModelEx
{
	public class RenderManager
	{
		private Thread renderThread;
		private int syncInterval = 1;
		private FrameCounter fc = FrameCounter.Instance;

		public bool Resize = false;

		public Color BackgroundColour = Color.Gray;
		public bool Wireframe = false;

		public readonly SortedList<string, RenderResource> Resources = new SortedList<string, RenderResource>();

		public Scene CurrentScene { get; private set; }
		public Renderable CurrentObject { get { return CurrentScene?.CurrentObject; } }

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

		public void Initialize()
		{
			renderThread = new Thread(new ThreadStart(Render));
			renderThread.Name = "RenderThread";
			renderThread.Start();

			RenderResourceShapes shapesResource = new RenderResourceShapes();
			shapesResource.LoadModels();
			Resources.Add(shapesResource.Name, shapesResource);
		}

		public void ShutDown()
		{
			renderThread.Abort();

			UnloadRenderResources();

			// This should be the unnamed one with the shapes.
			RenderResource resource = Resources[Resources.Keys[0]];
			resource.Dispose();
			Resources.Remove(Resources.Keys[0]);
		}

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

		public void LoadRenderResourceCDC(string fileName, Game game, ExportOptions options, bool isReload = false)
		{
			LoadRenderResourceCDC(fileName, game, options, isReload, -1);
		}

		public void LoadRenderResourceCDC(string fileName, Game game, ExportOptions options, bool isReload, int childIndex)
		{
			SceneCDC.progressLevel = 0;
			SceneCDC.progressLevels = 1;
			SceneCDC.ProgressStage = "Reading Data";

			SRFile srFile = SRFile.Create(fileName, game, options, childIndex);

			RenderResourceCDC renderResource = null;

			if (!isReload && Resources.ContainsKey(srFile.Name))
			{
				renderResource = (RenderResourceCDC)Resources[srFile.Name];
				renderResource.Dispose();
				renderResource = null;
				Resources.Remove(srFile.Name);
			}

			if (renderResource == null)
			{
				renderResource = new RenderResourceCDC(srFile);
			}

			renderResource.LoadModels(options);

			SceneCDC.progressLevel = 1;

			renderResource.LoadTextures(fileName, options);

			Resources.Add(renderResource.Name, renderResource);

			SceneCDC.progressLevel = SceneCDC.progressLevels;
			SceneCDC.ProgressStage = "Done";
		}

		public void UnloadRenderResources()
		{
			if (CurrentScene != null)
			{
				CurrentScene.Dispose();
				CurrentScene = null;
			}

			while (Resources.Count > 1)
			{
				RenderResource resource = Resources[Resources.Keys[1]];

				resource.Dispose();
				Resources.Remove(Resources.Keys[1]);
			}
		}

		public void ExportTextureResource(string filename, ExportOptions options)
		{
			if (Resources.ContainsKey(""))
			{
				RenderResourceCDC renderResource = (RenderResourceCDC)Resources[""];
				renderResource.ExportToFile(filename, options);
			}
		}

		public void SetCurrentScene(string sceneName)
		{
			if (CurrentScene != null)
			{
				CurrentScene.Dispose();
				CurrentScene = null;
			}

			if (Resources.ContainsKey(sceneName))
			{
				RenderResourceCDC renderResource = (RenderResourceCDC)Resources[sceneName];
				CurrentScene = new SceneCDC(renderResource.File);
			}
		}

		protected void Render()
		{
			while (true)
			{
				Timer.Instance.Tick();

				if (Resize)
				{
					DeviceManager.Instance.Resize();
					Resize = false;
				}

				fc.Count();

				DeviceManager deviceManager = DeviceManager.Instance;
				deviceManager.context.ClearDepthStencilView(deviceManager.depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
				deviceManager.context.ClearRenderTargetView(deviceManager.renderTarget, new Color4(BackgroundColour));

				CameraManager.Instance.UpdateFrameCamera();

				if (CurrentScene != null)
				{
					CurrentScene.Render();
				}

				// syncInterval can be 0
				deviceManager.swapChain.Present(syncInterval, PresentFlags.None);
			}
		}
	}
}
