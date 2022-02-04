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
		public string DataFile { get; private set; } = "";
		public string TextureFile { get; private set; } = "";
		public string ObjectListFile { get; private set; } = "";
		public CDC.Game GameType { get; private set; } = CDC.Game.Gex;
		public CDC.Platform Platform { get; private set; } = CDC.Platform.PC;
		public bool ClearLoadedFiles { get; private set; } = false;

		private readonly List<string> _specialFolders = new List<string>();

		class PlatformNode
		{
			public readonly CDC.Platform Platform;
			public readonly string PlatformName;

			public PlatformNode(CDC.Platform platform)
			{
				Platform = platform;
				switch (Platform)
				{
					default:
					case CDC.Platform.None: PlatformName = "None"; break;
					case CDC.Platform.PC: PlatformName = "PC"; break;
					case CDC.Platform.PSX: PlatformName = "PlayStation"; break;
					case CDC.Platform.PlayStation2: PlatformName = "PlayStation 2"; break;
					case CDC.Platform.Dreamcast: PlatformName = "Dreamcast"; break;
					case CDC.Platform.Xbox: PlatformName = "XBox"; break;
				}
			}

			public override string ToString()
			{
				return PlatformName;
			}
		}

		class FileNode
		{
			public readonly bool FileExists;
			public readonly string FileName;
			public readonly string FileDescription;

			public FileNode(string fileName, bool fileExists)
			{
				FileName = fileName;
				FileExists = fileExists && File.Exists(fileName);
				FileDescription = FileExists ? FileName : (FileName + " (NOT FOUND)");
			}

			public override string ToString()
			{
				return FileDescription;
			}
		}

		public LoadResourceDialog()
		{
			InitializeComponent();

			gameTypeComboBox.Items.Add("Gex 3 Files(*.drm)");
			gameTypeComboBox.Items.Add("Soul Reaver 1 Files (*.drm)");
			gameTypeComboBox.Items.Add("Soul Reaver 2 Files (*.drm)");
			gameTypeComboBox.Items.Add("Defiance Files (*.drm)");
			gameTypeComboBox.Items.Add("Tomb Raider Files(*.drm)");
			gameTypeComboBox.SelectedIndex = 1;

			// UpdateGameType will be called automatically by setting the selected index.
		}

		private void LoadResourceDialog_Load(object sender, System.EventArgs e)
		{
			browserTreeView.BeginUpdate();

			/*this.BackColor = Color.FromArgb(0x40, 0x40, 0x40);

            browserTreeView.BackColor = Color.FromArgb(0x40, 0x40, 0x40);
            browserTreeView.ForeColor = Color.White;
            browserTreeView.LineColor = Color.White;

            browserListView.BackColor = Color.FromArgb(0x40, 0x40, 0x40);
            browserListView.ForeColor = Color.White;

            panel1.BackColor = Color.FromArgb(0x40, 0x40, 0x40);

            button1.BackColor = SystemColors.ButtonFace;
            button2.BackColor = SystemColors.ButtonFace;

            label1.ForeColor = Color.White;
            label2.ForeColor = Color.White;
            checkBox1.ForeColor = Color.White;*/

			imageList1.Images.Add("", Icon);
			Bitmap folderIcon = Win32Icons.GetStockIcon(Win32Icons.SHSTOCKICONID.Folder, false);
			imageList1.Images.Add("", folderIcon);

			for (int i = 0; i < 60; i++)
			{
				try
				{
					string specialFolderName = Environment.GetFolderPath((Environment.SpecialFolder)i);

					DirectoryInfo directoryInfo = new DirectoryInfo(specialFolderName);

					_specialFolders.Add(specialFolderName);
					imageList1.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(specialFolderName, false));
				}
				catch (Exception)
				{
				}
			}

			TreeNode rootNode;

			DriveInfo[] driveInfos = DriveInfo.GetDrives();
			foreach (DriveInfo driveInfo in driveInfos)
			{
				DirectoryInfo directoryInfo = new DirectoryInfo(driveInfo.Name);
				if (!imageList1.Images.ContainsKey(directoryInfo.Name))
				{
					_specialFolders.Add(directoryInfo.Name);
					imageList1.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(directoryInfo.Name, false));
				}
				rootNode = CreateTreeNode(directoryInfo);
				browserTreeView.Nodes.Add(rootNode);
			}

			browserTreeView.EndUpdate();

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
				TreeNodeCollection expandNodeCollection = browserTreeView.Nodes;
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
				browserTreeView.CollapseAll();
			}

			browserTreeView.SelectedNode = expandNode;
		}

		private void LoadResourceDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (DialogResult != DialogResult.OK)
			{
				return;
			}

			if (ActiveControl != okButton)
			{
				e.Cancel = true;
				DialogResult = DialogResult.None;

				return;
			}

			try
			{
				if (dataFileTextBox.Text == "")
				{
					MessageBox.Show(
						"Data file not selected.",
						"File not selected",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation
					);

					e.Cancel = true;
					return;
				}

				if (!File.Exists(dataFileTextBox.Text))
				{
					MessageBox.Show(
						"Data file \"" + dataFileTextBox.Text + "\" not found.\r\nPlease select another option.",
						"File not found",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation
					);

					e.Cancel = true;
					return;
				}

				FileNode textureFileNode = (FileNode)textureFileComboBox.SelectedItem;
				if (textureFileComboBox.Enabled && !textureFileNode.FileExists)
				{
					MessageBox.Show(
						"Texture file \"" + dataFileTextBox.Text + "\" not found.\r\nPlease select another option.",
						"File not found",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation
					);

					e.Cancel = true;
					return;
				}

				FileNode objectListFileNode = (FileNode)objectListFileComboBox.SelectedItem;
				if (objectListFileComboBox.Enabled && !objectListFileNode.FileExists)
				{
					MessageBox.Show(
						"Object list file \"" + objectListFileNode.FileName + "\" not found.\r\nPlease select another option.",
						"File not found",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation
					);

					e.Cancel = true;
					return;
				}

				DataFile = dataFileTextBox.Text;
				TextureFile = textureFileNode.FileName;
				ObjectListFile = objectListFileNode.FileName;
				ClearLoadedFiles = clearLoadedFilesCheckBox.Checked;
			}
			catch (Exception)
			{
				MessageBox.Show(
					"Unknown error",
					"Error",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error
				);

				DialogResult = DialogResult.Cancel;
			}
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

		private TreeNode CreateTreeNode(DirectoryInfo directoryInfo)
		{
			TreeNode treeNode = new TreeNode(directoryInfo.Name, 1, 1);
			treeNode.Name = directoryInfo.Name;
			treeNode.Tag = directoryInfo;

			if (_specialFolders.Contains(directoryInfo.FullName))
			{
				treeNode.ImageKey = directoryInfo.Name;
				treeNode.SelectedImageKey = directoryInfo.Name;
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

		private void UpdateGameType()
		{
			GameType = (CDC.Game)gameTypeComboBox.SelectedIndex;

			platformComboBox.Items.Clear();

			if (GameType == CDC.Game.Gex)
			{
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PSX));
			}
			else if (GameType == CDC.Game.SR1)
			{
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PC));
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PSX));
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.Dreamcast));
			}
			else if (GameType == CDC.Game.SR2 || GameType == CDC.Game.Defiance)
			{
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PC));
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PlayStation2));
			}
			else
			{
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PC));
			}

			platformComboBox.SelectedIndex = 0;

			// UpdateSelections will be called automatically by setting the selected index.
		}

		private void UpdatePlatform()
		{
			Platform = ((PlatformNode)platformComboBox.SelectedItem).Platform;

			UpdateSelections();
		}

		private void UpdateSelections()
		{
			if (browserListView.SelectedItems.Count > 0)
			{
				if (browserListView.SelectedItems[0].Tag is FileInfo)
				{
					FileInfo fileInfo = (FileInfo)browserListView.SelectedItems[0].Tag;

					dataFileTextBox.Text = fileInfo.FullName;

					textureFileComboBox.Items.Clear();
					textureFileComboBox.Enabled = false;
					textureFileComboBox.SelectedIndex = -1;

					objectListFileComboBox.Items.Clear();
					objectListFileComboBox.Enabled = false;
					objectListFileComboBox.SelectedIndex = -1;

					string rootDirectory = Path.GetDirectoryName(fileInfo.FullName);
					string rootFolderName = "";
					switch (GameType)
					{
						case CDC.Game.Gex: rootFolderName = "g3"; break;
						case CDC.Game.SR1: rootFolderName = "kain2"; break;
						case CDC.Game.SR2: rootFolderName = "pcenglish"; break;
						case CDC.Game.Defiance: rootFolderName = "pcenglish"; break;
						case CDC.Game.TRL: rootFolderName = "pc-w"; break;
						default: break;
					}

					bool foundRoot = false;
					while (rootDirectory != null && rootDirectory != "")
					{
						string parentDirectory = Path.GetFileName(rootDirectory);
						rootDirectory = Path.GetDirectoryName(rootDirectory);
						if (parentDirectory == rootFolderName)
						{
							foundRoot = true;
							break;
						}
					}

					if (foundRoot)
					{
						projectFolderTextBox.Text = rootDirectory;
					}
					else
					{
						rootDirectory = "";
						projectFolderTextBox.Text = "(NOT FOUND)";
					}

					#region Textures
					string textureFileName = fileInfo.FullName;
					switch (GameType)
					{
						case CDC.Game.SR1:
						{
							if (Platform == CDC.Platform.PSX)
							{
								textureFileName = Path.ChangeExtension(fileInfo.FullName, "crm");
								textureFileComboBox.Items.Add(new FileNode(textureFileName, true));
							}
							else if (Platform == CDC.Platform.PC)
							{
								textureFileName = Path.Combine(rootDirectory, "textures.big");
								textureFileComboBox.Items.Add(new FileNode(textureFileName, foundRoot));
								textureFileName = Path.Combine(fileInfo.DirectoryName, "textures.big");
								textureFileComboBox.Items.Add(new FileNode(textureFileName, true));
								textureFileComboBox.Enabled = true;
							}
							else if (Platform == CDC.Platform.Dreamcast)
							{
								textureFileName = Path.Combine(rootDirectory, "textures.vq");
								textureFileComboBox.Items.Add(new FileNode(textureFileName, foundRoot));
								textureFileName = Path.Combine(fileInfo.DirectoryName, "textures.vq");
								textureFileComboBox.Items.Add(new FileNode(textureFileName, true));
								textureFileComboBox.Enabled = true;
							}
							break;
						}
						case CDC.Game.Gex:
						case CDC.Game.SR2:
						case CDC.Game.Defiance:
						{
							textureFileName = Path.ChangeExtension(fileInfo.FullName, "vrm");
							textureFileComboBox.Items.Add(new FileNode(textureFileName, true));
							textureFileComboBox.Enabled = true;
							break;
						}
						case CDC.Game.TRL:
						{
							textureFileComboBox.Items.Add(new FileNode(textureFileName, true));
							break;
						}
						default:
							break;
					}

					textureFileComboBox.SelectedIndex = 0;
					#endregion

					#region Object List
					string objectListFileName = fileInfo.FullName;
					if (GameType == CDC.Game.Defiance || GameType == CDC.Game.TRL)
					{
						if (foundRoot)
						{
							string gameFolderName = (GameType == CDC.Game.Defiance) ? "sr3" : "tr7";
							objectListFileName = Path.Combine(rootDirectory, gameFolderName, rootFolderName, "objectlist.txt");
						}
						else
						{
							objectListFileName = "objectlist.txt";
						}
						objectListFileComboBox.Items.Add(new FileNode(objectListFileName, foundRoot));
						objectListFileName = Path.Combine(fileInfo.DirectoryName, "objectlist.txt");
						objectListFileComboBox.Items.Add(new FileNode(objectListFileName, true));
						objectListFileComboBox.Enabled = true;
					}
					else
					{
						objectListFileComboBox.Items.Add(new FileNode(objectListFileName, true));
					}

					objectListFileComboBox.SelectedIndex = 0;
					#endregion
				}
				else
				{
					dataFileTextBox.Text = "";
					projectFolderTextBox.Text = "";
					textureFileComboBox.Items.Clear();
					objectListFileComboBox.Items.Clear();
				}
			}
			else
			{
				dataFileTextBox.Text = "";
				projectFolderTextBox.Text = "";
				textureFileComboBox.Items.Clear();
				objectListFileComboBox.Items.Clear();
			}
		}

		private void UpdateBrowserListView(DirectoryInfo nodeDirInfo)
		{
			if (nodeDirInfo == null)
			{
				return;
			}

			browserListView.Items.Clear();

			ListViewItem.ListViewSubItem[] subItems;
			ListViewItem item;

			DirectoryInfo[] subDirectoryInfos = GetSubDirectories(nodeDirInfo);
			if (subDirectoryInfos.Length > 0)
			{
				foreach (DirectoryInfo dir in nodeDirInfo.GetDirectories())
				{
					string folderImageKey = "";
					if (_specialFolders.Contains(dir.FullName))
					{
						item = new ListViewItem(dir.Name, dir.Name);
					}
					else
					{
						item = new ListViewItem(dir.Name, 1);
					}

					item.Tag = dir;
					subItems = new ListViewItem.ListViewSubItem[]
					{
						new ListViewItem.ListViewSubItem(item, "Directory"),
						new ListViewItem.ListViewSubItem(item, dir.LastAccessTime.ToShortDateString())
					};
					item.SubItems.AddRange(subItems);
					browserListView.Items.Add(item);
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
					browserListView.Items.Add(item);
				}
			}

			browserListView.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		private void hrowserTreeView_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
		{
			if (e.Node.Tag != null)
			{
				UpdateBrowserListView((DirectoryInfo)e.Node.Tag);
			}
		}

		private void browserTreeView_AfterCollapse(object sender, TreeViewEventArgs e)
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

		private void browserTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
		{
			System.Diagnostics.Debug.WriteLine("BeforeExpand");
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

		private void hrowserTreeView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;

				TreeView treeView = (TreeView)sender;
				if (treeView.SelectedNode != null)
				{
					UpdateBrowserListView((DirectoryInfo)treeView.SelectedNode.Tag);
				}
			}
		}

		private void browserListView_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			UpdateSelections();
		}

		private void browserListView_ItemActivate(object sender, EventArgs e)
		{
			ListView listView = (ListView)sender;
			if (listView.SelectedItems.Count > 0)
			{
				ListViewItem listViewItem = listView.SelectedItems[0];
				if (listViewItem.Tag != null && listViewItem.Tag is DirectoryInfo)
				{
					UpdateBrowserListView((DirectoryInfo)listViewItem.Tag);
				}
			}
		}

		private void browserListView_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				e.Handled = true;
				e.SuppressKeyPress = true;

				ListView listView = (ListView)sender;
				if (listView.SelectedItems.Count > 0)
				{
					ListViewItem listViewItem = listView.SelectedItems[0];
					if (listViewItem.Tag != null && listViewItem.Tag is DirectoryInfo)
					{
						UpdateBrowserListView((DirectoryInfo)listViewItem.Tag);
					}
				}
			}
		}

		private void platformComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdatePlatform();
		}

		private void gameTypeComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateGameType();
		}
	}
}
