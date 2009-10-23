using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommitMonkey.Properties;

namespace CommitMonkey {
	class Program : IDisposable {
		readonly Form       Splash;
		readonly NotifyIcon NotifyIcon;

		Bitmap _icon;
		Bitmap DisplayIcon { get {
			return _icon;
		} set {
			if ( _icon != null ) DestroyIcon( NotifyIcon.Icon.Handle );
			_icon = value;
			NotifyIcon.Icon = Icon.FromHandle( _icon.GetHicon() );
		}}

		Program() {
			Splash = new SplashForm();
			NotifyIcon = new NotifyIcon()
				{ ContextMenu = new ContextMenu( new[]
					{ new MenuItem( "Watch..." , (s,a) => PromptWatch() )
					, new MenuItem( "Show List", (s,a) => ShowProjectStatusList() )
					, new MenuItem( "-" )
					, new MenuItem( "E&xit"    , (s,a) => Application.Exit() )
					})
				, Text    = "CommitMonkey"
				, Visible = true
				};
			DisplayIcon = Resources.CommitMonkey;
		}

		public void Dispose() {
			DestroyIcon( NotifyIcon.Icon.Handle );
			NotifyIcon.Dispose();
		}
		
		readonly List<IProjectWatcherFactory> WatcherTypes = new List<IProjectWatcherFactory>()
			{ new GitProjectWatcherFactory()
			};
		readonly List<IProjectWatcher>        Watchers     = new List<IProjectWatcher>();

		public static Bitmap GetStatusIconFor( IProjectWatcher watcher ) {
			return watcher.IsDirty
				? Resources.CommitMonkeyAlert
				: Resources.CommitMonkey
				;
		}

		void UpdateStatus() {
			DisplayIcon = Watchers.Any( (watcher) => watcher.IsDirty )
				? Resources.CommitMonkeyAlert
				: Resources.CommitMonkey
				;
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

		void ShowProjectStatusList() {
			var psl = new ProjectStatusListForm(Watchers);
			psl.Show();
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

		public static void Begin( Action a ) {
			Instance.Splash.BeginInvoke(a);
		}
		static Program Instance;
		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			using ( Instance = new Program() ) {
				Instance.Splash.Show();
				Instance.Splash.Hide();
				Application.Run();
			}
		}

		[DllImport("user32.dll", SetLastError=true)] static extern bool DestroyIcon(IntPtr hIcon);
	}
}
