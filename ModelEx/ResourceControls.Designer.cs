
namespace ModelEx
{
    partial class ResourceControls
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResourceControls));
            this.container = new System.Windows.Forms.SplitContainer();
            this.refreshButton = new System.Windows.Forms.Button();
            this.buttonIcons = new System.Windows.Forms.ImageList(this.components);
            this.commonControls = new ModelEx.CommonControls();
            this.resourceLabel = new System.Windows.Forms.Label();
            this.resourceCombo = new System.Windows.Forms.ComboBox();
            this.resourceTree = new ModelEx.SceneTreeView();
            this.refreshToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.container)).BeginInit();
            this.container.Panel1.SuspendLayout();
            this.container.Panel2.SuspendLayout();
            this.container.SuspendLayout();
            this.SuspendLayout();
            // 
            // container
            // 
            this.container.Dock = System.Windows.Forms.DockStyle.Fill;
            this.container.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.container.Location = new System.Drawing.Point(0, 0);
            this.container.Name = "container";
            this.container.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // container.Panel1
            // 
            this.container.Panel1.Controls.Add(this.refreshButton);
            this.container.Panel1.Controls.Add(this.commonControls);
            this.container.Panel1.Controls.Add(this.resourceLabel);
            this.container.Panel1.Controls.Add(this.resourceCombo);
            // 
            // container.Panel2
            // 
            this.container.Panel2.Controls.Add(this.resourceTree);
            this.container.Size = new System.Drawing.Size(253, 579);
            this.container.SplitterDistance = 98;
            this.container.TabIndex = 0;
            // 
            // refreshButton
            // 
            this.refreshButton.ImageKey = "RefreshIconImage";
            this.refreshButton.ImageList = this.buttonIcons;
            this.refreshButton.Location = new System.Drawing.Point(228, 2);
            this.refreshButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(21, 21);
            this.refreshButton.TabIndex = 20;
            this.refreshToolTip.SetToolTip(this.refreshButton, "Reload Resource");
            this.refreshButton.UseVisualStyleBackColor = true;
            this.refreshButton.Click += new System.EventHandler(this.refreshButton_Click);
            // 
            // buttonIcons
            // 
            this.buttonIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("buttonIcons.ImageStream")));
            this.buttonIcons.TransparentColor = System.Drawing.Color.Transparent;
            this.buttonIcons.Images.SetKeyName(0, "RefreshIconImage");
            this.buttonIcons.Images.SetKeyName(1, "RefreshIconDisabledImage");
            // 
            // commonControls
            // 
            this.commonControls.BackColor = System.Drawing.Color.Transparent;
            this.commonControls.Location = new System.Drawing.Point(0, 30);
            this.commonControls.Name = "commonControls";
            this.commonControls.Size = new System.Drawing.Size(253, 75);
            this.commonControls.TabIndex = 19;
            // 
            // resourceLabel
            // 
            this.resourceLabel.AutoSize = true;
            this.resourceLabel.Location = new System.Drawing.Point(3, 3);
            this.resourceLabel.Name = "resourceLabel";
            this.resourceLabel.Size = new System.Drawing.Size(75, 13);
            this.resourceLabel.TabIndex = 18;
            this.resourceLabel.Text = "Current Scene";
            // 
            // resourceCombo
            // 
            this.resourceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.resourceCombo.FormattingEnabled = true;
            this.resourceCombo.Location = new System.Drawing.Point(84, 3);
            this.resourceCombo.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.resourceCombo.Name = "resourceCombo";
            this.resourceCombo.Size = new System.Drawing.Size(145, 21);
            this.resourceCombo.TabIndex = 17;
            // 
            // resourceTree
            // 
            this.resourceTree.CheckBoxes = true;
            this.resourceTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceTree.Location = new System.Drawing.Point(0, 0);
            this.resourceTree.Name = "resourceTree";
            this.resourceTree.Size = new System.Drawing.Size(253, 477);
            this.resourceTree.TabIndex = 4;
            // 
            // ResourceControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.container);
            this.Name = "ResourceControls";
            this.Size = new System.Drawing.Size(253, 579);
            this.container.Panel1.ResumeLayout(false);
            this.container.Panel1.PerformLayout();
            this.container.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.container)).EndInit();
            this.container.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer container;
        private CommonControls commonControls;
        private System.Windows.Forms.Label resourceLabel;
        private System.Windows.Forms.ComboBox resourceCombo;
        private SceneTreeView resourceTree;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.ImageList buttonIcons;
        private System.Windows.Forms.ToolTip refreshToolTip;
    }
}
