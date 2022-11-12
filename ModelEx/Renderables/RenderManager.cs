using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using SpriteTextRenderer;
using SlimDX;
using SlimDX.Direct3D11;
using SlimDX.DXGI;
using Game = CDC.Game;
using Platform = CDC.Platform;
using ExportOptions = CDC.ExportOptions;

namespace ModelEx
{
	public enum SceneMode : int
	{
		Current = -1,
		None = 0,
		Scene = 1,
		Object = 2,
		Debug = 3
	}

	public class LoadRequestCDC : System.ICloneable
	{
		public string ResourceName = "";
		public string DataFile = "";
		public string TextureFile = "";
		public string ObjectListFile = "";
		public string ProjectFolder = "";
		public Game GameType = Game.Gex;
		public Platform Platform = Platform.PC;
		public int ChildIndex = -1;
		public ExportOptions ExportOptions;

		public void CopyFrom(LoadRequestCDC loadRequest)
        {
			ResourceName = loadRequest.ResourceName;
			DataFile = loadRequest.DataFile;
			TextureFile = loadRequest.TextureFile;
			ObjectListFile = loadRequest.ObjectListFile;
			ProjectFolder = loadRequest.ProjectFolder;
			GameType = loadRequest.GameType;
			Platform = loadRequest.Platform;
			ChildIndex = loadRequest.ChildIndex;
			ExportOptions = loadRequest.ExportOptions;
        }

		public object Clone()
		{
			LoadRequestCDC loadRequest = new LoadRequestCDC();
			loadRequest.CopyFrom(this);
			return loadRequest;
		}
	};

	public class RenderManager
	{
		private Thread renderThread;
		private int syncInterval = 1;
		private FrameCounter frameCounter = FrameCounter.Instance;

		public bool Resize = false;

		public SceneMode SceneMode = SceneMode.Scene;
		public Color BackgroundColour = Color.Gray;
		public bool Wireframe = false;

		public Scene CurrentScene { get; private set; }
		public Scene CurrentObject { get; private set; }
		public Scene CurrentDebug { get; private set; }
		public RenderResource DebugResource { get; private set; }

		SpriteRenderer _spriteRenderer;
		public TextBlockRenderer _textBlockRenderer;

		public readonly SortedList<string, RenderResource> Resources = new SortedList<string, RenderResource>();

		private List<LoadRequestCDC> _loadRequestsCDC = new List<LoadRequestCDC>();

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

			UnloadResources();

			// This should be the unnamed one with the shapes.
			RenderResource resource = Resources[Resources.Keys[0]];
			Resources.Remove(Resources.Keys[0]);
			resource.Dispose();

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

		public void RequestResourceCDC(LoadRequestCDC loadRequest)
		{
			if (_loadRequestsCDC.Find(x => x.DataFile == loadRequest.DataFile) == null)
			{
				_loadRequestsCDC.Add(loadRequest);
			}
		}

		public void LoadRequestedResourcesCDC()
		{
			while (_loadRequestsCDC.Count > 0)
			{
				LoadResourceCDC(_loadRequestsCDC[0]);
				_loadRequestsCDC.RemoveAt(0);
			}
		}

		public void LoadResourceCDC(LoadRequestCDC loadRequest, bool loadDependancies = false, bool loadDebugResource = false)
		{
			SceneCDC.progressLevel = 0;
			SceneCDC.progressLevels = 1;
			SceneCDC.ProgressStage = "Reading Data";

			CDC.DataFile dataFile = CDC.DataFile.Create(loadRequest.DataFile, loadRequest.ObjectListFile, loadRequest.GameType, loadRequest.Platform, loadRequest.ExportOptions, loadRequest.ChildIndex);

			if (dataFile == null)
			{
				SceneCDC.progressLevel = 1;
				SceneCDC.ProgressStage = "Done";
			}

			RenderResourceCDC renderResource;

			if (loadDebugResource)
			{
				CurrentDebug = null;
				DebugResource?.Dispose();
				DebugResource = null;
			}
			else if (Resources.ContainsKey(dataFile.Name))
			{
				renderResource = (RenderResourceCDC)Resources[dataFile.Name];
				Resources.Remove(dataFile.Name);
				CurrentObject?.UpdateModels();
				CurrentScene?.UpdateModels();
				renderResource.Dispose();
			}

			renderResource = new RenderResourceCDC(dataFile, loadRequest);
			renderResource.LoadModels();

			SceneCDC.progressLevel = 1;

			renderResource.LoadTextures(loadRequest.TextureFile);

			if (loadDebugResource)
			{
				DebugResource = renderResource;
				CurrentDebug = new SceneCDC(renderResource.File, renderResource);
			}
			else
			{
				Resources.Add(renderResource.Name, renderResource);
				CurrentObject?.UpdateModels();
				CurrentScene?.UpdateModels();
			}

			SceneCDC.progressLevel = SceneCDC.progressLevels;
			SceneCDC.ProgressStage = "Done";

			loadRequest.ResourceName = dataFile.Name;

			if (loadDependancies && dataFile.ObjectNames != null && dataFile.ObjectNames.Length > 0)
			{
				foreach (string objectName in dataFile.ObjectNames)
				{
					LoadRequestCDC objectLoadRequest = new LoadRequestCDC();

					if (loadRequest.GameType == Game.SR1)
					{
						string extension = (loadRequest.Platform == Platform.PC) ? ".pcm" : ".drm";

						if (loadRequest.ProjectFolder != "")
						{
							objectLoadRequest.DataFile = System.IO.Path.Combine(loadRequest.ProjectFolder, "kain2\\object", objectName, objectName + extension);
						}
						else
						{
							string projectFolder = System.IO.Path.GetDirectoryName(loadRequest.DataFile);
							objectLoadRequest.DataFile = System.IO.Path.Combine(projectFolder, objectName, extension);
						}

						if (!System.IO.File.Exists(objectLoadRequest.DataFile))
						{
							continue;
						}

						if (loadRequest.Platform != Platform.PSX)
						{
							objectLoadRequest.TextureFile = loadRequest.TextureFile;
						}
						else
						{
							objectLoadRequest.TextureFile = System.IO.Path.ChangeExtension(objectLoadRequest.DataFile, "crm");
						}

						objectLoadRequest.ObjectListFile = loadRequest.ObjectListFile;
						objectLoadRequest.ProjectFolder = loadRequest.ProjectFolder;
						objectLoadRequest.GameType = loadRequest.GameType;
						objectLoadRequest.Platform = loadRequest.Platform;
						objectLoadRequest.ExportOptions = loadRequest.ExportOptions;
						LoadResourceCDC(objectLoadRequest);
					}
					else if (loadRequest.GameType == Game.SR2)
					{
						if (loadRequest.ProjectFolder != "")
						{
							objectLoadRequest.DataFile = System.IO.Path.Combine(loadRequest.ProjectFolder, "pcenglish", objectName + ".drm");
						}
						else
						{
							string projectFolder = System.IO.Path.GetDirectoryName(loadRequest.DataFile);
							objectLoadRequest.DataFile = System.IO.Path.Combine(projectFolder, objectName, ".drm");
						}

						if (!System.IO.File.Exists(objectLoadRequest.DataFile))
						{
							continue;
						}

						objectLoadRequest.TextureFile = System.IO.Path.ChangeExtension(objectLoadRequest.DataFile, "vrm");

						objectLoadRequest.ObjectListFile = loadRequest.ObjectListFile;
						objectLoadRequest.ProjectFolder = loadRequest.ProjectFolder;
						objectLoadRequest.GameType = loadRequest.GameType;
						objectLoadRequest.Platform = loadRequest.Platform;
						objectLoadRequest.ExportOptions = loadRequest.ExportOptions;
						LoadResourceCDC(objectLoadRequest);
					}
				}
			}
		}

		public void UnloadResource(string resourceName)
		{
			if (resourceName != "" && Resources.ContainsKey(resourceName))
			{
				RenderResource renderResource = Resources[resourceName];
				Resources.Remove(resourceName);
				CurrentObject?.UpdateModels();
				CurrentScene?.UpdateModels();
				renderResource.Dispose();

				if (CurrentScene != null && CurrentScene.Name == resourceName)
				{
					CurrentScene.Dispose();
					CurrentScene = null;
				}

				if (CurrentObject != null && CurrentObject.Name == resourceName)
				{
					CurrentObject.Dispose();
					CurrentObject = null;
				}
			}
		}

		public void UnloadResources()
		{
			_loadRequestsCDC.Clear();

			if (CurrentScene != null)
			{
				CurrentScene.Dispose();
				CurrentScene = null;
			}

			if (CurrentObject != null)
			{
				CurrentObject.Dispose();
				CurrentObject = null;
			}

			while (Resources.Count > 1)
			{
				RenderResource resource = Resources[Resources.Keys[1]];
				Resources.Remove(Resources.Keys[1]);
				CurrentObject?.UpdateModels();
				CurrentScene?.UpdateModels();
				resource.Dispose();
			}
		}

		public void ExportResourceCDC(string resourceName, string filename)
		{
			if (resourceName != "" && Resources.ContainsKey(resourceName))
			{
				RenderResourceCDC renderResource = (RenderResourceCDC)Resources[resourceName];
				renderResource.ExportToFile(filename);
			}
		}

		public void SetCurrentObject(string objectName)
		{
			if (CurrentObject != null)
			{
				CurrentObject = null;
			}

			if (objectName != "" && Resources.ContainsKey(objectName))
			{
				RenderResourceCDC renderResource = (RenderResourceCDC)Resources[objectName];
				CurrentObject = new SceneCDC(renderResource.File, false);
			}
		}

		public void ExportCurrentObject(string fileName)
		{
			if (CurrentObject != null)
			{
				ExportResourceCDC(CurrentObject.Name, fileName);
			}
		}

		public void SetCurrentScene(string sceneName)
		{
			if (CurrentScene != null)
			{
				CurrentScene.Dispose();
				CurrentScene = null;
			}

			if (sceneName != "" && Resources.ContainsKey(sceneName))
			{
				RenderResourceCDC renderResource = (RenderResourceCDC)Resources[sceneName];
				CurrentScene = new SceneCDC(renderResource.File, true);
			}
		}

		public void ExportCurrentScene(string fileName)
		{
			if (CurrentScene != null)
			{
				ExportResourceCDC(CurrentScene.Name, fileName);
			}
		}

		public Renderable GetCameraTarget()
		{
			switch (SceneMode)
            {
				case SceneMode.Scene: return CurrentScene;
				case SceneMode.Object: return CurrentObject;
				case SceneMode.Debug: return CurrentDebug;
				default: return null;
            }
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

				switch (SceneMode)
				{
					case SceneMode.Scene: CurrentScene?.Render(); break;
					case SceneMode.Object: CurrentObject?.Render(); break;
					case SceneMode.Debug: CurrentDebug?.Render(); break;
					default: break;
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
