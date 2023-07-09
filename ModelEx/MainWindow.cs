using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class MainWindow : Form
	{
		ProgressWindow _ProgressWindow;
		CDC.ExportOptions _ImportExportOptions;
		int _MainSplitPanelPosition;
		protected bool _RunUIMonitoringThread;
		protected bool _ReloadModelOnRenderModeChange;
		protected bool _LoadDebugResource;
		protected bool _LoadDependancies;
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
			bspTreeRootFlagsHashToolStripMenuItem.Checked = false;
			bspTreeNodeIDToolStripMenuItem.Checked = false;
			bspTreeRootFlags1ToolStripMenuItem.Checked = false;
			bspTreeRootFlags2ToolStripMenuItem.Checked = false;
			bspTreeRootFlags3ToolStripMenuItem.Checked = false;
			bspTreeRootFlags4ToolStripMenuItem.Checked = false;
			bspTreeRootFlags5ToolStripMenuItem.Checked = false;
			bspTreeRootFlags6ToolStripMenuItem.Checked = false;
			bspTreeNodeFlags1ToolStripMenuItem.Checked = false;
			bspTreeNodeFlags2ToolStripMenuItem.Checked = false;
			bspTreeNodeFlags3ToolStripMenuItem.Checked = false;
			bspTreeNodeFlags4ToolStripMenuItem.Checked = false;
			bspTreeNodeFlags5ToolStripMenuItem.Checked = false;
			bspTreeNodeFlags6ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORd1ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORd2ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORd3ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORd4ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORd5ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORd6ToolStripMenuItem.Checked = false;
			bspTreeParentNodeFlagsORdHashToolStripMenuItem.Checked = false;
			bspTreeLeafFlags1ToolStripMenuItem.Checked = false;
			bspTreeLeafFlags2ToolStripMenuItem.Checked = false;
			bspTreeLeafFlags3ToolStripMenuItem.Checked = false;
			bspTreeLeafFlags4ToolStripMenuItem.Checked = false;
			bspTreeLeafFlags5ToolStripMenuItem.Checked = false;
			bspTreeLeafFlags6ToolStripMenuItem.Checked = false;
			bspTreeNodeFlagsHashToolStripMenuItem.Checked = false;
			bspTreeLeafFlagsHashToolStripMenuItem.Checked = false;
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
			RenderManager.Instance.Wireframe = _ImportExportOptions.RenderMode == CDC.RenderMode.Wireframe;

			if (_ReloadModelOnRenderModeChange)
			{
				if (RenderManager.Instance.DebugResource != null)
				{
					RenderResourceCDC debugRenderResource = (RenderResourceCDC)RenderManager.Instance.DebugResource;
					_LoadRequest.CopyFrom(debugRenderResource.LoadRequest);

					_LoadDebugResource = true;
					_LoadDependancies = false;
					_ClearResourcesOnLoad = false;

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
			_ImportExportOptions = new CDC.ExportOptions();

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

			if (_LoadDebugResource)
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
			LoadRequestCDC loadRequest = _LoadRequest;

			if (_LoadDebugResource)
			{
				string resourceName = RenderManager.Instance.DebugResource.Name;
				debugControls.ResourceCombo.Items.Add(resourceName);

				if (debugControls.ResourceCombo.Items.Contains(loadRequest.ResourceName))
				{
					debugControls.ResourceCombo.SelectedIndex = debugControls.ResourceCombo.Items.IndexOf(loadRequest.ResourceName);
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

				if (sceneControls.ResourceCombo.Items.Contains(loadRequest.ResourceName))
				{
					sceneControls.ResourceCombo.SelectedIndex = sceneControls.ResourceCombo.Items.IndexOf(loadRequest.ResourceName);
				}

				if (objectControls.ResourceCombo.Items.Contains(loadRequest.ResourceName))
				{
					objectControls.ResourceCombo.SelectedIndex = objectControls.ResourceCombo.Items.IndexOf(loadRequest.ResourceName);
				}
			}

			if (_SceneModeOnLoad == SceneMode.Scene)
			{
				optionTabs.SelectedIndex = 1;
				SceneMode = _SceneModeOnLoad;
			}

			if (_SceneModeOnLoad == SceneMode.Object)
			{
				optionTabs.SelectedIndex = 1;
				SceneMode = _SceneModeOnLoad;
			}

			if (_SceneModeOnLoad == SceneMode.Debug)
			{
				optionTabs.SelectedIndex = 1;
				SceneMode = _SceneModeOnLoad;
			}

			_LoadDebugResource = false;
			_LoadDependancies = false;
			_ClearResourcesOnLoad = false;
			_SceneModeOnLoad = SceneMode.Current;

			Enabled = true;
			_ProgressWindow.Hide();
			_ProgressWindow.Dispose();
		}

		protected bool SelectResourceToLoad(SceneMode sceneModeOnLoad)
		{
			bool result = true;

			LoadRequestCDC loadRequest = new LoadRequestCDC();
			LoadResourceDialog loadResourceDialog = new LoadResourceDialog();

			if (Properties.Settings.Default.RecentFolder != null &&
				Directory.Exists(Properties.Settings.Default.RecentFolder))
			{
				loadResourceDialog.SelectedFolder = Properties.Settings.Default.RecentFolder;
			}

			loadResourceDialog.SelectedGameType = (CDC.Game)Properties.Settings.Default.RecentGame;
			loadResourceDialog.SelectedPlatform = (CDC.Platform)Properties.Settings.Default.RecentPlatform;
			loadResourceDialog.ClearLoadedFiles = Properties.Settings.Default.ClearLoadedFiles;

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
				result = false;
			}

			if (result && loadResourceDialog.SelectedGameType == CDC.Game.Gex)
			{
				CDC.GexFile gexFile;
				ObjectSelectWindow objectSelectDlg = new ObjectSelectWindow();

				try
				{
					gexFile = new CDC.GexFile(loadResourceDialog.DataFile, loadResourceDialog.SelectedPlatform, new CDC.ExportOptions());

					if (gexFile.Asset == CDC.Asset.Unit)
					{
						objectSelectDlg.SetObjectNames(gexFile.ObjectNames);

						if (objectSelectDlg.ShowDialog() != DialogResult.OK)
						{
							result = false;
						}
					}
				}
				catch
				{
					result = false;
				}

				if (result)
				{
					loadRequest.ChildIndex = objectSelectDlg.SelectedObject;
				}

				objectSelectDlg.Dispose();
			}

			if (result)
			{
				loadRequest.DataFile = loadResourceDialog.DataFile;
				loadRequest.TextureFile = loadResourceDialog.TextureFile;
				loadRequest.ObjectListFile = loadResourceDialog.ObjectListFile;
				loadRequest.ProjectFolder = loadResourceDialog.ProjectFolder;
				loadRequest.GameType = loadResourceDialog.SelectedGameType;
				loadRequest.Platform = loadResourceDialog.SelectedPlatform;
				loadRequest.ExportOptions = sceneModeOnLoad == SceneMode.Debug ? _ImportExportOptions : new CDC.ExportOptions();

				_LoadRequest = loadRequest;
				_LoadDebugResource = sceneModeOnLoad == SceneMode.Debug;
				_LoadDependancies = false;
				_ClearResourcesOnLoad = loadResourceDialog.ClearLoadedFiles;
				_SceneModeOnLoad = sceneModeOnLoad;

				reloadCurrentModelToolStripMenuItem.Enabled = true;
			}

			Properties.Settings.Default.RecentFolder = loadResourceDialog.SelectedFolder;
			Properties.Settings.Default.RecentGame = (int)loadResourceDialog.SelectedGameType;
			Properties.Settings.Default.RecentPlatform = (int)loadResourceDialog.SelectedPlatform;
			Properties.Settings.Default.ClearLoadedFiles = loadResourceDialog.ClearLoadedFiles;
			Properties.Settings.Default.Save();
			loadResourceDialog.Dispose();

			return result;
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

				RenderManager.Instance.LoadResourceCDC(_LoadRequest, _LoadDependancies, _LoadDebugResource);

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
			CameraManager.Instance.ResetPosition();
		}

		private void EgoToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RenderManager.Instance.UpdateCameraSelection((int)CameraSet.CameraMode.Ego);
			egoToolStripMenuItem.Checked = true;
			orbitToolStripMenuItem.Checked = false;
			orbitPanToolStripMenuItem.Checked = false;
		}

		private void OrbitToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RenderManager.Instance.UpdateCameraSelection((int)CameraSet.CameraMode.Orbit);
			egoToolStripMenuItem.Checked = false;
			orbitToolStripMenuItem.Checked = true;
			orbitPanToolStripMenuItem.Checked = false;
		}

		private void OrbitPanToolStripMenuItem_Click(object sender, EventArgs e)
		{
			RenderManager.Instance.UpdateCameraSelection((int)CameraSet.CameraMode.OrbitPan);
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
			CDC.ExportOptions options = _ImportExportOptions;

			options.RenderMode =
				(sender == standardToolStripMenuItem) ? CDC.RenderMode.Standard :
				(sender == wireframeToolStripMenuItem) ? CDC.RenderMode.Wireframe :
				(sender == noTexturemapsToolStripMenuItem) ? CDC.RenderMode.NoTextures :
				(sender == debugPolygonFlags1ToolStripMenuItem) ? CDC.RenderMode.DebugPolygonFlags1 :
				(sender == debugPolygonFlags2ToolStripMenuItem) ? CDC.RenderMode.DebugPolygonFlags2 :
				(sender == debugPolygonFlags3ToolStripMenuItem) ? CDC.RenderMode.DebugPolygonFlags3 :
				(sender == debugPolygonFlagsSoulReaverAToolStripMenuItem) ? CDC.RenderMode.DebugPolygonFlagsSoulReaverA :
				(sender == debugPolygonFlagsHashToolStripMenuItem) ? CDC.RenderMode.DebugPolygonFlagsHash :
				(sender == debugTextureAttributes1ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributes1 :
				(sender == debugTextureAttributes2ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributes2 :
				(sender == debugTextureAttributes3ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributes3 :
				(sender == debugTextureAttributes4ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributes4 :
				(sender == debugTextureAttributes5ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributes5 :
				(sender == debugTextureAttributes6ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributes6 :
				(sender == debugTextureAttributesHashToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesHash :
				(sender == debugTextureAttributesA1ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesA1 :
				(sender == debugTextureAttributesA2ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesA2 :
				(sender == debugTextureAttributesA3ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesA3 :
				(sender == debugTextureAttributesA4ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesA4 :
				(sender == debugTextureAttributesA5ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesA5 :
				(sender == debugTextureAttributesA6ToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesA6 :
				(sender == debugTextureAttributesAHashToolStripMenuItem) ? CDC.RenderMode.DebugTextureAttributesAHash :
				(sender == cLUT1ToolStripMenuItem) ? CDC.RenderMode.DebugCLUT1 :
				(sender == cLUT2ToolStripMenuItem) ? CDC.RenderMode.DebugCLUT2 :
				(sender == cLUT3ToolStripMenuItem) ? CDC.RenderMode.DebugCLUT3 :
				(sender == cLUT4ToolStripMenuItem) ? CDC.RenderMode.DebugCLUT4 :
				(sender == cLUT5ToolStripMenuItem) ? CDC.RenderMode.DebugCLUT5 :
				(sender == cLUT6ToolStripMenuItem) ? CDC.RenderMode.DebugCLUT6 :
				(sender == cLUTHashToolStripMenuItem) ? CDC.RenderMode.DebugCLUTHash :
				(sender == cLUTNonRowColBits1ToolStripMenuItem) ? CDC.RenderMode.DebugCLUTNonRowColBits1 :
				(sender == cLUTNonRowColBits2ToolStripMenuItem) ? CDC.RenderMode.DebugCLUTNonRowColBits2 :
				(sender == cLUTNonRowColBitsHashToolStripMenuItem) ? CDC.RenderMode.DebugCLUTNonRowColBitsHash :
				(sender == debugTexturePage1ToolStripMenuItem) ? CDC.RenderMode.DebugTexturePage1 :
				(sender == debugTexturePage2ToolStripMenuItem) ? CDC.RenderMode.DebugTexturePage2 :
				(sender == debugTexturePage3ToolStripMenuItem) ? CDC.RenderMode.DebugTexturePage3 :
				(sender == debugTexturePage4ToolStripMenuItem) ? CDC.RenderMode.DebugTexturePage4 :
				(sender == debugTexturePage5ToolStripMenuItem) ? CDC.RenderMode.DebugTexturePage5 :
				(sender == debugTexturePage6ToolStripMenuItem) ? CDC.RenderMode.DebugTexturePage6 :
				(sender == debugTexturePageHashToolStripMenuItem) ? CDC.RenderMode.DebugTexturePageHash :
				(sender == debugTexturePageUpper28BitsHashToolStripMenuItem) ? CDC.RenderMode.DebugTexturePageUpper28BitsHash :
				(sender == debugTexturePageUpper5BitsHashToolStripMenuItem) ? CDC.RenderMode.DebugTexturePageUpper5BitsHash :
				(sender == rootBSPTreeNumberToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeNumber :
				(sender == bspTreeNodeIDToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeNodeID :
				(sender == bspTreeRootFlags1ToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlags1 :
				(sender == bspTreeRootFlags2ToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlags2 :
				(sender == bspTreeRootFlags3ToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlags3 :
				(sender == bspTreeRootFlags4ToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlags4 :
				(sender == bspTreeRootFlags5ToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlags5 :
				(sender == bspTreeRootFlags6ToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlags6 :
				(sender == bspTreeRootFlagsHashToolStripMenuItem) ? CDC.RenderMode.DebugBSPRootTreeFlagsHash :
				(sender == bspTreeNodeFlags1ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlags1 :
				(sender == bspTreeNodeFlags2ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlags2 :
				(sender == bspTreeNodeFlags3ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlags3 :
				(sender == bspTreeNodeFlags4ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlags4 :
				(sender == bspTreeNodeFlags5ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlags5 :
				(sender == bspTreeNodeFlags6ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlags6 :
				(sender == bspTreeNodeFlagsHashToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeImmediateParentFlagsHash :
				(sender == bspTreeParentNodeFlagsORd1ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORd1 :
				(sender == bspTreeParentNodeFlagsORd2ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORd2 :
				(sender == bspTreeParentNodeFlagsORd3ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORd3 :
				(sender == bspTreeParentNodeFlagsORd4ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORd4 :
				(sender == bspTreeParentNodeFlagsORd5ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORd5 :
				(sender == bspTreeParentNodeFlagsORd6ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORd6 :
				(sender == bspTreeParentNodeFlagsORdHashToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeAllParentFlagsORdHash :
				(sender == bspTreeLeafFlags1ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlags1 :
				(sender == bspTreeLeafFlags2ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlags2 :
				(sender == bspTreeLeafFlags3ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlags3 :
				(sender == bspTreeLeafFlags4ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlags4 :
				(sender == bspTreeLeafFlags5ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlags5 :
				(sender == bspTreeLeafFlags6ToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlags6 :
				(sender == bspTreeLeafFlagsHashToolStripMenuItem) ? CDC.RenderMode.DebugBSPTreeLeafFlagsHash :
				(sender == boneIDHashToolStripMenuItem) ? CDC.RenderMode.DebugBoneIDHash :
				(sender == debugSortPushHashToolStripMenuItem) ? CDC.RenderMode.DebugSortPushHash :
				(sender == debugSortPushFlags1ToolStripMenuItem) ? CDC.RenderMode.DebugSortPushFlags1 :
				(sender == debugSortPushFlags2ToolStripMenuItem) ? CDC.RenderMode.DebugSortPushFlags2 :
				(sender == debugSortPushFlags3ToolStripMenuItem) ? CDC.RenderMode.DebugSortPushFlags3 :
				(sender == averageVertexAlphaToolStripMenuItem) ? CDC.RenderMode.AverageVertexAlpha :
				(sender == polygonAlphaToolStripMenuItem) ? CDC.RenderMode.PolygonAlpha :
				(sender == polygonOpacityToolStripMenuItem) ? CDC.RenderMode.PolygonOpacity :
				options.RenderMode;

			HandleRenderModeChange();
		}

		private void debugToggleToolStripMenuItem_Click(object sender, EventArgs e)
		{
			CDC.ExportOptions options = _ImportExportOptions;
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
			if (sender == ignoreBackfacingFlagForTerrainToolStripMenuItem) options.IgnoreBackfacingFlagForTerrain = menuItem.Checked;
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

				_LoadDebugResource = true;
				_LoadDependancies = false;
				_ClearResourcesOnLoad = false;

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
			egoToolStripMenuItem.Checked = (currentScene.Cameras.CameraIndex == (int)CameraSet.CameraMode.Ego);
			orbitToolStripMenuItem.Checked = (currentScene.Cameras.CameraIndex == (int)CameraSet.CameraMode.Orbit);
			orbitPanToolStripMenuItem.Checked = (currentScene.Cameras.CameraIndex == (int)CameraSet.CameraMode.OrbitPan);

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

			egoToolStripMenuItem.Checked = (currentObject.Cameras.CameraIndex == (int)CameraSet.CameraMode.Ego);
			orbitToolStripMenuItem.Checked = (currentObject.Cameras.CameraIndex == (int)CameraSet.CameraMode.Orbit);
			orbitPanToolStripMenuItem.Checked = (currentObject.Cameras.CameraIndex == (int)CameraSet.CameraMode.OrbitPan);

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

			egoToolStripMenuItem.Checked = (currentDebug.Cameras.CameraIndex == (int)CameraSet.CameraMode.Ego);
			orbitToolStripMenuItem.Checked = (currentDebug.Cameras.CameraIndex == (int)CameraSet.CameraMode.Orbit);
			orbitPanToolStripMenuItem.Checked = (currentDebug.Cameras.CameraIndex == (int)CameraSet.CameraMode.OrbitPan);

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
			if (RenderManager.Instance.CurrentScene != null)
			{
				Scene scene = RenderManager.Instance.CurrentScene;
				RenderResource renderResource = RenderManager.Instance.Resources[scene.Name];
				RenderResourceCDC sceneRenderResource = (RenderResourceCDC)renderResource;
				_LoadRequest.CopyFrom(sceneRenderResource.LoadRequest);

				_LoadDebugResource = false;
				_LoadDependancies = false;
				_ClearResourcesOnLoad = false;

				LoadResource();
			}
		}

        private void objectControls_RefreshClick(object sender, EventArgs e)
        {
			if (RenderManager.Instance.CurrentObject != null)
			{
				Scene scene = RenderManager.Instance.CurrentObject;
				RenderResource renderResource = RenderManager.Instance.Resources[scene.Name];
				RenderResourceCDC objectRenderResource = (RenderResourceCDC)renderResource;
				_LoadRequest.CopyFrom(objectRenderResource.LoadRequest);

				_LoadDebugResource = false;
				_LoadDependancies = false;
				_ClearResourcesOnLoad = false;

				LoadResource();
			}
		}

		private void debugControls_RefreshClick(object sender, EventArgs e)
        {
			if (RenderManager.Instance.DebugResource != null)
			{
				RenderResourceCDC debugRenderResource = (RenderResourceCDC)RenderManager.Instance.DebugResource;
				_LoadRequest.CopyFrom(debugRenderResource.LoadRequest);

				_LoadDebugResource = true;
				_LoadDependancies = false;
				_ClearResourcesOnLoad = false;

				LoadResource();
			}
		}

        private void loadDependanciesButton_Click(object sender, EventArgs e)
		{
			if (resourceList.SelectedItems.Count > 0)
			{
				string selectedItem = resourceList.SelectedItems[0].Text;
				RenderResource renderResource = RenderManager.Instance.Resources[selectedItem];
				RenderResourceCDC sceneRenderResource = (RenderResourceCDC)renderResource;
				_LoadRequest.CopyFrom(sceneRenderResource.LoadRequest);

				_LoadDebugResource = false;
				_LoadDependancies = true;
				_ClearResourcesOnLoad = false;

				LoadResource();
			}
		}
    }
}