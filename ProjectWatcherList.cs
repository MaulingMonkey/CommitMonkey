using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Collections;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.IO;
using System.Linq;

namespace CommitMonkey {
	class ProjectWatcherList : IEnumerable<IProjectWatcher> {
		readonly List<IProjectWatcherFactory> WatcherTypes = new List<IProjectWatcherFactory>()
			{ new GitProjectWatcherFactory()
			};
		readonly List<IProjectWatcher> Watchers = new List<IProjectWatcher>();

		public event Action<IProjectWatcher> WatcherAdded = delegate {};
		public event Action<IProjectWatcher> WatcherRemoved = delegate {};
		public event Action<IProjectWatcher> WatcherDirtyChanged = delegate {};

		readonly string XmlConfigPath;
		public ProjectWatcherList( string xmlconfigpath ) {
			XmlConfigPath = xmlconfigpath;
			if ( File.Exists(xmlconfigpath) ) {
				XmlSerializer s = new XmlSerializer(typeof(XmlConfig));
				XmlConfig xmlconfig;
				using ( TextReader r = new StreamReader(xmlconfigpath) ) xmlconfig = (XmlConfig)s.Deserialize(r);
				foreach ( var project in xmlconfig.Projects ) try {
					Watch(project.Path);
				} catch ( DirectoryNotFoundException ) {
					MessageBox.Show( "Cannot watch "+project.Path+" -- can't find directory!", "Error loading config",MessageBoxButtons.OK, MessageBoxIcon.Error ); 
				}
			}
		}

		public void SaveToXmlFile( string xmlconfigpath ) {
			var xmlconfig = new XmlConfig()
				{ Projects = Watchers.Select( (watcher) => new XmlConfig.Project() { Path = watcher.Path } ).ToArray()
				};

			XmlSerializer s = new XmlSerializer( typeof(XmlConfig) );
			using ( TextWriter w = new StreamWriter(xmlconfigpath) ) s.Serialize( w, xmlconfig );
		}

		public void Add( IProjectWatcher watcher ) {
			Watchers.Add(watcher);
			WatcherAdded(watcher);
			watcher.IsDirtyChanged += watcher_IsDirtyChanged;
			SaveToXmlFile( XmlConfigPath );
		}

		public void Remove( IProjectWatcher watcher ) {
			Debug.Assert( Watchers.Remove(watcher) );
			WatcherRemoved(watcher);
			watcher.IsDirtyChanged -= watcher_IsDirtyChanged;
			SaveToXmlFile( XmlConfigPath );
		}

		void Watch( string path ) {
			foreach ( var wtype in WatcherTypes ) {
				IProjectWatcher watcher = wtype.Create(path);
				if ( watcher != null ) {
					Add(watcher);
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

		public void PromptWatch() {
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

		void watcher_IsDirtyChanged(IProjectWatcher watcher) {
			WatcherDirtyChanged(watcher);
		}

		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}

		public IEnumerator<IProjectWatcher> GetEnumerator() {
			return Watchers.GetEnumerator();
		}
	}
}
