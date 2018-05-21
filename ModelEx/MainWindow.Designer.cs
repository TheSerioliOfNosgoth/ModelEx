namespace ModelEx
{
    partial class MainWindow
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
            this.menuBar = new System.Windows.Forms.MenuStrip();
            this.fileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetPositionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sceneViewContainer = new System.Windows.Forms.SplitContainer();
            this.sceneTree = new System.Windows.Forms.TreeView();
            this.sceneTreeContainer = new System.Windows.Forms.SplitContainer();
            this.FPSText = new System.Windows.Forms.TextBox();
            this.sceneView = new ModelEx.RenderControl();
            this.modeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orbitPanToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.orbitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.egoToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuBar.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sceneViewContainer)).BeginInit();
            this.sceneViewContainer.Panel1.SuspendLayout();
            this.sceneViewContainer.Panel2.SuspendLayout();
            this.sceneViewContainer.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sceneTreeContainer)).BeginInit();
            this.sceneTreeContainer.Panel2.SuspendLayout();
            this.sceneTreeContainer.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuBar
            // 
            this.menuBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenu,
            this.cameraToolStripMenuItem});
            this.menuBar.Location = new System.Drawing.Point(0, 0);
            this.menuBar.Name = "menuBar";
            this.menuBar.Size = new System.Drawing.Size(684, 24);
            this.menuBar.TabIndex = 3;
            this.menuBar.Text = "menuStrip1";
            // 
            // fileMenu
            // 
            this.fileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.exportToolStripMenuItem,
            this.exitToolStripMenuItem});
            this.fileMenu.Name = "fileMenu";
            this.fileMenu.Size = new System.Drawing.Size(37, 20);
            this.fileMenu.Text = "File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.openToolStripMenuItem.Text = "Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // exportToolStripMenuItem
            // 
            this.exportToolStripMenuItem.Name = "exportToolStripMenuItem";
            this.exportToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.exportToolStripMenuItem.Text = "Export";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            // 
            // cameraToolStripMenuItem
            // 
            this.cameraToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.modeToolStripMenuItem,
            this.resetPositionToolStripMenuItem});
            this.cameraToolStripMenuItem.Name = "cameraToolStripMenuItem";
            this.cameraToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.cameraToolStripMenuItem.Text = "Camera";
            // 
            // resetPositionToolStripMenuItem
            // 
            this.resetPositionToolStripMenuItem.Name = "resetPositionToolStripMenuItem";
            this.resetPositionToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.resetPositionToolStripMenuItem.Text = "Reset Position";
            this.resetPositionToolStripMenuItem.Click += new System.EventHandler(this.resetPositionToolStripMenuItem_Click);
            // 
            // sceneViewContainer
            // 
            this.sceneViewContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneViewContainer.Location = new System.Drawing.Point(0, 24);
            this.sceneViewContainer.Name = "sceneViewContainer";
            // 
            // sceneViewContainer.Panel1
            // 
            this.sceneViewContainer.Panel1.Controls.Add(this.sceneTree);
            // 
            // sceneViewContainer.Panel2
            // 
            this.sceneViewContainer.Panel2.Controls.Add(this.sceneView);
            this.sceneViewContainer.Panel2.Controls.Add(this.sceneTreeContainer);
            this.sceneViewContainer.Size = new System.Drawing.Size(684, 437);
            this.sceneViewContainer.SplitterDistance = 227;
            this.sceneViewContainer.TabIndex = 4;
            // 
            // sceneTree
            // 
            this.sceneTree.CheckBoxes = true;
            this.sceneTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneTree.Location = new System.Drawing.Point(0, 0);
            this.sceneTree.Name = "sceneTree";
            this.sceneTree.Size = new System.Drawing.Size(227, 437);
            this.sceneTree.TabIndex = 0;
            this.sceneTree.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            // 
            // sceneTreeContainer
            // 
            this.sceneTreeContainer.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneTreeContainer.Location = new System.Drawing.Point(0, 0);
            this.sceneTreeContainer.Name = "sceneTreeContainer";
            this.sceneTreeContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // sceneTreeContainer.Panel2
            // 
            this.sceneTreeContainer.Panel2.Controls.Add(this.FPSText);
            this.sceneTreeContainer.Size = new System.Drawing.Size(453, 437);
            this.sceneTreeContainer.SplitterDistance = 316;
            this.sceneTreeContainer.TabIndex = 2;
            // 
            // FPSText
            // 
            this.FPSText.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(192)))));
            this.FPSText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.FPSText.ForeColor = System.Drawing.Color.Yellow;
            this.FPSText.Location = new System.Drawing.Point(0, 0);
            this.FPSText.Multiline = true;
            this.FPSText.Name = "FPSText";
            this.FPSText.ReadOnly = true;
            this.FPSText.Size = new System.Drawing.Size(453, 117);
            this.FPSText.TabIndex = 0;
            this.FPSText.Text = "Testing...\r\nTesting...\r\nTesting...";
            // 
            // sceneView
            // 
            this.sceneView.BackColor = System.Drawing.Color.Gray;
            this.sceneView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sceneView.Location = new System.Drawing.Point(0, 0);
            this.sceneView.Name = "sceneView";
            this.sceneView.Size = new System.Drawing.Size(453, 437);
            this.sceneView.TabIndex = 0;
            // 
            // modeToolStripMenuItem
            // 
            this.modeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.egoToolStripMenuItem,
            this.orbitToolStripMenuItem,
            this.orbitPanToolStripMenuItem});
            this.modeToolStripMenuItem.Name = "modeToolStripMenuItem";
            this.modeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.modeToolStripMenuItem.Text = "Mode";
            // 
            // orbitPanToolStripMenuItem
            // 
            this.orbitPanToolStripMenuItem.Name = "orbitPanToolStripMenuItem";
            this.orbitPanToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.orbitPanToolStripMenuItem.Text = "Orbit Pan";
            this.orbitPanToolStripMenuItem.Click += new System.EventHandler(this.orbitPanToolStripMenuItem_Click);
            // 
            // orbitToolStripMenuItem
            // 
            this.orbitToolStripMenuItem.Name = "orbitToolStripMenuItem";
            this.orbitToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.orbitToolStripMenuItem.Text = "Orbit";
            this.orbitToolStripMenuItem.Click += new System.EventHandler(this.orbitToolStripMenuItem_Click);
            // 
            // egoToolStripMenuItem
            // 
            this.egoToolStripMenuItem.Checked = true;
            this.egoToolStripMenuItem.CheckState = System.Windows.Forms.CheckState.Checked;
            this.egoToolStripMenuItem.Name = "egoToolStripMenuItem";
            this.egoToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.egoToolStripMenuItem.Text = "Ego";
            this.egoToolStripMenuItem.Click += new System.EventHandler(this.egoToolStripMenuItem_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 461);
            this.Controls.Add(this.sceneViewContainer);
            this.Controls.Add(this.menuBar);
            this.Name = "MainWindow";
            this.Text = "ModelEx v5.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuBar.ResumeLayout(false);
            this.menuBar.PerformLayout();
            this.sceneViewContainer.Panel1.ResumeLayout(false);
            this.sceneViewContainer.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.sceneViewContainer)).EndInit();
            this.sceneViewContainer.ResumeLayout(false);
            this.sceneTreeContainer.Panel2.ResumeLayout(false);
            this.sceneTreeContainer.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.sceneTreeContainer)).EndInit();
            this.sceneTreeContainer.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuBar;
        private System.Windows.Forms.ToolStripMenuItem fileMenu;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.SplitContainer sceneViewContainer;
        private System.Windows.Forms.TreeView sceneTree;
        private RenderControl sceneView;
        private System.Windows.Forms.SplitContainer sceneTreeContainer;
        private System.Windows.Forms.TextBox FPSText;
        private System.Windows.Forms.ToolStripMenuItem cameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem resetPositionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem modeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem egoToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orbitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem orbitPanToolStripMenuItem;
    }
}