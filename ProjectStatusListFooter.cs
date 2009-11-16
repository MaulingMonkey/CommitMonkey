using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace CommitMonkey {
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
}
