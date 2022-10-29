using System.IO;
using essentialMix.Helpers;
using JetBrains.Annotations;
using FileHelper = essentialMix.Windows.Helpers.FileHelper;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class StreamExtension
{

	[NotNull]
	public static FileStream SaveToTempStream([NotNull] this Stream thisValue) { return SaveToTempStream(thisValue, null, null, 0); }

	[NotNull]
	public static FileStream SaveToTempStream([NotNull] this Stream thisValue, string basePath) { return SaveToTempStream(thisValue, basePath, null, 0); }

	[NotNull]
	public static FileStream SaveToTempStream([NotNull] this Stream thisValue, string basePath, string prefix) { return SaveToTempStream(thisValue, basePath, prefix, 0); }

	[NotNull]
	public static FileStream SaveToTempStream([NotNull] this Stream thisValue, string basePath, string prefix, uint unique)
	{
		FileStream stream = FileHelper.GetTempStream(basePath, prefix, unique);
		thisValue.CopyTo(stream);
		return stream;
	}

	public static string SaveToTempFile([NotNull] this Stream thisValue) { return SaveToTempFile(thisValue, null, null, 0); }

	public static string SaveToTempFile([NotNull] this Stream thisValue, string basePath) { return SaveToTempFile(thisValue, basePath, null, 0); }

	public static string SaveToTempFile([NotNull] this Stream thisValue, string basePath, string prefix) { return SaveToTempFile(thisValue, basePath, prefix, 0); }

	public static string SaveToTempFile([NotNull] this Stream thisValue, string basePath, string prefix, uint unique)
	{
		string path;

		using (FileStream output = SaveToTempStream(thisValue, basePath, prefix, unique))
		{
			path = output.Name;
		}

		return path;
	}

	public static FileStream SaveToStream([NotNull] this Stream thisValue, [NotNull] string fileName, bool overwrite = false)
	{
		if (PathHelper.IsPathQualified(fileName))
		{
			string path = Path.GetDirectoryName(fileName);

			if (!string.IsNullOrEmpty(path))
			{
				if (!DirectoryHelper.Ensure(path)) return null;
			}
		}

		if (File.Exists(fileName))
		{
			if (!overwrite) return null;
			File.SetAttributes(fileName, File.GetAttributes(fileName) & ~(FileAttributes.Hidden | FileAttributes.ReadOnly));
		}

		FileStream stream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
		thisValue.CopyTo(stream);
		return stream;
	}

	public static bool SaveToFile([NotNull] this Stream thisValue, [NotNull] string fileName, bool overwrite = false)
	{
		using (FileStream output = SaveToStream(thisValue, fileName, overwrite))
		{
			return output != null;
		}
	}
}