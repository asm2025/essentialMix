using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using asm.Extensions;
using JetBrains.Annotations;
using CharEnumerator = asm.Collections.CharEnumerator;

namespace asm.Helpers
{
	/// <summary>
	/// As a general rule of thumb, It's recommended to use Path.GetFullPath to normalize it before hitting any shimming method.
	/// </summary>
	public static class PathHelper
	{
		public static readonly string EXTENDED_PATH_PREFIX = @"\\?\";
		public static readonly string UNC_PATH_PREFIX = @"\\";
		public static readonly string UNC_EXTENDED_PREFIX_TO_INSERT = @"?\UNC\";
		public static readonly string UNC_EXTENDED_PATH_PREFIX = @"\\?\UNC\";
		public static readonly string DEVICE_PATH_PREFIX = @"\\.\";
		public static readonly string PARENT_DIRECTORY_PREFIX = @"..\";
		public static readonly int MAX_SHORT_PATH = 260;
		public static readonly int MAX_SHORT_DIRECTORY_PATH = 248;
		// \\?\, \\.\, \??\
		public static readonly int DEVICE_PREFIX_LENGTH = 4;
		// \\
		public static readonly int UNC_PREFIX_LENGTH = 2;
		// \\?\UNC\, \\.\UNC\
		public static readonly int UNC_EXTENDED_PREFIX_LENGTH = 8;

		private static readonly Regex __isPathQualified = new Regex(@"^(?:[\\/][\\/\?][\.\?][\\/])?(?:[\\/]|(?:[a-zA-Z]:)|(?:[\w\-]+))(?:(?<=[\\/])(?:[\w\p{P}\s]+)+)?(?:[\\/](?:[\w\p{P}\s]+)+)*[\\/]*$", RegexHelper.OPTIONS_I | RegexOptions.Singleline);
		
		private static char __directorySeparator = Path.DirectorySeparatorChar;
		private static char __altDirectorySeparator = Path.AltDirectorySeparatorChar;

		public static char DirectorySeparator
		{
			get => __directorySeparator;
			set
			{
				if (value != Path.DirectorySeparatorChar && value != Path.AltDirectorySeparatorChar) throw new InvalidOperationException($"{nameof(DirectorySeparator)} must be one of the characters [{Path.DirectorySeparatorChar}, {Path.AltDirectorySeparatorChar}]");
				__directorySeparator = value;
			}
		}

		public static char AltDirectorySeparator
		{
			get => __altDirectorySeparator;
			set
			{
				if (value != Path.DirectorySeparatorChar && value != Path.AltDirectorySeparatorChar) throw new InvalidOperationException($"{nameof(AltDirectorySeparator)} must be one of the characters [{Path.DirectorySeparatorChar}, {Path.AltDirectorySeparatorChar}]");
				__altDirectorySeparator = value;
			}
		}

		public static string Combine([NotNull] string path, [NotNull] params string[] other)
		{
			List<string> paths = new List<string>();
			if (!string.IsNullOrWhiteSpace(path)) paths.Add(path);

			paths.AddRange(other.SkipNullOrEmptyTrim());
			return paths.Count == 0 ? null : Path.Combine(paths.ToArray());
		}

		public static bool Exists(string path) { return !string.IsNullOrEmpty(path) && Directory.Exists(path) || File.Exists(path); }

		public static string AddDirectorySeparator(string path)
		{
			path = Trim(path);
			return string.IsNullOrEmpty(path)
						? path
						: path.EndsWith(DirectorySeparator)
							? path
							: string.Concat(path, DirectorySeparator);
		}

		public static string Extension(string path, string value = null)
		{
			path = Trim(path) ?? throw new ArgumentNullException(nameof(path));
			
			// return the file extension if value is null
			if (value == null)
			{
				string ext = Path.GetExtension(path);
				return string.IsNullOrEmpty(ext) ? null : ext.Prefix('.');
			}

			string baseName = Path.GetFileNameWithoutExtension(path);
			path = path.ContainsAny(Path.VolumeSeparatorChar, Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
						? Path.Combine(Path.GetDirectoryName(path) ?? string.Empty, baseName)
						: baseName;
			value = Trim(value);
			// return path without extension if value is empty
			return string.IsNullOrEmpty(value)
						? path
						: path + value.Prefix('.');
		}

		[NotNull]
		public static string Compact(string path, uint maxLength)
		{
			if (string.IsNullOrEmpty(path) || maxLength == 0) return string.Empty;
			path = path.Trim();
			if (path.Length < maxLength) return path;

			StringBuilder sb = new StringBuilder((int)(maxLength + 1));
			Win32.PathCompactPathEx(sb, path, maxLength, 0);
			return sb.ToString();
		}

		public static string CommonPrefix(string path1, string path2)
		{
			if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)) return null;

			StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
			int ret = Win32.PathCommonPrefix(path1, path2, sb);
			return ret > 0 ? sb.ToString() : null;
		}

		public static string GetFullPath(string path, bool mustExist = false)
		{
			path = Trim(path);
			if (string.IsNullOrEmpty(path)) return null;
			path = Path.GetFullPath(path);
			if (IsPathQualified(path) && (!mustExist || Exists(path))) return path;

			string paths = Environment.GetEnvironmentVariable("PATH");
			if (string.IsNullOrEmpty(paths) || !paths.Contains(';')) return null;

			using (CharEnumerator parts = paths.Enumerate(';'))
			{
				return parts.Select(p => Path.Combine(p, path)).FirstOrDefault(Exists);
			}
		}

		[NotNull]
		public static string GetRelativePath(string rootPath, string targetPath)
		{
			rootPath = AddDirectorySeparator(rootPath);
			if (string.IsNullOrEmpty(rootPath)) throw new ArgumentNullException(nameof(rootPath));
			targetPath = Trim(targetPath);
			if (string.IsNullOrEmpty(targetPath)) throw new ArgumentNullException(nameof(targetPath));

			if (!Win32.PathIsUNC(rootPath) && !Win32.PathIsUNC(targetPath))
			{
				if (Uri.TryCreate(rootPath, UriKind.Absolute, out Uri root)
					&& Uri.TryCreate(targetPath, UriKind.Absolute, out Uri target))
				{
					string schm1 = root.IsAbsoluteUri ? root.Scheme : null;
					string schm2 = target.IsAbsoluteUri ? target.Scheme : schm1;

					if (schm1 != null && schm1.IsSame(schm2))
					{
						Uri result = root.MakeRelativeUri(target);
						string relativePath = Uri.UnescapeDataString(result.ToString());

						if (target.Scheme.IsSame(Uri.UriSchemeFile)) relativePath = relativePath.Replace(AltDirectorySeparator, DirectorySeparator);
						return relativePath;
					}
				}
			}

			StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
			return Win32.PathRelativePathTo(sb, rootPath, FileAttributes.Directory, targetPath, FileAttributes.Normal)
				? sb.ToString()
				: targetPath;
		}

		public static bool IsPathRooted(string path)
		{
			return !string.IsNullOrWhiteSpace(path) && Path.IsPathRooted(path) && path.IndexOfAny(Path.GetInvalidPathChars()) < 0;
		}

		/// <summary>
		/// Returns true if the path is absolute starting with a drive letter, UNC prefix, or rooted with a directory separator.
		/// It's recommended to normalize the path before checking it or do any shimming operation.
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool IsPathQualified(string path)
		{
			// fu* all the manual validation. Nothing better than a good regex
			return !string.IsNullOrEmpty(path) && __isPathQualified.IsMatch(path);
		}

		public static bool IsPathSeparator(char value) { return value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar || value == '|'; }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDirectorySeparator(char value) { return value == Path.DirectorySeparatorChar || value == Path.AltDirectorySeparatorChar; }

		public static bool IsValidDriveChar(char value) { return value >= 'A' && value <= 'Z' || value >= 'a' && value <= 'z'; }

		public static bool IsValidPathChar(char value) { return !Path.GetInvalidPathChars().Contains(value); }
		
		public static bool EndsWithPeriodOrSpace(string path)
        {
            if (string.IsNullOrEmpty(path)) return false;
            char c = path[path.Length - 1];
            return c == ' ' || c == '.';
        }

        /// <summary>
        /// Adds the extended path prefix (\\?\) if not already a device path, IF the path is not relative,
        /// AND the path is more than 259 characters. (> MAX_PATH + null). This will also insert the extended
        /// prefix if the path ends with a period or a space. Trailing periods and spaces are normally eaten
        /// away from paths during normalization, but if we see such a path at this point it should be
        /// normalized and has retained the final characters. (Typically from one of the *Info classes)
        /// </summary>
        public static string EnsureExtendedPrefixIfNeeded(string path)
		{
			return path != null && (path.Length >= MAX_SHORT_PATH || EndsWithPeriodOrSpace(path))
						? EnsureExtendedPrefix(path)
						: path;
		}

        /// <summary>
        /// Adds the extended path prefix (\\?\) if not relative or already a device path.
        /// If the path might contain invalid characters, then Path.GetFullPath must be used before hitting this shimming method.
        /// </summary>
        public static string EnsureExtendedPrefix(string path)
		{
			// Putting the extended prefix on the path changes the processing of the path. It won't get normalized, which
            // means adding to relative paths will prevent them from getting the appropriate current directory inserted.

            // If it already has some variant of a device path (\??\, \\?\, \\.\, //./, etc.) we don't need to change it
            // as it is either correct or we will be changing the behavior. When/if Windows supports long paths implicitly
            // in the future we wouldn't want normalization to come back and break existing code.

            // In any case, all internal usages should be hitting normalize path (Path.GetFullPath) before they hit this
            // shimming method. (Or making a change that doesn't impact normalization, such as adding a filename to a
            // normalized base path.)
            if (IsPathQualified(path) || IsDevice(path))
                return path;

            // Given \\server\share in long path becomes \\?\UNC\server\share
			return path.StartsWith(UNC_PATH_PREFIX, StringComparison.OrdinalIgnoreCase)
						? path.Insert(2, UNC_EXTENDED_PREFIX_TO_INSERT)
						: EXTENDED_PATH_PREFIX + path;
		}

		/// <summary>
		/// Returns true if the path is a drive (C:, C:\)
		/// </summary>
		public static bool IsDrive(string path)
		{
			return path != null
					&& path.Length >= 2
					&& IsValidDriveChar(path[0])
					&& IsDirectorySeparator(path[1])
					&& (path.Length == 2 || IsDirectorySeparator(path[2]));
		}

        /// <summary>
        /// Returns true if the path uses any of the DOS device path syntax. ("\\.\", "\\?\", or "\??\")
        /// </summary>
        public static bool IsDevice(string path)
        {
            // If the path begins with any two separators is will be recognized and normalized and prepped with
            // "\??\" for internal usage correctly. "\??\" is recognized and handled, "/??/" is not.
            return IsExtended(path)
				|| path != null
				&& path.Length >= DEVICE_PREFIX_LENGTH
				&& IsDirectorySeparator(path[0])
				&& IsDirectorySeparator(path[1])
				&& (path[2] == '.' || path[2] == '?')
				&& IsDirectorySeparator(path[3]);
        }

        /// <summary>
        /// Returns true if the path is a device UNC (\\?\UNC\, \\.\UNC\)
        /// </summary>
        public static bool IsDeviceUNC(string path)
        {
			return path != null
				&& path.Length >= UNC_EXTENDED_PREFIX_LENGTH
                && IsDevice(path)
                && IsDirectorySeparator(path[7])
                && path[4] == 'U'
                && path[5] == 'N'
                && path[6] == 'C';
        }

        /// <summary>
        /// Returns true if the path uses the canonical form of extended syntax ("\\?\" or "\??\"). If the
        /// path matches exactly (cannot use alternate directory separators) Windows will skip normalization
        /// and path length checks.
        /// </summary>
        public static bool IsExtended(string path)
        {
            // While paths like "//?/C:/" will work, they're treated the same as "\\.\" paths.
            // Skipping of normalization will *only* occur if back slashes ('\') are used.
			return path != null
				&& path.Length >= DEVICE_PREFIX_LENGTH
                && path[0] == '\\'
                && (path[1] == '\\' || path[1] == '?')
                && path[2] == '?'
                && path[3] == '\\';
        }

        /// <summary>
        /// Check for known wildcard characters. '*' and '?' are the most common ones.
        /// </summary>
        public static bool HasWildCardCharacters([NotNull] string path)
        {
            // Question mark is part of dos device syntax so we have to skip if we are
            int startIndex = IsDevice(path) ? EXTENDED_PATH_PREFIX.Length : 0;

            // [MS - FSA] 2.1.4.4 Algorithm for Determining if a FileName Is in an Expression
            // https://msdn.microsoft.com/en-us/library/ff469270.aspx
            for (int i = startIndex; i < path.Length; i++)
            {
                char c = path[i];

                if (c <= '?') // fast path for common case - '?' is highest wildcard character
                {
                    if (c == '\"' || c == '<' || c == '>' || c == '*' || c == '?')
                        return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the length of the root of the path (drive, share, etc.).
        /// </summary>
        public static int GetRootLength([NotNull] string path)
        {
            int pathLength = path.Length;
            int i = 0;

            bool deviceSyntax = IsDevice(path);
            bool deviceUnc = deviceSyntax && IsDeviceUNC(path);

            if ((!deviceSyntax || deviceUnc) && pathLength > 0 && IsDirectorySeparator(path[0]))
            {
                // UNC or simple rooted path (e.g. "\foo", NOT "\\?\C:\foo")
                if (deviceUnc || pathLength > 1 && IsDirectorySeparator(path[1]))
                {
                    // UNC (\\?\UNC\ or \\), scan past server\share

                    // Start past the prefix ("\\" or "\\?\UNC\")
                    i = deviceUnc ? UNC_EXTENDED_PREFIX_LENGTH : UNC_PREFIX_LENGTH;

                    // Skip two separators at most
                    int n = 2;

                    while (i < pathLength && (!IsDirectorySeparator(path[i]) || --n > 0))
                        i++;
                }
                else
                {
                    // Current drive rooted (e.g. "\foo")
                    i = 1;
                }
            }
            else if (deviceSyntax)
            {
                // Device path (e.g. "\\?\.", "\\.\")
                // Skip any characters following the prefix that aren't a separator
                i = DEVICE_PREFIX_LENGTH;

                while (i < pathLength && !IsDirectorySeparator(path[i]))
                    i++;

                // If there is another separator take it, as long as we have had at least one
                // non-separator after the prefix (e.g. don't take "\\?\\", but take "\\?\a\")
                if (i < pathLength && i > DEVICE_PREFIX_LENGTH && IsDirectorySeparator(path[i]))
                    i++;
            }
            else if (pathLength >= 2
                && path[1] == Path.VolumeSeparatorChar
                && IsValidDriveChar(path[0]))
            {
                // Valid drive specified path ("C:", "D:", etc.)
                i = 2;

                // If the colon is followed by a directory separator, move past it (e.g "C:\")
                if (pathLength > 2 && IsDirectorySeparator(path[2]))
                    i++;
            }

            return i;
        }

        /// <summary>
        /// Normalize separators in the given path. Converts forward slashes into back slashes and compresses slash runs, keeping initial 2 if present.
        /// Also trims initial whitespace in front of "rooted" paths (see PathStartSkip).
        /// 
        /// This effectively replicates the behavior of the legacy NormalizePath when it was called with fullCheck=false and expandShortpaths=false.
        /// The current NormalizePath gets directory separator normalization from Win32's GetFullPathName(), which will resolve relative paths and as
        /// such can't be used here (and is overkill for our uses).
        /// 
        /// Like the current NormalizePath this will not try and analyze periods/spaces within directory segments.
        /// </summary>
        /// <remarks>
        /// The only callers that used to use Path.Normalize(fullCheck=false) were Path.GetDirectoryName() and Path.GetPathRoot(). Both usages do
        /// not need trimming of trailing whitespace here.
        /// 
        /// GetPathRoot() could technically skip normalizing separators after the second segment- consider as a future optimization.
        /// 
        /// For legacy desktop behavior with ExpandShortPaths:
        ///  - It has no impact on GetPathRoot() so doesn't need consideration.
        ///  - It could impact GetDirectoryName(), but only if the path isn't relative (C:\ or \\Server\Share).
        /// 
        /// In the case of GetDirectoryName() the ExpandShortPaths behavior was undocumented and provided inconsistent results if the path was
        /// fixed/relative. For example: "C:\PROGRA~1\A.TXT" would return "C:\Program Files" while ".\PROGRA~1\A.TXT" would return ".\PROGRA~1". If you
        /// ultimately call GetFullPath() this doesn't matter, but if you don't or have any intermediate string handling could easily be tripped up by
        /// this undocumented behavior.
        /// 
        /// We won't match this old behavior because:
        /// 
        ///   1. It was undocumented
        ///   2. It was costly (extremely so if it actually contained '~')
        ///   3. Doesn't play nice with string logic
        ///   4. Isn't a cross-plat friendly concept/behavior
        /// </remarks>
        public static string NormalizeDirectorySeparators(string path)
        {
            if (string.IsNullOrEmpty(path)) return path;

            char current;
            // Make a pass to see if we need to normalize so we can potentially skip allocating
            bool normalized = true;

            for (int i = 0; i < path.Length; i++)
            {
                current = path[i];

                if (IsDirectorySeparator(current)
                    && (current != Path.DirectorySeparatorChar
                        // Check for sequential separators past the first position (we need to keep initial two for UNC/extended)
                        || i > 0 && i + 1 < path.Length && IsDirectorySeparator(path[i + 1])))
                {
                    normalized = false;
                    break;
                }
            }

            if (normalized) return path;

            StringBuilder builder = new StringBuilder(path.Length);
            int start = 0;

            if (IsDirectorySeparator(path[start]))
            {
                start++;
                builder.Append(Path.DirectorySeparatorChar);
            }

            for (int i = start; i < path.Length; i++)
            {
                current = path[i];

                // If we have a separator
                if (IsDirectorySeparator(current))
                {
                    // If the next is a separator, skip adding this
                    if (i + 1 < path.Length && IsDirectorySeparator(path[i + 1]))
                    {
                        continue;
                    }

                    // Ensure it is the primary separator
                    current = Path.DirectorySeparatorChar;
                }

                builder.Append(current);
            }

            return builder.ToString();
        }

		public static bool IsSameRoot(string path1, string path2)
		{
			if (string.IsNullOrEmpty(path1) || string.IsNullOrEmpty(path2)) return false;
			return Win32.PathIsSameRoot(path1, path2);
		}

		public static string UrlToPath(string url)
		{
			url = UriHelper.Trim(url);
			if (string.IsNullOrEmpty(url)) return url;

			uint sz = Win32.INTERNET_MAX_URL_LENGTH;
			StringBuilder sb = new StringBuilder((int)sz);
			int ret = Win32.PathCreateFromUrl(url, sb, ref sz, 0);
			return ret != ResultCom.S_OK ? null : sb.ToString();
		}

		public static string PathToUrl(string path)
		{
			path = Trim(path);
			if (string.IsNullOrEmpty(path)) return path;

			uint sz = Win32.INTERNET_MAX_URL_LENGTH;
			StringBuilder sb = new StringBuilder((int)sz);
			int ret = Win32.UrlCreateFromPath(path, sb, ref sz, 0);
			return ret != ResultCom.S_OK ? null : sb.ToString();
		}

		public static string NextComponent(string path)
		{
			return string.IsNullOrEmpty(path)
						? null
						: Win32.PathFindNextComponent(path);
		}

		public static bool IsUnc(string path) { return !string.IsNullOrEmpty(path) && Win32.PathIsUNC(path); }

		public static bool IsValidPath(string path, params char[] includeChars)
		{
			if (string.IsNullOrWhiteSpace(path) || path.StartsWith(' ') || path.EndsWith(' ')) return false;
			if (includeChars != null && includeChars.Length > 0 && path.ContainsAny(includeChars)) return false;
			return !path.ContainsAny(Path.GetInvalidPathChars());
		}

		[NotNull]
		public static string UniqueName(string path)
		{
			path = Trim(path);
			if (string.IsNullOrEmpty(path)) throw new ArgumentNullException(nameof(path));

			string fileName;

			if (path.Contains(Path.DirectorySeparatorChar))
			{
				fileName = Path.GetFileName(path);
				path = Path.GetDirectoryName(path);
			}
			else
			{
				fileName = path;
				path = null;
			}

			return UniqueName(path, fileName);
		}

		[NotNull]
		public static string UniqueName(string path, string fileOrDirectoryName)
		{
			fileOrDirectoryName = Trim(fileOrDirectoryName);
			if (string.IsNullOrEmpty(fileOrDirectoryName)) throw new ArgumentNullException(nameof(fileOrDirectoryName));
			if (path != null && path != Path.DirectorySeparatorChar.ToString()) path = path.Trim(Path.DirectorySeparatorChar, ' ');

			if (string.IsNullOrEmpty(path))
			{
				path = Directory.GetCurrentDirectory();
			}
			else if (IsPathQualified(fileOrDirectoryName))
			{
				path = Path.GetDirectoryName(fileOrDirectoryName);
				fileOrDirectoryName = Path.GetFileName(fileOrDirectoryName);
			}
			else
			{
				path = GetFullPath(path);
			}

			path = AddDirectorySeparator(path);
			string fullPath = path + fileOrDirectoryName;

			if (Exists(fullPath))
			{
				string ext = Path.GetExtension(fileOrDirectoryName);
				fileOrDirectoryName = Path.GetFileNameWithoutExtension(fileOrDirectoryName);

				for (int i = 1; Exists(fullPath); ++i)
					fullPath = $"{path}{fileOrDirectoryName} ({i}){ext}";
			}

			return fullPath;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Trim(string path) { return path?.Trim(DirectorySeparator, AltDirectorySeparator, ' ').ToNullIfEmpty(); }
	}
}