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

        public LoadResourceDialog()
        {
            InitializeComponent();

            comboBox2.Items.Add("Gex 3 (*.drm)");
            comboBox2.Items.Add("Soul Reaver 1 (*.drm)");
            comboBox2.Items.Add("Soul Reaver 2 (*.drm)");
            comboBox2.Items.Add("Defiance (*.drm)");
            comboBox2.Items.Add("Tomb Raider (*.drm)");
            comboBox2.SelectedIndex = 1;
        }

        private void PopulateTreeView()
        {
            treeView1.BeginUpdate();

            imageList1.Images.Add("", this.Icon);
            
            string folderPath = Environment.GetFolderPath(Environment.SpecialFolder.Windows);
            imageList1.Images.Add(_folderImageKey, Win32Icons.GetDirectoryIcon(folderPath, false));

            TreeNode rootNode;

            /*string currentDirectory = InitialDirectory;
            if (!Directory.Exists(currentDirectory))
            {
                currentDirectory = @"../..";
            }*/

            DriveInfo[] driveInfos = DriveInfo.GetDrives();
            foreach (DriveInfo driveInfo in driveInfos)
            {
                DirectoryInfo subDirectoryInfo = new DirectoryInfo(driveInfo.Name);
                rootNode = CreateTreeNode(subDirectoryInfo); // GetDirectories(info);
                treeView1.Nodes.Add(rootNode);
            }

            treeView1.EndUpdate();
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
            treeNode.Tag = directoryInfo;

            if (directoryInfo.Parent?.Parent == null)
            {
                if (!imageList1.Images.ContainsKey(directoryInfo.Name))
                {
                    imageList1.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(directoryInfo.FullName, false));
                }

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
                    item = new ListViewItem(dir.Name, _folderImageKey);
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

		private void LoadResourceDialog_Load(object sender, System.EventArgs e)
        {
            //this.BackColor = Color.Black;

            //treeView1.BackColor = Color.DarkGray;
            //treeView1.ForeColor = Color.White;
            //treeView1.LineColor = Color.White;

            //listView1.BackColor = Color.DarkGray;
            //listView1.ForeColor = Color.White;

            //panel1.BackColor = Color.Transparent;

            //button1.BackColor = SystemColors.ButtonFace;
            //button2.BackColor = SystemColors.ButtonFace;

            PopulateTreeView();
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

                    string textureFileName = Path.ChangeExtension(fileInfo.FullName, "crm");
                    textBox2.Text = textureFileName;
                }
                else
                {
                    textBox1.Text = "";
                    textBox2.Text = "";
                }
            }
            else
            {
                textBox1.Text = "";
                textBox2.Text = "";
            }
		}
	}
}
