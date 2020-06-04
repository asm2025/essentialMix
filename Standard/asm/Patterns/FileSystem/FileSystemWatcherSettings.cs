﻿using System;
using System.IO;
using JetBrains.Annotations;
using asm.Extensions;
using asm.Helpers;

namespace asm.Patterns.FileSystem
{
	public struct FileSystemWatcherSettings
	{
		private string _filter;

		public FileSystemWatcherSettings(string path)
			: this(new DirectoryInfo(PathHelper.Trim(path) ?? throw new ArgumentNullException(nameof(path))))
		{
		}

		public FileSystemWatcherSettings([NotNull] DirectoryInfo directory)
		{
			Directory = directory;
			IncludeSubdirectories = false;
			_filter = "*.*";
			ChangeType = WatcherChangeTypes.All;
			NotifyFilter = NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite;
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
	}
}
