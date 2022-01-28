
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
			this.bgColourPanel = new System.Windows.Forms.Panel();
			this.realmBlendBar = new System.Windows.Forms.TrackBar();
			this.planeBlendLabel = new System.Windows.Forms.Label();
			this.bgColourLabel = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.realmBlendBar)).BeginInit();
			this.SuspendLayout();
			// 
			// bgColourPanel
			// 
			this.bgColourPanel.BackColor = System.Drawing.Color.Gray;
			this.bgColourPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.bgColourPanel.Location = new System.Drawing.Point(101, 3);
			this.bgColourPanel.Name = "bgColourPanel";
			this.bgColourPanel.Size = new System.Drawing.Size(24, 21);
			this.bgColourPanel.TabIndex = 10;
			this.bgColourPanel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.bgColorPanel_MouseClick);
			// 
			// realmBlendBar
			// 
			this.realmBlendBar.LargeChange = 1;
			this.realmBlendBar.Location = new System.Drawing.Point(3, 45);
			this.realmBlendBar.Maximum = 100;
			this.realmBlendBar.Name = "realmBlendBar";
			this.realmBlendBar.Size = new System.Drawing.Size(244, 45);
			this.realmBlendBar.TabIndex = 8;
			this.realmBlendBar.TickStyle = System.Windows.Forms.TickStyle.None;
			this.realmBlendBar.Scroll += new System.EventHandler(this.realmBlendBar_Scroll);
			// 
			// planeBlendLabel
			// 
			this.planeBlendLabel.AutoSize = true;
			this.planeBlendLabel.Location = new System.Drawing.Point(3, 28);
			this.planeBlendLabel.Name = "planeBlendLabel";
			this.planeBlendLabel.Size = new System.Drawing.Size(64, 13);
			this.planeBlendLabel.TabIndex = 7;
			this.planeBlendLabel.Text = "Plane Blend";
			// 
			// bgColourLabel
			// 
			this.bgColourLabel.AutoSize = true;
			this.bgColourLabel.Location = new System.Drawing.Point(3, 3);
			this.bgColourLabel.Name = "bgColourLabel";
			this.bgColourLabel.Size = new System.Drawing.Size(92, 13);
			this.bgColourLabel.TabIndex = 9;
			this.bgColourLabel.Text = "Background Color";
			// 
			// CommonControls
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.Transparent;
			this.Controls.Add(this.bgColourPanel);
			this.Controls.Add(this.realmBlendBar);
			this.Controls.Add(this.planeBlendLabel);
			this.Controls.Add(this.bgColourLabel);
			this.Name = "CommonControls";
			this.Size = new System.Drawing.Size(253, 75);
			((System.ComponentModel.ISupportInitialize)(this.realmBlendBar)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel bgColourPanel;
		private System.Windows.Forms.TrackBar realmBlendBar;
		private System.Windows.Forms.Label planeBlendLabel;
		private System.Windows.Forms.Label bgColourLabel;
	}
}
