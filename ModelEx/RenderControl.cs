using System;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class RenderControl : UserControl
	{
		public RenderControl()
		{
			InitializeComponent();
			this.MouseWheel += new MouseEventHandler(RenderControl_MouseWheel);
			FrameCounter.Instance.FPSCalculatedEvent += new FrameCounter.FPSCalculatedHandler(Instance_FPSCalculatedEvent);
		}

		delegate void setFPS(string fps);
		void Instance_FPSCalculatedEvent(string fps)
		{
			if (this.InvokeRequired)
			{
				setFPS d = new setFPS(Instance_FPSCalculatedEvent);
				this.Invoke(d, new object[] { fps });
			}
			else
			{
				this.DebugTextLabel.Text = fps;
			}
		}

		public void Initialize()
		{
			DeviceManager.Instance.Initialize(this);
			ShaderManager.Instance.Initialize();
			FontManager.Instance.Initialize();
			RenderManager.Instance.Initialize();

			Timer.Instance.Reset();
			Timer.Instance.Start();
		}

		public void ShutDown()
		{
			RenderManager.Instance.ShutDown();
			FontManager.Instance.ShutDown();
			ShaderManager.Instance.ShutDown();
			DeviceManager.Instance.ShutDown();
		}

		private void RenderControl_MouseUp(object sender, MouseEventArgs e)
		{
			CameraManager.Instance.currentCamera.MouseUp(sender, e);
		}

		private void RenderControl_MouseDown(object sender, MouseEventArgs e)
		{
			CameraManager.Instance.currentCamera.MouseDown(sender, e);
		}

		private void RenderControl_MouseMove(object sender, MouseEventArgs e)
		{
			CameraManager.Instance.currentCamera.MouseMove(sender, e);
		}

		private void RenderControl_MouseWheel(object sender, MouseEventArgs e)
		{
			CameraManager.Instance.currentCamera.MouseWheel(sender, e);
		}

		private void RenderControl_KeyUp(object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.F1)
			{
				RenderManager.Instance.SwitchSyncInterval();
			}

			CameraManager.Instance.currentCamera.KeyUp(sender, e);
		}

		private void RenderControl_KeyPress(object sender, KeyPressEventArgs e)
		{
			CameraManager.Instance.currentCamera.KeyPress(sender, e);
		}

		private void RenderControl_KeyDown(object sender, KeyEventArgs e)
		{
			CameraManager.Instance.currentCamera.KeyDown(sender, e);
		}

		private void RenderControl_Resize(object sender, EventArgs e)
		{
			RenderManager.Instance.Resize = true;
		}
	}
}
