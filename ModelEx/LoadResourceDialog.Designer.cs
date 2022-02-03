
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
			this.panel1 = new System.Windows.Forms.Panel();
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
			this.splitContainer1 = new System.Windows.Forms.SplitContainer();
			this.hrowserTreeView = new System.Windows.Forms.TreeView();
			this.imageList1 = new System.Windows.Forms.ImageList(this.components);
			this.browserListView = new System.Windows.Forms.ListView();
			this.nameHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.typeHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.lastModifiedHeader = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.panel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.platformComboBox);
			this.panel1.Controls.Add(this.objectListFileLabel);
			this.panel1.Controls.Add(this.objectListFileComboBox);
			this.panel1.Controls.Add(this.projectFolderLabel);
			this.panel1.Controls.Add(this.textureFileComboBox);
			this.panel1.Controls.Add(this.clearLoadedFilesCheckBox);
			this.panel1.Controls.Add(this.projectFolderTextBox);
			this.panel1.Controls.Add(this.textureFileLabel);
			this.panel1.Controls.Add(this.dataFileTextBox);
			this.panel1.Controls.Add(this.dataFileLabel);
			this.panel1.Controls.Add(this.cancelButton);
			this.panel1.Controls.Add(this.okButton);
			this.panel1.Controls.Add(this.gameTypeComboBox);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.panel1.Location = new System.Drawing.Point(0, 362);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(800, 110);
			this.panel1.TabIndex = 0;
			// 
			// platformComboBox
			// 
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
			this.objectListFileLabel.AutoSize = true;
			this.objectListFileLabel.Location = new System.Drawing.Point(14, 87);
			this.objectListFileLabel.Name = "objectListFileLabel";
			this.objectListFileLabel.Size = new System.Drawing.Size(79, 13);
			this.objectListFileLabel.TabIndex = 13;
			this.objectListFileLabel.Text = "Object List File:";
			// 
			// objectListFileComboBox
			// 
			this.objectListFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.objectListFileComboBox.FormattingEnabled = true;
			this.objectListFileComboBox.Location = new System.Drawing.Point(94, 84);
			this.objectListFileComboBox.Name = "objectListFileComboBox";
			this.objectListFileComboBox.Size = new System.Drawing.Size(514, 21);
			this.objectListFileComboBox.TabIndex = 12;
			// 
			// projectFolderLabel
			// 
			this.projectFolderLabel.AutoSize = true;
			this.projectFolderLabel.Location = new System.Drawing.Point(14, 9);
			this.projectFolderLabel.Name = "projectFolderLabel";
			this.projectFolderLabel.Size = new System.Drawing.Size(75, 13);
			this.projectFolderLabel.TabIndex = 11;
			this.projectFolderLabel.Text = "Project Folder:";
			// 
			// textureFileComboBox
			// 
			this.textureFileComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.textureFileComboBox.FormattingEnabled = true;
			this.textureFileComboBox.Location = new System.Drawing.Point(94, 58);
			this.textureFileComboBox.Name = "textureFileComboBox";
			this.textureFileComboBox.Size = new System.Drawing.Size(514, 21);
			this.textureFileComboBox.TabIndex = 10;
			// 
			// clearLoadedFilesCheckBox
			// 
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
			this.projectFolderTextBox.Location = new System.Drawing.Point(94, 6);
			this.projectFolderTextBox.Name = "projectFolderTextBox";
			this.projectFolderTextBox.ReadOnly = true;
			this.projectFolderTextBox.Size = new System.Drawing.Size(514, 20);
			this.projectFolderTextBox.TabIndex = 7;
			// 
			// textureFileLabel
			// 
			this.textureFileLabel.AutoSize = true;
			this.textureFileLabel.Location = new System.Drawing.Point(14, 61);
			this.textureFileLabel.Name = "textureFileLabel";
			this.textureFileLabel.Size = new System.Drawing.Size(65, 13);
			this.textureFileLabel.TabIndex = 6;
			this.textureFileLabel.Text = "Texture File:";
			// 
			// dataFileTextBox
			// 
			this.dataFileTextBox.Location = new System.Drawing.Point(94, 32);
			this.dataFileTextBox.Name = "dataFileTextBox";
			this.dataFileTextBox.ReadOnly = true;
			this.dataFileTextBox.Size = new System.Drawing.Size(514, 20);
			this.dataFileTextBox.TabIndex = 5;
			// 
			// dataFileLabel
			// 
			this.dataFileLabel.AutoSize = true;
			this.dataFileLabel.Location = new System.Drawing.Point(14, 35);
			this.dataFileLabel.Name = "dataFileLabel";
			this.dataFileLabel.Size = new System.Drawing.Size(52, 13);
			this.dataFileLabel.TabIndex = 4;
			this.dataFileLabel.Text = "Data File:";
			// 
			// cancelButton
			// 
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
			this.gameTypeComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.gameTypeComboBox.FormattingEnabled = true;
			this.gameTypeComboBox.Location = new System.Drawing.Point(614, 32);
			this.gameTypeComboBox.Name = "gameTypeComboBox";
			this.gameTypeComboBox.Size = new System.Drawing.Size(174, 21);
			this.gameTypeComboBox.TabIndex = 1;
			this.gameTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.gameTypeComboBox_SelectedIndexChanged);
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = new System.Drawing.Point(0, 0);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.hrowserTreeView);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.browserListView);
			this.splitContainer1.Size = new System.Drawing.Size(800, 362);
			this.splitContainer1.SplitterDistance = 266;
			this.splitContainer1.TabIndex = 1;
			// 
			// hrowserTreeView
			// 
			this.hrowserTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.hrowserTreeView.ImageIndex = 0;
			this.hrowserTreeView.ImageList = this.imageList1;
			this.hrowserTreeView.Location = new System.Drawing.Point(0, 0);
			this.hrowserTreeView.Name = "hrowserTreeView";
			this.hrowserTreeView.SelectedImageIndex = 0;
			this.hrowserTreeView.Size = new System.Drawing.Size(266, 362);
			this.hrowserTreeView.TabIndex = 0;
			this.hrowserTreeView.AfterCollapse += new System.Windows.Forms.TreeViewEventHandler(this.browserTreeView_AfterCollapse);
			this.hrowserTreeView.BeforeExpand += new System.Windows.Forms.TreeViewCancelEventHandler(this.browserTreeView_BeforeExpand);
			this.hrowserTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.browserTreeView_AfterSelect);
			// 
			// imageList1
			// 
			this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
			this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
			this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
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
			this.browserListView.Size = new System.Drawing.Size(530, 362);
			this.browserListView.SmallImageList = this.imageList1;
			this.browserListView.TabIndex = 0;
			this.browserListView.UseCompatibleStateImageBehavior = false;
			this.browserListView.View = System.Windows.Forms.View.Details;
			this.browserListView.ItemActivate += new System.EventHandler(this.browserListView_ItemActivate);
			this.browserListView.SelectedIndexChanged += new System.EventHandler(this.browserListView_SelectedIndexChanged);
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
			// LoadResourceDialog
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.CancelButton = this.cancelButton;
			this.ClientSize = new System.Drawing.Size(800, 472);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.panel1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.MinimumSize = new System.Drawing.Size(300, 200);
			this.Name = "LoadResourceDialog";
			this.ShowInTaskbar = false;
			this.Text = "LoadResourceDialog";
			this.Load += new System.EventHandler(this.LoadResourceDialog_Load);
			this.panel1.ResumeLayout(false);
			this.panel1.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.SplitContainer splitContainer1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.TreeView hrowserTreeView;
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
	}
}