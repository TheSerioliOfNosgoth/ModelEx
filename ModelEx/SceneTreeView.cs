using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class SceneTreeView : TreeView
	{
		public SceneTreeView()
		{
			InitializeComponent();
		}

		protected override void DefWndProc(ref Message m)
		{
			if (m.Msg == 515) // WM_LBUTTONDBLCLK 
			{
				OnDoubleClick(EventArgs.Empty);
				return;
			}

			base.DefWndProc(ref m);
		}
	}
}
