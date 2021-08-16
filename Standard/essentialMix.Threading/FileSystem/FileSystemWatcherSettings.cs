using System;
using System.IO;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.FileSystem
{
	public struct FileSystemWatcherSettings
	{
		private string _filter;

		public FileSystemWatcherSettings(string path)
			: this(new DirectoryInfo(PathHelper.Trim(path) ?? throw new ArgumentNullException(nameof(path))))
		{
		}

		public FileSystemWatcherSettings([NotNull] DirectoryInfo directory)
			: this()
		{
			Directory = directory;
			IncludeSubdirectories = false;
			_filter = "*.*";
			ChangeType = WatcherChangeTypes.All;
			NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite;
			IsBackground = true;
			Priority = ThreadPriority.Normal;
		}

		[NotNull]
		public DirectoryInfo Directory { get; }

		public string Filter
		{
			get => _filter;
			set => _filter = value.ToNullIfEmpty() ?? "*.*";
		}

		public bool IncludeSubdirectories { get; set; }

		public WatcherChangeTypes ChangeType { get; set; }

		public NotifyFilters NotifyFilter { get; set; }
			
		public bool IsBackground { get; set; }
		public ThreadPriority Priority { get; set; }
		public bool SynchronizeContext { get; set; }
		public bool WaitOnDispose { get; set; }
		public Action<FileSystemNotifier> WorkStartedCallback { get; set; }
		public Action<FileSystemNotifier> WorkCompletedCallback { get; set; }
	}
}
