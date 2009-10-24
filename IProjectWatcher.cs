using System.Windows.Forms;
using System;
using System.IO;

namespace CommitMonkey {
	interface IProjectWatcherFactory {
		IProjectWatcher Create( string path );
	}
	interface IProjectWatcher {
		bool IsDirty { get; }
		event Action<IProjectWatcher> IsDirtyChanged;
		string Path { get; }
	}
	
	abstract class ProjectWatcherBase : IProjectWatcher {
		Timer DirtyCheckTimer;
		FileSystemWatcher DirtyWatcher;

		public ProjectWatcherBase( string path ) {
			Path = path;

			DirtyCheckTimer = new Timer()
				{ Interval = 1000
				};
			DirtyCheckTimer.Tick += DirtyCheckTimer_Tick;
			DirtyCheckTimer.Start();
			
			DirtyWatcher = new FileSystemWatcher(path)
				{ EnableRaisingEvents = true
				, IncludeSubdirectories = true
				, InternalBufferSize = 4*1024*1024
				, NotifyFilter = NotifyFilters.LastWrite
				};
			DirtyWatcher.Changed += DirtyWatcher_SomethingHappened;
			DirtyWatcher.Created += DirtyWatcher_SomethingHappened;
			DirtyWatcher.Deleted += DirtyWatcher_SomethingHappened;
			DirtyWatcher.Renamed += DirtyWatcher_SomethingHappened;
		}

		void DirtyCheckTimer_Tick(object sender, EventArgs e) {
			//try {
				UpdateDirtyState();
				DirtyCheckTimer.Interval = 1000;
			//} catch ( Exception ) {} // TODO:  Handle exceptions from UpdateDirtyState
			DirtyCheckTimer.Start();
		}

		void DirtyWatcher_SomethingHappened(object sender, FileSystemEventArgs e) {
			if ( e.FullPath.StartsWith(Path     ) )
			if (!e.FullPath.StartsWith(GitDirSub) )
			if ( e.FullPath != GitDir )
			Program.Begin( () => {
				DirtyCheckTimer.Interval = 100;
				DirtyCheckTimer.Start();
			});
		}

		protected abstract void UpdateDirtyState();

		public string Path { get; private set; }
		public string GitDir { get { return Path + System.IO.Path.DirectorySeparatorChar + ".git"; } }
		public string GitDirSub { get { return GitDir + System.IO.Path.DirectorySeparatorChar; } }
		
		private bool _IsDirty = false;
		public bool IsDirty { get {
			return _IsDirty;
		} protected set {
			bool oldvalue = _IsDirty;
			_IsDirty = value;
			if ( value != oldvalue ) IsDirtyChanged(this);
		}}
		public event Action<IProjectWatcher> IsDirtyChanged = delegate {};
	}
}
