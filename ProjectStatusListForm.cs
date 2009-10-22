using System.Collections.Generic;
using System.Windows.Forms;
using CommitMonkey.Properties;
using System.Drawing;
using System;

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
			Watcher_IsDirtyChanged();
		}

		protected override void Dispose(bool disposing) {
			if ( disposing ) {
				Watcher.IsDirtyChanged -= Watcher_IsDirtyChanged;
				_StatusIcon.Dispose();
				_PathLabel.Dispose();
			}
			base.Dispose(disposing);
		}

		void Watcher_IsDirtyChanged() {
			_StatusIcon.Image = Program.GetStatusIconFor(Watcher);
			UpdateControlPositions();
		}

		void UpdateControlPositions() {
			Height = Math.Max( _StatusIcon.Height, _PathLabel.Height );
			_StatusIcon.Top = (Height-_StatusIcon.Height)/2;
			_PathLabel .Top = (Height-_PathLabel .Height)/2;
			_PathLabel.Left = _StatusIcon.Right + 3;
		}
	}

	[System.ComponentModel.DesignerCategory("")]
	class ProjectStatusListForm : Form {
		readonly Program Program;
		readonly List<ProjectStatusLineControl> Lines = new List<ProjectStatusLineControl>();

		public ProjectStatusListForm( Program program ) {
			Program = program;

			foreach ( var watcher in Program.Watchers ) {
				watcher.IsDirtyChanged += Watcher_IsDirtyChanged;
				int top = (Lines.Count == 0 ? 0 : Lines[Lines.Count-1].Bottom) + 3;

				var line = new ProjectStatusLineControl(watcher)
					{ Top  = top
					, Left = 3
					};
				Controls.Add(line);
				Lines.Add(line);
			}
		}

		protected override void Dispose(bool disposing) {
			if ( disposing ) {
				foreach ( var line in Lines ) {
					line.Watcher.IsDirtyChanged -= Watcher_IsDirtyChanged;
					line.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		void Watcher_IsDirtyChanged() {
			ProjectStatusLineControl previous = null;

			foreach ( var line in Lines ) {
				line.Top = ((previous==null) ? 0 : previous.Bottom) + 3;
			}
		}
	}
}
