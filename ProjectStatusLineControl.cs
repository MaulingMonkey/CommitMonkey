using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;
using CommitMonkey.Properties;

namespace CommitMonkey {
	[System.ComponentModel.DesignerCategory("")]
	class ProjectStatusLineControl : UserControl {
		public readonly IProjectWatcher Watcher;

		readonly PictureBox _StatusIcon;
		readonly Button     _Button;
		readonly Label      _PathLabel;

		public ProjectStatusLineControl( IProjectWatcher watcher, EventHandler toremove ) {
			Watcher = watcher;
			ClientSize = new Size(400,300);
			var icon = Program.GetStatusIconFor(Watcher);
			Controls.Add( _StatusIcon = new PictureBox()
				{ ClientSize = icon.Size
				, Location   = new Point(0,0)
				, Image      = icon
				, SizeMode   = PictureBoxSizeMode.AutoSize
				});
			Controls.Add( _Button = new Button()
				{ ClientSize = Resources.Eraser.Size
				, Location   = new Point(19,0)
				, Image      = Resources.Eraser
				, FlatStyle  = FlatStyle.Flat
				});
			_Button.FlatAppearance.BorderSize = 0;
			_Button.Click += toremove;
			Controls.Add( _PathLabel = new Label()
				{ AutoSize   = true
				, ClientSize = new Size(100,16)
				, Location   = new Point(38,0)
				, TextAlign  = ContentAlignment.MiddleLeft
				, Text       = Watcher.Path.Replace("&","&&")
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
			Height = Max( _StatusIcon.Height, _Button.Height, _PathLabel.Height );
			_StatusIcon.Top = (Height-_StatusIcon.Height)/2;
			_Button.Top     = (Height-_Button    .Height)/2;
			_PathLabel .Top = (Height-_PathLabel .Height)/2;
			Width = _PathLabel.Right;
		}

		static int Max( params int[] p ) {
			int i = p[0];
			foreach ( int i2 in p ) i = Math.Max(i,i2);
			return i;
		}
	}
}
