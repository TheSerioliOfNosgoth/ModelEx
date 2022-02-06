
namespace ModelEx
{
	partial class LoadResourceDialog
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoadResourceDialog));
			this.selectionsPanel = new System.Windows.Forms.Panel();
			this.platformComboBox = new System.Windows.Forms.ComboBox();
			this.objectListFileLabel = new System.Windows.Forms.Label();
			this.objectListFileComboBox = new System.Windows.Forms.ComboBox();
			this.projectFolderLabel = new System.Windows.Forms.Label();
			this.textureFileComboBox = new System.Windows.Forms.ComboBox();
			this.clearLoadedFilesCheckBox = new System.Windows.Forms.CheckBox();
			this.projectFolderTextBox = new System.Windows.Forms.TextBox();
			this.textureFileLabel = new System.Windows.Forms.Label();
			this.dataFileTextBox = new System.Windows.Forms.TextBox();
			this.dataFileLabel = new System.Windows.Forms.Label();
			this.cancelButton = new System.Windows.Forms.Button();
			this.okButton = new System.Windows.Forms.Button();
			this.gameTypeComboBox = new System.Windows.Forms.ComboBox();
			this.browserSplitContainer = new System.Windows.Forms.SplitContainer();
			this.fileFolderImageList = new System.Windows.Forms.ImageList(this.components);
			this.browserListView = new System.Windows.Forms.ListView();
			this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.typeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lastModifiedHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.navigationBarPanel = new System.Windows.Forms.Panel();
			this.navigateRefreshButton = new System.Windows.Forms.Button();
			this.recentLocationsComboBox = new System.Windows.Forms.ComboBox();
			this.navigateUpButton = new System.Windows.Forms.Button();
			this.buttonImageList = new System.Windows.Forms.ImageList(this.components);
			this.browserTreeView = new ModelEx.SceneTreeView();
			this.selectionsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.browserSplitContainer)).BeginInit();
			this.browserSplitContainer.Panel1.SuspendLayout();
			this.browserSplitContainer.Panel2.SuspendLayout();
			this.browserSplitContainer.SuspendLayout();
			this.navigationBarPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// selectionsPanel
			// 
			this.selectionsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.selectionsPanel.Controls.Add(this.platformComboBox);
			this.selectionsPanel.Controls.Add(this.objectListFileLabel);
			this.selectionsPanel.Controls.Add(this.objectListFileComboBox);
			this.selectionsPanel.Controls.Add(this.projectFolderLabel);
			this.selectionsPanel.Controls.Add(this.textureFileComboBox);
			this.selectionsPanel.Controls.Add(this.clearLoadedFilesCheckBox);
			this.selectionsPanel.Controls.Add(this.projectFolderTextBox);
			this.selectionsPanel.Controls.Add(this.textureFileLabel);
			this.selectionsPanel.Controls.Add(this.dataFileTextBox);
			this.selectionsPanel.Controls.Add(this.dataFileLabel);
			this.selectionsPanel.Controls.Add(this.cancelButton);
			this.selectionsPanel.Controls.Add(this.okButton);
			this.selectionsPanel.Controls.Add(this.gameTypeComboBox);
			this.selectionsPanel.Location = new System.Drawing.Point(0, 360);
			this.selectionsPanel.Name = "selectionsPanel";
			this.selectionsPanel.Size = new System.Drawing.Size(800, 110);
			this.selectionsPanel.TabIndex = 0;
			// 
			// platformComboBox
			// 
			this.platformComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.platformComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.platformComboBox.FormattingEnabled = true;
			this.platformComboBox.Location = new System.Drawing.Point(614, 58);
			this.platformComboBox.Name = "platformComboBox";
			this.platformComboBox.Size = new System.Drawing.Size(174, 21);
			this.platformComboBox.TabIndex = 14;
			this.platformComboBox.SelectedIndexChanged += new System.EventHandler(this.platformComboBox_SelectedIndexChanged);
			// 
			// objectListFileLabel
			// 
			this.objectListFileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.objectListFileLabel.AutoSize = true;
			this.objectListFileLabel.Location = new System.Drawing.Point(14, 87);
			this.objectListFileLabel.Name = "objectListFileLabel";
			this.objectListFileLabel.Size = new System.Drawing.Size(79, 13);
			this.objectListFileLabel.TabIndex = 13;
			this.objectListFileLabel.Text = "Object List File:";
			// 
			// objectListFileComboBox
			// 
			this.objectListFileComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.objectListFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.objectListFileComboBox.FormattingEnabled = true;
			this.objectListFileComboBox.Location = new System.Drawing.Point(94, 84);
			this.objectListFileComboBox.Name = "objectListFileComboBox";
			this.objectListFileComboBox.Size = new System.Drawing.Size(514, 21);
			this.objectListFileComboBox.TabIndex = 12;
			// 
			// projectFolderLabel
			// 
			this.projectFolderLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.projectFolderLabel.AutoSize = true;
			this.projectFolderLabel.Location = new System.Drawing.Point(14, 9);
			this.projectFolderLabel.Name = "projectFolderLabel";
			this.projectFolderLabel.Size = new System.Drawing.Size(75, 13);
			this.projectFolderLabel.TabIndex = 11;
			this.projectFolderLabel.Text = "Project Folder:";
			// 
			// textureFileComboBox
			// 
			this.textureFileComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.textureFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.textureFileComboBox.FormattingEnabled = true;
			this.textureFileComboBox.Location = new System.Drawing.Point(94, 58);
			this.textureFileComboBox.Name = "textureFileComboBox";
			this.textureFileComboBox.Size = new System.Drawing.Size(514, 21);
			this.textureFileComboBox.TabIndex = 10;
			// 
			// clearLoadedFilesCheckBox
			// 
			this.clearLoadedFilesCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.clearLoadedFilesCheckBox.AutoSize = true;
			this.clearLoadedFilesCheckBox.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.clearLoadedFilesCheckBox.Location = new System.Drawing.Point(614, 6);
			this.clearLoadedFilesCheckBox.Name = "clearLoadedFilesCheckBox";
			this.clearLoadedFilesCheckBox.Size = new System.Drawing.Size(164, 17);
			this.clearLoadedFilesCheckBox.TabIndex = 9;
			this.clearLoadedFilesCheckBox.Text = "Clear Previously Loaded Files";
			this.clearLoadedFilesCheckBox.UseVisualStyleBackColor = true;
			// 
			// projectFolderTextBox
			// 
			this.projectFolderTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.projectFolderTextBox.Location = new System.Drawing.Point(94, 6);
			this.projectFolderTextBox.Name = "projectFolderTextBox";
			this.projectFolderTextBox.ReadOnly = true;
			this.projectFolderTextBox.Size = new System.Drawing.Size(514, 20);
			this.projectFolderTextBox.TabIndex = 7;
			// 
			// textureFileLabel
			// 
			this.textureFileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.textureFileLabel.AutoSize = true;
			this.textureFileLabel.Location = new System.Drawing.Point(14, 61);
			this.textureFileLabel.Name = "textureFileLabel";
			this.textureFileLabel.Size = new System.Drawing.Size(65, 13);
			this.textureFileLabel.TabIndex = 6;
			this.textureFileLabel.Text = "Texture File:";
			// 
			// dataFileTextBox
			// 
			this.dataFileTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.dataFileTextBox.Location = new System.Drawing.Point(94, 32);
			this.dataFileTextBox.Name = "dataFileTextBox";
			this.dataFileTextBox.ReadOnly = true;
			this.dataFileTextBox.Size = new System.Drawing.Size(514, 20);
			this.dataFileTextBox.TabIndex = 5;
			// 
			// dataFileLabel
			// 
			this.dataFileLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.dataFileLabel.AutoSize = true;
			this.dataFileLabel.Location = new System.Drawing.Point(14, 35);
			this.dataFileLabel.Name = "dataFileLabel";
			this.dataFileLabel.Size = new System.Drawing.Size(52, 13);
			this.dataFileLabel.TabIndex = 4;
			this.dataFileLabel.Text = "Data File:";
			// 
			// cancelButton
			// 
			this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = new System.Drawing.Point(704, 82);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.Size = new System.Drawing.Size(84, 23);
			this.cancelButton.TabIndex = 3;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.UseVisualStyleBackColor = true;
			// 
			// okButton
			// 
			this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.okButton.Location = new System.Drawing.Point(614, 82);
			this.okButton.Name = "okButton";
			this.okButton.Size = new System.Drawing.Size(84, 23);
			this.okButton.TabIndex = 2;
			this.okButton.Text = "OK";
			this.okButton.UseVisualStyleBackColor = true;
			// 
			// gameTypeComboBox
			// 
			this.gameTypeComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.gameTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.gameTypeComboBox.FormattingEnabled = true;
			this.gameTypeComboBox.Location = new System.Drawing.Point(614, 32);
			this.gameTypeComboBox.Name = "gameTypeComboBox";
			this.gameTypeComboBox.Size = new System.Drawing.Size(174, 21);
			this.gameTypeComboBox.TabIndex = 1;
			this.gameTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.gameTypeComboBox_SelectedIndexChanged);
			// 
			// browserSplitContainer
			// 
			this.browserSplitContainer.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.browserSplitContainer.Location = new System.Drawing.Point(0, 25);
			this.browserSplitContainer.Name = "browserSplitContainer";
			// 
			// browserSplitContainer.Panel1
			// 
			this.browserSplitContainer.Panel1.Controls.Add(this.browserTreeView);
			// 
			// browserSplitContainer.Panel2
			// 
			this.browserSplitContainer.Panel2.Controls.Add(this.browserListView);
			this.browserSplitContainer.Size = new System.Drawing.Size(800, 339);
			this.browserSplitContainer.SplitterDistance = 266;
			this.browserSplitContainer.TabIndex = 1;
			// 
			// fileFolderImageList
			// 
			this.fileFolderImageList.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.fileFolderImageList.ImageSize = new System.Drawing.Size(16, 16);
			this.fileFolderImageList.TransparentColor = System.Drawing.Color.Transparent;
			// 
			// browserListView
			// 
			this.browserListView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.nameHeader,
            this.typeHeader,
            this.lastModifiedHeader});
			this.browserListView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.browserListView.HideSelection = false;
			this.browserListView.Location = new System.Drawing.Point(0, 0);
			this.browserListView.Name = "browserListView";
			this.browserListView.Size = new System.Drawing.Size(530, 339);
			this.browserListView.SmallImageList = this.fileFolderImageList;
			this.browserListView.TabIndex = 0;
			this.browserListView.UseCompatibleStateImageBehavior = false;
			this.browserListView.View = System.Windows.Forms.View.Details;
			this.browserListView.ItemActivate += new System.EventHandler(this.browserListView_ItemActivate);
			this.browserListView.SelectedIndexChanged += new System.EventHandler(this.browserListView_SelectedIndexChanged);
			this.browserListView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.browserListView_KeyDown);
			// 
			// nameHeader
			// 
			this.nameHeader.Text = "Name";
			this.nameHeader.Width = 300;
			// 
			// typeHeader
			// 
			this.typeHeader.Text = "Type";
			// 
			// lastModifiedHeader
			// 
			this.lastModifiedHeader.Text = "Last Modified";
			this.lastModifiedHeader.Width = 100;
			// 
			// navigationBarPanel
			// 
			this.navigationBarPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.navigationBarPanel.Controls.Add(this.navigateRefreshButton);
			this.navigationBarPanel.Controls.Add(this.recentLocationsComboBox);
			this.navigationBarPanel.Controls.Add(this.navigateUpButton);
			this.navigationBarPanel.Location = new System.Drawing.Point(0, 0);
			this.navigationBarPanel.Name = "navigationBarPanel";
			this.navigationBarPanel.Size = new System.Drawing.Size(800, 25);
			this.navigationBarPanel.TabIndex = 2;
			// 
			// navigateRefreshButton
			// 
			this.navigateRefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.navigateRefreshButton.ImageIndex = 1;
			this.navigateRefreshButton.ImageList = this.buttonImageList;
			this.navigateRefreshButton.Location = new System.Drawing.Point(775, 0);
			this.navigateRefreshButton.Name = "navigateRefreshButton";
			this.navigateRefreshButton.Size = new System.Drawing.Size(25, 25);
			this.navigateRefreshButton.TabIndex = 2;
			this.navigateRefreshButton.UseVisualStyleBackColor = true;
			this.navigateRefreshButton.Click += new System.EventHandler(this.navigateRefreshButton_Click);
			// 
			// recentLocationsComboBox
			// 
			this.recentLocationsComboBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.recentLocationsComboBox.FormattingEnabled = true;
			this.recentLocationsComboBox.Location = new System.Drawing.Point(27, 2);
			this.recentLocationsComboBox.Name = "recentLocationsComboBox";
			this.recentLocationsComboBox.Size = new System.Drawing.Size(745, 21);
			this.recentLocationsComboBox.TabIndex = 1;
			// 
			// navigateUpButton
			// 
			this.navigateUpButton.ImageIndex = 0;
			this.navigateUpButton.ImageList = this.buttonImageList;
			this.navigateUpButton.Location = new System.Drawing.Point(0, 0);
			this.navigateUpButton.Name = "navigateUpButton";
			this.navigateUpButton.Size = new System.Drawing.Size(25, 25);
			this.navigateUpButton.TabIndex = 0;
			this.navigateUpButton.UseVisualStyleBackColor = true;
			this.navigateUpButton.Click += new System.EventHandler(this.navigateUpButton_Click);
			// 
			// buttonImageList
			// 
			this.buttonImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("buttonImageList.ImageStream")));
			this.buttonImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.buttonImageList.Images.SetKeyName(0, "NavigateUpIcon.ico");
			this.buttonImageList.Images.SetKeyName(1, "RefreshIcon.ico");
			// 
			// browserTreeView
			// 
			this.browserTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.browserTreeView.ImageIndex = 0;
			this.browserTreeView.ImageList = this.fileFolderImageList;
			this.browserTreeView.Location = new System.Drawing.Point(0, 0);
			this.browserTreeView.Name = "browserTreeView";
			this.browserTreeView.SelectedImageIndex = 0;
			this.browserTreeView.Size = new System.Drawing.Size(266, 339);
			this.browserTreeView.TabIndex = 0;
			this.browserTreeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.browserTreeView_AfterCollapse);
			this.browserTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.browserTreeView_BeforeExpand);
			this.browserTreeView.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.browserTreeView_NodeMouseDoubleClick);
			this.browserTreeView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.browserTreeView_KeyDown);
			// 
			// LoadResourceDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(800, 471);
			this.Controls.Add(this.navigationBarPanel);
			this.Controls.Add(this.browserSplitContainer);
			this.Controls.Add(this.selectionsPanel);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 200);
			this.Name = "LoadResourceDialog";
			this.ShowInTaskbar = false;
			this.Text = "LoadResourceDialog";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LoadResourceDialog_FormClosing);
			this.Load += new System.EventHandler(this.LoadResourceDialog_Load);
			this.selectionsPanel.ResumeLayout(false);
			this.selectionsPanel.PerformLayout();
			this.browserSplitContainer.Panel1.ResumeLayout(false);
			this.browserSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.browserSplitContainer)).EndInit();
			this.browserSplitContainer.ResumeLayout(false);
			this.navigationBarPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel selectionsPanel;
		private System.Windows.Forms.SplitContainer browserSplitContainer;
		private System.Windows.Forms.ImageList fileFolderImageList;
		private SceneTreeView browserTreeView;
		private System.Windows.Forms.ListView browserListView;
		private System.Windows.Forms.ColumnHeader nameHeader;
		private System.Windows.Forms.ColumnHeader typeHeader;
		private System.Windows.Forms.ColumnHeader lastModifiedHeader;
		private System.Windows.Forms.Label dataFileLabel;
		private System.Windows.Forms.Button cancelButton;
		private System.Windows.Forms.Button okButton;
		private System.Windows.Forms.ComboBox gameTypeComboBox;
		private System.Windows.Forms.TextBox projectFolderTextBox;
		private System.Windows.Forms.Label textureFileLabel;
		private System.Windows.Forms.TextBox dataFileTextBox;
		private System.Windows.Forms.CheckBox clearLoadedFilesCheckBox;
		private System.Windows.Forms.ComboBox textureFileComboBox;
		private System.Windows.Forms.Label projectFolderLabel;
		private System.Windows.Forms.ComboBox objectListFileComboBox;
		private System.Windows.Forms.Label objectListFileLabel;
		private System.Windows.Forms.ComboBox platformComboBox;
		private System.Windows.Forms.Panel navigationBarPanel;
		private System.Windows.Forms.Button navigateUpButton;
		private System.Windows.Forms.ComboBox recentLocationsComboBox;
		private System.Windows.Forms.Button navigateRefreshButton;
		private System.Windows.Forms.ImageList buttonImageList;
	}
}