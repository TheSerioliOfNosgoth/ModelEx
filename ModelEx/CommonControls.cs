using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace ModelEx
{
	public partial class CommonControls : UserControl
	{
		public class Bindables : INotifyPropertyChanged
		{
			public int _spectralPercentage = 0;
			public Color _backgroundColour = Color.Gray;
			public bool _showSpheres = false;
			public int SpectralPercentage { get { return _spectralPercentage; } set { _spectralPercentage = value; OnChanged("SpectralPercentage"); } }
			public Color BackgroundColour { get { return _backgroundColour; } set { _backgroundColour = value; OnChanged("BackgroundColour"); } }
			public bool ShowSpheres {  get { return _showSpheres; } set { _showSpheres = value; OnChanged("ShowSpheres"); } }

			public event PropertyChangedEventHandler PropertyChanged;

			public void OnChanged(string propertyName)
			{
				PropertyChangedEventArgs e = new PropertyChangedEventArgs(propertyName);
				PropertyChanged?.Invoke(this, e);
			}
		}
		static Bindables _Bindables = new Bindables();
		
		public Label ResourceLabel { get { return resourceLabel; } }
		public ComboBox ResourceCombo { get { return resourceCombo; } }

		public Button RefreshButton { get { return refreshButton; } }

		public CommonControls()
		{
			InitializeComponent();

			realmBlendBar.DataBindings.Add("Value", _Bindables, "SpectralPercentage");
			bgColourPanel.DataBindings.Add("BackColor", _Bindables, "BackgroundColour");
			showSpheresCheckBox.DataBindings.Add("Checked", _Bindables, "ShowSpheres");
		}

		private void realmBlendBar_Scroll(object sender, EventArgs e)
		{
			TrackBar trackBar = (TrackBar)sender;
			MeshMorphingUnit.RealmBlend = ((float)trackBar.Value / trackBar.Maximum);
			_Bindables.SpectralPercentage = realmBlendBar.Value;
		}

		private void bgColorPanel_MouseClick(object sender, MouseEventArgs e)
		{
			ColorDialog colorDialog = new ColorDialog();
			colorDialog.AllowFullOpen = true;
			colorDialog.AnyColor = true;
			colorDialog.SolidColorOnly = true;
			colorDialog.FullOpen = true;

			colorDialog.Color = _Bindables.BackgroundColour;
			if (colorDialog.ShowDialog() == DialogResult.OK)
			{
				_Bindables.BackgroundColour = colorDialog.Color;
				RenderManager.Instance.BackgroundColour = colorDialog.Color;
			}
		}

		private void showSpheresCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			CheckBox checkBox = (CheckBox)sender;
			RenderManager.Instance.ShowSpheres = checkBox.Checked;
			_Bindables.ShowSpheres = RenderManager.Instance.ShowSpheres;
		}
	}
}
