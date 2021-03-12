using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
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
		public static IEnumerable<string> Enumerate(string path) { return Enumerate(path, null, SearchOption.TopDirectoryOnly, false); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, SearchOption option) { return Enumerate(path, null, option, false); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern) { return Enumerate(path, pattern, SearchOption.TopDirectoryOnly, false); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, SearchOption option) { return Enumerate(path, pattern, option, false); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, bool applyPatternToDirectories) { return Enumerate(path, pattern, SearchOption.TopDirectoryOnly, applyPatternToDirectories); }
		[NotNull]
		public static IEnumerable<string> Enumerate(string path, string pattern, SearchOption option, bool applyPatternToDirectories)
		{
			path = PathHelper.Trim(path) ?? @".\";
			pattern = pattern?.Trim().Replace(';', '|');
			if (string.IsNullOrEmpty(pattern)) pattern = "*";

			bool multiPatterns = pattern.Contains('|');
			if (multiPatterns) pattern = RegexHelper.FromFilePattern(pattern);

			if (pattern == "^*$")
			{
				multiPatterns = false;
				pattern = "*";
			}

			string dirPattern = applyPatternToDirectories ? pattern : "*";
			bool useFSPattern = dirPattern.Equals(pattern, StringComparison.OrdinalIgnoreCase);

			if (!multiPatterns)
			{
				return useFSPattern
							? Directory.EnumerateFileSystemEntries(path, pattern, option)
							: option == SearchOption.TopDirectoryOnly
								? Directory.EnumerateDirectories(path, dirPattern).Combine(Directory.EnumerateFiles(path, pattern))
								: EnumerateLocal(path, dirPattern, pattern);
			}

			Regex rgxPattern = new Regex(pattern, RegexHelper.OPTIONS_I);
			if (useFSPattern) return Directory.EnumerateFileSystemEntries(path, "*", option).Where(e => rgxPattern.IsMatch(e));

			Regex rgxDirectory = dirPattern == "*"
									? null
									: new Regex(dirPattern, RegexHelper.OPTIONS_I);
			return EnumerateRgxLocal(path, rgxDirectory, rgxPattern, option);

			static IEnumerable<string> EnumerateLocal(string path, string dirPattern, string filePattern)
			{
				Queue<string> queue = new Queue<string>();
				queue.Enqueue(path);

				while (queue.Count > 0)
				{
					path = queue.Dequeue();

					foreach (string directory in Directory.EnumerateDirectories(path, dirPattern))
					{
						yield return directory;
						queue.Enqueue(directory);
					}

					foreach (string file in Directory.EnumerateFiles(path, filePattern))
						yield return file;
				}
			}

			static IEnumerable<string> EnumerateRgxLocal(string path, Regex dirPattern, Regex filePattern, SearchOption option)
			{
				if (option == SearchOption.TopDirectoryOnly)
				{
					IEnumerable<string> enumerable = Directory.EnumerateDirectories(path);
					if (dirPattern != null) enumerable = enumerable.Where(e => dirPattern.IsMatch(e));

					enumerable = enumerable.Combine(Directory.EnumerateFiles(path).Where(e => filePattern.IsMatch(e)));
					
					foreach (string entry in enumerable)
						yield return entry;

					yield break;
				}

				Queue<string> queue = new Queue<string>();
				queue.Enqueue(path);

				while (queue.Count > 0)
				{
					path = queue.Dequeue();
					IEnumerable<string> directories = Directory.EnumerateDirectories(path);
					if (dirPattern != null) directories = directories.Where(e => dirPattern.IsMatch(e));

					foreach (string directory in directories)
					{
						yield return directory;
						queue.Enqueue(directory);
					}

					foreach (string file in Directory.EnumerateFiles(path).Where(e => filePattern.IsMatch(e)))
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
			path = PathHelper.Trim(path) ?? @".\";
			pattern = pattern?.Trim().Replace(';', '|');
			if (string.IsNullOrEmpty(pattern)) pattern = "*";

			bool multiPatterns = pattern.Contains('|');
			if (multiPatterns) pattern = RegexHelper.FromFilePattern(pattern);

			if (pattern == "^*$")
			{
				multiPatterns = false;
				pattern = "*";
			}

			if (!multiPatterns) return Directory.EnumerateDirectories(path, pattern, option);

			Regex rgxPattern = new Regex(pattern, RegexHelper.OPTIONS_I);
			return Directory.EnumerateDirectories(path, "*", option).Where(e => rgxPattern.IsMatch(e));
		}

		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path) { return EnumerateFiles(path, null, SearchOption.TopDirectoryOnly); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, SearchOption option) { return EnumerateFiles(path, null, option); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, string pattern) { return EnumerateFiles(path, pattern, SearchOption.TopDirectoryOnly); }
		[NotNull]
		public static IEnumerable<string> EnumerateFiles(string path, string pattern, SearchOption option)
		{
			path = PathHelper.Trim(path) ?? @".\";
			pattern = pattern?.Trim().Replace(';', '|');
			if (string.IsNullOrEmpty(pattern)) pattern = "*";

			bool multiPatterns = pattern.Contains('|');
			if (multiPatterns) pattern = RegexHelper.FromFilePattern(pattern);

			if (pattern == "^*$")
			{
				multiPatterns = false;
				pattern = "*";
			}

			if (!multiPatterns) return Directory.EnumerateFiles(path, pattern, option);

			Regex rgxPattern = new Regex(pattern, RegexHelper.OPTIONS_I);
			return Directory.EnumerateFiles(path, "*", option).Where(e => rgxPattern.IsMatch(e));
		}
	}
}