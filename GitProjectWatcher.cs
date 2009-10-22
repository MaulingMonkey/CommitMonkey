using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

namespace CommitMonkey {
	static class Git {
		static string Find32BitProgramFiles() {
			string path = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
			if ( String.IsNullOrEmpty(path) ) path = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
			return path;
		}
		static IEnumerable<string> GetPossibleCommandPaths() {
			yield return "git";
			string pfx86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");
			if (!string.IsNullOrEmpty(pfx86)) yield return pfx86+@"\Git\bin\git.exe";
			yield return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)+@"\Git\bin\git.exe";
		}

		public static readonly string Command = FindGitCommand();

		static string FindGitCommand() {
			foreach ( var path in GetPossibleCommandPaths() ) try {
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
			
			if ( errors.Length != 0 ) {
				// throw new Exception( errors ); // TODO:  Reenable when we decide to start actually handling errors
			} else {
				IsDirty = output.Length != 0;
			}
		}
	}
}
