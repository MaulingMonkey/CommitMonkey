namespace CommitMonkey {
	interface IProjectWatcherFactory {
		IProjectWatcher Create( string path );
	}
	interface IProjectWatcher {
		bool IsDirty { get; }
		string Path { get; }
	}
}
