using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using SpriteTextRenderer;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Game = CDC.Game;
using ExportOptions = CDC.Objects.ExportOptions;
using SRFile = CDC.Objects.SRFile;

namespace ModelEx
{
	public enum ViewMode : int
	{
		Resources = 0,
		Object = 1,
		Scene = 2
	}

	public class RenderManager
	{
		private Thread renderThread;
		private int syncInterval = 1;
		private FrameCounter frameCounter = FrameCounter.Instance;

		public bool Resize = false;

		public ViewMode ViewMode = ViewMode.Scene;
		public Color BackgroundColour = Color.Gray;
		public bool Wireframe = false;

		public readonly SortedList<string, RenderResource> Resources = new SortedList<string, RenderResource>();

		public Scene CurrentScene { get; private set; }
		public Renderable CurrentObject { get; private set; }

		SpriteRenderer _spriteRenderer;
		public TextBlockRenderer _textBlockRenderer;

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
			_spriteRenderer = new SpriteRenderer();
			_textBlockRenderer = new TextBlockRenderer(_spriteRenderer, "Arial", SlimDX.DirectWrite.FontWeight.Bold, SlimDX.DirectWrite.FontStyle.Normal, SlimDX.DirectWrite.FontStretch.Normal, 16);

			RenderResourceShapes shapesResource = new RenderResourceShapes();
			shapesResource.LoadModels();
			Resources.Add(shapesResource.Name, shapesResource);

			renderThread = new Thread(new ThreadStart(Render));
			renderThread.Name = "RenderThread";
			renderThread.Start();
		}

		public void ShutDown()
		{
			renderThread.Abort();

			UnloadRenderResources();

			// This should be the unnamed one with the shapes.
			RenderResource resource = Resources[Resources.Keys[0]];
			resource.Dispose();
			Resources.Remove(Resources.Keys[0]);

			_textBlockRenderer.Dispose();
			_spriteRenderer.Dispose();
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

		public string LoadRenderResourceCDC(string fileName, Game game, ExportOptions options)
		{
			return LoadRenderResourceCDC(fileName, game, options, -1);
		}

		public string LoadRenderResourceCDC(string fileName, Game game, ExportOptions options, int childIndex)
		{
			SceneCDC.progressLevel = 0;
			SceneCDC.progressLevels = 1;
			SceneCDC.ProgressStage = "Reading Data";

			SRFile srFile = SRFile.Create(fileName, game, options, childIndex);

			RenderResourceCDC renderResource;

			if (Resources.ContainsKey(srFile.Name))
			{
				renderResource = (RenderResourceCDC)Resources[srFile.Name];
				renderResource.Dispose();
				Resources.Remove(srFile.Name);
			}

			renderResource = new RenderResourceCDC(srFile);
			renderResource.LoadModels(options);

			SceneCDC.progressLevel = 1;

			renderResource.LoadTextures(fileName, options);

			Resources.Add(renderResource.Name, renderResource);

			SceneCDC.progressLevel = SceneCDC.progressLevels;
			SceneCDC.ProgressStage = "Done";

			return srFile.Name;
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

		public void SetCurrentObject(string objectName)
		{
			if (CurrentObject != null)
			{
				CurrentScene = null;
			}

			if (Resources.ContainsKey(objectName))
			{
				CurrentObject = new Physical(objectName, 0);
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

		public Renderable GetCameraTarget()
		{
			if (ViewMode == ViewMode.Object)
			{
				return CurrentObject;
			}

			if (ViewMode == ViewMode.Scene && CurrentScene != null)
			{
				return CurrentScene.CurrentObject;
			}

			return null;
		}

		public void DrawString(string text, Vector2 position, float realFontSize, Color4 color)
		{
			Monitor.Enter(DeviceManager.Instance.device);
			_textBlockRenderer.DrawString(text, position, realFontSize, color, CoordinateType.Absolute);
			Monitor.Exit(DeviceManager.Instance.device);
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

				frameCounter.Count();

				DeviceManager deviceManager = DeviceManager.Instance;
				deviceManager.context.ClearDepthStencilView(deviceManager.depthStencil, DepthStencilClearFlags.Depth | DepthStencilClearFlags.Stencil, 1.0f, 0);
				deviceManager.context.ClearRenderTargetView(deviceManager.renderTarget, new Color4(BackgroundColour));

				CameraManager.Instance.UpdateFrameCamera();

				SlimDX.Direct3D11.DepthStencilState oldDSState = DeviceManager.Instance.context.OutputMerger.DepthStencilState;
				SlimDX.Direct3D11.BlendState oldBlendState = DeviceManager.Instance.context.OutputMerger.BlendState;
				SlimDX.Direct3D11.RasterizerState oldRasterizerState = DeviceManager.Instance.context.Rasterizer.State;
				SlimDX.Direct3D11.VertexShader oldVertexShader = DeviceManager.Instance.context.VertexShader.Get();
				SlimDX.Direct3D11.Buffer[] oldVSCBuffers = DeviceManager.Instance.context.VertexShader.GetConstantBuffers(0, 10);
				SlimDX.Direct3D11.PixelShader oldPixelShader = DeviceManager.Instance.context.PixelShader.Get();
				SlimDX.Direct3D11.Buffer[] oldPSCBuffers = DeviceManager.Instance.context.PixelShader.GetConstantBuffers(0, 10);
				SlimDX.Direct3D11.ShaderResourceView[] oldShaderResources = DeviceManager.Instance.context.PixelShader.GetShaderResources(0, 10);
				SlimDX.Direct3D11.GeometryShader oldGeometryShader = DeviceManager.Instance.context.GeometryShader.Get();

				if (ViewMode == ViewMode.Scene)
				{
					if (CurrentScene != null)
					{
						CurrentScene.Render();
					}
				}
				else if (ViewMode == ViewMode.Object)
				{
					if (CurrentObject != null)
					{
						CurrentObject.Render();
					}
				}

				DeviceManager.Instance.context.OutputMerger.DepthStencilState = oldDSState;
				DeviceManager.Instance.context.OutputMerger.BlendState = oldBlendState;
				DeviceManager.Instance.context.Rasterizer.State = oldRasterizerState;
				DeviceManager.Instance.context.VertexShader.Set(oldVertexShader);
				DeviceManager.Instance.context.VertexShader.SetConstantBuffers(oldVSCBuffers, 0, 10);
				DeviceManager.Instance.context.PixelShader.Set(oldPixelShader);
				DeviceManager.Instance.context.PixelShader.SetConstantBuffers(oldPSCBuffers, 0, 10);
				DeviceManager.Instance.context.PixelShader.SetShaderResources(oldShaderResources, 0, 10);
				DeviceManager.Instance.context.GeometryShader.Set(oldGeometryShader);

				Monitor.Enter(DeviceManager.Instance.device);
				_spriteRenderer.RefreshViewport();
				_spriteRenderer.Flush();
				Monitor.Exit(DeviceManager.Instance.device);

				DeviceManager.Instance.context.OutputMerger.DepthStencilState = oldDSState;
				DeviceManager.Instance.context.OutputMerger.BlendState = oldBlendState;
				DeviceManager.Instance.context.Rasterizer.State = oldRasterizerState;
				DeviceManager.Instance.context.VertexShader.Set(oldVertexShader);
				DeviceManager.Instance.context.VertexShader.SetConstantBuffers(oldVSCBuffers, 0, 10);
				DeviceManager.Instance.context.PixelShader.Set(oldPixelShader);
				DeviceManager.Instance.context.PixelShader.SetConstantBuffers(oldPSCBuffers, 0, 10);
				DeviceManager.Instance.context.PixelShader.SetShaderResources(oldShaderResources, 0, 10);
				DeviceManager.Instance.context.GeometryShader.Set(oldGeometryShader);

				// syncInterval can be 0
				deviceManager.swapChain.Present(syncInterval, PresentFlags.None);
			}
		}
	}
}
