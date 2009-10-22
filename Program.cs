using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace CommitMonkey {
	class Program : IDisposable {
		readonly NotifyIcon NotifyIcon;
		readonly Timer      DirtyCheck;

		Bitmap _icon;
		Bitmap DisplayIcon { get {
			return _icon;
		} set {
			if ( _icon != null ) DestroyIcon( NotifyIcon.Icon.Handle );
			_icon = value;
			NotifyIcon.Icon = Icon.FromHandle( _icon.GetHicon() );
		}}

		Program() {
			NotifyIcon = new NotifyIcon()
				{ ContextMenu = new ContextMenu( new[]
					{ new MenuItem( "Watch...", (s,a) => PromptWatch() )
					, new MenuItem( "-" )
					, new MenuItem( "E&xit"   , (s,a) => Application.Exit() )
					})
				, Text    = "AutoVCS"
				, Visible = true
				};
			DisplayIcon = Resources.CommitMonkey;
			DirtyCheck = new Timer()
				{ Interval = 1000
				};
			DirtyCheck.Tick += (s,a) => UpdateStatus();
			DirtyCheck.Start();
		}

		public void Dispose() {
			DestroyIcon( NotifyIcon.Icon.Handle );
			NotifyIcon.Dispose();
		}
		
		readonly List<IProjectWatcherFactory> WatcherTypes = new List<IProjectWatcherFactory>()
			{ new GitProjectWatcherFactory()
			};
		readonly List<IProjectWatcher>        Watchers     = new List<IProjectWatcher>();

		void UpdateStatus() {
			DisplayIcon = Watchers.Any( (watcher) => watcher.IsDirty )
				? Resources.CommitMonkeyAlert
				: Resources.CommitMonkey
				;
			DirtyCheck.Start();
		}

		void Watch( string path ) {
			foreach ( var wtype in WatcherTypes ) {
				IProjectWatcher watcher = wtype.Create(path);
				if ( watcher != null ) {
					watcher.IsDirtyChanged += UpdateStatus;
					Watchers.Add(watcher);
					return;
				}
			}

			throw new Exception
				( "Could not recognize the version control system for\n"
				+ path+"\n"
				+ "\n"
				+ "Are you sure it's under version control?"
				);
		}

		void PromptWatch() {
			using ( var fbd = new FolderBrowserDialog()
				{ Description = "Select a path to watch"
				, RootFolder = System.Environment.SpecialFolder.MyComputer
				})
			{
				for ( DialogResult retryresult = DialogResult.Retry ; retryresult == DialogResult.Retry ; )
				switch ( fbd.ShowDialog() )
				{
				case DialogResult.OK:
					try {
						Watch( fbd.SelectedPath );
						return;
					} catch ( Exception e ) {
						retryresult = MessageBox.Show
							( e.Message
							, "Error watching \""+fbd.SelectedPath+"\""
							, MessageBoxButtons.RetryCancel
							, MessageBoxIcon.Error
							);
					}
					break;
				default:
					return;
				}
			}
		}

		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			using ( var p = new Program() ) Application.Run();
		}

		[DllImport("user32.dll", SetLastError=true)] static extern bool DestroyIcon(IntPtr hIcon);
	}
}
