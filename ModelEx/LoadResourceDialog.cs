using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace ModelEx
{
    public partial class LoadResourceDialog : Form
    {
        public string InitialDirectory { get; set; }
        public string FileName { get; private set; } = "";

        private static readonly string _folderImageKey = "WINDOWS";

        private readonly List<string> _specialFolders = new List<string>();

        public LoadResourceDialog()
        {
            InitializeComponent();

            comboBox2.Items.Add("Gex 3 Files(*.drm)");
            comboBox2.Items.Add("Soul Reaver 1 Files (*.drm)");
            comboBox2.Items.Add("Soul Reaver 2 Files (*.drm)");
            comboBox2.Items.Add("Defiance Files (*.drm)");
            comboBox2.Items.Add("Tomb Raider Files(*.drm)");
            comboBox2.SelectedIndex = 1;
        }

		private void LoadResourceDialog_Load(object sender, System.EventArgs e)
        {
            treeView1.BeginUpdate();

            /*this.BackColor = Color.FromArgb(0x40, 0x40, 0x40);

            treeView1.BackColor = Color.FromArgb(0x40, 0x40, 0x40);
            treeView1.ForeColor = Color.White;
            treeView1.LineColor = Color.White;

            listView1.BackColor = Color.FromArgb(0x40, 0x40, 0x40);
            listView1.ForeColor = Color.White;

            panel1.BackColor = Color.FromArgb(0x40, 0x40, 0x40);

            button1.BackColor = SystemColors.ButtonFace;
            button2.BackColor = SystemColors.ButtonFace;

            label1.ForeColor = Color.White;
            label2.ForeColor = Color.White;
            checkBox1.ForeColor = Color.White;*/

            imageList1.Images.Add("", Icon);

            for (int i = 0; i < 60; i++)
            {
                try
                {
                    string specialFolderName = Environment.GetFolderPath((Environment.SpecialFolder)i);

                    DirectoryInfo directoryInfo = new DirectoryInfo(specialFolderName);

                    _specialFolders.Add(specialFolderName);
                    imageList1.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(specialFolderName, false).ToBitmap());
                }
                catch (Exception)
                {
                }
            }

            TreeNode rootNode;

            /*string currentDirectory = InitialDirectory;
            if (!Directory.Exists(currentDirectory))
            {
                currentDirectory = @"../..";
            }*/

            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in driveInfos)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(driveInfo.Name);
                if (!imageList1.Images.ContainsKey(directoryInfo.Name))
                {
                    _specialFolders.Add(directoryInfo.Name);
                    imageList1.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(directoryInfo.Name, false).ToBitmap());
                }
                rootNode = CreateTreeNode(directoryInfo); // GetDirectories(info);
                treeView1.Nodes.Add(rootNode);
            }

            treeView1.EndUpdate();

            List<DirectoryInfo> breadCrumbs = new List<DirectoryInfo>();
            try
            {
                DirectoryInfo initialDirectoryInfo = new DirectoryInfo(InitialDirectory);
                DirectoryInfo expandDirectory = initialDirectoryInfo;
                while (expandDirectory != null)
                {
                    breadCrumbs.Insert(0, expandDirectory);
                    expandDirectory = expandDirectory.Parent;
                }
            }
            catch (Exception)
            {
                breadCrumbs.Clear();
            }

            TreeNode expandNode = null;

            try
            {
                TreeNodeCollection expandNodeCollection = treeView1.Nodes;
                while (breadCrumbs.Count > 0)
                {
                    expandNode = expandNodeCollection.Find(breadCrumbs[0].Name, false)[0];
                    expandNodeCollection = expandNode.Nodes;

                    if (expandNodeCollection.Count > 0)
                    {
                        expandNode.Expand();
                    }

                    breadCrumbs.RemoveAt(0);
                }
            }
            catch (Exception)
            {
                treeView1.CollapseAll();
            }

            treeView1.SelectedNode = expandNode;
        }

        private DirectoryInfo[] GetSubDirectories(DirectoryInfo directoryInfo)
        {
            DirectoryInfo[] subDirectoryInfo;

            try
            {
                subDirectoryInfo = directoryInfo.GetDirectories();
            }
            catch (System.Exception/*System.UnauthorizedAccessException*/)
            {
                subDirectoryInfo = new DirectoryInfo[0];
            }

            return subDirectoryInfo;
        }

        private FileInfo[] GetFiles(DirectoryInfo directoryInfo)
        {
            FileInfo[] fileInfo;

            try
            {
                fileInfo = directoryInfo.GetFiles();
            }
            catch (System.Exception/*System.UnauthorizedAccessException*/)
            {
                fileInfo = new FileInfo[0];
            }

            return fileInfo;
        }

        private TreeNode GetDirectories(DirectoryInfo directoryInfo)
        {
            TreeNode nodeToAddTo = new TreeNode(directoryInfo.Name, 0, 0);
            nodeToAddTo.Name = directoryInfo.Name;
            nodeToAddTo.Tag = directoryInfo;
            nodeToAddTo.ImageKey = _folderImageKey;
            nodeToAddTo.SelectedImageKey = _folderImageKey;

            DirectoryInfo[] subDirectoryInfos = GetSubDirectories(directoryInfo);
            foreach (DirectoryInfo subDirectoryInfo in subDirectoryInfos)
            {
                #region Validation
                /*bool writeAllow = false;
                bool writeDeny = false;
                var accessControlList = Directory.GetAccessControl(subDirectoryInfo.FullName);
                if (accessControlList == null)
                {
                    continue;
                }
                var accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                if (accessRules == null)
                {
                    continue;
                }

                foreach (FileSystemAccessRule rule in accessRules)
                {
                    if ((FileSystemRights.ListDirectory & rule.FileSystemRights) == 0)
                    {
                        continue;
                    }

                    if (rule.AccessControlType == AccessControlType.Allow)
                    {
                        writeAllow = true;
                    }
                    else if (rule.AccessControlType == AccessControlType.Deny)
                    {
                        writeDeny = true;
                    }
                }

                if (writeDeny)
                {
                    continue;
                }*/
                #endregion

                TreeNode subDirNode = GetDirectories(subDirectoryInfo);
                nodeToAddTo.Nodes.Add(subDirNode);
            }

            return nodeToAddTo;
        }

        private TreeNode CreateTreeNode(DirectoryInfo directoryInfo)
        {
            TreeNode treeNode = new TreeNode(directoryInfo.Name, 0, 0);
            treeNode.Name = directoryInfo.Name;
            treeNode.Tag = directoryInfo;

            if (_specialFolders.Contains(directoryInfo.FullName))
            {
                treeNode.ImageKey = directoryInfo.Name;
                treeNode.SelectedImageKey = directoryInfo.Name;
            }
            else
            {
                treeNode.ImageKey = _folderImageKey;
                treeNode.SelectedImageKey = _folderImageKey;
            }

            DirectoryInfo[] subDirectoryInfos = GetSubDirectories(directoryInfo);

            /*foreach (DirectoryInfo subDirectoryInfo in subDirectoryInfos)
            {
            }*/

            if (subDirectoryInfos.Length > 0)
            {
                TreeNode dummyChildNode = new TreeNode();
                treeNode.Nodes.Add(dummyChildNode);
            }

            return treeNode;
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            listView1.Items.Clear();
            TreeNode newSelected = e.Node;
            DirectoryInfo nodeDirInfo = (DirectoryInfo)newSelected.Tag;
            ListViewItem.ListViewSubItem[] subItems;
            ListViewItem item;

            DirectoryInfo[] subDirectoryInfos = GetSubDirectories(nodeDirInfo);
            if (subDirectoryInfos.Length > 0)
            {
                foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
                {
                    string folderImageKey = _folderImageKey;
                    if (_specialFolders.Contains(dir.FullName))
                    {
                        folderImageKey = dir.Name;
                    }

                    item = new ListViewItem(dir.Name, folderImageKey);
                    item.Tag = dir;
                    subItems = new ListViewItem.ListViewSubItem[]
                    {
                    new ListViewItem.ListViewSubItem(item, "Directory"),
                    new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString())
                    };
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
            }

            FileInfo[] fileInfos = GetFiles(nodeDirInfo);
            if (fileInfos.Length > 0)
            {
                foreach (FileInfo file in nodeDirInfo.GetFiles())
                {
                    if (file.Extension != ".pcm" && file.Extension != ".drm")
                    {
                        continue;
                    }

                    item = new ListViewItem(file.Name, 0);
                    item.Tag = file;
                    subItems = new ListViewItem.ListViewSubItem[]
                    {
                    new ListViewItem.ListViewSubItem(item, "File"),
                    new ListViewItem.ListViewSubItem(item, file.LastAccessTime.ToShortDateString())
                    };
                    item.SubItems.AddRange(subItems);
                    listView1.Items.Add(item);
                }
            }

            listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
        }

		private void treeView1_AfterCollapse(object sender, TreeViewEventArgs e)
        {
            TreeNode treeNode = e.Node;
            if (treeNode.Tag is DirectoryInfo)
            {
                DirectoryInfo directoryInfo = (DirectoryInfo)treeNode.Tag;
                treeNode.Nodes.Clear();

                DirectoryInfo[] subDirectoryInfos = GetSubDirectories(directoryInfo);
                if (subDirectoryInfos.Length > 0)
                {
                    TreeNode dummyChildNode = new TreeNode();
                    treeNode.Nodes.Add(dummyChildNode);
                }
            }
        }

		private void treeView1_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
            TreeNode treeNode = e.Node;
            if (treeNode.Tag is DirectoryInfo)
            {
                DirectoryInfo directoryInfo = (DirectoryInfo)treeNode.Tag;
                treeNode.Nodes.Clear();

                DirectoryInfo[] subDirectoryInfos = GetSubDirectories(directoryInfo);
                if (subDirectoryInfos.Length > 0)
                {
                    foreach (DirectoryInfo subDirectoryInfo in subDirectoryInfos)
                    {
                        TreeNode subDirNode = CreateTreeNode(subDirectoryInfo);
                        treeNode.Nodes.Add(subDirNode);
                    }
                }
            }
		}

		private void listView1_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            ListView listView = (ListView)sender;
            if (listView.SelectedItems.Count > 0)
            {
                if (listView.SelectedItems[0].Tag is FileInfo)
                {
                    FileInfo fileInfo = (FileInfo)listView.SelectedItems[0].Tag;
                    textBox1.Text = fileInfo.FullName;

                    comboBox1.Items.Clear();
                    comboBox1.Enabled = false;
                    comboBox3.Items.Clear();
                    comboBox3.Enabled = false;

                    string textureFileName = fileInfo.FullName;
                    string objectIDFileName = fileInfo.FullName;
                    switch ((CDC.Game)comboBox2.SelectedIndex)
                    {
                        case CDC.Game.SR1:
                        {
                            textureFileName = Path.ChangeExtension(fileInfo.FullName, "crm");
                            comboBox1.Enabled = true;
                            break;
                        }
                        case CDC.Game.Gex:
                        case CDC.Game.SR2:
                        case CDC.Game.Defiance:
                        {
                            textureFileName = Path.ChangeExtension(fileInfo.FullName, "vrm");
                            comboBox1.Enabled = true;
                            comboBox3.Enabled = true;
                            break;
                        }
                        case CDC.Game.TRL:
                        {
                            comboBox3.Enabled = true;
                            break;
                        }
                        default:
                            break;
                    }

                    comboBox1.Items.Add(textureFileName);
                    comboBox1.SelectedIndex = 0;

                    comboBox3.Items.Add(objectIDFileName);
                    comboBox3.SelectedIndex = 0;
                }
                else
                {
                    textBox1.Text = "";
                    textBox2.Text = "";
                    comboBox1.Items.Clear();
                    comboBox3.Items.Clear();
                }
            }
            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
                comboBox1.Items.Clear();
                comboBox3.Items.Clear();
            }
		}

		private void comboBox2_SelectedIndexChanged(object sender, EventArgs e)
		{

		}
	}
}
