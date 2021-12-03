namespace ModelEx
{
	partial class RenderControl
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
			this.DebugTextLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// DebugTextLabel
			// 
			this.DebugTextLabel.AutoSize = true;
			this.DebugTextLabel.BackColor = System.Drawing.Color.Silver;
			this.DebugTextLabel.ForeColor = System.Drawing.Color.Black;
			this.DebugTextLabel.Location = new System.Drawing.Point(15, 15);
			this.DebugTextLabel.Name = "DebugTextLabel";
			this.DebugTextLabel.Size = new System.Drawing.Size(0, 13);
			this.DebugTextLabel.TabIndex = 0;
			// 
			// RenderControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(128)))), ((int)(((byte)(0)))));
			this.Controls.Add(this.DebugTextLabel);
			this.Name = "RenderControl";
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.RenderControl_KeyDown);
			this.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.RenderControl_KeyPress);
			this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.RenderControl_KeyUp);
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.RenderControl_MouseDown);
			this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.RenderControl_MouseMove);
			this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.RenderControl_MouseUp);
			this.Resize += new System.EventHandler(this.RenderControl_Resize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label DebugTextLabel;
	}
}
