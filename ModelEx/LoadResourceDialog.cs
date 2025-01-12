using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Security.AccessControl;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class LoadResourceDialog : Form
	{
		public string SelectedFolder { get; set; } = "";
		public string DataFile { get; private set; } = "";
		public string ProjectFolder { get; private set; } = "";
		public string TexturesFolder { get; private set; } = "";
		public string ObjectListFolder { get; private set; } = "";
		public CDC.Game SelectedGameType { get; set; } = CDC.Game.Gex;
		public CDC.Platform SelectedPlatform { get; set; } = CDC.Platform.PC;
		public bool ClearLoadedFiles { get; set; } = false;

		private DirectoryInfo _currentDirectory;
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
					case CDC.Platform.Remaster: PlatformName = "Remaster"; break;
				}
			}

			public override string ToString()
			{
				return PlatformName;
			}
		}

		class FileNode
		{
			public readonly string FolderName;
			public readonly bool FolderExists;
			public readonly List<string> FileNames;
			public readonly bool FilesExist;
			public readonly string Description;

			public FileNode(string folderName, params string[] fileNames)
			{
				FolderName = folderName;
				FolderExists = folderName != null && folderName != "" && Directory.Exists(folderName);
				FileNames = new List<string>(fileNames);
				FilesExist = true;
				Description = (FolderExists) ? FolderName : "(No Project Folder)";

				for (int i = 0; i < FileNames.Count; i++)
				{
					if (FileNames[i] == null)
					{
						FileNames[i] = "";
					}

					if (i == 0)
					{
						Description += " - (";
					}
					else
					{
						Description += ", ";
					}

					Description += FileNames[i];

					if ((i + 1) == FileNames.Count)
					{
						Description += ")";
					}

					// Don't check if the file exists if there's no folder to look in,
					// but still set FilesExist as false.
					if (!FolderExists || !File.Exists(Path.Combine(FolderName, FileNames[i])))
					{
						FilesExist = false;
					}
				}

				// Only show the Files Missing message if the folder was found.
				if (FolderExists && !FilesExist)
				{
					Description += " - (Files Missing)";
				}
			}

			public string GetFilePath(int index)
			{
				return Path.Combine(FolderName, FileNames[index]);
			}

			public override string ToString()
			{
				return Description;
			}
		}

		public LoadResourceDialog()
		{
			InitializeComponent();

			gameTypeComboBox.Items.Add("Gex 3 Files(*.drm)");
			gameTypeComboBox.Items.Add("Soul Reaver 1 Files (*.drm)");
			gameTypeComboBox.Items.Add("Soul Reaver 2 Files (*.drm)");
			gameTypeComboBox.Items.Add("Defiance Files (*.drm)");
			gameTypeComboBox.Items.Add("Tomb Raider Legend Files(*.drm)");
			gameTypeComboBox.Items.Add("Tomb Raider Anniversary Files(*.drm)");
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

			fileFolderImageList.Images.Add("", Icon);
			Bitmap folderIcon = Win32Icons.GetStockIcon(Win32Icons.SHSTOCKICONID.Folder, false);
			fileFolderImageList.Images.Add("", folderIcon);

			for (int i = 0; i < 60; i++)
			{
				try
				{
					string specialFolderName = Environment.GetFolderPath((Environment.SpecialFolder)i);

					DirectoryInfo directoryInfo = new DirectoryInfo(specialFolderName);

					_specialFolders.Add(specialFolderName);
					fileFolderImageList.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(specialFolderName, false));
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
				if (!fileFolderImageList.Images.ContainsKey(directoryInfo.Name))
				{
					_specialFolders.Add(directoryInfo.Name);
					fileFolderImageList.Images.Add(directoryInfo.Name, Win32Icons.GetDirectoryIcon(directoryInfo.Name, false));
				}
				rootNode = CreateTreeNode(directoryInfo);
				browserTreeView.Nodes.Add(rootNode);
			}

			browserTreeView.EndUpdate();

			List<DirectoryInfo> breadCrumbs = new List<DirectoryInfo>();
			try
			{
				/*DirectoryInfo initialDirectoryInfo = new DirectoryInfo(InitialDirectory);
				DirectoryInfo expandDirectory = initialDirectoryInfo;
				while (expandDirectory != null)
				{
					breadCrumbs.Insert(0, expandDirectory);
					expandDirectory = expandDirectory.Parent;
				}*/

				recentLocationsComboBox.Items.Add(SelectedFolder);
				recentLocationsComboBox.SelectedIndex = 0;
			}
			catch (Exception)
			{
				breadCrumbs.Clear();
			}

			CDC.Platform selectedPlatform = SelectedPlatform;
			gameTypeComboBox.SelectedIndex = (int)SelectedGameType;
			for (int platformIndex = 0; platformIndex < platformComboBox.Items.Count; platformIndex++)
            {
				if (((PlatformNode)platformComboBox.Items[platformIndex]).Platform == selectedPlatform)
                {
					platformComboBox.SelectedIndex = platformIndex;
					break;
                }
            }

			clearLoadedFilesCheckBox.Checked = ClearLoadedFiles;
		}

		private void LoadResourceDialog_FormClosing(object sender, FormClosingEventArgs e)
		{
			if (_currentDirectory != null)
			{
				SelectedFolder = _currentDirectory.FullName;
			}
			else
			{
				SelectedFolder = "";
			}

			ClearLoadedFiles = clearLoadedFilesCheckBox.Checked;

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
				if (!textureFileNode.FilesExist)
				{
					string textureFileList = "";
					for (int i = 0; i < textureFileNode.FileNames.Count; i++)
					{
						if (!File.Exists(textureFileNode.GetFilePath(i)))
						{
							textureFileList += "\r\n" + textureFileNode.FileNames[i];
						}
					}

					MessageBox.Show(
						"Texture file(s) not found. Please select another option." + textureFileList,
						"File(s) not found",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation
					);

					e.Cancel = true;
					return;
				}

				FileNode objectListFileNode = (FileNode)objectListFileComboBox.SelectedItem;
				if (!objectListFileNode.FilesExist)
				{
					string objectListFile = "";
					if (objectListFileNode.FileNames.Count > 0)
					{
						if (!File.Exists(objectListFileNode.GetFilePath(0)))
						{
							objectListFile += "\r\n" + objectListFileNode.FileNames[0];
						}
					}

					MessageBox.Show(
						"Object list file(s) not found. Please select another option." + objectListFile,
						"File(s) not found",
						MessageBoxButtons.OK,
						MessageBoxIcon.Exclamation
					);

					e.Cancel = true;
					return;
				}

				DataFile = dataFileTextBox.Text;
				ProjectFolder = projectFolderTextBox.Text;
				TexturesFolder = textureFileNode.FolderName;
				ObjectListFolder = objectListFileNode.FolderName;
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
			SelectedGameType = (CDC.Game)gameTypeComboBox.SelectedIndex;

			platformComboBox.Items.Clear();

			if (SelectedGameType == CDC.Game.Gex)
			{
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PSX));
			}
			else if (SelectedGameType == CDC.Game.SR1)
			{
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PC));
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.PSX));
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.Dreamcast));
				platformComboBox.Items.Add(new PlatformNode(CDC.Platform.Remaster));
			}
			else if (SelectedGameType == CDC.Game.SR2 || SelectedGameType == CDC.Game.Defiance)
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
			SelectedPlatform = ((PlatformNode)platformComboBox.SelectedItem).Platform;

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
					string fallbackDirectory = "";
					string rootFolderName = "";
					switch (SelectedGameType)
					{
						case CDC.Game.Gex: rootFolderName = "g3"; break;
						case CDC.Game.SR1: rootFolderName = "kain2"; break;
						case CDC.Game.SR2: rootFolderName = "pcenglish"; break;
						case CDC.Game.Defiance: rootFolderName = "pcenglish"; break;
						case CDC.Game.TRL: rootFolderName = "pc-w"; break;
						case CDC.Game.TRA: rootFolderName = "pc-w"; break;
						default: break;
					}

					bool foundRoot = false;
					bool foundFallback = false;
					while (rootDirectory != null && rootDirectory != "")
					{
						string parentDirectory = Path.GetFileName(rootDirectory);
						rootDirectory = Path.GetDirectoryName(rootDirectory);
						if (parentDirectory.ToLower() == rootFolderName)
						{
							foundRoot = true;
							break;
						}

						if (SelectedGameType == CDC.Game.SR1 && SelectedPlatform == CDC.Platform.Remaster && !foundFallback &&
							(parentDirectory.ToLower() == "area" || parentDirectory.ToLower() == "object"))
						{
							fallbackDirectory = rootDirectory;
							foundFallback = true;
						}
					}

					if (foundRoot)
					{
						ProjectFolder = rootDirectory;
						projectFolderTextBox.Text = rootDirectory;
					}
					else if (foundFallback)
					{
						rootDirectory = fallbackDirectory;
						ProjectFolder = rootDirectory;
						projectFolderTextBox.Text = rootDirectory;
					}
					else
					{
						rootDirectory = "";
						ProjectFolder = "";
						projectFolderTextBox.Text = "[No Project Folder]";
					}

					#region Textures
					switch (SelectedGameType)
					{
						case CDC.Game.SR1:
						{
							if (SelectedPlatform == CDC.Platform.PSX)
							{
								string textureFileName = Path.ChangeExtension(fileInfo.Name, "crm");
								textureFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, textureFileName));
							}
							else if (SelectedPlatform == CDC.Platform.PC)
							{
								string textureFileName = "textures.big";
								string outputDirectory = Path.Combine(rootDirectory, "output");
								textureFileComboBox.Items.Add(new FileNode(rootDirectory, textureFileName));
								textureFileComboBox.Items.Add(new FileNode(outputDirectory, textureFileName));
								textureFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, textureFileName));
								textureFileComboBox.Enabled = true;
							}
							else if (SelectedPlatform == CDC.Platform.Dreamcast)
							{
								string textureFileName = "textures.vq";
								textureFileComboBox.Items.Add(new FileNode(rootDirectory, textureFileName));
								textureFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, textureFileName));
								textureFileComboBox.Enabled = true;
							}
							else if (SelectedPlatform == CDC.Platform.Remaster)
							{
								string tex0 = "TEXTURES.BIG";
								string tex1 = "MOVIES.BIG";
								string tex2 = "BONUS.BIG";
								textureFileComboBox.Items.Add(new FileNode(rootDirectory, tex0, tex1, tex2));
								textureFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, tex0, tex1, tex2));
								textureFileComboBox.Enabled = true;
							}
							break;
						}
						case CDC.Game.Gex:
						case CDC.Game.SR2:
						case CDC.Game.Defiance:
						{
							string textureFileName = Path.ChangeExtension(fileInfo.Name, "vrm");
							textureFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, textureFileName));
							break;
						}
						case CDC.Game.TRL:
						case CDC.Game.TRA:
						{
							string textureFileName = fileInfo.Name;
							textureFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, textureFileName));
							break;
						}
						default:
							break;
					}

					textureFileComboBox.SelectedIndex = 0;
					#endregion

					#region Object List
					if (SelectedGameType == CDC.Game.Defiance || SelectedGameType == CDC.Game.TRL || SelectedGameType == CDC.Game.TRA)
					{
						string objectListDirectoryName = "";
						if (foundRoot)
						{
							string gameFolderName = "";
							switch (SelectedGameType)
							{
								case CDC.Game.Defiance:
									gameFolderName = "sr3";
									break;
								case CDC.Game.TRL:
									gameFolderName = "tr7";
									break;
								case CDC.Game.TRA:
									gameFolderName = "trae";
									break;
							}
							objectListDirectoryName = Path.Combine(rootDirectory, gameFolderName, rootFolderName);
						}
						objectListFileComboBox.Items.Add(new FileNode(objectListDirectoryName, "objectlist.txt"));
						objectListFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, "objectlist.txt"));
						objectListFileComboBox.Enabled = true;
					}
					else
					{
						objectListFileComboBox.Items.Add(new FileNode(fileInfo.DirectoryName, fileInfo.Name));
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
					ProjectFolder = "";
				}
			}
			else
			{
				dataFileTextBox.Text = "";
				projectFolderTextBox.Text = "";
				textureFileComboBox.Items.Clear();
				objectListFileComboBox.Items.Clear();
				ProjectFolder = "";
			}
		}

		private void UpdateBrowserListView(DirectoryInfo directoryInfo)
		{
			_currentDirectory = directoryInfo;
			browserListView.Items.Clear();

			if (directoryInfo == null)
			{
				recentLocationsComboBox.Text = "";
				return;
			}

			recentLocationsComboBox.Text = directoryInfo.FullName;

			ListViewItem.ListViewSubItem[] subItems;
			ListViewItem item;

			DirectoryInfo[] subDirectoryInfos = GetSubDirectories(directoryInfo);
			if (subDirectoryInfos.Length > 0)
			{
				foreach (DirectoryInfo dir in directoryInfo.GetDirectories())
				{
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

			FileInfo[] fileInfos = GetFiles(directoryInfo);
			if (fileInfos.Length > 0)
			{
				foreach (FileInfo file in directoryInfo.GetFiles())
				{
					string extension = file.Extension.ToLower();
					if (extension != ".pcm" && extension != ".drm")
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

		private void browserTreeView_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
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

		private void browserTreeView_KeyDown(object sender, KeyEventArgs e)
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

		private void navigateUpButton_Click(object sender, EventArgs e)
		{
			UpdateBrowserListView(_currentDirectory?.Parent);
		}

		private void navigateRefreshButton_Click(object sender, EventArgs e)
		{
			UpdateBrowserListView(_currentDirectory);
		}

		private void recentLocationsComboBox_TextChanged(object sender, EventArgs e)
		{
		}

		private void recentLocationsComboBox_Validating(object sender, System.ComponentModel.CancelEventArgs e)
		{
		}

		private void recentLocationsComboBox_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Enter)
			{
				ComboBox comboBox = (ComboBox)sender;
				string folderName = comboBox.Text;
				try
				{
					if (!Directory.Exists(folderName))
					{
						throw new DirectoryNotFoundException();
					}

					int currentIndex = comboBox.Items.IndexOf(folderName);
					if (currentIndex >= 0)
					{
						comboBox.Items.RemoveAt(currentIndex);
					}

					comboBox.Items.Insert(0, folderName);
					comboBox.SelectedIndex = 0;
				}
				catch (Exception)
				{
				}

				e.SuppressKeyPress = true;
			}
		}

		private void recentLocationsComboBox_SelectedIndexChanged(object sender, EventArgs e)
		{
			ComboBox comboBox = (ComboBox)sender;
			DirectoryInfo directoryInfo = new DirectoryInfo(comboBox.Text);
			UpdateBrowserListView(directoryInfo);
		}
	}
}
