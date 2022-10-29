using System;
using System.IO;
using System.Text;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;
using EMPathHelper = essentialMix.Helpers.PathHelper;

namespace essentialMix.Windows.Helpers;

/// <summary>
/// As a general rule of thumb, It's recommended to use Path.GetFullPath to normalize it before hitting any shimming method.
/// </summary>
public static class PathHelper
{
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

	[NotNull]
	public static string GetRelativePath(string rootPath, string targetPath)
	{
		rootPath = EMPathHelper.AddDirectorySeparator(rootPath);
		if (string.IsNullOrEmpty(rootPath)) throw new ArgumentNullException(nameof(rootPath));
		targetPath = EMPathHelper.Trim(targetPath);
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

					if (target.Scheme.IsSame(Uri.UriSchemeFile)) relativePath = relativePath.Replace(EMPathHelper.AltDirectorySeparator, EMPathHelper.DirectorySeparator);
					return relativePath;
				}
			}
		}

		StringBuilder sb = new StringBuilder(Win32.MAX_PATH);
		return Win32.PathRelativePathTo(sb, rootPath, FileAttributes.Directory, targetPath, FileAttributes.Normal)
					? sb.ToString()
					: targetPath;
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
		path = EMPathHelper.Trim(path);
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
}