using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;
using EMPathHelper = essentialMix.Helpers.PathHelper;

namespace essentialMix.Windows.Helpers;

public static class FileHelper
{
	[NotNull]
	public static string GetTempName() { return GetTempName(null, null, 0); }

	[NotNull]
	public static string GetTempName(string basePath) { return GetTempName(basePath, null, 0); }

	[NotNull]
	public static string GetTempName(string basePath, string prefix) { return GetTempName(basePath, prefix, 0); }

	[NotNull]
	public static string GetTempName(string basePath, string prefix, uint unique)
	{
		basePath = EMPathHelper.Trim(basePath);
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