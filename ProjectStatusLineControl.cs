using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace CommitMonkey {
	[System.ComponentModel.DesignerCategory("")]
	class ProjectStatusLineControl : UserControl {
		public readonly IProjectWatcher Watcher;

		readonly PictureBox _StatusIcon;
		readonly Label      _PathLabel;

		public ProjectStatusLineControl( IProjectWatcher watcher ) {
			Watcher = watcher;
			ClientSize = new Size(400,300);
			var icon = Program.GetStatusIconFor(Watcher);
			Controls.Add( _StatusIcon = new PictureBox()
				{ ClientSize = icon.Size
				, Image      = icon
				, SizeMode   = PictureBoxSizeMode.AutoSize
				});
			Controls.Add( _PathLabel = new Label()
				{ AutoSize   = true
				, Text       = Watcher.Path.Replace("&","&&")
				, TextAlign  = ContentAlignment.MiddleLeft
				});
			UpdateControlPositions();
			Watcher.IsDirtyChanged += Watcher_IsDirtyChanged;
			Watcher_IsDirtyChanged(Watcher);
		}

		protected override void Dispose(bool disposing) {
			if ( disposing ) {
				Watcher.IsDirtyChanged -= Watcher_IsDirtyChanged;
				_StatusIcon.Dispose();
				_PathLabel.Dispose();
			}
			base.Dispose(disposing);
		}

		void Watcher_IsDirtyChanged( IProjectWatcher watcher ) {
			Debug.Assert( Watcher == watcher );
			_StatusIcon.Image = Program.GetStatusIconFor(Watcher);
			UpdateControlPositions();
		}

		void UpdateControlPositions() {
			Height = Math.Max( _StatusIcon.Height, _PathLabel.Height );
			_StatusIcon.Top = (Height-_StatusIcon.Height)/2;
			_PathLabel .Top = (Height-_PathLabel .Height)/2;
			_PathLabel.Left = _StatusIcon.Right + 3;
			Width = _PathLabel.Right;
		}
	}
}
