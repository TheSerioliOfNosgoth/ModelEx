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

	public enum LoadResourceFlags : int
	{
		None = 0,
		LoadDependencies = 1,
		LoadDebugResource = 2,
		ReloadScene = 4,
		ResetCamera = 8
	}

	public static class LoadResourceFlagsExtensions
	{
		public static LoadResourceFlags Check(this LoadResourceFlags flags, bool condition)
		{
			return (condition) ? flags : LoadResourceFlags.None;
		}
	}

	// The load request contains the data that will be kept and reused if the uder requests a reload.
	// Therefore is should only contain data that will be the same as the initial load.
	// TODO - Figure out what to do with ReloadScene and ResetCamera, in light of the above.
	// Should they just be parameters?
	public class LoadRequestCDC : System.ICloneable
	{
		public string ResourceName = "";
		public string DataFile = "";
		public string ProjectFolder = "";
		public string TexturesFolder = "";
		public string ObjectListFolder = "";
		public Game GameType = Game.Gex;
		public Platform Platform = Platform.PC;
		public int ChildIndex = -1;
		public ExportOptions ExportOptions;

		public void CopyFrom(LoadRequestCDC loadRequest)
        {
			ResourceName = loadRequest.ResourceName;
			DataFile = loadRequest.DataFile;
			ProjectFolder = loadRequest.ProjectFolder;
			TexturesFolder = loadRequest.TexturesFolder;
			ObjectListFolder = loadRequest.ObjectListFolder;
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

		private SceneMode _sceneMode = SceneMode.Scene;
		public SceneMode SceneMode
		{
			get
			{
				return _sceneMode;
			}
			set
			{
				_sceneMode = value;
				UpdateCameraSelection();
			}
		}

		public Color BackgroundColour = Color.Gray;
		public bool Wireframe = false;

		private SortedList<string, Scene> Scenes = new SortedList<string, Scene>();
		private SortedList<string, Scene> Objects = new SortedList<string, Scene>();

		public Scene CurrentScene { get; private set; }
		public Scene CurrentObject { get; private set; }
		public Scene CurrentDebug { get; private set; }
		public RenderResource DebugResource { get; private set; }
		public Scene CameraTarget { get; private set; }

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
				LoadResourceCDCInternal(_loadRequestsCDC[0]);
				_loadRequestsCDC.RemoveAt(0);
			}

			UpdateModels();
		}

		public void LoadResourceCDC(LoadRequestCDC loadRequest, LoadResourceFlags flags = LoadResourceFlags.ReloadScene)
		{
			// I can disable rendering during the load here if needed.
			LoadResourceCDCInternal(loadRequest, flags);

			// Update all the models once done loading.
			UpdateModels();
		}

		private void LoadResourceCDCInternal(LoadRequestCDC loadRequest, LoadResourceFlags flags = LoadResourceFlags.ReloadScene)
		{
			SceneCDC.progressLevel = 0;
			SceneCDC.progressLevels = 1;
			SceneCDC.ProgressStage = "Reading Data";

			CDC.DataFile dataFile = CDC.DataFile.Create(loadRequest.DataFile, loadRequest.ObjectListFolder, loadRequest.GameType, loadRequest.Platform, loadRequest.ExportOptions, loadRequest.ChildIndex);

			if (dataFile == null)
			{
				SceneCDC.progressLevel = 1;
				SceneCDC.ProgressStage = "Done";
			}

			RenderResourceCDC renderResource;

			if ((flags & LoadResourceFlags.LoadDebugResource) != 0)
			{
				if ((flags & LoadResourceFlags.ReloadScene) != 0)
				{
					if (CameraTarget == CurrentDebug)
					{
						// These should be null because both the target and the camera are being destroyed.
						CameraTarget = null;
						CameraManager.Instance.CurrentCamera = null;
					}

					CurrentDebug?.Dispose();
					CurrentDebug = null;
				}
				else
				{
					CurrentDebug?.UpdateModels(null);
				}

				DebugResource = null;
				DebugResource?.Dispose();
			}
			else if (Resources.ContainsKey(dataFile.Name))
			{
				RenderResource removeResource = Resources[dataFile.Name];
				Resources.Remove(dataFile.Name);

				if ((flags & LoadResourceFlags.ReloadScene) != 0)
				{
					if (CurrentScene?.Name == dataFile.Name)
					{
						if (CameraTarget == CurrentScene)
						{
							// These should be null because both the target and the camera are being destroyed.
							CameraTarget = null;
							CameraManager.Instance.CurrentCamera = null;
						}

						// The CurrentScene should be restored after everything else in this function is completed.
						CurrentScene = null;
					}

					if (CurrentObject?.Name == dataFile.Name)
					{
						if (CameraTarget == CurrentObject)
						{
							// These should be null because both the target and the camera are being destroyed.
							CameraTarget = null;
							CameraManager.Instance.CurrentCamera = null;
						}

						// The CurrentObject should be restored after everything else in this function is completed.
						CurrentObject = null;
					}

					if (Scenes.ContainsKey(dataFile.Name))
					{
						Scene removeScene = Scenes[dataFile.Name];
						Scenes.Remove(dataFile.Name);
						removeScene.Dispose();
					}

					if (Objects.ContainsKey(dataFile.Name))
					{
						Scene removeObject = Objects[dataFile.Name];
						Objects.Remove(dataFile.Name);
						removeObject.Dispose();
					}
				}
				else
				{
					foreach (Scene scene in Scenes.Values)
					{
						scene.UpdateModels();
					}

					foreach (Scene scene in Objects.Values)
					{
						scene.UpdateModels();
					}
				}

				removeResource.Dispose();
			}

			renderResource = new RenderResourceCDC(dataFile, loadRequest);
			renderResource.LoadModels();

			SceneCDC.progressLevel = 1;

			renderResource.LoadTextures(loadRequest.TexturesFolder);

			if ((flags & LoadResourceFlags.LoadDebugResource) != 0)
			{
				DebugResource = renderResource;

				if (CurrentDebug?.Name != dataFile.Name)
				{
					CurrentDebug = new SceneCDC(renderResource.File, renderResource);
					CurrentDebug.Cameras.ResetPositions();

					if (SceneMode == SceneMode.Debug)
					{
						CameraTarget = CurrentDebug;
						CameraManager.Instance.CurrentCamera = CameraTarget.Cameras.CurrentCamera;
					}
				}
				else // loadRequest.ReloadScene should be false to get here.
				{
					CurrentDebug.UpdateModels(renderResource);

					if ((flags & LoadResourceFlags.ResetCamera) != 0)
					{
						CurrentDebug.Cameras.ResetPositions();
					}
				}
			}
			else
			{
				Resources.Add(dataFile.Name, renderResource);

				Scene addScene;

				if (!Scenes.ContainsKey(dataFile.Name))
				{
					addScene = new SceneCDC(renderResource.File, true);
					Scenes.Add(dataFile.Name, addScene);
					addScene.Cameras.ResetPositions();
				}
				else // loadRequest.ReloadScene should be false to get here.
				{
					addScene = Scenes[dataFile.Name];
					addScene.UpdateModels();

					if ((flags & LoadResourceFlags.ResetCamera) != 0)
					{
						addScene.Cameras.ResetPositions();
					}
				}

				Scene addObject;

				if (!Objects.ContainsKey(dataFile.Name))
				{
					addObject = new SceneCDC(renderResource.File, false);
					Objects.Add(dataFile.Name, addObject);
					addObject.Cameras.ResetPositions();
				}
				else // loadRequest.ReloadScene should be false to get here.
				{
					addObject = Objects[dataFile.Name];
					addObject.UpdateModels();

					if ((flags & LoadResourceFlags.ResetCamera) != 0)
					{
						addObject.Cameras.ResetPositions();
					}
				}
			}

			SceneCDC.progressLevel = SceneCDC.progressLevels;
			SceneCDC.ProgressStage = "Done";

			loadRequest.ResourceName = dataFile.Name;

			if (((flags & LoadResourceFlags.LoadDebugResource) == 0) &&
				((flags & LoadResourceFlags.LoadDependencies) != 0) &&
				dataFile.ObjectNames != null && dataFile.ObjectNames.Length > 0)
			{
				foreach (string objectName in dataFile.ObjectNames)
				{
					LoadRequestCDC objectLoadRequest = new LoadRequestCDC();

					if (loadRequest.GameType == Game.SR1)
					{
						// Soul Spiral incorrectly exports PSX files as *.pcm!
						//string extension = (loadRequest.Platform == Platform.PC) ? ".pcm" : ".drm";
						string extension = ".drm";

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
							objectLoadRequest.DataFile = System.IO.Path.ChangeExtension(objectLoadRequest.DataFile, ".pcm");
						}

						if (!System.IO.File.Exists(objectLoadRequest.DataFile))
						{
							continue;
						}

						if (loadRequest.Platform != Platform.PSX)
						{
							objectLoadRequest.TexturesFolder = loadRequest.TexturesFolder;
						}
						else
						{
							objectLoadRequest.TexturesFolder = System.IO.Path.GetDirectoryName(objectLoadRequest.DataFile);
						}

						objectLoadRequest.ProjectFolder = loadRequest.ProjectFolder;
						objectLoadRequest.ObjectListFolder = loadRequest.ObjectListFolder;
						objectLoadRequest.GameType = loadRequest.GameType;
						objectLoadRequest.Platform = loadRequest.Platform;
						objectLoadRequest.ExportOptions = loadRequest.ExportOptions;
						LoadResourceCDCInternal(objectLoadRequest);
					}
					else if (loadRequest.GameType == Game.SR2 || loadRequest.GameType == Game.Defiance)
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

						objectLoadRequest.ProjectFolder = loadRequest.ProjectFolder;
						objectLoadRequest.TexturesFolder = System.IO.Path.GetDirectoryName(objectLoadRequest.DataFile);
						objectLoadRequest.ObjectListFolder = loadRequest.ObjectListFolder;
						objectLoadRequest.GameType = loadRequest.GameType;
						objectLoadRequest.Platform = loadRequest.Platform;
						objectLoadRequest.ExportOptions = loadRequest.ExportOptions;
						LoadResourceCDCInternal(objectLoadRequest);
					}
					else if (loadRequest.GameType == Game.TRL || loadRequest.GameType == Game.TRA)
					{
						if (loadRequest.ProjectFolder != "")
						{
							objectLoadRequest.DataFile = System.IO.Path.Combine(loadRequest.ProjectFolder, "pc-w", objectName + ".drm");
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

						objectLoadRequest.ProjectFolder = loadRequest.ProjectFolder;
						objectLoadRequest.TexturesFolder = System.IO.Path.GetDirectoryName(objectLoadRequest.DataFile);
						objectLoadRequest.ObjectListFolder = loadRequest.ObjectListFolder;
						objectLoadRequest.GameType = loadRequest.GameType;
						objectLoadRequest.Platform = loadRequest.Platform;
						objectLoadRequest.ExportOptions = loadRequest.ExportOptions;
						LoadResourceCDCInternal(objectLoadRequest);
					}
				}
			}
		}

		public void UnloadResource(string resourceName)
		{
			if (resourceName != "" && Resources.ContainsKey(resourceName))
			{
				if (CurrentScene?.Name == resourceName)
				{
					CurrentScene = null;
				}

				if (CurrentObject?.Name == resourceName)
				{
					CurrentObject = null;
				}

				if (Scenes.ContainsKey(resourceName))
				{
					Scene removeScene = Scenes[resourceName];
					Scenes.Remove(resourceName);
					removeScene.Dispose();
				}

				if (Objects.ContainsKey(resourceName))
				{
					Scene removeObject = Objects[resourceName];
					Objects.Remove(resourceName);
					removeObject.Dispose();
				}

				RenderResource removeResource = Resources[resourceName];
				Resources.Remove(resourceName);
				removeResource.Dispose();

				UpdateModels();
			}
		}

		public void UnloadResources()
		{
			_loadRequestsCDC.Clear();

			CurrentScene = null;
			CurrentObject = null;

			while (Scenes.Count > 0)
			{
				Scene removeScene = Scenes.Values[0];
				Scenes.RemoveAt(0);
				removeScene.Dispose();
			}

			while (Objects.Count > 0)
			{
				Scene removeObject = Objects.Values[0];
				Objects.RemoveAt(0);
				removeObject.Dispose();
			}

			while (Resources.Count > 1)
			{
				RenderResource removeResource = Resources.Values[1];
				Resources.RemoveAt(1);
				removeResource.Dispose();
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
			if (objectName != "" && Objects.ContainsKey(objectName))
			{
				CurrentObject = Objects[objectName];
				UpdateCameraSelection();
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
			if (sceneName != "" && Scenes.ContainsKey(sceneName))
			{
				CurrentScene = Scenes[sceneName];
				UpdateCameraSelection();
			}
		}

		public void ExportCurrentScene(string fileName)
		{
			if (CurrentScene != null)
			{
				ExportResourceCDC(CurrentScene.Name, fileName);
			}
		}

		protected void UpdateModels()
		{
			foreach (Scene scene in Scenes.Values)
			{
				scene.UpdateModels();
			}

			foreach (Scene scene in Objects.Values)
			{
				scene.UpdateModels();
			}

			CurrentDebug?.UpdateModels(DebugResource);
		}

		public void UpdateCameraSelection(int cameraIndex = -1)
		{
			switch (SceneMode)
			{
				case SceneMode.Scene:
					CameraTarget = CurrentScene;
					break;
				case SceneMode.Object:
					CameraTarget = CurrentObject;
					break;
				case SceneMode.Debug:
					CameraTarget = CurrentDebug;
					break;
				default:
					CameraTarget = null;
					break;
			}

			if (CameraTarget != null)
			{
				if (cameraIndex != -1)
				{
					CameraTarget.Cameras.CameraIndex = cameraIndex;
				}

				CameraManager.Instance.CurrentCamera = CameraTarget.Cameras.CurrentCamera;
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

				CameraTarget?.Render();

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
