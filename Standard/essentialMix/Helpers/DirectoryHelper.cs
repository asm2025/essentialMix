using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Helpers
{
	public static class DirectoryHelper
	{
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

		[NotNull]
		public static IEnumerable<string> Enumerate(string path) { return Enumerate(path, null, false, SearchOption.TopDirectoryOnly, null); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, SearchOption option) { return Enumerate(path, null, false, option, null); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern) { return Enumerate(path, pattern, false, SearchOption.TopDirectoryOnly, null); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, SearchOption option) { return Enumerate(path, pattern, false, option, null); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, bool applyPatternToDirectories) { return Enumerate(path, pattern, applyPatternToDirectories, SearchOption.TopDirectoryOnly, null); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, bool applyPatternToDirectories, SearchOption option) { return Enumerate(path, pattern, applyPatternToDirectories, option, null); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, Predicate<string> onEnqueue) { return Enumerate(path, null, false, SearchOption.TopDirectoryOnly, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, SearchOption option, Predicate<string> onEnqueue) { return Enumerate(path, null, false, option, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, Predicate<string> onEnqueue) { return Enumerate(path, pattern, false, SearchOption.TopDirectoryOnly, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, SearchOption option, Predicate<string> onEnqueue) { return Enumerate(path, pattern, false, option, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, bool applyPatternToDirectories, Predicate<string> onEnqueue) { return Enumerate(path, pattern, applyPatternToDirectories, SearchOption.TopDirectoryOnly, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, bool applyPatternToDirectories, SearchOption option, Predicate<string> onEnqueue)
		{
			path = Path.GetFullPath(PathHelper.Trim(path) ?? ".\\");
			pattern = pattern?.Trim(';', '|', ' ')
							.Replace(';', '|')
							.Replace("||", "|")
							.ToNullIfEmpty();
			if (pattern == "|") throw new ArgumentException("Invalid pattern.", nameof(pattern));
			// multi-pattern means RegEx will be used to match the entries
			bool multiPatterns = pattern != null && pattern.Length > 1 && pattern.Contains('|');
			if (multiPatterns) pattern = RegexHelper.FromFilePattern(pattern);
			if (pattern == null || RegexHelper.AllAsterisks.IsMatch(pattern)) multiPatterns = false;

			if (!multiPatterns)
			{
				// useFSPattern means EnumerateFileSystemEntries will be used when onEnqueue is null.
				// must be careful not to apply the pattern to directories when not needed
				bool useFSPattern = applyPatternToDirectories || pattern == null || RegexHelper.AllAsterisks.IsMatch(pattern);

				// no RegEx match here, just handle onEnqueue
				pattern ??= "*";
				// dirPattern is merely an * if applyPatternToDirectories is false
				string dirPattern = applyPatternToDirectories
										? pattern
										: "*";

				if (onEnqueue == null)
				{
					// EnumerateFileSystemEntries can be used here
					// simplest case
					if (useFSPattern) return Directory.EnumerateFileSystemEntries(path, pattern, option);
					// different patterns will be applied
					// local function use a Queue to navigate the tree so check if only the top level will be searched.
					if (option == SearchOption.TopDirectoryOnly)
					{
						return Directory.EnumerateDirectories(path, dirPattern)
										.Combine(Directory.EnumerateFiles(path, pattern));
					}

					// here, a Queue must be used. onEnqueue will be handled in the local function
					return EnumerateLocal(path, dirPattern, pattern, null);
				}

				// at this point, onEnqueue is not null. useFSPattern is also therefore useless because directories must be separated from files.
				// local function use a Queue to navigate the tree so check if only the top level will be searched.
				if (option == SearchOption.TopDirectoryOnly)
				{
					return Directory.EnumerateDirectories(path, dirPattern, SearchOption.TopDirectoryOnly)
									.Where(e => onEnqueue(e))
									.Combine(Directory.EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly));
				}

				// handle navigation and queueing. It doesn't matter if dirPattern == pattern or not
				return EnumerateLocal(path, dirPattern, pattern, onEnqueue);
			}

			// RegEx will be used here because multiPatterns is true
			// multiPatterns also means pattern is not null and is not *
			// RegEx cannot use useFSPattern unless applyPatternToDirectories is true only
			Regex rgxPattern = new Regex(pattern, RegexHelper.OPTIONS_I);

			// local function use a Queue to navigate the tree so check if only the top level will be searched.
			if (option != SearchOption.TopDirectoryOnly) return EnumerateRgxLocal(path, rgxPattern, applyPatternToDirectories, onEnqueue);
			
			IEnumerable<string> enumerable = Directory.EnumerateDirectories(path);

			if (applyPatternToDirectories)
			{
				enumerable = enumerable.Where(e =>
				{
					// use Path.GetFileName for directory too.
					// the weird naming of Path.GetDirectoryName is misleading
					string name = Path.GetFileName(e);
					return name == null || rgxPattern.IsMatch(name);
				});
			}

			if (onEnqueue != null) enumerable = enumerable.Where(e => onEnqueue(e));
			return enumerable.Combine(Directory.EnumerateFiles(path)
												.Where(e =>
												{
													string name = Path.GetFileName(e);
													return name == null || rgxPattern.IsMatch(name);
												}));

			static IEnumerable<string> EnumerateLocal(string path, string dirPattern, string filePattern, Predicate<string> onEnqueue)
			{
				Queue<string> queue = new Queue<string>();
				queue.Enqueue(path);

				while (queue.Count > 0)
				{
					path = queue.Dequeue();
					IEnumerable<string> enumerable = Directory.EnumerateDirectories(path, dirPattern);
					if (onEnqueue != null) enumerable = enumerable.Where(e => onEnqueue(e));

					foreach (string directory in enumerable)
					{
						yield return directory;
						queue.Enqueue(directory);
					}

					foreach (string file in Directory.EnumerateFiles(path, filePattern))
						yield return file;
				}
			}

			static IEnumerable<string> EnumerateRgxLocal(string path, Regex pattern, bool applyPatternToDirectories, Predicate<string> onEnqueue)
			{
				// the problem with RegEx is only the file/directory name must be matched therefore file/directory name must be extracted.
				Queue<string> queue = new Queue<string>();
				queue.Enqueue(path);

				while (queue.Count > 0)
				{
					path = queue.Dequeue();
					IEnumerable<string> enumerable = Directory.EnumerateDirectories(path);
					
					if (applyPatternToDirectories)
					{
						enumerable = enumerable.Where(e =>
						{
							// use Path.GetFileName for directory too.
							// the weird naming of Path.GetDirectoryName is misleading
							string name = Path.GetFileName(e);
							return name == null || pattern.IsMatch(name);
						});
					}

					if (onEnqueue != null) enumerable = enumerable.Where(e => onEnqueue(e));

					foreach (string directory in enumerable)
					{
						yield return directory;
						queue.Enqueue(directory);
					}

					enumerable = Directory.EnumerateFiles(path).Where(e =>
					{
						string name = Path.GetFileName(e);
						return name == null || pattern.IsMatch(name);
					});

					foreach (string file in enumerable)
						yield return file;
				}
			}
		}

		[NotNull]
		public static IEnumerable<string> EnumerateDirectories(string path) { return EnumerateDirectories(path, null, SearchOption.TopDirectoryOnly); }
		[NotNull]
		public static IEnumerable<string> EnumerateDirectories(string path, SearchOption option) { return EnumerateDirectories(path, null, option); }
		[NotNull]
		public static IEnumerable<string> EnumerateDirectories(string path, string pattern) { return EnumerateDirectories(path, pattern, SearchOption.TopDirectoryOnly); }
		[NotNull]
		public static IEnumerable<string> EnumerateDirectories(string path, string pattern, SearchOption option)
		{
			path = Path.GetFullPath(PathHelper.Trim(path) ?? ".\\");
			pattern = pattern?.Trim(';', '|', ' ')
							.Replace(';', '|')
							.Replace("||", "|")
							.ToNullIfEmpty();
			if (pattern == "|") throw new ArgumentException("Invalid pattern.", nameof(pattern));

			// multi-pattern means RegEx will be used to match the entries
			bool multiPatterns = pattern != null && pattern.Length > 1 && pattern.Contains('|');
			if (multiPatterns) pattern = RegexHelper.FromFilePattern(pattern);
			if (pattern == null || RegexHelper.AllAsterisks.IsMatch(pattern)) multiPatterns = false;

			if (!multiPatterns)
			{
				// no RegEx match here
				return Directory.EnumerateDirectories(path, pattern ?? "*", option);
			}

			// RegEx will be used here because multiPatterns is true
			// multiPatterns means pattern is not null and is not *
			Regex rgxPattern = new Regex(pattern, RegexHelper.OPTIONS_I);
			return Directory.EnumerateDirectories(path, "*", option).Where(e =>
			{
				// use Path.GetFileName for directory too.
				// the weird naming of Path.GetDirectoryName is misleading
				string name = Path.GetFileName(e);
				return name == null || rgxPattern.IsMatch(name);
			});
		}

		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path) { return EnumerateFiles(path, null, SearchOption.TopDirectoryOnly, null); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, SearchOption option) { return EnumerateFiles(path, null, option, null); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, string pattern) { return EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly, null); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option) { return EnumerateFiles(path, pattern, option, null); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, Predicate<string> onEnqueue) { return EnumerateFiles(path, null, SearchOption.TopDirectoryOnly, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, SearchOption option, Predicate<string> onEnqueue) { return EnumerateFiles(path, null, option, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, string pattern, Predicate<string> onEnqueue) { return EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly, onEnqueue); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option, Predicate<string> onEnqueue)
		{
			path = Path.GetFullPath(PathHelper.Trim(path) ?? ".\\");
			pattern = pattern?.Trim(';', '|', ' ')
							.Replace(';', '|')
							.Replace("||", "|")
							.ToNullIfEmpty();
			if (pattern == "|") throw new ArgumentException("Invalid pattern.", nameof(pattern));
			// multi-pattern means RegEx will be used to match the entries
			bool multiPatterns = pattern != null && pattern.Length > 1 && pattern.Contains('|');
			if (multiPatterns) pattern = RegexHelper.FromFilePattern(pattern);
			if (pattern == null || RegexHelper.AllAsterisks.IsMatch(pattern)) multiPatterns = false;

			if (!multiPatterns)
			{
				// no RegEx match here, just handle onEnqueue
				pattern ??= "*";
				// simplest case
				return option == SearchOption.TopDirectoryOnly || onEnqueue == null
							// onEnqueue will be ignored if option is SearchOption.TopDirectoryOnly
							? Directory.EnumerateFiles(path, pattern, option)
							// here, a Queue must be used. onEnqueue will be handled in the local function
							: EnumerateLocal(path, pattern, onEnqueue);
			}

			Regex rgxPattern = new Regex(pattern, RegexHelper.OPTIONS_I);
			return option == SearchOption.TopDirectoryOnly || onEnqueue == null
						// onEnqueue will be ignored if option is SearchOption.TopDirectoryOnly
						? Directory.EnumerateFiles(path, "*", option).Where(e =>
						{
							string name = Path.GetFileName(e);
							return name == null || rgxPattern.IsMatch(name);
						})
						// here, a Queue must be used. onEnqueue will be handled in the local function
						: EnumerateRgxLocal(path, rgxPattern, onEnqueue);

			static IEnumerable<string> EnumerateLocal(string path, string pattern, Predicate<string> onEnqueue)
			{
				Queue<string> queue = new Queue<string>();
				queue.Enqueue(path);

				while (queue.Count > 0)
				{
					path = queue.Dequeue();
					IEnumerable<string> enumerable = Directory.EnumerateDirectories(path);
					if (onEnqueue != null) enumerable = enumerable.Where(e => onEnqueue(e));

					foreach (string directory in enumerable)
					{
						yield return directory;
						queue.Enqueue(directory);
					}

					foreach (string file in Directory.EnumerateFiles(path, pattern))
						yield return file;
				}
			}

			static IEnumerable<string> EnumerateRgxLocal(string path, Regex pattern, Predicate<string> onEnqueue)
			{
				// the problem with RegEx is only the file/directory name must be matched therefore file/directory name must be extracted.
				Queue<string> queue = new Queue<string>();
				queue.Enqueue(path);

				while (queue.Count > 0)
				{
					path = queue.Dequeue();
					IEnumerable<string> enumerable = Directory.EnumerateDirectories(path);
					if (onEnqueue != null) enumerable = enumerable.Where(e => onEnqueue(e));

					foreach (string directory in enumerable) 
						queue.Enqueue(directory);

					enumerable = Directory.EnumerateFiles(path).Where(e =>
					{
						string name = Path.GetFileName(e);
						return name == null || pattern.IsMatch(name);
					});

					foreach (string file in enumerable)
						yield return file;
				}
			}
		}
	}
}