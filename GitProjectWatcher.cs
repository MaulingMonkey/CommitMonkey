using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace CommitMonkey {
	static class Git {
		static readonly string[] PossibleCommandPaths = new string[]
			{ "git"
			, @"C:\Program Files\Git\bin\git.exe"
			, @"C:\Program Files (x86)\Git\bin\git.exe"
			};

		public static readonly string Command = FindGitCommand();

		static string FindGitCommand() {
			foreach ( var path in PossibleCommandPaths ) try {
				var process = Process.Start(path, "--version");
				process.WaitForExit();
				return path;
			} catch ( Exception ) {
				// invalid command
			}

			return null;
		}
	}

	class GitProjectWatcherFactory : IProjectWatcherFactory {
		IProjectWatcher IProjectWatcherFactory.Create( string path ) { return Create(path); }
		public GitProjectWatcher Create( string path ) {
			if (!Directory.GetDirectories(path,".git").Any( (dir) => dir.EndsWith( Path.DirectorySeparatorChar + ".git" ) )) return null;
			return new GitProjectWatcher(path);
		}
	}

	class GitProjectWatcher : ProjectWatcherBase {
		public GitProjectWatcher( string path ): base(path) {}

		protected override void UpdateDirtyState() {
			var psi = new ProcessStartInfo()
				{ Arguments              = "add --dry-run ."
				, CreateNoWindow         = true
				, FileName               = Git.Command
				, RedirectStandardOutput = true
				, RedirectStandardError  = true
				, RedirectStandardInput  = true
				, UseShellExecute        = false
				, WindowStyle            = ProcessWindowStyle.Hidden
				, WorkingDirectory       = Path
				};
			var process = Process.Start(psi);
			process.Start();
			process.WaitForExit();
			string output = process.StandardOutput.ReadToEnd();
			string errors = process.StandardError.ReadToEnd();
			// TODO:  Check errors -- we spam git commands enough that many of the errors are transitory, though.
			if ( errors.Length == 0 ) IsDirty = output.Length != 0;
		}
	}
}
