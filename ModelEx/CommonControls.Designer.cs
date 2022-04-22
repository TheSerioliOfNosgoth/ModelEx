
namespace ModelEx
{
	partial class CommonControls
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommonControls));
            this.bgColourPanel = new System.Windows.Forms.Panel();
            this.realmBlendBar = new System.Windows.Forms.TrackBar();
            this.planeBlendLabel = new System.Windows.Forms.Label();
            this.bgColourLabel = new System.Windows.Forms.Label();
            this.refreshButton = new System.Windows.Forms.Button();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.resourceLabel = new System.Windows.Forms.Label();
            this.resourceCombo = new System.Windows.Forms.ComboBox();
            this.refreshToolTip = new System.Windows.Forms.ToolTip(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.realmBlendBar)).BeginInit();
            this.SuspendLayout();
            // 
            // bgColourPanel
            // 
            this.bgColourPanel.BackColor = System.Drawing.Color.Gray;
            this.bgColourPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.bgColourPanel.Location = new System.Drawing.Point(101, 31);
            this.bgColourPanel.Name = "bgColourPanel";
            this.bgColourPanel.Size = new System.Drawing.Size(24, 21);
            this.bgColourPanel.TabIndex = 10;
            this.bgColourPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bgColorPanel_MouseClick);
            // 
            // realmBlendBar
            // 
            this.realmBlendBar.LargeChange = 1;
            this.realmBlendBar.Location = new System.Drawing.Point(3, 76);
            this.realmBlendBar.Maximum = 100;
            this.realmBlendBar.Name = "realmBlendBar";
            this.realmBlendBar.Size = new System.Drawing.Size(250, 45);
            this.realmBlendBar.TabIndex = 8;
            this.realmBlendBar.TickStyle = System.Windows.Forms.TickStyle.None;
            this.realmBlendBar.Scroll += new System.EventHandler(this.realmBlendBar_Scroll);
            // 
            // planeBlendLabel
            // 
            this.planeBlendLabel.AutoSize = true;
            this.planeBlendLabel.Location = new System.Drawing.Point(3, 59);
            this.planeBlendLabel.Name = "planeBlendLabel";
            this.planeBlendLabel.Size = new System.Drawing.Size(64, 13);
            this.planeBlendLabel.TabIndex = 7;
            this.planeBlendLabel.Text = "Plane Blend";
            // 
            // bgColourLabel
            // 
            this.bgColourLabel.AutoSize = true;
            this.bgColourLabel.Location = new System.Drawing.Point(3, 31);
            this.bgColourLabel.Name = "bgColourLabel";
            this.bgColourLabel.Size = new System.Drawing.Size(92, 13);
            this.bgColourLabel.TabIndex = 9;
            this.bgColourLabel.Text = "Background Color";
            // 
            // refreshButton
            // 
            this.refreshButton.ImageKey = "RefreshIconImage";
            this.refreshButton.ImageList = this.imageList1;
            this.refreshButton.Location = new System.Drawing.Point(232, 3);
            this.refreshButton.Margin = new System.Windows.Forms.Padding(0, 3, 3, 3);
            this.refreshButton.Name = "refreshButton";
            this.refreshButton.Size = new System.Drawing.Size(21, 21);
            this.refreshButton.TabIndex = 23;
            this.refreshButton.UseVisualStyleBackColor = true;
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "RefreshIconImage");
            this.imageList1.Images.SetKeyName(1, "RefreshIconDisabledImage");
            // 
            // resourceLabel
            // 
            this.resourceLabel.AutoSize = true;
            this.resourceLabel.Location = new System.Drawing.Point(3, 3);
            this.resourceLabel.Name = "resourceLabel";
            this.resourceLabel.Size = new System.Drawing.Size(75, 13);
            this.resourceLabel.TabIndex = 22;
            this.resourceLabel.Text = "Current Scene";
            // 
            // resourceCombo
            // 
            this.resourceCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.resourceCombo.FormattingEnabled = true;
            this.resourceCombo.Location = new System.Drawing.Point(84, 3);
            this.resourceCombo.Margin = new System.Windows.Forms.Padding(3, 3, 0, 3);
            this.resourceCombo.Name = "resourceCombo";
            this.resourceCombo.Size = new System.Drawing.Size(148, 21);
            this.resourceCombo.TabIndex = 21;
            // 
            // CommonControls
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.refreshButton);
            this.Controls.Add(this.resourceLabel);
            this.Controls.Add(this.resourceCombo);
            this.Controls.Add(this.bgColourPanel);
            this.Controls.Add(this.realmBlendBar);
            this.Controls.Add(this.planeBlendLabel);
            this.Controls.Add(this.bgColourLabel);
            this.Name = "CommonControls";
            this.Size = new System.Drawing.Size(253, 104);
            ((System.ComponentModel.ISupportInitialize)(this.realmBlendBar)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel bgColourPanel;
		private System.Windows.Forms.TrackBar realmBlendBar;
		private System.Windows.Forms.Label planeBlendLabel;
		private System.Windows.Forms.Label bgColourLabel;
        private System.Windows.Forms.Button refreshButton;
        private System.Windows.Forms.Label resourceLabel;
        private System.Windows.Forms.ComboBox resourceCombo;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolTip refreshToolTip;
    }
}
