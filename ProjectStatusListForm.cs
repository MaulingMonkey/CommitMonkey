using System.Collections.Generic;
using System.Windows.Forms;
using CommitMonkey.Properties;
using System.Drawing;
using System;
using System.Diagnostics;

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

	[System.ComponentModel.DesignerCategory("")]
	class ProjectStatusListFooter : UserControl {
		readonly ProjectWatcherList Watchers;

		readonly Button[] Buttons;

		public ProjectStatusListFooter( ProjectWatcherList watchers ) {
			Watchers = watchers;

			Button AddWatcher, Close;

			Buttons = new[]
				{ AddWatcher = new Button() { Text = "&Add New Watcher" }
				, Close      = new Button() { Text = "&Close" }
				};
			AddWatcher.Click += AddWatcher_Click;
			Close.Click      += Close_Click;

			foreach ( var button in Buttons ) Controls.Add(button);
		}

		protected override void Dispose(bool disposing) {
			if ( disposing ) {
				foreach ( var button in Buttons ) button.Dispose();
			}
			base.Dispose(disposing);
		}

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);

			int margin = 3;
			int totalmargins = margin*(Buttons.Length-1);
			int buttonwidth  = (Width-totalmargins)/Buttons.Length;
			int buttonstride = buttonwidth+margin;

			for ( int i = 0 ; i < Buttons.Length ; ++i ) {
				Buttons[i].Left = buttonstride*i;
				Buttons[i].Width = buttonwidth;
				Buttons[i].Height = Height;
			}

			Invalidate();
			foreach ( var button in Buttons ) button.Invalidate();
		}

		void AddWatcher_Click(object sender, EventArgs e) {
			Watchers.PromptWatch();
		}

		void Close_Click(object sender, EventArgs e) {
			ParentForm.Close();
		}
	}

	[System.ComponentModel.DesignerCategory("")]
	class ProjectStatusListForm : Form {
		readonly ProjectWatcherList Watchers;
		readonly List<ProjectStatusLineControl> Lines = new List<ProjectStatusLineControl>();
		readonly ProjectStatusListFooter Footer;

		public ProjectStatusListForm( ProjectWatcherList watchers ) {
			Watchers = watchers;
			Watchers.WatcherAdded         += Watchers_WatcherAdded;
			Watchers.WatcherRemoved       += Watchers_WatcherRemoved;
			Watchers.WatcherDirtyChanged  += Watcher_IsDirtyChanged;

			Text            = "Project Watch List";
			FormBorderStyle = FormBorderStyle.FixedSingle;
			StartPosition   = FormStartPosition.CenterScreen;

			Controls.Add( Footer = new ProjectStatusListFooter(Watchers)
				{ Top    = ClientSize.Height-23
				, Left   = 3
				, Width  = ClientSize.Width-6
				, Height = 20
				});

			ClientSize = new Size( 300, 26 );

			foreach ( var watcher in Watchers ) Watchers_WatcherAdded(watcher);
		}

		void Watchers_WatcherAdded(IProjectWatcher watcher) {
			int top = (Lines.Count == 0 ? 0 : Lines[Lines.Count-1].Bottom) + 3;
			var ctrl = new ProjectStatusLineControl(watcher)
				{ Top = top
				, Left = 3
				};
			Lines.Add(ctrl);
			Controls.Add(ctrl);
			ClientSize = new Size( 300, ((Lines.Count == 0) ? 0 : Lines[Lines.Count-1].Bottom) + 26 );
		}

		void Watchers_WatcherRemoved(IProjectWatcher watcher) {
			var ctrl = Lines.Find( (line) => line.Watcher == watcher );
			Lines.Remove(ctrl);
			Controls.Remove(ctrl);

			// Relocate removed elements:
			int nexttop = 0;
			foreach ( var line in Lines ) {
				line.Top = nexttop;
				nexttop = line.Bottom + 3;
			}
		}

		protected override void Dispose(bool disposing) {
			if ( disposing ) {
				foreach ( var line in Lines ) line.Dispose();
				Watchers.WatcherDirtyChanged -= Watcher_IsDirtyChanged;
			}
			base.Dispose(disposing);
		}

		protected override void OnResize(EventArgs e) {
			base.OnResize(e);

			Footer.Width = ClientSize.Width-6;
			Footer.Top = ClientSize.Height-23;
			Footer.Invalidate();
			Invalidate();
		}

		void Watcher_IsDirtyChanged( IProjectWatcher watcher ) {
			ProjectStatusLineControl previous = null;

			foreach ( var line in Lines ) {
				line.Top = ((previous==null) ? 0 : previous.Bottom) + 3;
			}
		}
	}
}
