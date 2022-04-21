using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class MainWindow : Form
	{
		ProgressWindow _ProgressWindow;
		CDC.Objects.ExportOptions _ImportExportOptions;
		int _MainSplitPanelPosition;
		protected bool _RunUIMonitoringThread;
		protected bool _ReloadModelOnRenderModeChange;
		protected bool _ClearResourcesOnLoad;
		protected bool _ResetCameraOnModelLoad;
		protected SceneMode _SceneModeOnLoad = SceneMode.Current;
		protected string _LastExportDirectory = "";
		protected LoadRequestCDC _LoadRequest;

		SceneMode _SceneMode;
		public SceneMode SceneMode
		{
			get { return _SceneMode; }
			set
			{
				if (value == SceneMode.Scene)
				{
					_SceneMode = value;
					RenderManager.Instance.SceneMode = value;
					sceneControls.Visible = true;
					objectControls.Visible = false;
					debugControls.Visible = false;
					sceneToolStripMenuItem.Checked = true;
					objectToolStripMenuItem.Checked = false;
					debugToolStripMenuItem.Checked = false;
				}
				else if (value == SceneMode.Object)
				{
					_SceneMode = value;
					RenderManager.Instance.SceneMode = value;
					sceneControls.Visible = false;
					objectControls.Visible = true;
					debugControls.Visible = false;
					sceneToolStripMenuItem.Checked = false;
					objectToolStripMenuItem.Checked = true;
					debugToolStripMenuItem.Checked = false;
				}
				else if (value == SceneMode.Debug)
				{
					_SceneMode = value;
					RenderManager.Instance.SceneMode = value;
					sceneControls.Visible = false;
					objectControls.Visible = false;
					debugControls.Visible = true;
					sceneToolStripMenuItem.Checked = false;
					objectToolStripMenuItem.Checked = false;
					debugToolStripMenuItem.Checked = true;
				}
				else
				{
					_SceneMode = SceneMode.None;
					RenderManager.Instance.SceneMode = SceneMode.None;
					sceneControls.Visible = false;
					objectControls.Visible = false;
					debugControls.Visible = false;
					sceneToolStripMenuItem.Checked = false;
					objectToolStripMenuItem.Checked = false;
					debugToolStripMenuItem.Checked = false;
				}
			}
		}

		protected void UIMonitor()
		{
			while (_RunUIMonitoringThread)
			{
				UpdateSplitPanelPosition();
				Thread.Sleep(1000);
			}
		}

		protected void UpdateSplitPanelPosition()
		{
			_MainSplitPanelPosition = sceneViewContainer.SplitterDistance;
			//Console.WriteLine(string.Format("Splitter position is now {0}", _MainSplitPanelPosition));
		}

		protected void ResetSplitPanelPosition()
		{
			sceneViewContainer.SplitterDistance = _MainSplitPanelPosition;
			//Console.WriteLine(string.Format("Reset splitter position to {0}", _MainSplitPanelPosition));
		}

		protected void ResetRenderModeMenu()
		{
			standardToolStripMenuItem.Checked = false;
			wireframeToolStripMenuItem.Checked = false;
			noTexturemapsToolStripMenuItem.Checked = false;
			debugPolygonFlags1ToolStripMenuItem.Checked = false;
			debugPolygonFlags2ToolStripMenuItem.Checked = false;
			debugPolygonFlags3ToolStripMenuItem.Checked = false;
			debugPolygonFlagsSoulReaverAToolStripMenuItem.Checked = false;
			debugPolygonFlagsHashToolStripMenuItem.Checked = false;
			debugTextureAttributes1ToolStripMenuItem.Checked = false;
			debugTextureAttributes2ToolStripMenuItem.Checked = false;
			debugTextureAttributes3ToolStripMenuItem.Checked = false;
			debugTextureAttributes4ToolStripMenuItem.Checked = false;
			debugTextureAttributes5ToolStripMenuItem.Checked = false;
			debugTextureAttributes6ToolStripMenuItem.Checked = false;
			debugTextureAttributesHashToolStripMenuItem.Checked = false;
			debugTextureAttributesAHashToolStripMenuItem.Checked = false;
			debugTextureAttributesA1ToolStripMenuItem.Checked = false;
			debugTextureAttributesA2ToolStripMenuItem.Checked = false;
			debugTextureAttributesA3ToolStripMenuItem.Checked = false;
			debugTextureAttributesA4ToolStripMenuItem.Checked = false;
			debugTextureAttributesA5ToolStripMenuItem.Checked = false;
			debugTextureAttributesA6ToolStripMenuItem.Checked = false;
			debugTexturePage1ToolStripMenuItem.Checked = false;
			debugTexturePage2ToolStripMenuItem.Checked = false;
			debugTexturePage3ToolStripMenuItem.Checked = false;
			debugTexturePage4ToolStripMenuItem.Checked = false;
			debugTexturePage5ToolStripMenuItem.Checked = false;
			debugTexturePage6ToolStripMenuItem.Checked = false;
			debugTexturePageHashToolStripMenuItem.Checked = false;
			debugTexturePageUpper28BitsHashToolStripMenuItem.Checked = false;
			debugTexturePageUpper5BitsHashToolStripMenuItem.Checked = false;
			rootBSPTreeNumberToolStripMenuItem.Checked = false;
			bSPTreeRootFlagsHashToolStripMenuItem.Checked = false;
			bSPTreeNodeIDToolStripMenuItem.Checked = false;
			bSPTreeRootFlags1ToolStripMenuItem.Checked = false;
			bSPTreeRootFlags2ToolStripMenuItem.Checked = false;
			bSPTreeRootFlags3ToolStripMenuItem.Checked = false;
			bSPTreeRootFlags4ToolStripMenuItem.Checked = false;
			bSPTreeRootFlags5ToolStripMenuItem.Checked = false;
			bSPTreeRootFlags6ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlags1ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlags2ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlags3ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlags4ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlags5ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlags6ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORd1ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORd2ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORd3ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORd4ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORd5ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORd6ToolStripMenuItem.Checked = false;
			bSPTreeParentNodeFlagsORdHashToolStripMenuItem.Checked = false;
			bSPTreeLeafFlags1ToolStripMenuItem.Checked = false;
			bSPTreeLeafFlags2ToolStripMenuItem.Checked = false;
			bSPTreeLeafFlags3ToolStripMenuItem.Checked = false;
			bSPTreeLeafFlags4ToolStripMenuItem.Checked = false;
			bSPTreeLeafFlags5ToolStripMenuItem.Checked = false;
			bSPTreeLeafFlags6ToolStripMenuItem.Checked = false;
			bSPTreeNodeFlagsHashToolStripMenuItem.Checked = false;
			bSPTreeLeafFlagsHashToolStripMenuItem.Checked = false;
			cLUT1ToolStripMenuItem.Checked = false;
			cLUT2ToolStripMenuItem.Checked = false;
			cLUT3ToolStripMenuItem.Checked = false;
			cLUT4ToolStripMenuItem.Checked = false;
			cLUT5ToolStripMenuItem.Checked = false;
			cLUT6ToolStripMenuItem.Checked = false;
			cLUTHashToolStripMenuItem.Checked = false;
			cLUTNonRowColBitsHashToolStripMenuItem.Checked = false;
			cLUTNonRowColBits1ToolStripMenuItem.Checked = false;
			cLUTNonRowColBits2ToolStripMenuItem.Checked = false;
			boneIDHashToolStripMenuItem.Checked = false;
			debugSortPushHashToolStripMenuItem.Checked = false;
			debugSortPushFlags1ToolStripMenuItem.Checked = false;
			debugSortPushFlags2ToolStripMenuItem.Checked = false;
			debugSortPushFlags3ToolStripMenuItem.Checked = false;
			averageVertexAlphaToolStripMenuItem.Checked = false;
			polygonAlphaToolStripMenuItem.Checked = false;
			polygonOpacityToolStripMenuItem.Checked = false;
		}

		protected void HandleRenderModeChange()
		{
			RenderManager.Instance.Wireframe = _ImportExportOptions.RenderMode == CDC.Objects.RenderMode.Wireframe;

			if (_ReloadModelOnRenderModeChange)
			{
				if (RenderManager.Instance.DebugResource != null)
				{
					RenderResourceCDC debugRenderResource = (RenderResourceCDC)RenderManager.Instance.DebugResource;
					_LoadRequest.CopyFrom(debugRenderResource.LoadRequest);

					LoadResource();
				}
			}
		}

		public MainWindow()
		{
			_RunUIMonitoringThread = true;

			InitializeComponent();
			UpdateSplitPanelPosition();
			sceneControls.ResourceLabel.Text = "Current Scene";
			objectControls.ResourceLabel.Text = "Current Object";
			debugControls.ResourceLabel.Text = "Current Debug";
			debugControls.ResourceCombo.Enabled = false;
			SceneMode = SceneMode.Scene;
			_ImportExportOptions = new CDC.Objects.ExportOptions();

			ThreadStart tsUIMonitor = new ThreadStart(UIMonitor);
			Thread uiMonitor = new Thread(tsUIMonitor);
			uiMonitor.Start();
		}

		private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
		{
			_RunUIMonitoringThread = false;
			sceneView.ShutDown();
		}

		private void MainWindow_Load(object sender, EventArgs e)
		{
			sceneView.Initialize();
			_ReloadModelOnRenderModeChange = reloadModelWhenRenderModeIsChangedToolStripMenuItem.Checked;
			_ResetCameraOnModelLoad = resetCameraPositionWhenModelIsLoadedToolStripMenuItem.Checked;
		}

		protected void BeginLoading()
		{
			Enabled = false;
			_ProgressWindow?.Dispose();
			_ProgressWindow = new ProgressWindow();
			_ProgressWindow.Title = "Loading";
			_ProgressWindow.SetMessage("");
			//progressWindow.Icon = this.Icon;
			_ProgressWindow.Owner = this;
			_ProgressWindow.TopLevel = true;
			_ProgressWindow.ShowInTaskbar = false;
			_ProgressWindow.Show();

			if (_LoadRequest.IsDebugResource)
			{
				debugControls.ResourceCombo.SelectedIndex = -1;
				debugControls.ResourceCombo.Items.Clear();
			}

			if (_ClearResourcesOnLoad)
			{
				objectControls.ResourceCombo.SelectedIndex = -1;
				sceneControls.ResourceCombo.SelectedIndex = -1;

				resourceList.Items.Clear();
				objectControls.ResourceCombo.Items.Clear();
				sceneControls.ResourceCombo.Items.Clear();
			}
		}

		protected void EndLoading()
		{
			if (_LoadRequest.IsDebugResource)
			{
				string resourceName = RenderManager.Instance.DebugResource.Name;
				debugControls.ResourceCombo.Items.Add(resourceName);

				if (debugControls.ResourceCombo.Items.Contains(_LoadRequest.ResourceName))
				{
					debugControls.ResourceCombo.SelectedIndex = debugControls.ResourceCombo.Items.IndexOf(_LoadRequest.ResourceName);
				}
			}
			else
			{
				foreach (string resourceName in RenderManager.Instance.Resources.Keys)
				{
					if (resourceName != "")
					{
						if (!resourceList.Items.ContainsKey(resourceName))
						{
							resourceList.Items.Add(resourceName, resourceName, -1);
						}

						if (!objectControls.ResourceCombo.Items.Contains(resourceName))
						{
							objectControls.ResourceCombo.Items.Add(resourceName);
						}

						if (!sceneControls.ResourceCombo.Items.Contains(resourceName))
						{
							// Maybe filter for only levels here?
							sceneControls.ResourceCombo.Items.Add(resourceName);
						}
					}
				}

				if (sceneControls.ResourceCombo.Items.Contains(_LoadRequest.ResourceName))
				{
					sceneControls.ResourceCombo.SelectedIndex = sceneControls.ResourceCombo.Items.IndexOf(_LoadRequest.ResourceName);
				}

				if (objectControls.ResourceCombo.Items.Contains(_LoadRequest.ResourceName))
				{
					objectControls.ResourceCombo.SelectedIndex = objectControls.ResourceCombo.Items.IndexOf(_LoadRequest.ResourceName);
				}
			}

			if (_SceneModeOnLoad == SceneMode.Scene)
			{
				optionTabs.SelectedIndex = 1;
				SceneMode = _SceneModeOnLoad;
				CameraManager.Instance.Reset();
			}

			if (_SceneModeOnLoad == SceneMode.Object)
			{
				optionTabs.SelectedIndex = 1;
				SceneMode = _SceneModeOnLoad;
				CameraManager.Instance.Reset();
			}

			if (_SceneModeOnLoad == SceneMode.Debug)
			{
				optionTabs.SelectedIndex = 1;
				SceneMode = _SceneModeOnLoad;
				CameraManager.Instance.Reset();
			}

			_ClearResourcesOnLoad = false;
			_SceneModeOnLoad = SceneMode.Current;

			if (_ResetCameraOnModelLoad)
			{
				CameraManager.Instance.Reset();
			}

			Enabled = true;
			_ProgressWindow.Hide();
			_ProgressWindow.Dispose();
		}

		protected bool SelectResourceToLoad(SceneMode sceneModeOnLoad)
		{
			_LoadRequest = new LoadRequestCDC();
			LoadResourceDialog loadResourceDialog = new LoadResourceDialog();

			if (Properties.Settings.Default.RecentFolder != null &&
				Directory.Exists(Properties.Settings.Default.RecentFolder))
			{
				loadResourceDialog.SelectedFolder = Properties.Settings.Default.RecentFolder;
			}

			loadResourceDialog.SelectedGameType = (CDC.Game)Properties.Settings.Default.RecentGame;
			loadResourceDialog.SelectedPlatform = (CDC.Platform)Properties.Settings.Default.RecentPlatform;

			if (sceneModeOnLoad == SceneMode.Scene)
			{
				loadResourceDialog.Text = "Load Scene...";
			}
			else if (sceneModeOnLoad == SceneMode.Object)
			{
				loadResourceDialog.Text = "Load Object...";
			}
			else if (sceneModeOnLoad == SceneMode.Debug)
			{
				loadResourceDialog.Text = "Load Debug...";
			}
			else
			{
				loadResourceDialog.Text = "Load Resource...";
			}

			if (loadResourceDialog.ShowDialog() != DialogResult.OK)
			{
				Properties.Settings.Default.RecentFolder = loadResourceDialog.SelectedFolder;
				Properties.Settings.Default.RecentGame = (int)loadResourceDialog.SelectedGameType;
				Properties.Settings.Default.RecentPlatform = (int)loadResourceDialog.SelectedPlatform;
				Properties.Settings.Default.Save();
				loadResourceDialog.Dispose();
				return false;
			}

			loadResourceDialog.Dispose();

			if (loadResourceDialog.SelectedGameType == CDC.Game.Gex)
			{
				CDC.Objects.GexFile gexFile;
				ObjectSelectWindow objectSelectDlg = new ObjectSelectWindow();

				try
				{
					gexFile = new CDC.Objects.GexFile(loadResourceDialog.DataFile, loadResourceDialog.SelectedPlatform, new CDC.Objects.ExportOptions());
					if (gexFile.Asset == CDC.Asset.Unit)
					{
						objectSelectDlg.SetObjectNames(gexFile.ObjectNames);
						if (objectSelectDlg.ShowDialog() != DialogResult.OK)
						{
							objectSelectDlg.Dispose();
							return false;
						}

						objectSelectDlg.Dispose();
					}
				}
				catch
				{
					return false;
				}

				_LoadRequest.ChildIndex = objectSelectDlg.SelectedObject;
			}

			_LoadRequest.DataFile = loadResourceDialog.DataFile;
			_LoadRequest.TextureFile = loadResourceDialog.TextureFile;
			_LoadRequest.ObjectListFile = loadResourceDialog.ObjectListFile;
			_LoadRequest.GameType = loadResourceDialog.SelectedGameType;
			_LoadRequest.Platform = loadResourceDialog.SelectedPlatform;
			_LoadRequest.ExportOptions = sceneModeOnLoad == SceneMode.Debug ? _ImportExportOptions : new CDC.Objects.ExportOptions();
			_LoadRequest.IsDebugResource = sceneModeOnLoad == SceneMode.Debug;

			_ClearResourcesOnLoad = loadResourceDialog.ClearLoadedFiles;
			_SceneModeOnLoad = sceneModeOnLoad;

			Properties.Settings.Default.RecentFolder = loadResourceDialog.SelectedFolder;
			Properties.Settings.Default.RecentGame = (int)loadResourceDialog.SelectedGameType;
			Properties.Settings.Default.RecentPlatform = (int)loadResourceDialog.SelectedPlatform;
			Properties.Settings.Default.Save();

			reloadCurrentModelToolStripMenuItem.Enabled = true;

			return true;
		}

		protected void LoadResource()
		{
			if (!File.Exists(_LoadRequest.DataFile))
			{
				return;
			}

			Thread loadingThread = new Thread((() =>
			{
				Invoke(new MethodInvoker(BeginLoading));

				if (_ClearResourcesOnLoad)
				{
					RenderManager.Instance.UnloadResources();
				}

				RenderManager.Instance.LoadResourceCDC(_LoadRequest);

				Invoke(new MethodInvoker(EndLoading));
			}));

			loadingThread.Name = "LoadingThread";
			loadingThread.SetApartmentState(ApartmentState.STA);
			loadingThread.Start();
			//loadingThread.Join();

			Thread progressThread = new Thread(() =>
			{
				do
				{
					if (_ProgressWindow != null)
					{
						lock (SceneCDC.ProgressStage)
						{
							_ProgressWindow.SetMessage(SceneCDC.ProgressStage);
						}

						int oldProgress = _ProgressWindow.GetProgress();
						if (oldProgress < SceneCDC.ProgressPercent)
						{
							_ProgressWindow.SetProgress(oldProgress + 1);
						}
					}

					Thread.Sleep(20);
				}
				while (loadingThread.IsAlive);
			});

			progressThread.Name = "ProgressThread";
			progressThread.SetApartmentState(ApartmentState.STA);
			progressThread.Start();
		}

		private void exportObjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog SaveDlg = new SaveFileDialog
			{
				CheckPathExists = true,
				Filter =
					"Collada Mesh Files (*.dae)|*.dae",
				//"All Files (*.*)|*.*";
				DefaultExt = "dae",
				FilterIndex = 1
			};

			if (Directory.Exists(_LastExportDirectory))
			{
				SaveDlg.InitialDirectory = _LastExportDirectory;
			}

			if (SaveDlg.ShowDialog() == DialogResult.OK)
			{
				_LastExportDirectory = Path.GetDirectoryName(SaveDlg.FileName);
				RenderManager.Instance.ExportCurrentObject(SaveDlg.FileName);
			}
		}

		private void exportSceneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SaveFileDialog SaveDlg = new SaveFileDialog
			{
				CheckPathExists = true,
				Filter =
					"Collada Mesh Files (*.dae)|*.dae",
				//"All Files (*.*)|*.*";
				DefaultExt = "dae",
				FilterIndex = 1
			};

			if (Directory.Exists(_LastExportDirectory))
			{
				SaveDlg.InitialDirectory = _LastExportDirectory;
			}

			if (SaveDlg.ShowDialog() == DialogResult.OK)
			{
				_LastExportDirectory = Path.GetDirectoryName(SaveDlg.FileName);
				RenderManager.Instance.ExportCurrentScene(SaveDlg.FileName);
			}
		}

		private void sceneTree_AfterCheck(object sender, TreeViewEventArgs e)
		{
			Scene currentScene = (Scene)RenderManager.Instance.CurrentScene;
			if (currentScene != null)
			{
				foreach (Renderable renderable in currentScene.RenderInstances)
				{
					if (renderable is RenderInstance)
					{
						if (renderable.Name == e.Node.Text)
						{
							((RenderInstance)renderable).Root.Visible = e.Node.Checked;
							continue;
						}

						VisibilityNode node = ((RenderInstance)renderable).FindNode(e.Node.Text);
						if (node != null)
						{
							node.Visible = e.Node.Checked;
						}
					}
				}
			}
		}

		private void objectTree_AfterCheck(object sender, TreeViewEventArgs e)
		{
			Scene currentObject = (Scene)RenderManager.Instance.CurrentObject;
			if (currentObject != null)
			{
				foreach (Renderable renderable in currentObject.RenderInstances)
				{
					if (renderable is RenderInstance)
					{
						if (renderable.Name == e.Node.Text)
						{
							((RenderInstance)renderable).Root.Visible = e.Node.Checked;
							continue;
						}

						VisibilityNode node = ((RenderInstance)renderable).FindNode(e.Node.Text);
						if (node != null)
						{
							node.Visible = e.Node.Checked;
						}
					}
				}
			}
		}

		private void debugTree_AfterCheck(object sender, TreeViewEventArgs e)
		{
			Scene currentDebug = (Scene)RenderManager.Instance.CurrentDebug;
			if (currentDebug != null)
			{
				foreach (Renderable renderable in currentDebug.RenderInstances)
				{
					if (renderable is RenderInstance)
					{
						if (renderable.Name == e.Node.Text)
						{
							((RenderInstance)renderable).Root.Visible = e.Node.Checked;
							continue;
						}

						VisibilityNode node = ((RenderInstance)renderable).FindNode(e.Node.Text);
						if (node != null)
						{
							node.Visible = e.Node.Checked;
						}
					}
				}
			}
		}

		private void ResetPositionToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CameraManager.Instance.Reset();
		}

		private void EgoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CameraManager.Instance.SetCamera(CameraManager.CameraMode.Ego);
			egoToolStripMenuItem.Checked = true;
			orbitToolStripMenuItem.Checked = false;
			orbitPanToolStripMenuItem.Checked = false;
		}

		private void OrbitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CameraManager.Instance.SetCamera(CameraManager.CameraMode.Orbit);
			egoToolStripMenuItem.Checked = false;
			orbitToolStripMenuItem.Checked = true;
			orbitPanToolStripMenuItem.Checked = false;
		}

		private void OrbitPanToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CameraManager.Instance.SetCamera(CameraManager.CameraMode.OrbitPan);
			egoToolStripMenuItem.Checked = false;
			orbitToolStripMenuItem.Checked = false;
			orbitPanToolStripMenuItem.Checked = true;
		}

		private void MainWindow_Resize(object sender, EventArgs e)
		{
			ResetSplitPanelPosition();
		}

		private void MainWindow_Enter(object sender, EventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void MainWindow_Leave(object sender, EventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_SplitterMoved(object sender, SplitterEventArgs e)
		{

		}

		private void sceneViewContainer_MouseDown(object sender, MouseEventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_MouseClick(object sender, MouseEventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_MouseUp(object sender, MouseEventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_MouseEnter(object sender, EventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_Enter(object sender, EventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_Click(object sender, EventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_Leave(object sender, EventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_SplitterMoving(object sender, SplitterCancelEventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void sceneViewContainer_SplitterMoved_1(object sender, SplitterEventArgs e)
		{
			UpdateSplitPanelPosition();
		}

		private void debugRenderModeToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ResetRenderModeMenu();
			((ToolStripMenuItem)sender).Checked = true;
			CDC.Objects.ExportOptions options = _ImportExportOptions;

			options.RenderMode =
				(sender == standardToolStripMenuItem) ? CDC.Objects.RenderMode.Standard :
				(sender == wireframeToolStripMenuItem) ? CDC.Objects.RenderMode.Wireframe :
				(sender == noTexturemapsToolStripMenuItem) ? CDC.Objects.RenderMode.NoTextures :
				(sender == debugPolygonFlags1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugPolygonFlags1 :
				(sender == debugPolygonFlags2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugPolygonFlags2 :
				(sender == debugPolygonFlags3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugPolygonFlags3 :
				(sender == debugPolygonFlagsSoulReaverAToolStripMenuItem) ? CDC.Objects.RenderMode.DebugPolygonFlagsSoulReaverA :
				(sender == debugPolygonFlagsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugPolygonFlagsHash :
				(sender == debugTextureAttributes1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributes1 :
				(sender == debugTextureAttributes2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributes2 :
				(sender == debugTextureAttributes3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributes3 :
				(sender == debugTextureAttributes4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributes4 :
				(sender == debugTextureAttributes5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributes5 :
				(sender == debugTextureAttributes6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributes6 :
				(sender == debugTextureAttributesHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesHash :
				(sender == debugTextureAttributesA1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesA1 :
				(sender == debugTextureAttributesA2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesA2 :
				(sender == debugTextureAttributesA3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesA3 :
				(sender == debugTextureAttributesA4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesA4 :
				(sender == debugTextureAttributesA5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesA5 :
				(sender == debugTextureAttributesA6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesA6 :
				(sender == debugTextureAttributesAHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTextureAttributesAHash :
				(sender == cLUT1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUT1 :
				(sender == cLUT2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUT2 :
				(sender == cLUT3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUT3 :
				(sender == cLUT4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUT4 :
				(sender == cLUT5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUT5 :
				(sender == cLUT6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUT6 :
				(sender == cLUTHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUTHash :
				(sender == cLUTNonRowColBits1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUTNonRowColBits1 :
				(sender == cLUTNonRowColBits2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUTNonRowColBits2 :
				(sender == cLUTNonRowColBitsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugCLUTNonRowColBitsHash :
				(sender == debugTexturePage1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePage1 :
				(sender == debugTexturePage2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePage2 :
				(sender == debugTexturePage3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePage3 :
				(sender == debugTexturePage4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePage4 :
				(sender == debugTexturePage5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePage5 :
				(sender == debugTexturePage6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePage6 :
				(sender == debugTexturePageHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePageHash :
				(sender == debugTexturePageUpper28BitsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePageUpper28BitsHash :
				(sender == debugTexturePageUpper5BitsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugTexturePageUpper5BitsHash :
				(sender == rootBSPTreeNumberToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeNumber :
				(sender == bSPTreeNodeIDToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeNodeID :
				(sender == bSPTreeRootFlags1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlags1 :
				(sender == bSPTreeRootFlags2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlags2 :
				(sender == bSPTreeRootFlags3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlags3 :
				(sender == bSPTreeRootFlags4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlags4 :
				(sender == bSPTreeRootFlags5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlags5 :
				(sender == bSPTreeRootFlags6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlags6 :
				(sender == bSPTreeRootFlagsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPRootTreeFlagsHash :
				(sender == bSPTreeNodeFlags1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags1 :
				(sender == bSPTreeNodeFlags2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags2 :
				(sender == bSPTreeNodeFlags3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags3 :
				(sender == bSPTreeNodeFlags4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags4 :
				(sender == bSPTreeNodeFlags5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags5 :
				(sender == bSPTreeNodeFlags6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags6 :
				(sender == bSPTreeNodeFlagsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlagsHash :
				(sender == bSPTreeParentNodeFlagsORd1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd1 :
				(sender == bSPTreeParentNodeFlagsORd2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd2 :
				(sender == bSPTreeParentNodeFlagsORd3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd3 :
				(sender == bSPTreeParentNodeFlagsORd4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd4 :
				(sender == bSPTreeParentNodeFlagsORd5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd5 :
				(sender == bSPTreeParentNodeFlagsORd6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd6 :
				(sender == bSPTreeParentNodeFlagsORdHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORdHash :
				(sender == bSPTreeLeafFlags1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlags1 :
				(sender == bSPTreeLeafFlags2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlags2 :
				(sender == bSPTreeLeafFlags3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlags3 :
				(sender == bSPTreeLeafFlags4ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlags4 :
				(sender == bSPTreeLeafFlags5ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlags5 :
				(sender == bSPTreeLeafFlags6ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlags6 :
				(sender == bSPTreeLeafFlagsHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBSPTreeLeafFlagsHash :
				(sender == boneIDHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugBoneIDHash :
				(sender == debugSortPushHashToolStripMenuItem) ? CDC.Objects.RenderMode.DebugSortPushHash :
				(sender == debugSortPushFlags1ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugSortPushFlags1 :
				(sender == debugSortPushFlags2ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugSortPushFlags2 :
				(sender == debugSortPushFlags3ToolStripMenuItem) ? CDC.Objects.RenderMode.DebugSortPushFlags3 :
				(sender == averageVertexAlphaToolStripMenuItem) ? CDC.Objects.RenderMode.AverageVertexAlpha :
				(sender == polygonAlphaToolStripMenuItem) ? CDC.Objects.RenderMode.PolygonAlpha :
				(sender == polygonOpacityToolStripMenuItem) ? CDC.Objects.RenderMode.PolygonOpacity :
				options.RenderMode;

			HandleRenderModeChange();
		}

		private void debugToggleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CDC.Objects.ExportOptions options = _ImportExportOptions;
			ToolStripMenuItem menuItem = ((ToolStripMenuItem)sender);
			menuItem.Checked ^= true;

			if (sender == unhide100InvisibleTexturesToolStripMenuItem) options.UnhideCompletelyTransparentTextures = menuItem.Checked;
			if (sender == discardHiddenPolygonsToolStripMenuItem) options.DiscardNonVisible = menuItem.Checked;
			if (sender == missingPalettesInGreyscaleToolStripMenuItem) options.AlwaysUseGreyscaleForMissingPalettes = menuItem.Checked;
			if (sender == exportDoubleSidedMaterialsToolStripMenuItem) options.ExportDoubleSidedMaterials = menuItem.Checked;
			if (sender == exportSpectralVersionOfAreaFilesToolStripMenuItem) options.ExportSpectral = menuItem.Checked;
			if (sender == makeAllPolygonsVisibleToolStripMenuItem) options.MakeAllPolygonsVisible = menuItem.Checked;
			if (sender == makeAllPolygonsOpaqueToolStripMenuItem) options.MakeAllPolygonsOpaque = menuItem.Checked;
			if (sender == unhide100InvisibleTexturesToolStripMenuItem) options.UnhideCompletelyTransparentTextures = menuItem.Checked;
			if (sender == oRAllPolygonColoursWithGreenToolStripMenuItem) options.SetAllPolygonColoursToValue = menuItem.Checked;
			if (sender == includeTreeRootFlagsInORdParentFlagsToolStripMenuItem) options.BSPRenderingIncludeRootTreeFlagsWhenORing = menuItem.Checked;
			if (sender == includeLeafFlagsInORdParentFlagsToolStripMenuItem) options.BSPRenderingIncludeLeafFlagsWhenORing = menuItem.Checked;
			if (sender == ignorePolygonFlag2ForTerrainToolStripMenuItem) options.IgnorePolygonFlag2ForTerrain = menuItem.Checked;
			if (sender == createDistinctMaterialsForAllFlagsEvenIfUnusedToolStripMenuItem) options.DistinctMaterialsForAllFlags = menuItem.Checked;
			if (sender == adjustUVCoordinatesForBilinearFilteringToolStripMenuItem) options.AdjustUVs = menuItem.Checked;
			if (sender == ignoreVertexColoursToolStripMenuItem) options.IgnoreVertexColours = menuItem.Checked;
			if (sender == interpolatePolygonColoursToolStripMenuItem) options.InterpolatePolygonColoursWhenColouringBasedOnVertices = menuItem.Checked;
			if (sender == useEachUniqueTextureCLUTVariationToolStripMenuItem) options.UseEachUniqueTextureCLUTVariation = menuItem.Checked;
			if (sender == augmentAlphaMaskingFlagsBasedOnImageContentToolStripMenuItem) options.AlsoInferAlphaMaskingFromTexturePixels = menuItem.Checked;

			HandleRenderModeChange();
		}

		private void reloadModelWhenRenderModeIsChangedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			reloadModelWhenRenderModeIsChangedToolStripMenuItem.Checked = !reloadModelWhenRenderModeIsChangedToolStripMenuItem.Checked;
			_ReloadModelOnRenderModeChange = reloadModelWhenRenderModeIsChangedToolStripMenuItem.Checked;
		}

		private void resetCameraPositionWhenModelIsLoadedToolStripMenuItem_Click(object sender, EventArgs e)
		{
			resetCameraPositionWhenModelIsLoadedToolStripMenuItem.Checked = !resetCameraPositionWhenModelIsLoadedToolStripMenuItem.Checked;
			_ResetCameraOnModelLoad = resetCameraPositionWhenModelIsLoadedToolStripMenuItem.Checked;
		}

		private void reloadCurrentModelToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (RenderManager.Instance.DebugResource != null)
			{
				RenderResourceCDC debugRenderResource = (RenderResourceCDC)RenderManager.Instance.DebugResource;
				_LoadRequest.CopyFrom(debugRenderResource.LoadRequest);

				LoadResource();
			}
		}

		private void exitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			Close();
		}

		private void currentSceneCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			sceneControls.ResourceTree.Nodes.Clear();

			ComboBox comboBox = (ComboBox)sender;
			if (comboBox.SelectedItem == null)
			{
				return;
			}

			string selectedItem = comboBox.SelectedItem.ToString();
			RenderManager.Instance.SetCurrentScene(selectedItem);

			if (RenderManager.Instance.CurrentScene == null)
			{
				return;
			}

			Scene currentScene = (Scene)RenderManager.Instance.CurrentScene;

			TreeNode sceneTreeNode = new TreeNode("Scene");
			sceneTreeNode.Checked = true;

			foreach (Renderable renderable in currentScene.RenderInstances)
			{
				if (renderable is RenderInstance)
				{
					VisibilityNode objectNode = ((RenderInstance)renderable).Root;
					TreeNode objectTreeNode = new TreeNode(renderable.Name);
					objectTreeNode.Checked = objectNode.Visible;

					foreach (VisibilityNode modelNode in objectNode.Nodes)
					{
						TreeNode modelTreeNode = new TreeNode(modelNode.Name);
						modelTreeNode.Checked = modelNode.Visible;

						if (sceneTreeNode.Nodes.Count > 0)
						{
							continue;
						}

						foreach (VisibilityNode groupNode in modelNode.Nodes)
						{
							TreeNode groupTreeNode = new TreeNode(groupNode.Name);
							groupTreeNode.Checked = groupNode.Visible;
							modelTreeNode.Nodes.Add(groupTreeNode);
						}

						objectTreeNode.Nodes.Add(modelTreeNode);
					}

					sceneTreeNode.Nodes.Add(objectTreeNode);
				}
			}

			if (sceneTreeNode.Nodes.Count > 0)
			{
				sceneControls.ResourceTree.Nodes.Add(sceneTreeNode);
				sceneControls.ResourceTree.Nodes[0].Expand();
				sceneControls.ResourceTree.Nodes[0].Nodes[0].Expand();
			}
		}

		private void currentObjectCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			objectControls.ResourceTree.Nodes.Clear();

			ComboBox comboBox = (ComboBox)sender;
			if (comboBox.SelectedItem == null)
			{
				return;
			}

			string selectedItem = comboBox.SelectedItem.ToString();
			RenderManager.Instance.SetCurrentObject(selectedItem);

			if (RenderManager.Instance.CurrentObject == null)
			{
				return;
			}

			Scene currentObject = (Scene)RenderManager.Instance.CurrentObject;

			TreeNode objectSceneTreeNode = new TreeNode("Object");
			objectSceneTreeNode.Checked = true;

			foreach (Renderable renderable in currentObject.RenderInstances)
			{
				if (renderable is RenderInstance)
				{
					VisibilityNode objectNode = ((RenderInstance)renderable).Root;
					TreeNode objectTreeNode = new TreeNode(renderable.Name);
					objectTreeNode.Checked = objectNode.Visible;

					foreach (VisibilityNode modelNode in objectNode.Nodes)
					{
						TreeNode modelTreeNode = new TreeNode(modelNode.Name);
						modelTreeNode.Checked = modelNode.Visible;

						foreach (VisibilityNode groupNode in modelNode.Nodes)
						{
							TreeNode groupTreeNode = new TreeNode(groupNode.Name);
							groupTreeNode.Checked = groupNode.Visible;
							modelTreeNode.Nodes.Add(groupTreeNode);
						}

						objectTreeNode.Nodes.Add(modelTreeNode);
					}

					objectSceneTreeNode.Nodes.Add(objectTreeNode);
				}
			}

			if (objectSceneTreeNode.Nodes.Count > 0)
			{
				objectControls.ResourceTree.Nodes.Add(objectSceneTreeNode);
				objectControls.ResourceTree.Nodes[0].Expand();
			}
		}

		private void currentDebugCombo_SelectedIndexChanged(object sender, EventArgs e)
		{
			debugControls.ResourceTree.Nodes.Clear();

			ComboBox comboBox = (ComboBox)sender;
			if (comboBox.SelectedItem == null)
			{
				return;
			}

			if (RenderManager.Instance.CurrentDebug == null)
			{
				return;
			}

			Scene currentDebug = (Scene)RenderManager.Instance.CurrentDebug;

			TreeNode debugSceneTreeNode = new TreeNode("Debug");
			debugSceneTreeNode.Checked = true;

			foreach (Renderable renderable in currentDebug.RenderInstances)
			{
				if (renderable is RenderInstance)
				{
					VisibilityNode debugNode = ((RenderInstance)renderable).Root;
					TreeNode debugTreeNode = new TreeNode(renderable.Name);
					debugTreeNode.Checked = debugNode.Visible;

					foreach (VisibilityNode modelNode in debugNode.Nodes)
					{
						TreeNode modelTreeNode = new TreeNode(modelNode.Name);
						modelTreeNode.Checked = modelNode.Visible;

						foreach (VisibilityNode groupNode in modelNode.Nodes)
						{
							TreeNode groupTreeNode = new TreeNode(groupNode.Name);
							groupTreeNode.Checked = groupNode.Visible;
							modelTreeNode.Nodes.Add(groupTreeNode);
						}

						debugTreeNode.Nodes.Add(modelTreeNode);
					}

					debugSceneTreeNode.Nodes.Add(debugTreeNode);
				}
			}

			if (debugSceneTreeNode.Nodes.Count > 0)
			{
				debugControls.ResourceTree.Nodes.Add(debugSceneTreeNode);
				debugControls.ResourceTree.Nodes[0].Expand();
			}
		}

		private void loadResourceButton_Click(object sender, EventArgs e)
		{
			if (SelectResourceToLoad(SceneMode.Current))
			{
				LoadResource();
			}
		}

		private void unloadResourceButton_Click(object sender, EventArgs e)
		{
			if (resourceList.SelectedItems.Count > 0)
			{
				string selectedItem = resourceList.SelectedItems[0].Text;

				RenderManager.Instance.UnloadResource(selectedItem);

				if (objectControls.ResourceCombo.SelectedItem?.ToString() == selectedItem)
				{
					objectControls.ResourceCombo.SelectedIndex = -1;
				}
				objectControls.ResourceCombo.Items.Remove(selectedItem);

				if (sceneControls.ResourceCombo.SelectedItem?.ToString() == selectedItem)
				{
					sceneControls.ResourceCombo.SelectedIndex = -1;
				}
				sceneControls.ResourceCombo.Items.Remove(selectedItem);

				resourceList.Items.Remove(resourceList.SelectedItems[0]);
			}
		}

		private void loadSceneToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SelectResourceToLoad(SceneMode.Scene))
			{
				LoadResource();
			}
		}

		private void loadObjectToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SelectResourceToLoad(SceneMode.Object))
			{
				LoadResource();
			}
		}

		private void loadDebugToolStripMenuItem_Click(object sender, EventArgs e)
		{
			if (SelectResourceToLoad(SceneMode.Debug))
			{
				LoadResource();
			}
		}

		private void optionTabs_SelectedIndexChanged(object sender, EventArgs e)
		{
		}

		private void unloadAllResources_Click(object sender, EventArgs e)
		{
			RenderManager.Instance.UnloadResources();

			objectControls.ResourceCombo.SelectedIndex = -1;
			sceneControls.ResourceCombo.SelectedIndex = -1;

			resourceList.Items.Clear();
			objectControls.ResourceCombo.Items.Clear();
			sceneControls.ResourceCombo.Items.Clear();
		}

        private void sceneToolStripMenuItem_Click(object sender, EventArgs e)
        {
			SceneMode = SceneMode.Scene;
		}

        private void objectToolStripMenuItem_Click(object sender, EventArgs e)
        {
			SceneMode = SceneMode.Object;
		}

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SceneMode = SceneMode.Debug;
		}

        private void sceneControls_RefreshClick(object sender, EventArgs e)
        {

        }

        private void objectControls_RefreshClick(object sender, EventArgs e)
        {

        }

        private void debugControls_RefreshClick(object sender, EventArgs e)
        {
			if (RenderManager.Instance.DebugResource != null)
			{
				RenderResourceCDC debugRenderResource = (RenderResourceCDC)RenderManager.Instance.DebugResource;
				_LoadRequest.CopyFrom(debugRenderResource.LoadRequest);

				LoadResource();
			}
		}
    }
}