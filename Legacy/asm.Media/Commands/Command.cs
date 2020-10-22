using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using asm.Extensions;
using asm.Helpers;
using asm.Threading;
using JetBrains.Annotations;

namespace asm.Media.Commands
{
	public class Command : Runnable
	{
		//https://www.labnol.org/internet/useful-ffmpeg-commands/28490/
		//https://community.spiceworks.com/topic/344566-convert-video-from-web-page-using-ffmpeg

		private static string __workingDir;

		public Command()
			: this(null)
		{
		}

		protected Command(string command)
			: base(command)
		{
			SetDefaults();
		}

		protected override void OnCreate(Process process)
		{
			base.OnCreate(process);
			Monitor?.Reset();
			EnsureDependencies(this);
		}

		protected override void OnOutput(string data)
		{
			base.OnOutput(data);
			Monitor?.ProcessOutput(data);
		}

		protected override void OnError(string data)
		{
			base.OnError(data);
			Monitor?.ProcessOutput(data);
		}

		public event EventHandler ProgressStart;
		public event ProgressChangedEventHandler Progress;
		public event EventHandler ProgressCompleted;
		public event EventHandler<string> ProgressError;
		protected virtual IProgressMonitor Monitor { get; } = null;
		public bool HasProgress => Monitor != null;

		protected virtual void OnProgressStart(EventArgs args) { ProgressStart?.Invoke(this, args); }
		protected virtual void OnProgress(ProgressChangedEventArgs args) { Progress?.Invoke(this, args); }
		protected virtual void OnProgressCompleted(EventArgs args) { ProgressCompleted?.Invoke(this, args); }
		protected virtual void OnProgressError(string args) { ProgressError?.Invoke(this, args); }

		[NotNull]
		protected IList<string> Dependencies { get; } = new List<string>();

		private void SetDefaults()
		{
			CanChangeExecutableName = false;
			Settings = RunSettingsBase.AsAdminHiddenNoWindow;
		}

		public static string EnsureDependencies([NotNull] Command command)
		{
			__workingDir ??= PathHelper.AddDirectorySeparator(AssemblyHelper.GetEntryAssembly()?.GetDirectoryPath() ?? Directory.GetCurrentDirectory());

			if (string.IsNullOrEmpty(command.ExecutableName)) return null;

			string exePath = __workingDir + command.ExecutableName;
			if (!File.Exists(exePath)) throw new FileNotFoundException("Dependency not found.", exePath);
			
			foreach (string dependency in command.Dependencies.SkipNullOrEmptyTrim())
			{
				if (File.Exists(__workingDir + dependency)) continue;
				throw new FileNotFoundException("Dependency not found.", dependency);
			}

			return exePath;
		}
	}
}