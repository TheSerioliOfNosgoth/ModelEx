using System;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class ProgressWindow : Form
	{
		delegate void IntDelegate(int newValue);
		delegate void StringDelegate(string newValue);

		public ProgressWindow()
		{
			InitializeComponent();
		}

		public int GetProgress()
		{
			return pbProgress.Value;
		}

		public void SetProgress(int newProgress)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new IntDelegate(SetProgress), new object[] { newProgress });
					return;
				}
				pbProgress.Value = Math.Max(pbProgress.Minimum, Math.Min(pbProgress.Maximum, newProgress));
			}
			catch (Exception ex)
			{
				// do nothing - this is a non-critical operation
			}
		}

		public void SetMessage(string newMessage)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new StringDelegate(SetMessage), new object[] { newMessage });
					return;
				}
				lblText.Text = newMessage;
			}
			catch (Exception ex)
			{
				// do nothing - this is a non-critical operation
			}
		}

		public void SetTitle(string newMessage)
		{
			try
			{
				if (InvokeRequired)
				{
					BeginInvoke(new StringDelegate(SetTitle), new object[] { newMessage });
					return;
				}
				this.Text = newMessage;
			}
			catch (Exception ex)
			{
				// do nothing - this is a non-critical operation
			}
		}

		public string Title
		{
			get
			{
				return this.Text;
			}
			set
			{
				this.Text = value;
			}
		}
	}
}
