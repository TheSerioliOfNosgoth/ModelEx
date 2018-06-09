using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelEx
{
    public partial class MainWindow : Form
    {
        ProgressWindow progressWindow;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            sceneView.ShutDown();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            sceneView.Initialize();
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

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter =
                    "Soul Reaver 1 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Soul Reaver 2 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Collada Mesh Files (*.dae)|*.dae",
                    //"Soul Reaver DRM Files (*.drm)|*.drm|" +
                    //"Soul Reaver PCM Files (*.pcm)|*.pcm|" +
                    //"All Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    //"All Files (*.*)|*.*";
                DefaultExt = "drm",
                FilterIndex = 1
            };

            if (OpenDlg.ShowDialog() == DialogResult.OK)
            {
                Invoke(new MethodInvoker(BeginLoading));

                Thread loadingThread = new Thread((() =>
                {
                    SceneManager.Instance.ShutDown();
                    if (OpenDlg.FilterIndex == 1)
                    {
                        SceneManager.Instance.AddScene(new SceneCDC(false));
                        SceneManager.Instance.CurrentScene.ImportFromFile(OpenDlg.FileName);
                    }
                    else if (OpenDlg.FilterIndex == 2)
                    {
                        SceneManager.Instance.AddScene(new SceneCDC(true));
                        SceneManager.Instance.CurrentScene.ImportFromFile(OpenDlg.FileName);
                    }

                    CameraManager.Instance.Reset();

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
                //progressThread.Join();
            }
        }

        private void ExportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter =
                    "Soul Reaver 1 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Soul Reaver 2 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    "Collada Mesh Files (*.dae)|*.dae",
                    //"Soul Reaver DRM Files (*.drm)|*.drm|" +
                    //"Soul Reaver PCM Files (*.pcm)|*.pcm|" +
                    //"All Mesh Files|*.SRObj;*.drm;*.pcm|" +
                    //"All Files (*.*)|*.*";
                DefaultExt = "drm",
                FilterIndex = 1
            };

            if (OpenDlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    CDC.Objects.SRFile srFile = new CDC.Objects.SR1File(OpenDlg.FileName);
                    if (srFile != null)
                    {
                        string saveFileName = "C://Users//Andrew//Desktop//TestModel.dae";
                        srFile.ExportToFile(saveFileName);
                    }
                }
                catch
                {
                    return;
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
    }
}