using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class FileHelper
	{
		private static readonly Regex RESERVED_NAMES = new Regex(@"^(?:PRN|AUX|CLOCK\$|NUL|CON|COM\d{1,3}|LPT\d{1,3})$", RegexHelper.OPTIONS_I);
		private static readonly char[] INVALID_FILE_NAME_CHAR = Path.GetInvalidFileNameChars();
		private static readonly Regex VALID_NAME = new Regex(@"^(?:\.*?(?!\.))[^\x00-\x1f\?*:\x22;|/<>]+(?<![\s.])$", RegexHelper.OPTIONS_I);
		private static readonly char[] INVALID_PATH_CHAR = Path.GetInvalidPathChars();
		private static readonly Regex VALID_PATH = new Regex(@"^(?:[a-z]:\\)?(?:(?:\.*?(?!\.))[^\x00-\x1f\?*:\x22;|/<>]+(?<![\s.])\\?)+$", RegexHelper.OPTIONS_I);

		public static int RemoveLines([NotNull] string fileName, int count, Encoding encoding = null, int startAtLine = 0)
		{
			if (count < 1) return 0;
			startAtLine = startAtLine.NotBelow(0);

			int result = 0;
			int skip = startAtLine;
			string path;
			string name;

			if (PathHelper.IsPathQualified(fileName))
			{
				path = Path.GetDirectoryName(fileName) ?? string.Empty;
				name = Path.GetFileNameWithoutExtension(fileName);
			}
			else
			{
				path = string.Empty;
				name = Path.GetFileNameWithoutExtension(fileName);
			}

			string newName = Path.Combine(path, string.Concat("___", name, ".tmp"));

			try
			{
				using (Stream stream = File.OpenRead(fileName))
				{
					if (encoding == null) encoding = stream.DetectEncoding();

					using (StreamReader reader = new StreamReader(stream, encoding, true))
					{
						if (skip > 0)
						{
							reader.BaseStream.Seek(0, SeekOrigin.Begin);
							while (skip-- > 0 && !reader.EndOfStream) reader.ReadLine();
						}

						if (reader.EndOfStream) return 0;

						int linesToDelete = count;

						using (StreamWriter writer = new StreamWriter(newName, false, reader.CurrentEncoding))
						{
							while (linesToDelete-- > 0 && !reader.EndOfStream)
							{
								reader.ReadLine();
								result++;
							}

							string line;

							while ((line = reader.ReadLine()) != null)
							{
								if (reader.EndOfStream) writer.Write(line);
								else writer.WriteLine(line);
							}
						}
					}
				}

				Delete(fileName);
				File.Move(newName, fileName);
			}
			catch
			{
				result = -1;
			}

			return result;
		}

		public static int MoveLines([NotNull] string fileName, [NotNull] string destination, int count, Encoding encoding = null)
		{
			if (!File.Exists(fileName)) throw new FileNotFoundException("File not found.", fileName);
			if (count == 0) throw new ArgumentOutOfRangeException(nameof(count), "This must be a joke, right?");

			int result;

			try
			{
				using (Stream stream = File.OpenRead(fileName))
				{
					if (encoding == null) encoding = stream.DetectEncoding();

					using (StreamReader reader = new StreamReader(stream, encoding, true))
					{
						IList<string> lines = new List<string>();
						result = reader.ReadLines(lines, count);
						if (result < 1) return result;

						using (StreamWriter writer = new StreamWriter(destination, false, reader.CurrentEncoding))
						{
							result = writer.WriteLines(lines);
						}

						if (result < 1) return result;
					}
				}

				result = RemoveLines(fileName, count);
			}
			catch
			{
				result = -1;
			}

			return result;
		}

		public static int ReadLines([NotNull] string fileName, IList<string> list, int count, int startAtLine = 0, Encoding encoding = null)
		{
			if (count < 1) return 0;

			int result;

			try
			{
				using (Stream stream = File.OpenRead(fileName))
				{
					if (encoding == null) encoding = stream.DetectEncoding();

					using (StreamReader reader = new StreamReader(stream, encoding, true))
					{
						result = reader.ReadLines(list, count, startAtLine);
					}
				}
			}
			catch
			{
				result = -1;
			}

			return result;
		}

		public static int ReadLines([NotNull] string fileName, ISet<string> set, int count, int startAtLine = 0, Encoding encoding = null)
		{
			if (count < 1) return 0;

			int result;

			try
			{
				using (Stream stream = File.OpenRead(fileName))
				{
					if (encoding == null) encoding = stream.DetectEncoding();

					using (StreamReader reader = new StreamReader(stream, encoding, true))
					{
						result = reader.ReadLines(set, count, startAtLine);
					}
				}
			}
			catch
			{
				result = -1;
			}

			return result;
		}

		public static string ToValidPathName([NotNull] string value, char replaceChar = '_', char[] includeChars = null)
		{
			if (string.IsNullOrWhiteSpace(value)) return null;

			char[] invalid = includeChars?.Union(INVALID_FILE_NAME_CHAR).ToArray() ?? INVALID_FILE_NAME_CHAR;
			return value.Aggregate(new StringBuilder(value.Length),
				(builder, c) => builder.Append(invalid.Contains(c) ? replaceChar : c),
				builder => builder.ToString());
		}

		public static bool IsValidPathName(string value, params char[] moreInvalidChars)
		{
			if (string.IsNullOrWhiteSpace(value) || !VALID_NAME.IsMatch(value) || RESERVED_NAMES.IsMatch(value)) return false;

			char[] invalid = moreInvalidChars.IsNullOrEmpty() ? INVALID_FILE_NAME_CHAR : moreInvalidChars.Union(INVALID_FILE_NAME_CHAR).ToArray();
			if (value.ContainsAny(invalid)) return false;

			FileInfo fi;

			try
			{
				fi = new FileInfo(value);
			}
			catch
			{
				fi = null;
			}

			return fi != null;
		}

		public static bool IsValidNameChar(char value) { return !INVALID_FILE_NAME_CHAR.Contains(value); }

		public static bool IsValidPath(string value, params char[] moreInvalidChars)
		{
			if (string.IsNullOrWhiteSpace(value) || !VALID_PATH.IsMatch(value) || RESERVED_NAMES.IsMatch(value)) return false;

			char[] invalid = moreInvalidChars.IsNullOrEmpty() ? INVALID_PATH_CHAR : moreInvalidChars.Union(INVALID_PATH_CHAR).ToArray();
			return !value.ContainsAny(invalid);
		}

		public static bool Join([NotNull] string fileName, [NotNull] params string[] sources) { return Join(fileName, EncodingHelper.Default, sources); }

		public static bool Join([NotNull] string fileName, Encoding encoding, [NotNull] params string[] sources)
		{
			if (string.IsNullOrEmpty(fileName)) throw new ArgumentNullException(nameof(fileName));
			if (sources.IsNullOrEmpty()) throw new ArgumentNullException(nameof(sources));

			foreach (string source in sources.Where(s => !string.IsNullOrEmpty(s)))
			{
				if (!File.Exists(source)) throw new FileNotFoundException("Cannot find one of the source files.", source);

				using (Stream stream = File.OpenRead(source))
				{
					if (encoding == null) encoding = stream.DetectEncoding();

					using (StreamReader reader = new StreamReader(stream, encoding, true))
					{
						using (StreamWriter writer = new StreamWriter(fileName, true, reader.CurrentEncoding))
						{
							string line;

							while ((line = reader.ReadLine()) != null)
							{
								writer.WriteLine(line);
							}
						}
					}
				}
			}

			return true;
		}

		public static int UniqueSplitByLines([NotNull] string fileName, uint linesPerFile, Encoding encoding = null, IEqualityComparer<string> comparer = null)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));
			if (linesPerFile == 0) throw new ArgumentOutOfRangeException(nameof(linesPerFile), "This must be a joke, right?");

			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists) throw new FileNotFoundException("File not found.", fileName);
			if (fi.Length == 0) return 0;

			int n = -1;
			string pattern = string.Concat(Path.GetFileNameWithoutExtension(fi.Name), "_split{0}", fi.Extension);

			using (Stream stream = File.OpenRead(fileName))
			{
				if (encoding == null) encoding = stream.DetectEncoding();

				using (StreamReader reader = new StreamReader(stream, encoding, true))
				{
					HashSet<string> set = new HashSet<string>(comparer ?? StringComparer.CurrentCultureIgnoreCase);

					while (!reader.EndOfStream)
					{
						uint lines = 0;
						string name = string.Format(pattern, n++);
						set.Clear();

						using (StreamWriter writer = new StreamWriter(name, false, reader.CurrentEncoding))
						{
							string line;

							while (lines < linesPerFile && !reader.EndOfStream && (line = reader.ReadLine()) != null)
							{
								if (line.Length == 0 || !set.Add(line)) continue;
								writer.WriteLine(line);
								lines++;
							}
						}
					}
				}
			}

			return n;
		}

		public static int SplitByLines([NotNull] string fileName, uint linesPerFile, Encoding encoding = null)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));
			if (linesPerFile == 0) throw new ArgumentOutOfRangeException(nameof(linesPerFile), "This must be a joke, right?");

			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists) throw new FileNotFoundException("File not found.", fileName);
			if (fi.Length == 0) return 0;

			int n = -1;
			string pattern = string.Concat(Path.GetFileNameWithoutExtension(fi.Name), "_split{0}", fi.Extension);

			using (Stream stream = File.OpenRead(fileName))
			{
				if (encoding == null) encoding = stream.DetectEncoding();

				using (StreamReader reader = new StreamReader(stream, encoding, true))
				{
					while (!reader.EndOfStream)
					{
						uint lines = 0;
						string name = string.Format(pattern, n++);

						using (StreamWriter writer = new StreamWriter(name, false, reader.CurrentEncoding))
						{
							string line;

							while (lines < linesPerFile && !reader.EndOfStream && (line = reader.ReadLine()) != null)
							{
								if (line.Length == 0) continue;
								writer.WriteLine(line);
								lines++;
							}
						}
					}
				}
			}

			return n;
		}

		public static int SplitByCount([NotNull] string fileName, int count, Encoding encoding = null)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "This must be a joke, right?");

			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists) throw new FileNotFoundException("File not found.", fileName);
			if (fi.Length == 0) return 0;

			uint lines = 0;

			foreach (string line in File.ReadLines(fileName))
			{
				if (string.IsNullOrEmpty(line)) continue;
				lines++;
			}

			if (lines == 0) return 0;

			uint lpf = (uint)Math.Ceiling((double)lines / count);
			return SplitByLines(fileName, lpf, encoding);
		}

		public static int BinarySplitBySize([NotNull] string fileName, long sizePerFile, Encoding encoding = null)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));
			if (sizePerFile == 0) throw new ArgumentOutOfRangeException(nameof(sizePerFile), "This must be a joke, right?");

			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists) throw new FileNotFoundException("File not found.", fileName);
			if (fi.Length == 0) return 0;

			int n = -1;
			string pattern = string.Concat(Path.GetFileNameWithoutExtension(fi.Name), "_split{0}", fi.Extension);

			using (Stream stream = File.OpenRead(fileName))
			{
				if (encoding == null) encoding = stream.DetectEncoding();

				using (BinaryReader reader = new BinaryReader(stream, encoding))
				{
					int bufferLen = (int)Math.Min(sizePerFile, Constants.GetBufferKB(20));
					byte[] buffer = new byte[bufferLen];

					while (!reader.EndOfStream())
					{
						long bytes = 0;
						string name = string.Format(pattern, n++);

						using (BinaryWriter writer = new BinaryWriter(File.OpenWrite(name), encoding))
						{
							int r;

							while (bytes < sizePerFile && !reader.EndOfStream() && (r = reader.Read(buffer)) > 0)
							{
								writer.Write(buffer);
								bytes += r;
							}
						}
					}
				}
			}

			return n;
		}

		public static int BinarySplitByCount([NotNull] string fileName, int count, Encoding encoding = null)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));
			if (count < 1) throw new ArgumentOutOfRangeException(nameof(count), "This must be a joke, right?");

			FileInfo fi = new FileInfo(fileName);
			if (!fi.Exists) throw new FileNotFoundException("File not found.", fileName);
			if (fi.Length == 0) return 0;

			long lpf = (long)Math.Ceiling((double)fi.Length / count);
			return BinarySplitBySize(fileName, lpf, encoding);
		}

		public static bool Copy([NotNull] string fileName, [NotNull] string destination, bool overwrite = false)
		{
			try
			{
				if (!File.Exists(fileName)) return false;
				File.Copy(fileName, destination, overwrite);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool Move([NotNull] string fileName, [NotNull] string destination, bool overwrite = false)
		{
			try
			{
				if (!File.Exists(fileName)) return false;
				if (overwrite && File.Exists(destination)) File.Delete(destination);
				File.Move(fileName, destination);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool Rename([NotNull] string fileName, [NotNull] string destination, bool overwrite = false) { return Move(fileName, destination, overwrite); }

		public static bool Delete([NotNull] string fileName)
		{
			if (!File.Exists(fileName)) return true;

			try
			{
				File.Delete(fileName);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool Ensure([NotNull] string fileName, bool empty = false)
		{
			if (string.IsNullOrWhiteSpace(fileName)) throw new ArgumentNullException(nameof(fileName));

			try
			{
				FileInfo file = new FileInfo(fileName);

				if (file.Exists)
				{
					if (!empty) return true;
					file.Delete();
				}

				using (file.CreateText()) { }
				return true;
			}
			catch
			{
				return false;
			}
		}

		public static bool HasContents([NotNull] string fileName) { return GetLength(fileName) > 0; }

		public static long GetLength([NotNull] string fileName)
		{
			FileInfo file = new FileInfo(fileName);
			return file.Exists ? file.Length : -1;
		}

		public static bool TryGetLength([NotNull] string fileName, out long length)
		{
			try
			{
				length = GetLength(fileName);
				return true;
			}
			catch
			{
				length = -1;
				return false;
			}
		}

		public static void ResetFiles(string extension, [NotNull] params string[] files)
		{
			if (files.IsNullOrEmpty()) return;
			extension = extension.ToNullIfEmpty() ?? throw new ArgumentNullException(nameof(extension));
			if (extension.ContainsAny(INVALID_FILE_NAME_CHAR)) throw new InvalidOperationException("Extension contains invalid characters.");
			if (!extension.StartsWith('.')) extension = extension.Insert(0, ".");

			foreach (string file in files)
			{
				string extFile = string.Concat(file, extension);
				if (!File.Exists(extFile)) continue;

				try
				{
					File.Delete(file);
					File.Move(extFile, file);
					File.Delete(extFile);
				}
				catch
				{
				}
			}
		}

		public static Encoding DetectEncoding([NotNull] string fileName)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));

			using (Stream stream = File.OpenRead(fileName))
			{
				return stream.DetectEncoding();
			}
		}

		public static bool TryDetectEncoding([NotNull] string fileName, out Encoding encoding)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));

			try
			{
				using (Stream stream = File.OpenRead(fileName))
				{
					encoding = stream.DetectEncoding();
				}
			}
			catch
			{
				encoding = null;
			}

			return encoding != null;
		}

		public static bool ConvertToEncoding([NotNull] string fileName, [NotNull] Encoding encoding, [NotNull] Func<string, string, string, bool> onConfirm)
		{

			if (TryDetectEncoding(fileName, out Encoding enc) && !enc.Equals(encoding))
			{
				if (!onConfirm(fileName, enc.EncodingName, encoding.EncodingName)) return false;
			}
			else
			{
				enc = null;
			}

			return ConvertToEncoding(fileName, encoding, enc);
		}

		public static bool ConvertToEncoding([NotNull] string fileName, [NotNull] Encoding encoding,  Encoding detectedEncoding = null)
		{
			if (fileName.Length == 0) throw new ArgumentNullException(nameof(fileName));

			long len = GetLength(fileName);
			if (len < 0) return false;

			int i = 0;
			string tmpName = $"{fileName}.tmp";

			while (File.Exists(tmpName))
			{
				tmpName = $"{fileName} ({++i}).tmp";
			}

			if (len <= int.MaxValue)
			{
				string content = detectedEncoding == null ? File.ReadAllText(fileName) : File.ReadAllText(fileName, detectedEncoding);
				File.WriteAllText(tmpName, content, encoding);
			}
			else
			{
				using (StreamReader reader = detectedEncoding == null ? new StreamReader(fileName) : new StreamReader(fileName, detectedEncoding, true))
				{
					using (StreamWriter writer = new StreamWriter(tmpName, true, encoding) {AutoFlush = true})
					{
						string line;

						while (!reader.EndOfStream && (line = reader.ReadLine()) != null)
						{
							writer.WriteLine(line);
						}
					}
				}
			}

			return Rename(tmpName, fileName, true);
		}

		public static long CountLines([NotNull] string fileName, Encoding encoding = null)
		{
			if (!File.Exists(fileName)) return -1;

			using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return stream.CountLines(encoding);
			}
		}

		public static long CountChar([NotNull] string fileName, char c, Encoding encoding = null)
		{
			if (!File.Exists(fileName)) return -1;

			using (FileStream stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return stream.CountChar(c, encoding);
			}
		}

		public static string Find(string name, [NotNull] params string[] searchPaths)
		{
			name = name?.Trim();
			if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));
			if (!PathHelper.IsValidPath(name)) throw new ArgumentException($"{nameof(name)} is invalid.");
			
			if (PathHelper.IsPathQualified(name))
			{
				return File.Exists(name)
							? PathHelper.AddDirectorySeparator(Path.GetDirectoryName(name))
							: null;
			}

			if (searchPaths.Length == 0) throw new ArgumentException($"{nameof(searchPaths)} is empty.");

			foreach (string path in searchPaths)
			{
				if (string.IsNullOrEmpty(path)) continue;

				if (EnvironmentHelper.HasVar(path))
				{
					string envVar = Environment.ExpandEnvironmentVariables(path);
					if (string.IsNullOrEmpty(envVar) || !File.Exists(Path.Combine(envVar, name))) continue;
					return PathHelper.AddDirectorySeparator(envVar);
				}

				if (path.IsSame("%PATH%"))
				{
					string envPath = Environment.ExpandEnvironmentVariables("%PATH%");
					if (string.IsNullOrEmpty(envPath)) continue;

					foreach (string path2 in envPath.Enumerate(';'))
					{
						if (string.IsNullOrEmpty(path2) || !File.Exists(Path.Combine(path2, name))) continue;
						return PathHelper.AddDirectorySeparator(path2);
					}

					continue;
				}

				if (!File.Exists(Path.Combine(path, name))) continue;
				return PathHelper.AddDirectorySeparator(path);
			}

			return null;
		}

		public static string GetRandomName() { return GetRandomName(true); }

		public static string GetRandomName(bool useTempDirectory) { return GetRandomName(null, useTempDirectory); }

		public static string GetRandomName(string extension) { return GetRandomName(extension, true); }

		[NotNull]
		public static string GetRandomName(string extension, bool useTempDirectory) { return GetRandomName(null, extension, useTempDirectory); }

		[NotNull]
		public static string GetRandomName(string basePath, string extension) { return GetRandomName(basePath, extension, true); }

		[NotNull]
		private static string GetRandomName(string basePath, string extension, bool useTempDirectory)
		{
			StringBuilder sPath = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(basePath) && Directory.Exists(basePath))
				sPath.Append(basePath);
			else if (useTempDirectory)
				sPath.Append(Path.GetTempPath());

			StringBuilder sb = new StringBuilder();
			StringBuilder sbExt = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(extension))
			{
				sbExt.Append(extension.Trim().ToLowerInvariant().Remove(Path.GetInvalidFileNameChars()));
				sbExt.Postfix('.');
			}
			else
			{
				sbExt.Append(".tmp");
			}

			if (sPath.Length > 0) sPath.Suffix('\\');

			TryPath:

			sb.Length = 0;

			for (int i = 0; i <= 8; i++)
			{
				bool bNum = RandomHelper.Default.Next(1, 2) == 2;

				if (bNum)
					sb.Append(RandomHelper.Default.Next(0, 9).ToString(CultureInfo.InvariantCulture));
				else
					sb.Append((char)RandomHelper.Default.Next('a', 'z'));
			}

			if (sPath.Length > 0) sb.Insert(0, sPath.ToString());
			if (sbExt.Length > 0) sb.Append(sbExt);

			string newName = sb.ToString();

			if (sPath.Length > 0 && File.Exists(newName))
				goto TryPath;

			return newName;
		}

		public static string GetRandomName(string basePath, string extension, string suffix) { return GetRandomName(basePath, extension, null, suffix); }

		[NotNull]
		public static string GetRandomName(string basePath, string extension, string prefix, string suffix) { return GetRandomName(basePath, null, extension, prefix, suffix); }

		[NotNull]
		public static string GetRandomName(string basePath, string name, string extension, string prefix, string suffix)
		{
			StringBuilder sPath = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(basePath) && Directory.Exists(basePath)) sPath.Append(basePath);

			StringBuilder sb = new StringBuilder();
			StringBuilder sbExt = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(extension))
			{
				sbExt.Append(extension.Trim().ToLowerInvariant().Remove(Path.GetInvalidFileNameChars()));
				sbExt.Postfix('.');
			}
			else
			{
				sbExt.Append(".tmp");
			}

			if (sPath.Length > 0) sPath.Suffix('\\');

			bool hasName = !string.IsNullOrEmpty(name);
			int fileCount = 0;

			TryPath:

			sb.Length = 0;

			StringBuilder sbName = new StringBuilder();

			if (hasName)
			{
				sbName.Append(name);
			}
			else
			{
				for (int i = 0; i <= 8; i++)
				{
					bool bNum = RandomHelper.Default.Next(1, 2) == 2;

					if (bNum)
						sbName.Append(RandomHelper.Default.Next(0, 9).ToString(CultureInfo.InvariantCulture));
					else
						sbName.Append((char)RandomHelper.Default.Next('a', 'z'));
				}
			}

			if (!string.IsNullOrEmpty(prefix)) sbName.Insert(0, prefix + "_");
			if (!string.IsNullOrEmpty(suffix)) sbName.AppendFormat("_{0}", suffix);

			if (hasName)
			{
				if (fileCount > 0) sbName.AppendFormat(" ({0})", fileCount);
				fileCount++;
			}

			if (sPath.Length > 0) sb.Insert(0, sPath);
			sb.Append(sbName);
			if (sbExt.Length > 0) sb.Append(sbExt);

			string newName = sb.ToString();

			if (sPath.Length > 0 && File.Exists(newName))
				goto TryPath;

			return newName;
		}

		public static string GetRandomGuidName() { return GetRandomGuidName(true); }

		public static string GetRandomGuidName(bool useTempDirectory) { return GetRandomGuidName(null, useTempDirectory); }

		public static string GetRandomGuidName(string sExtension) { return GetRandomGuidName(sExtension, true); }

		[NotNull]
		public static string GetRandomGuidName(string sExtension, bool useTempDirectory) { return GetRandomGuidName(null, sExtension, useTempDirectory); }

		[NotNull]
		public static string GetRandomGuidName(string basePath, string extension) { return GetRandomGuidName(basePath, extension, true); }

		[NotNull]
		private static string GetRandomGuidName(string basePath, string extension, bool useTempDirectory)
		{
			StringBuilder sPath = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(basePath) && Directory.Exists(basePath))
				sPath.Append(basePath);
			else if (useTempDirectory)
				sPath.Append(Path.GetTempPath());

			StringBuilder sb = new StringBuilder();
			StringBuilder sbExt = new StringBuilder();

			if (!string.IsNullOrWhiteSpace(extension))
			{
				sbExt.Append(extension.Trim().ToLowerInvariant().Remove(Path.GetInvalidFileNameChars()));
				sbExt.Postfix('.');
			}
			else
			{
				sbExt.Append(".tmp");
			}

			if (sPath.Length > 0) sPath.Suffix('\\');

			TryPath:

			sb.Length = 0;
			sb.Append(Guid.NewGuid().ToString("D").Replace('-', '_'));
			if (sPath.Length > 0) sb.Insert(0, sPath.ToString());
			if (sbExt.Length > 0) sb.Append(sbExt);

			string newName = sb.ToString();

			if (sPath.Length > 0 && File.Exists(newName))
				goto TryPath;

			return newName;
		}

		[NotNull]
		public static string GetTempName() { return GetTempName(null, null, 0); }

		[NotNull]
		public static string GetTempName(string basePath) { return GetTempName(basePath, null, 0); }

		[NotNull]
		public static string GetTempName(string basePath, string prefix) { return GetTempName(basePath, prefix, 0); }

		[NotNull]
		public static string GetTempName(string basePath, string prefix, uint unique)
		{
			basePath = PathHelper.Trim(basePath);
			if (string.IsNullOrEmpty(basePath)) basePath = Path.GetTempPath();

			StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
			uint ret = Win32.GetTempFileName(basePath, prefix, unique, sb);
			if (ret == 0) throw new Win32Exception(Marshal.GetLastWin32Error());
			return sb.ToString();
		}

		[NotNull]
		public static FileStream GetTempStream() { return GetTempStream(null, null, 0); }

		[NotNull]
		public static FileStream GetTempStream(string basePath) { return GetTempStream(basePath, null, 0); }

		[NotNull]
		public static FileStream GetTempStream(string basePath, string prefix) { return GetTempStream(basePath, prefix, 0); }

		[NotNull]
		public static FileStream GetTempStream(string basePath, string prefix, uint unique)
		{
			string path = GetTempName(basePath, prefix, unique);
			return File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
		}
	}
}