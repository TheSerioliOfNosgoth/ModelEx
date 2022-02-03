﻿using System;
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
		public string ObjectIDFile { get; private set; } = "";
		public CDC.Game GameType { get; private set; } = CDC.Game.Gex;
		public CDC.Platform Platform { get; private set; } = CDC.Platform.PC;
		public bool ClearLoadedFiles { get; private set; } = false;

		private static readonly string _folderImageKey = "WINDOWS";

		private readonly List<string> _specialFolders = new List<string>();

		class PlatformNode
		{
			public readonly CDC.Platform Platform = CDC.Platform.None;

			public PlatformNode(CDC.Platform platform)
			{
				Platform = platform;
			}

			public override string ToString()
			{
				switch (Platform)
				{
					default:
					case CDC.Platform.None: return "None";
					case CDC.Platform.PC: return "PC";
					case CDC.Platform.PSX: return "PlayStation";
					case CDC.Platform.PlayStation2: return "PlayStation 2";
					case CDC.Platform.Dreamcast: return "Dreamcast";
					case CDC.Platform.Xbox: return "XBox";
				}
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
			hrowserTreeView.BeginUpdate();

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
				rootNode = CreateTreeNode(directoryInfo);
				hrowserTreeView.Nodes.Add(rootNode);
			}

			hrowserTreeView.EndUpdate();

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
				TreeNodeCollection expandNodeCollection = hrowserTreeView.Nodes;
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
				hrowserTreeView.CollapseAll();
			}

			hrowserTreeView.SelectedNode = expandNode;
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
								if (!File.Exists(textureFileName))
								{
									textureFileName += " (NOT FOUND)";
								}
								textureFileComboBox.Items.Add(textureFileName);
							}
							else if (Platform == CDC.Platform.PC)
							{
								textureFileName = Path.Combine(rootDirectory, "textures.big");
								if (!foundRoot || !File.Exists(textureFileName))
								{
									textureFileName += " (NOT FOUND)";
								}
								textureFileComboBox.Items.Add(textureFileName);
								textureFileName = Path.Combine(fileInfo.DirectoryName, "textures.big");
								if (!File.Exists(textureFileName))
								{
									textureFileName += " (NOT FOUND)";
								}
								textureFileComboBox.Items.Add(textureFileName);
								textureFileComboBox.Enabled = true;
							}
							else if (Platform == CDC.Platform.Dreamcast)
							{
								textureFileName = Path.Combine(rootDirectory, "textures.vq");
								if (!foundRoot || !File.Exists(textureFileName))
								{
									textureFileName += " (NOT FOUND)";
								}
								textureFileComboBox.Items.Add(textureFileName);
								textureFileName = Path.Combine(fileInfo.DirectoryName, "textures.vq");
								if (!File.Exists(textureFileName))
								{
									textureFileName += " (NOT FOUND)";
								}
								textureFileComboBox.Items.Add(textureFileName);
								textureFileComboBox.Enabled = true;
							}
							break;
						}
						case CDC.Game.Gex:
						case CDC.Game.SR2:
						case CDC.Game.Defiance:
						{
							textureFileName = Path.ChangeExtension(fileInfo.FullName, "vrm");
							textureFileComboBox.Items.Add(textureFileName);
							break;
						}
						case CDC.Game.TRL:
						{
							textureFileComboBox.Items.Add(textureFileName);
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
						string gameFolderName = (GameType == CDC.Game.Defiance) ? "sr3" : "tr7";
						objectListFileName = Path.Combine(rootDirectory, gameFolderName, rootFolderName, "objectlist.txt");
						if (!File.Exists(objectListFileName))
						{
							objectListFileName += " (NOT FOUND)";
						}
						objectListFileComboBox.Items.Add(objectListFileName);
						objectListFileName = Path.Combine(fileInfo.DirectoryName, "objectlist.txt");
						if (!File.Exists(objectListFileName))
						{
							objectListFileName += " (NOT FOUND)";
						}
						objectListFileComboBox.Items.Add(objectListFileName);
						objectListFileComboBox.Enabled = true;
					}
					else
					{
						objectListFileComboBox.Items.Add(objectListFileName);
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

		private void browserTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			UpdateBrowserListView((DirectoryInfo)e?.Node?.Tag);
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
