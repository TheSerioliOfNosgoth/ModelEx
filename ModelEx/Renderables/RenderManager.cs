using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using Resource = SlimDX.Direct3D11.Resource;
using Device = SlimDX.Direct3D11.Device;
using SlimDX;
using SlimDX.Windows;
using SlimDX.D3DCompiler;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Game = CDC.Game;
using ExportOptions = CDC.Objects.ExportOptions;
using GexFile = CDC.Objects.GexFile;
using SRFile = CDC.Objects.SRFile;
using SR1File = CDC.Objects.SR1File;
using SR2File = CDC.Objects.SR2File;
using DefianceFile = CDC.Objects.DefianceFile;
using TRLFile = CDC.Objects.TRLFile;
using SRModel = CDC.Objects.Models.SRModel;

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
			renderThread = new Thread(new ThreadStart(RenderScene));
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

			// Temporary
			SceneManager.Instance.ShutDown();
			SceneManager.Instance.AddScene(new SceneCDC(srFile));
		}

		public void UnloadRenderResources()
		{
			SceneManager.Instance.ShutDown();

			while (Resources.Count > 0)
			{
				RenderResource resource = Resources[Resources.Keys[0]];
				resource.Dispose();
				Resources.Remove(Resources.Keys[0]);
			}
		}

		protected void RenderScene()
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

				DeviceManager dm = DeviceManager.Instance;
				dm.context.ClearDepthStencilView(dm.depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
				dm.context.ClearRenderTargetView(dm.renderTarget, new Color4(BackgroundColour));

				CameraManager.Instance.UpdateFrameCamera();

				SceneManager.Instance.Render();

				// syncInterval can be 0
				dm.swapChain.Present(syncInterval, PresentFlags.None);
			}
		}
	}
}
