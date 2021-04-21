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
    public partial class ObjectSelectWindow : Form
    {
        public int SelectedObject
        {
            get
            {
                return _objectList.SelectedIndex > 0 ? _objectList.SelectedIndex - 1 : -1;
            }
        }

        public ObjectSelectWindow()
        {
            InitializeComponent();
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
        }

        public void SetObjectNames(string[] objectNames)
        {
            _objectList.Items.Clear();
            _objectList.Items.Add("[Terrain]");
            _objectList.Items.AddRange(objectNames);
            _objectList.SelectedIndex = 0;
        }
    }
}
