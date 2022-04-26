using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType;

// based on https://github.com/neilharvey/FileSignatures/
public class FileFormatInspector : IFileFormatInspector
{
	private static readonly Assembly __assembly;
	private static readonly IList<FileFormat> __formats;

	private readonly IList<FileFormat> _formats;

	static FileFormatInspector()
	{
		__assembly = typeof(FileFormat).Assembly;
		__formats = GetFormats(__assembly, false)
			.ToList();
	}

	public FileFormatInspector()
		: this(__formats)
	{
	}

	public FileFormatInspector([NotNull] IEnumerable<FileFormat> formats)
	{
		_formats = formats.OrderByDescending(e => e.Signature?.Length ?? 0)
			.ToList();
	}

	/// <inheritdoc />
	public FileFormat Detect(Stream stream)
	{
		if (!stream.CanSeek) throw new NotSupportedException("Stream type is not supported.");
		if (stream.Length == 0) return null;
		return FindMatchingFormats(stream)
			.FirstOrDefault();
	}

	[NotNull]
	private IEnumerable<FileFormat> FindMatchingFormats([NotNull] Stream stream)
	{
		if (__formats.Count == 0) throw new Exception("Formats are not configured.");

		List<FileFormat> candidates = new List<FileFormat>();
		IDisposable compoundStream = null;

		try
		{
			foreach (FileFormat format in _formats)
			{
				if (!format.IsMatch(stream)) continue;

				if (format is ICompoundStream compound)
				{
					compoundStream ??= compound.GetStream(stream);
					if (!compound.IsMatch(compoundStream)) continue;
				}

				candidates.Add(format);
			}
		}
		finally
		{
			ObjectHelper.Dispose(ref compoundStream);
			stream.Position = 0;
		}

		if (candidates.Count < 2) return candidates;

		// Remove base types
		for (int i = candidates.Count - 1; i >= 0; i--)
		{
			for (int j = i - 1; j >= 0; j--)
			{
				if (candidates[i].GetType().IsInstanceOfType(candidates[j]))
				{
					candidates.RemoveAt(i);
					break;
				}

				if (!candidates[j].GetType().IsInstanceOfType(candidates[i])) continue;
				candidates.RemoveAt(j);
				i--;
				j--;
			}
		}

		return candidates;
	}

	[NotNull]
	public static IEnumerable<FileFormat> GetFormats([NotNull] Assembly assembly) { return GetFormats(assembly, false); }
	[NotNull]
	public static IEnumerable<FileFormat> GetFormats([NotNull] Assembly assembly, bool includeDefaults)
	{
		IEnumerable<FileFormat> assemblyFormats = FormatsOf(assembly);
		if (!includeDefaults) return assemblyFormats;
		return assembly == __assembly
					? assemblyFormats
					: assemblyFormats.Union(__formats);

		static IEnumerable<FileFormat> FormatsOf(Assembly assembly)
		{
			return assembly.GetTypes()
							.Where(e => !e.IsAbstract && typeof(FileFormat).IsAssignableFrom(e))
							.Select(Activator.CreateInstance)
							.Cast<FileFormat>();
		}
	}
}