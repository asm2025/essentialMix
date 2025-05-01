using System;
using System.IO;
using System.Threading;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Threading.FileSystem;

public struct FileSystemWatcherSettings([NotNull] DirectoryInfo directory)
{
	private string _filter = "*.*";

	public FileSystemWatcherSettings(string path)
		: this(new DirectoryInfo(PathHelper.Trim(path) ?? throw new ArgumentNullException(nameof(path))))
	{
	}

	[NotNull]
	public DirectoryInfo Directory { get; } = directory;

	public string Filter
	{
		get => _filter;
		set => _filter = value.ToNullIfEmpty() ?? "*.*";
	}

	public bool IncludeSubdirectories { get; set; } = false;

	public WatcherChangeTypes ChangeType { get; set; } = WatcherChangeTypes.All;

	public NotifyFilters NotifyFilter { get; set; } = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite;

	public bool IsBackground { get; set; } = true;
	public ThreadPriority Priority { get; set; } = ThreadPriority.Normal;
	public bool SynchronizeContext { get; set; }
	public bool WaitOnDispose { get; set; }
	public Action<FileSystemNotifier> WorkStartedCallback { get; set; }
	public Action<FileSystemNotifier> WorkCompletedCallback { get; set; }
}