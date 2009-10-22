using System.Windows.Forms;
using System;

namespace CommitMonkey {
	interface IProjectWatcherFactory {
		IProjectWatcher Create( string path );
	}
	interface IProjectWatcher {
		bool IsDirty { get; }
		event Action IsDirtyChanged;
		string Path { get; }
	}
	
	abstract class ProjectWatcherBase : IProjectWatcher {
		Timer DirtyCheckTimer;

		public ProjectWatcherBase( string path ) {
			Path = path;

			DirtyCheckTimer = new Timer()
				{ Interval = 1000
				};
			DirtyCheckTimer.Tick += (s,a) => UpdateDirtyState();
			DirtyCheckTimer.Start();
		}

		protected abstract void UpdateDirtyState();

		public string Path { get; private set; }
		
		private bool _IsDirty = false;
		public bool IsDirty { get {
			return _IsDirty;
		} protected set {
			bool oldvalue = _IsDirty;
			_IsDirty = value;
			if ( value != oldvalue ) IsDirtyChanged();
		}}
		public event Action IsDirtyChanged;
	}
}
