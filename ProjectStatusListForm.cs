using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CommitMonkey {
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
				previous = line;
			}
		}
	}
}
