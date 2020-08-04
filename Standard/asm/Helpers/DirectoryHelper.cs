using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class DirectoryHelper
	{
		private static readonly object __deleteFilesLock = new object();

		[SecuritySafeCritical]
		public static bool Ensure(string path)
		{
			path = PathHelper.Trim(path);
			if (string.IsNullOrEmpty(path)) return false;

			try
			{
				return Directory.CreateDirectory(path).Exists;
			}
			catch
			{
				return false;
			}
		}

		[NotNull]
		public static IEnumerable<string> GetEntries(string path, [NotNull] string pattern = "*", SearchOption options = SearchOption.TopDirectoryOnly)
		{
			path = PathHelper.Trim(path) ?? @".\";
			if (pattern.Length == 0) pattern = "*";
			return Directory.EnumerateFileSystemEntries(path, pattern, options);
		}

		[NotNull]
		public static IEnumerable<string> GetDirectories(string path, [NotNull] string pattern = "*", SearchOption options = SearchOption.TopDirectoryOnly)
		{
			path = PathHelper.Trim(path) ?? @".\";
			if (pattern.Length == 0) pattern = "*";
			return Directory.EnumerateDirectories(path, pattern, options);
		}

		[NotNull]
		public static IEnumerable<string> GetFiles(string path, [NotNull] string pattern = "*", SearchOption options = SearchOption.TopDirectoryOnly)
		{
			path = PathHelper.Trim(path) ?? @".\";
			if (pattern.Length == 0) pattern = "*";
			return Directory.EnumerateFiles(path, pattern, options);
		}

		public static bool Delete(string path, bool recursive = true)
		{
			path = PathHelper.Trim(path) ?? throw new ArgumentNullException(nameof(path));
			if (!Directory.Exists(path)) return true;

			try
			{
				Directory.Delete(path, recursive);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool DeleteFiles(string path, [NotNull] string filter)
		{
			path = PathHelper.Trim(path) ?? throw new ArgumentNullException(nameof(path));
			if (!Directory.Exists(path)) return false;

			lock (__deleteFilesLock)
			{
				try
				{
					DirectoryInfo dir = new DirectoryInfo(path);

					foreach (FileInfo file in dir.EnumerateFiles(filter, SearchOption.TopDirectoryOnly))
					{
						try
						{
							file.Delete();
						}
						catch
						{
							return false;
						}
					}

					return true;
				}
				catch
				{
					return false;
				}
			}
		}

		[NotNull]
		public static string GetTempName() { return GetTempName(null, null); }
		[NotNull]
		public static string GetTempName(string basePath) { return GetTempName(basePath, null); }
		[NotNull]
		public static string GetTempName(string basePath, string prefix)
		{
			const int LEN_NAME = 15;
			const int LEN_PFX = 3;

			basePath = PathHelper.Trim(basePath);
			if (string.IsNullOrEmpty(basePath)) basePath = Path.GetTempPath();

			StringBuilder sb = new StringBuilder(Win32.MAX_PATH)
				.Append(basePath)
				.AppendIfDoesNotEndWith(PathHelper.DirectorySeparator);
			prefix = prefix?.Trim();

			int bufferLen = LEN_NAME;

			if (!string.IsNullOrEmpty(prefix))
			{
				for (int i = 0, x = LEN_NAME - LEN_PFX; i < prefix.Length && bufferLen > x; i++)
				{
					if (!FileHelper.IsValidNameChar(prefix[i])) continue;
					sb.Append(prefix[i]);
					bufferLen--;
				}

				bufferLen = bufferLen.Previous(3);
			}

			int len = sb.Length;
			string path;

			do
			{
				sb.Length = len;
				sb.Append(StringHelper.RandomKey((byte)RandomHelper.Next(bufferLen / 2, bufferLen)));
				path = sb.ToString();
			}
			while (PathHelper.Exists(path));

			DirectoryInfo info = Directory.CreateDirectory(path);
			return info.FullName;
		}

		public static IEnumerable<string> Enumerate(string path) { return Enumerate(path, null, SearchOption.TopDirectoryOnly, false); }
		public static IEnumerable<string> Enumerate(string path, SearchOption option) { return Enumerate(path, null, option, false); }
		public static IEnumerable<string> Enumerate(string path, string pattern) { return Enumerate(path, pattern, SearchOption.AllDirectories, false); }
		public static IEnumerable<string> Enumerate(string path, string pattern, SearchOption option) { return Enumerate(path, pattern, option, false); }
		public static IEnumerable<string> Enumerate(string path, string pattern, SearchOption option, bool applyPatternToDirectories)
		{
			path = PathHelper.Trim(path) ?? @".\";
			pattern = pattern?.Trim();
			if (string.IsNullOrEmpty(pattern)) pattern = "*";
			string dirPattern = applyPatternToDirectories ? pattern : "*";
			return EnumerateEntries(path, dirPattern, pattern, option);

			IEnumerable<string> EnumerateEntries(string entryPath, string entryDirPattern, string entryPattern, SearchOption entryOption)
			{
				if (entryOption == SearchOption.TopDirectoryOnly)
				{
					if (entryDirPattern == entryPattern)
					{
						foreach (string entry in Directory.EnumerateFileSystemEntries(entryPath, entryDirPattern, SearchOption.TopDirectoryOnly))
							yield return entry;
					}
					else
					{
						foreach (string entry in Directory.EnumerateDirectories(entryPath, entryDirPattern, SearchOption.TopDirectoryOnly))
							yield return entry;

						foreach (string entry in Directory.EnumerateFiles(entryPath, entryPattern, SearchOption.TopDirectoryOnly))
							yield return entry;
					}
				}
				else
				{
					if (entryDirPattern == entryPattern)
					{
						foreach (string entry in Directory.EnumerateFileSystemEntries(entryPath, entryDirPattern, SearchOption.AllDirectories))
							yield return entry;
					}
					else
					{
						string[] directories = Directory.GetDirectories(entryPath, entryDirPattern, SearchOption.TopDirectoryOnly);
				
						foreach (string entry in directories)
							yield return entry;

						foreach (string entry in Directory.EnumerateFiles(entryPath, entryPattern, SearchOption.TopDirectoryOnly))
							yield return entry;
	
						foreach (string entry in directories.SelectMany(d => EnumerateEntries(d, dirPattern, pattern, option)))
							yield return entry;
					}
				}
			}
		}
	}
}