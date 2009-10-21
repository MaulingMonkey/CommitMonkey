using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CommitMonkey {
	class Program : IDisposable {
		NotifyIcon NotifyIcon;

		Program() {
			NotifyIcon = new NotifyIcon()
				{ ContextMenu = new ContextMenu( new[]
					{ new MenuItem( "E&xit", (s,a) => Application.Exit() )
					})
				, Icon    = Icon.FromHandle( Resources.CommitMonkey.GetHicon() )
				, Text    = "AutoVCS"
				, Visible = true
				};
		}

		public void Dispose() {
			DestroyIcon( NotifyIcon.Icon.Handle );
			NotifyIcon.Dispose();
		}

		[STAThread] static void Main() {
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			using ( var p = new Program() ) Application.Run();
		}

		[DllImport("user32.dll", SetLastError=true)] static extern bool DestroyIcon(IntPtr hIcon);
	}
}
