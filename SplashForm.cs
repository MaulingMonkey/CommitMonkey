using System.Drawing;
using System.Windows.Forms;
using CommitMonkey.Properties;

namespace CommitMonkey {
	[System.ComponentModel.DesignerCategory("")]
	class SplashForm : Form {
		public SplashForm() {
			BackgroundImage = Resources.CommitMonkeySplash;
			ClientSize      = BackgroundImage.Size;
			FormBorderStyle = FormBorderStyle.None;
			Icon            = Icon.FromHandle( Resources.CommitMonkey.GetHicon() );
			StartPosition   = FormStartPosition.CenterScreen;
		}
	}
}
