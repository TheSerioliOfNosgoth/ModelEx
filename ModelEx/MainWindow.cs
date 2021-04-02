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
        ProgressWindow progressWindow;
        int filterIndex = 2;
        CDC.Objects.ExportOptions ImportExportOptions;
        int _MainSplitPanelPosition;
        protected bool _RunUIMonitoringThread;
        protected bool _ReloadModelOnRenderModeChange;
        protected bool _ResetCameraOnModelLoad;
        protected string _CurrentModelPath;
        protected CDC.Game _CurrentModelType;
        protected string _LastOpenDirectory;
        protected string _LastExportDirectory;

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

        public MainWindow()
        {
            _RunUIMonitoringThread = true;
            _CurrentModelPath = "";
            _LastOpenDirectory = "";
            _LastExportDirectory = "";
            InitializeComponent();
            ImportExportOptions = new CDC.Objects.ExportOptions();
            UpdateSplitPanelPosition();
            ThreadStart tsUIMonitor = new ThreadStart(UIMonitor);
            Thread uiMonitor = new Thread(tsUIMonitor);
            uiMonitor.Start();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            _RunUIMonitoringThread = false;
            sceneView.ShutDown();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sceneView.Initialize();
            _ReloadModelOnRenderModeChange = reloadModelWhenRenderModeIsChangedToolStripMenuItem.Checked;
            _ResetCameraOnModelLoad = resetCameraPositionWhenModelIsLoadedToolStripMenuItem.Checked;
            ResetPlatformDetection();
            autodetectToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.None;
        }

        protected void BeginLoading()
        {
            if (progressWindow != null)
            {
                progressWindow.Dispose();
            }
            progressWindow = new ProgressWindow();
            progressWindow.Title = "Loading";
            progressWindow.SetMessage("");
            //progressWindow.Icon = this.Icon;
            progressWindow.Owner = this;
            progressWindow.TopLevel = true;
            progressWindow.ShowInTaskbar = false;
            this.Enabled = false;
            progressWindow.Show();
        }

        protected void EndLoading()
        {
            Enabled = true;
            progressWindow.Hide();
            progressWindow.Dispose();

            TreeNode sceneTreeNode = new TreeNode("Scene");
            sceneTreeNode.Checked = true;
            foreach (Renderable renderable in SceneManager.Instance.CurrentScene.RenderObjects)
            {
                if (renderable.GetType().IsSubclassOf(typeof(Model)))
                {
                    Node objectNode = ((Model)renderable).Root;

                    TreeNode objectTreeNode = new TreeNode(objectNode.Name);
                    objectTreeNode.Checked = true;
                    foreach (Node modelNode in objectNode.Nodes)
                    {
                        TreeNode modelTreeNode = new TreeNode(modelNode.Name);
                        modelTreeNode.Checked = true;
                        foreach (Node groupNode in modelNode.Nodes)
                        {
                            TreeNode groupTreeNode = new TreeNode(groupNode.Name);
                            groupTreeNode.Checked = true;
                            modelTreeNode.Nodes.Add(groupTreeNode);
                        }
                        objectTreeNode.Nodes.Add(modelTreeNode);
                    }
                    sceneTreeNode.Nodes.Add(objectTreeNode);
                }
            }

            sceneTree.Nodes.Clear();
            if (sceneTreeNode.Nodes.Count > 0)
            {
                sceneTree.Nodes.Add(sceneTreeNode);
                sceneTree.ExpandAll();
            }
        }

        protected void LoadCurrentModel()
        {
            if ((_CurrentModelPath == "") || (!File.Exists(_CurrentModelPath)))
            {
                return;
            }
            Invoke(new MethodInvoker(BeginLoading));

            Thread loadingThread = new Thread((() =>
            {
                SceneManager.Instance.ShutDown();
                SceneManager.Instance.AddScene(new SceneCDC(_CurrentModelType));
                SceneManager.Instance.CurrentScene.ImportFromFile(_CurrentModelPath, ImportExportOptions);

                if (_ResetCameraOnModelLoad)
                {
                    CameraManager.Instance.Reset();
                }

                Invoke(new MethodInvoker(EndLoading));
            }));

            loadingThread.Name = "LoadingThread";
            loadingThread.SetApartmentState(ApartmentState.STA);
            loadingThread.Start();
            //loadingThread.Join();

            Thread progressThread = new Thread((() =>
            {
                do
                {
                    lock (SceneCDC.ProgressStage)
                    {
                        progressWindow.SetMessage(SceneCDC.ProgressStage);

                        int oldProgress = progressWindow.GetProgress();
                        if (oldProgress < SceneCDC.ProgressPercent)
                        {
                            progressWindow.SetProgress(oldProgress + 1);
                        }
                    }
                    Thread.Sleep(20);
                }
                while (loadingThread.IsAlive);
            }));

            progressThread.Name = "ProgressThread";
            progressThread.SetApartmentState(ApartmentState.STA);
            progressThread.Start();
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter =
                    "Gex Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Soul Reaver 1 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Soul Reaver 2 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Defiance Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Collada Mesh Files (*.dae)|*.dae",
                //"Soul Reaver DRM Files (*.drm)|*.drm|" +
                //"Soul Reaver PCM Files (*.pcm)|*.pcm|" +
                //"All Mesh Files|*.SRObj;*.drm;*.pcm|" +
                //"All Files (*.*)|*.*";
                DefaultExt = "drm",
                FilterIndex = filterIndex
            };
            if (_LastOpenDirectory != "")
            {
                if (Directory.Exists(_LastOpenDirectory))
                {
                    OpenDlg.InitialDirectory = _LastOpenDirectory;
                }
            }

            if (OpenDlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            if (OpenDlg.FilterIndex == 1)
            {
                CDC.Objects.GexFile gexFile;

                try
                {
                    gexFile = new CDC.Objects.GexFile(OpenDlg.FileName, ImportExportOptions);
                    if (gexFile.Asset == CDC.Asset.Unit)
                    {
                        ObjectSelectWindow objectSelectDlg = new ObjectSelectWindow();
                        objectSelectDlg.SetObjectNames(gexFile.InstanceTypeNames);
                        if (objectSelectDlg.ShowDialog() != DialogResult.OK)
                        {
                            return;
                        }
                    }
                }
                catch
                {
                    return;
                }

                _CurrentModelType = CDC.Game.Gex;   // "Gex Mesh Files|*.SRObj;*.drm;*.pcm|"
            }
            else if (OpenDlg.FilterIndex == 2)
            {
                _CurrentModelType = CDC.Game.SR1;   // "Soul Reaver 1 Mesh Files|*.SRObj;*.drm;*.pcm|"
            }
            else if (OpenDlg.FilterIndex == 3)
            {
                _CurrentModelType = CDC.Game.SR2;   // "Soul Reaver 2 Mesh Files|*.SRObj;*.drm;*.pcm|" +   
            }
            else
            {
                _CurrentModelType = CDC.Game.Defiance;  // "Defiance Mesh Files|*.SRObj;*.drm;*.pcm|" +
            }

            _LastOpenDirectory = Path.GetDirectoryName(OpenDlg.FileName);
            _CurrentModelPath = OpenDlg.FileName;
            reloadCurrentModelToolStripMenuItem.Enabled = true;
            filterIndex = OpenDlg.FilterIndex;

            //Invoke(new MethodInvoker(BeginLoading));

            //Thread loadingThread = new Thread((() =>
            //{
            //    SceneManager.Instance.ShutDown();
            //    SceneManager.Instance.AddScene(new SceneCDC(_CurrentModelType));
            //    SceneManager.Instance.CurrentScene.ImportFromFile(_CurrentModelPath, ImportExportOptions);

            //    //CameraManager.Instance.Reset();

            //    Invoke(new MethodInvoker(EndLoading));
            //}));

            //loadingThread.Name = "LoadingThread";
            //loadingThread.SetApartmentState(ApartmentState.STA);
            //loadingThread.Start();
            ////loadingThread.Join();

            //Thread progressThread = new Thread((() =>
            //{
            //    do
            //    {
            //        lock (SceneCDC.ProgressStage)
            //        {
            //            progressWindow.SetMessage(SceneCDC.ProgressStage);
            //            int oldProgress = progressWindow.GetProgress();
            //            if (oldProgress < SceneCDC.ProgressPercent)
            //            {
            //                progressWindow.SetProgress(oldProgress + 1);
            //            }
            //        }
            //        Thread.Sleep(20);
            //    }
            //    while (loadingThread.IsAlive);
            //}));

            //progressThread.Name = "ProgressThread";
            //progressThread.SetApartmentState(ApartmentState.STA);
            //progressThread.Start();
            //progressThread.Join();

            LoadCurrentModel();
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDlg = new SaveFileDialog
            {
                CheckPathExists = true,
                Filter =
                    "Collada Mesh Files (*.dae)|*.dae",
                    //"Soul Reaver DRM Files (*.drm)|*.drm|" +
                    //"Soul Reaver PCM Files (*.pcm)|*.pcm|" +
                    //"All Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    //"All Files (*.*)|*.*";
                DefaultExt = "dae",
                FilterIndex = 1
            };
            if (_LastExportDirectory != "")
            {
                if (Directory.Exists(_LastExportDirectory))
                {
                    SaveDlg.InitialDirectory = _LastExportDirectory;
                }
            }

            if (SaveDlg.ShowDialog() == DialogResult.OK)
            {
                _LastExportDirectory = Path.GetDirectoryName(SaveDlg.FileName);
                Scene currentScene = SceneManager.Instance.CurrentScene;
                if (currentScene != null)
                {
                    //string saveFileName = "C://Users//Andrew//Desktop//TestModel.dae";
                    currentScene.ExportToFile(SaveDlg.FileName, ImportExportOptions);

                }
            }
        }

        private void TreeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            Scene currentScene = SceneManager.Instance.CurrentScene;
            if (currentScene != null)
            {
                foreach (Renderable renderable in currentScene.RenderObjects)
                {
                    if (renderable.GetType().IsSubclassOf(typeof(Model)))
                    {
                        Node node = ((Model)renderable).FindNode(e.Node.Text);
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

        private void RealmBlendBar_Scroll(object sender, EventArgs e)
        {
            MeshMorphingUnit.RealmBlend = ((float)realmBlendBar.Value / realmBlendBar.Maximum);
        }

        protected void resetRenderModeMenu()
        {
            standardToolStripMenuItem.Checked = false;
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
            if (_ReloadModelOnRenderModeChange)
            {
                LoadCurrentModel();
            }
        }

        private void standardToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            standardToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.Standard;
            HandleRenderModeChange();
        }

        private void noTexturemapsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            noTexturemapsToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.NoTextures;
            HandleRenderModeChange();
        }

        private void unhide100InvisibleTexturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            unhide100InvisibleTexturesToolStripMenuItem.Checked = !unhide100InvisibleTexturesToolStripMenuItem.Checked;
            ImportExportOptions.UnhideCompletelyTransparentTextures = unhide100InvisibleTexturesToolStripMenuItem.Checked;
        }

        private void discardHiddenPolygonsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            discardHiddenPolygonsToolStripMenuItem.Checked = !discardHiddenPolygonsToolStripMenuItem.Checked;
            ImportExportOptions.DiscardNonVisible = discardHiddenPolygonsToolStripMenuItem.Checked;
        }

        private void missingPalettesInGreyscaleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            missingPalettesInGreyscaleToolStripMenuItem.Checked = !missingPalettesInGreyscaleToolStripMenuItem.Checked;
            ImportExportOptions.AlwaysUseGreyscaleForMissingPalettes = missingPalettesInGreyscaleToolStripMenuItem.Checked;
        }

        private void exportDoubleSidedMaterialsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportDoubleSidedMaterialsToolStripMenuItem.Checked = !exportDoubleSidedMaterialsToolStripMenuItem.Checked;
            ImportExportOptions.ExportDoubleSidedMaterials = exportDoubleSidedMaterialsToolStripMenuItem.Checked;
        }

        private void exportSpectralVersionOfAreaFilesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            exportSpectralVersionOfAreaFilesToolStripMenuItem.Checked = !exportSpectralVersionOfAreaFilesToolStripMenuItem.Checked;
            ImportExportOptions.ExportSpectral = exportSpectralVersionOfAreaFilesToolStripMenuItem.Checked;
        }

        private void makeAllPolygonsVisibleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            makeAllPolygonsVisibleToolStripMenuItem.Checked = !makeAllPolygonsVisibleToolStripMenuItem.Checked;
            ImportExportOptions.MakeAllPolygonsVisible = makeAllPolygonsVisibleToolStripMenuItem.Checked;
        }

        private void makeAllPolygonsOpaqueToolStripMenuItem_Click(object sender, EventArgs e)
        {
            makeAllPolygonsOpaqueToolStripMenuItem.Checked = !makeAllPolygonsOpaqueToolStripMenuItem.Checked;
            ImportExportOptions.MakeAllPolygonsOpaque = makeAllPolygonsOpaqueToolStripMenuItem.Checked;
        }

        private void oRAllPolygonColoursWithGreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            oRAllPolygonColoursWithGreenToolStripMenuItem.Checked = !oRAllPolygonColoursWithGreenToolStripMenuItem.Checked;
            ImportExportOptions.SetAllPolygonColoursToValue = oRAllPolygonColoursWithGreenToolStripMenuItem.Checked;
        }

        private void includeTreeRootFlagsInORdParentFlagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            includeTreeRootFlagsInORdParentFlagsToolStripMenuItem.Checked = !includeTreeRootFlagsInORdParentFlagsToolStripMenuItem.Checked;
            ImportExportOptions.BSPRenderingIncludeRootTreeFlagsWhenORing = includeTreeRootFlagsInORdParentFlagsToolStripMenuItem.Checked;
        }

        private void includeLeafFlagsInORdParentFlagsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            includeLeafFlagsInORdParentFlagsToolStripMenuItem.Checked = !includeLeafFlagsInORdParentFlagsToolStripMenuItem.Checked;
            ImportExportOptions.BSPRenderingIncludeLeafFlagsWhenORing = includeLeafFlagsInORdParentFlagsToolStripMenuItem.Checked;
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



        private void debugPolygonFlags1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugPolygonFlags1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugPolygonFlags1;
            HandleRenderModeChange();
        }

        private void debugPolygonFlags2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugPolygonFlags2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugPolygonFlags2;
            HandleRenderModeChange();
        }

        private void debugPolygonFlags3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugPolygonFlags3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugPolygonFlags3;
            HandleRenderModeChange();
        }

        private void debugPolygonFlagsSoulReaverAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugPolygonFlagsSoulReaverAToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugPolygonFlagsSoulReaverA;
            HandleRenderModeChange();
        }

        private void debugTextureAttributes1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributes1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributes1;
            HandleRenderModeChange();
        }

        private void debugTextureAttributes2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributes2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributes2;
            HandleRenderModeChange();
        }

        private void debugTextureAttributes3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributes3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributes3;
            HandleRenderModeChange();
        }

        private void debugTextureAttributes4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributes4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributes4;
            HandleRenderModeChange();
        }

        private void debugTextureAttributes5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributes5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributes5;
            HandleRenderModeChange();
        }

        private void debugTextureAttributes6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributes6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributes6;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesAHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesAHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesAHash;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesA1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesA1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesA1;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesA2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesA2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesA2;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesA3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesA3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesA3;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesA4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesA4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesA4;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesA5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesA5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesA5;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesA6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesA6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesA6;
            HandleRenderModeChange();
        }

        private void debugTexturePage1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePage1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePage1;
            HandleRenderModeChange();
        }

        private void debugTexturePage2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePage2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePage2;
            HandleRenderModeChange();
        }

        private void debugTexturePage3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePage3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePage3;
            HandleRenderModeChange();
        }

        private void debugTexturePage4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePage4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePage4;
            HandleRenderModeChange();
        }

        private void debugTexturePage5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePage5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePage5;
            HandleRenderModeChange();
        }

        private void debugTexturePage6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePage6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePage6;
            HandleRenderModeChange();
        }

        private void rootBSPTreeNumberToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            rootBSPTreeNumberToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeNumber;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeIDToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeIDToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeNodeID;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlags1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlags1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlags1;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlags2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlags2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlags2;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlags3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlags3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlags3;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlags4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlags4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlags4;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlags5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlags5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlags5;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlags6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlags6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlags6;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlags1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlags1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags1;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlags2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlags2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags2;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlags3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlags3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags3;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlags4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlags4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags4;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlags5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlags5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags5;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlags6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlags6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlags6;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORd1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORd1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd1;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORd2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORd2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd2;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORd3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORd3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd3;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORd4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORd4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd4;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORd5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORd5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd5;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORd6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORd6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORd6;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlags1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlags1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlags1;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlags2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlags2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlags2;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlags3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlags3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlags3;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlags4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlags4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlags4;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlags5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlags5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlags5;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlags6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlags6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlags6;
            HandleRenderModeChange();
        }

        private void debugPolygonFlagsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugPolygonFlagsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugPolygonFlagsHash;
            HandleRenderModeChange();
        }

        private void debugTextureAttributesHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTextureAttributesHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTextureAttributesHash;
            HandleRenderModeChange();
        }

        private void debugTexturePageHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePageHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePageHash;
            HandleRenderModeChange();
        }

        private void bSPTreeRootFlagsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeRootFlagsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPRootTreeFlagsHash;
            HandleRenderModeChange();
        }

        private void bSPTreeNodeFlagsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeNodeFlagsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeImmediateParentFlagsHash;
            HandleRenderModeChange();
        }

        private void bSPTreeParentNodeFlagsORdHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeParentNodeFlagsORdHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeAllParentFlagsORdHash;
            HandleRenderModeChange();
        }

        private void bSPTreeLeafFlagsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            bSPTreeLeafFlagsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBSPTreeLeafFlagsHash;
            HandleRenderModeChange();
        }

        private void debugTexturePageUpper24BitsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePageUpper28BitsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePageUpper28BitsHash;
            HandleRenderModeChange();
        }

        private void debugTexturePageUpper5BitsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugTexturePageUpper5BitsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugTexturePageUpper5BitsHash;
            HandleRenderModeChange();
        }

        private void cLUTHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUTHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUTHash;
            HandleRenderModeChange();
        }

        private void cLUT1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUT1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUT1;
            HandleRenderModeChange();
        }

        private void cLUT2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUT2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUT2;
            HandleRenderModeChange();
        }

        private void cLUT3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUT3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUT3;
            HandleRenderModeChange();
        }

        private void cLUT4ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUT4ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUT4;
            HandleRenderModeChange();
        }

        private void cLUT5ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUT5ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUT5;
            HandleRenderModeChange();
        }

        private void cLUT6ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUT6ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUT6;
            HandleRenderModeChange();
        }

        private void cLUTNonRowColBitsHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUTNonRowColBitsHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUTNonRowColBitsHash;
            HandleRenderModeChange();
        }

        private void cLUTNonRowColBits1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUTNonRowColBits1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUTNonRowColBits1;
            HandleRenderModeChange();
        }

        private void cLUTNonRowColBits2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            cLUTNonRowColBits2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugCLUTNonRowColBits2;
            HandleRenderModeChange();
        }

        private void boneIDHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            boneIDHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugBoneIDHash;
            HandleRenderModeChange();
        }

        private void debugSortPushHashToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugSortPushHashToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugSortPushHash;
            HandleRenderModeChange();
        }

        private void debugSortPushFlags1ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugSortPushFlags1ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugSortPushFlags1;
            HandleRenderModeChange();
        }

        private void debugSortPushFlags2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugSortPushFlags2ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugSortPushFlags2;
            HandleRenderModeChange();
        }

        private void debugSortPushFlags3ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            debugSortPushFlags3ToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.DebugSortPushFlags3;
            HandleRenderModeChange();
        }

        private void averageVertexAlphaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            averageVertexAlphaToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.AverageVertexAlpha;
            HandleRenderModeChange();
        }

        private void polygonAlphaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            polygonAlphaToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.PolygonAlpha;
            HandleRenderModeChange();
        }

        private void polygonOpacityToolStripMenuItem_Click(object sender, EventArgs e)
        {
            resetRenderModeMenu();
            polygonOpacityToolStripMenuItem.Checked = true;
            ImportExportOptions.RenderMode = CDC.Objects.RenderMode.PolygonOpacity;
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
            LoadCurrentModel();
        }

        private void interpolatePolygonColoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            interpolatePolygonColoursToolStripMenuItem.Checked = !interpolatePolygonColoursToolStripMenuItem.Checked;
            ImportExportOptions.InterpolatePolygonColoursWhenColouringBasedOnVertices = interpolatePolygonColoursToolStripMenuItem.Checked;
            HandleRenderModeChange();
        }

        private void useEachUniqueTextureCLUTVariationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            useEachUniqueTextureCLUTVariationToolStripMenuItem.Checked = !useEachUniqueTextureCLUTVariationToolStripMenuItem.Checked;
            ImportExportOptions.UseEachUniqueTextureCLUTVariation = useEachUniqueTextureCLUTVariationToolStripMenuItem.Checked;
            HandleRenderModeChange();
        }

        private void augmentAlphaMaskingFlagsBasedOnImageContentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            augmentAlphaMaskingFlagsBasedOnImageContentToolStripMenuItem.Checked = !augmentAlphaMaskingFlagsBasedOnImageContentToolStripMenuItem.Checked;
            ImportExportOptions.AlsoInferAlphaMaskingFromTexturePixels = augmentAlphaMaskingFlagsBasedOnImageContentToolStripMenuItem.Checked;
            HandleRenderModeChange();
        }

        private void ignorePolygonFlag2ForTerrainToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ignorePolygonFlag2ForTerrainToolStripMenuItem.Checked = !ignorePolygonFlag2ForTerrainToolStripMenuItem.Checked;
            ImportExportOptions.IgnorePolygonFlag2ForTerrain = ignorePolygonFlag2ForTerrainToolStripMenuItem.Checked;
            HandleRenderModeChange();
        }

        private void createDistinctMaterialsForAllFlagsEvenIfUnusedToolStripMenuItem_Click(object sender, EventArgs e)
        {
            createDistinctMaterialsForAllFlagsEvenIfUnusedToolStripMenuItem.Checked = !createDistinctMaterialsForAllFlagsEvenIfUnusedToolStripMenuItem.Checked;
            ImportExportOptions.DistinctMaterialsForAllFlags = createDistinctMaterialsForAllFlagsEvenIfUnusedToolStripMenuItem.Checked;
        }

        private void adjustUVCoordinatesForBilinearFilteringToolStripMenuItem_Click(object sender, EventArgs e)
        {
            adjustUVCoordinatesForBilinearFilteringToolStripMenuItem.Checked = !adjustUVCoordinatesForBilinearFilteringToolStripMenuItem.Checked;
            ImportExportOptions.AdjustUVs = adjustUVCoordinatesForBilinearFilteringToolStripMenuItem.Checked;
        }

        private void ignoreVertexColoursToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ignoreVertexColoursToolStripMenuItem.Checked = !ignoreVertexColoursToolStripMenuItem.Checked;
            ImportExportOptions.IgnoreVertexColours = ignoreVertexColoursToolStripMenuItem.Checked;
        }

        protected void ResetPlatformDetection()
        {
            autodetectToolStripMenuItem.Checked = false;
            forcePlayStationToolStripMenuItem.Checked = false;
            forcePCToolStripMenuItem.Checked = false;
            forceDreamcastToolStripMenuItem.Checked = false;
            forcePlayStation2ToolStripMenuItem.Checked = false;
            forceXboxToolStripMenuItem.Checked = false;
        }

        private void autodetectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPlatformDetection();
            autodetectToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.None;
        }

        private void forcePlayStationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPlatformDetection();
            forcePlayStationToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.PSX;
        }

        private void forcePCToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPlatformDetection();
            forcePCToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.PC;
        }

        private void forceDreamcastToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPlatformDetection();
            forceDreamcastToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.Dreamcast;
        }

        private void forcePlayStation2ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPlatformDetection();
            forcePlayStation2ToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.PlayStation2;
        }

        private void forceXboxToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ResetPlatformDetection();
            forceXboxToolStripMenuItem.Checked = true;
            ImportExportOptions.ForcedPlatform = CDC.Platform.Xbox;
        }

    }
}