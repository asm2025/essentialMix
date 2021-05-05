using System;
using System.IO;
using System.Text.RegularExpressions;
using essentialMix.Helpers;
using essentialMix.Patterns.Object;

namespace essentialMix.Threading.FileSystem
{
	public class FileSystemWatcher : Disposable
	{
		private System.IO.FileSystemWatcher _watcher;

		public FileSystemWatcher(FileSystemWatcherSettings settings)
		{
			if (!settings.Directory.Exists) throw new DirectoryNotFoundException();

			string filter = RegexHelper.FromFilePattern(settings.Filter);
			Regex rgxFilter = RegexHelper.AllAsterisks.IsMatch(filter)
								? null
								: new Regex(filter, RegexHelper.OPTIONS_I);
			_watcher = new System.IO.FileSystemWatcher(settings.Directory.FullName)
			{
				IncludeSubdirectories = settings.IncludeSubdirectories,
				NotifyFilter = settings.NotifyFilter,
				EnableRaisingEvents = false
			};

			_watcher.Created += (_, args) =>
			{
				if (!settings.ChangeType.HasFlag(WatcherChangeTypes.Created) || rgxFilter != null && !rgxFilter.IsMatch(args.Name)) return;
				OnCreated(args);
			};

			_watcher.Renamed += (_, args) =>
			{
				if (!settings.ChangeType.HasFlag(WatcherChangeTypes.Renamed) || rgxFilter != null && !rgxFilter.IsMatch(args.OldName) && !rgxFilter.IsMatch(args.Name)) return;
				OnRenamed(args);
			};

			_watcher.Deleted += (_, args) =>
			{
				if (!settings.ChangeType.HasFlag(WatcherChangeTypes.Deleted) || rgxFilter != null && !rgxFilter.IsMatch(args.Name)) return;
				OnDeleted(args);
			};

			_watcher.Changed += (_, args) =>
			{
				if (!settings.ChangeType.HasFlag(WatcherChangeTypes.Changed) || rgxFilter != null && !rgxFilter.IsMatch(args.Name)) return;
				OnChanged(args);
			};
		}

		public event EventHandler<FileSystemEventArgs> Created;
		public event EventHandler<RenamedEventArgs> Renamed;
		public event EventHandler<FileSystemEventArgs> Deleted;
		public event EventHandler<FileSystemEventArgs> Changed;

		public virtual bool Enabled
		{
			get => _watcher.EnableRaisingEvents;
			set => _watcher.EnableRaisingEvents = value;
		}

		/// <inheritdoc />
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Enabled = false;
				ObjectHelper.Dispose(ref _watcher);
			}
			base.Dispose(disposing);
		}

		protected virtual void OnCreated(FileSystemEventArgs args)
		{
			Created?.Invoke(this, args);
		}

		protected virtual void OnRenamed(RenamedEventArgs args)
		{
			Renamed?.Invoke(this, args);
		}

		protected virtual void OnDeleted(FileSystemEventArgs args)
		{
			Deleted?.Invoke(this, args);
		}

		protected virtual void OnChanged(FileSystemEventArgs args)
		{
			Changed?.Invoke(this, args);
		}

		protected bool HasCreatedSubscribers => Created != null;
		protected bool HasRenamedSubscribers => Renamed != null;
		protected bool HasDeletedSubscribers => Deleted != null;
		protected bool HasChangedSubscribers => Changed != null;
	}
}