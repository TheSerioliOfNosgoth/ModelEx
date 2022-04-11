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
    public partial class ResourceControls : UserControl
    {
        public SplitContainer SContainer { get { return container; } }
        public CommonControls CommonControls { get { return commonControls; } }
        public Label ResourceLabel { get { return resourceLabel; } }
        public ComboBox ResourceCombo { get { return resourceCombo; } }
        public SceneTreeView ResourceTree { get { return resourceTree; } }

        public event EventHandler SelectedResourceChanged
        {
            add { resourceCombo.SelectedIndexChanged  += value; }
            remove { resourceCombo.SelectedIndexChanged -= value;  }
        }

        public event TreeViewEventHandler AfterResourceNodeCheck
        {
            add { resourceTree.AfterCheck  += value; }
            remove { resourceTree.AfterCheck -= value; }
        }

        public ResourceControls()
        {
            InitializeComponent();
        }
    }
}
