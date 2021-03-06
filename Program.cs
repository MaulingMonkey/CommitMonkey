﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CommitMonkey.Properties;
using Microsoft.Win32;

namespace CommitMonkey {
	class Program : IDisposable {
		readonly Form       Splash;
		readonly NotifyIcon NotifyIcon;

		public static string ConfigXml { get { return Application.LocalUserAppDataPath+@"\config.xml"; }}

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
					{ new MenuItem( "Configure List", (s,a) => ToggleShowProjectStatusList() )
					, new MenuItem( "-" )
					, new MenuItem( "E&xit"         , (s,a) => Application.Exit() )
					})
				, Text    = "CommitMonkey"
				, Visible = true
				};
			NotifyIcon.DoubleClick += (s,a) => ToggleShowProjectStatusList();
			DisplayIcon = Resources.CommitMonkey;
			Watchers.WatcherDirtyChanged += (w) => UpdateStatus();
			Watchers.WatcherRemoved      += (w) => UpdateStatus();
		}

		public void Dispose() {
			DestroyIcon( NotifyIcon.Icon.Handle );
			NotifyIcon.Dispose();
		}

		readonly ProjectWatcherList Watchers = new ProjectWatcherList(ConfigXml);

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

		ProjectStatusListForm PSL;
		void ToggleShowProjectStatusList() {
			if ( PSL != null && PSL.Visible ) {
				PSL.Close();
				PSL = null;
			} else {
				if ( PSL != null ) PSL.Dispose();
				PSL = new ProjectStatusListForm(Watchers);
				PSL.Show();
			}
		}

		public static void Begin( Action a ) {
			Instance.Splash.BeginInvoke(a);
		}
		static Program Instance;
		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			SystemEvents.SessionEnding += (sender,args) => {
				Application.Exit();
			};

			using ( Instance = new Program() ) {
				Instance.Splash.Show();
				Instance.Splash.Hide();
				Application.Run();
			}
		}

		[DllImport("user32.dll", SetLastError=true)] static extern bool DestroyIcon(IntPtr hIcon);
	}
}
