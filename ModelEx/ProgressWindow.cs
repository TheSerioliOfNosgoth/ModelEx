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
            if (InvokeRequired)
            {
                BeginInvoke(new IntDelegate(SetProgress), new object[] { newProgress });
                return;
            }
            pbProgress.Value = newProgress;
        }

        public void SetMessage(string newMessage)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(SetMessage), new object[] { newMessage });
                return;
            }
            lblText.Text = newMessage;
        }

        public void SetTitle(string newMessage)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new StringDelegate(SetTitle), new object[] { newMessage });
                return;
            }
            this.Text = newMessage;
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
