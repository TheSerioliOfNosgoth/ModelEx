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
        public class Bindables : INotifyPropertyChanged
        {
            public int _spectralPercentage = 0;
            public Color _backgroundColour = Color.Gray;
            public int SpectralPercentage { get { return _spectralPercentage; } set { _spectralPercentage = value; OnChanged("SpectralPercentage"); } }
            public Color BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; OnChanged("BackgroundColour"); } }

            public event PropertyChangedEventHandler PropertyChanged;

            public void OnChanged(string propertyName)
            {
                PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
                PropertyChanged?.Invoke(this, e);
            }
        }

        static Bindables _Bindables = new Bindables();

        public SplitContainer SContainer { get { return container; } }
        public CommonControls CommonControls { get { return commonControls; } }
        public Label ResourceLabel { get { return commonControls.ResourceLabel; } }
        public ComboBox ResourceCombo { get { return commonControls.ResourceCombo; } }
        public Button RefreshButton { get { return commonControls.RefreshButton; } }
        public SceneTreeView ResourceTree { get { return resourceTree; } }

        public event EventHandler SelectedResourceChanged
        {
            add { ResourceCombo.SelectedIndexChanged  += value; }
            remove { ResourceCombo.SelectedIndexChanged -= value;  }
        }

        public event TreeViewEventHandler AfterResourceNodeCheck
        {
            add { resourceTree.AfterCheck  += value; }
            remove { resourceTree.AfterCheck -= value; }
        }

        public event EventHandler RefreshClick
        {
            add { RefreshButton.Click  += value; }
            remove { RefreshButton.Click -= value; }
        }

        public ResourceControls()
        {
            InitializeComponent();
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {

        }
    }
}
