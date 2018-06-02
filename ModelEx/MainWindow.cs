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
            foreach (Renderable renderable in Scene.Instance.RenderObjects)
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

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OpenDlg = new OpenFileDialog();
            OpenDlg.CheckFileExists = true;
            OpenDlg.CheckPathExists = true;
            OpenDlg.Filter =
                "Soul Reaver 1 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                "Soul Reaver 2 Mesh Files|*.SRObj;*.drm;*.pcm|" +
                "Collada Mesh Files (*.dae)|*.dae";
            //"Soul Reaver DRM Files (*.drm)|*.drm|" +
            //"Soul Reaver PCM Files (*.pcm)|*.pcm|" +
            //"All Mesh Files|*.SRObj;*.drm;*.pcm|" +
            //"All Files (*.*)|*.*";
            OpenDlg.DefaultExt = "drm";
            OpenDlg.FilterIndex = 1;
            if (OpenDlg.ShowDialog() == DialogResult.OK)
            {
                Invoke(new MethodInvoker(BeginLoading));

                Thread loadingThread = new Thread((ThreadStart)(() =>
                {
                    Scene.Instance.ShutDown();
                    if (OpenDlg.FilterIndex == 1)
                    {
                        MeshLoader.LoadSoulReaverFile(OpenDlg.FileName, false);
                    }
                    else if (OpenDlg.FilterIndex == 2)
                    {
                        MeshLoader.LoadSoulReaverFile(OpenDlg.FileName, true);
                    }

                    Renderable mainObject = Scene.Instance.RenderObjects[0];
                    if (mainObject != null && mainObject.GetType() == typeof(Physical))
                    {
                        SlimDX.BoundingSphere boundingSphere = Scene.Instance.RenderObjects[0].GetBoundingSphere();
                        SlimDX.Vector3 target = boundingSphere.Center;
                        SlimDX.Vector3 eye = target - new SlimDX.Vector3(0.0f, 0.0f, boundingSphere.Radius * 2.5f);
                        CameraManager.Instance.SetView(eye, target);
                    }
                    else
                    {
                        SlimDX.Vector3 eye = new SlimDX.Vector3(0.0f, 0.0f, 0.0f);
                        SlimDX.Vector3 target = new SlimDX.Vector3(0.0f, 0.0f, 1.0f);
                        CameraManager.Instance.SetView(eye, target);
                    }

                    //MeshLoader.LoadSoulReaverFile("C:/Users/A/Documents/SR2-Models-PC/raziel.drm", true);
                    //MeshLoader.Import("C:/Program Files/Assimp/test/models-nonbsd/3DS/mar_rifle.3ds");

                    Invoke(new MethodInvoker(EndLoading));
                }));

                loadingThread.Name = "LoadingThread";
                loadingThread.SetApartmentState(ApartmentState.STA);
                loadingThread.Start();
                //loadingThread.Join();

                Thread progressThread = new Thread((ThreadStart)(() =>
                {
                    do
                    {
                        lock (MeshLoader.ProgressStage)
                        {
                            progressWindow.SetMessage(MeshLoader.ProgressStage);

                            int oldProgress = progressWindow.GetProgress();
                            if (oldProgress < MeshLoader.ProgressPercent)
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

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            foreach (Renderable renderable in Scene.Instance.RenderObjects)
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

        private void resetPositionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Renderable mainObject = Scene.Instance.RenderObjects[0];
            if (mainObject != null && mainObject.GetType() == typeof(Physical))
            {
                SlimDX.BoundingSphere boundingSphere = Scene.Instance.RenderObjects[0].GetBoundingSphere();
                SlimDX.Vector3 target = boundingSphere.Center;
                SlimDX.Vector3 eye = target - new SlimDX.Vector3(0.0f, 0.0f, boundingSphere.Radius * 2.5f);
                CameraManager.Instance.SetView(eye, target);
            }
            else
            {
                SlimDX.Vector3 eye = new SlimDX.Vector3(0.0f, 0.0f, 0.0f);
                SlimDX.Vector3 target = new SlimDX.Vector3(0.0f, 0.0f, 1.0f);
                CameraManager.Instance.SetView(eye, target);
            }
        }

        private void egoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CameraManager.Instance.SetCamera(CameraManager.CameraMode.Ego);
            egoToolStripMenuItem.Checked = true;
            orbitToolStripMenuItem.Checked = false;
            orbitPanToolStripMenuItem.Checked = false;
        }

        private void orbitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CameraManager.Instance.SetCamera(CameraManager.CameraMode.Orbit);
            egoToolStripMenuItem.Checked = false;
            orbitToolStripMenuItem.Checked = true;
            orbitPanToolStripMenuItem.Checked = false;
        }

        private void orbitPanToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CameraManager.Instance.SetCamera(CameraManager.CameraMode.OrbitPan);
            egoToolStripMenuItem.Checked = false;
            orbitToolStripMenuItem.Checked = false;
            orbitPanToolStripMenuItem.Checked = true;
        }

        private void trackBar1_Scroll(object sender, EventArgs e)
        {
            MeshMorphingUnit.RealmBlend = ((float)trackBar1.Value / (float)trackBar1.Maximum);
        }
    }
}