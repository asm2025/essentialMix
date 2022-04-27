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
	private static readonly IList<FileFormatBase> __formats;

	private readonly IList<FileFormatBase> _formats;

	static FileFormatInspector()
	{
		__assembly = typeof(FileFormatBase).Assembly;
		__formats = GetFormats(__assembly, false)
			.ToList();
	}

	public FileFormatInspector()
		: this(__formats)
	{
	}

	public FileFormatInspector([NotNull] IEnumerable<FileFormatBase> formats)
	{
		_formats = formats.OrderByDescending(e => e.Signature?.Length ?? 0)
			.ToList();
	}

	/// <inheritdoc />
	public FileFormatBase Detect(Stream stream)
	{
		if (!stream.CanSeek) throw new NotSupportedException("Stream type is not supported.");
		if (stream.Length == 0) return null;
		return FindMatchingFormats(stream)
			.FirstOrDefault();
	}

	[NotNull]
	private IEnumerable<FileFormatBase> FindMatchingFormats([NotNull] Stream stream)
	{
		if (__formats.Count == 0) throw new Exception("Formats are not configured.");

		List<FileFormatBase> candidates = new List<FileFormatBase>();
		IDisposable compoundStream = null;
		int longestSignature = 0;

		try
		{
			foreach (FileFormatBase format in _formats)
			{
				int sigLen = format.Signature?.Length ?? 0;
				if (sigLen < longestSignature
					|| stream.Length < format.Offset + sigLen
					|| !format.IsMatch(stream)) continue;

				if (format is ICompoundStream { HasStorage: true } compound)
				{
					compoundStream ??= compound.GetStream(stream);
					if (!compound.IsMatch(compoundStream)) continue;
				}

				if (sigLen > longestSignature) longestSignature = sigLen;
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
			Type typeI = candidates[i].GetType();
			int sigLenI = candidates[i].Signature?.Length ?? 0;

			for (int j = i - 1; j >= 0; j--)
			{
				Type typeJ = candidates[j].GetType();
				int sigLenJ = candidates[j].Signature?.Length ?? 0;

				if (typeI.IsAssignableFrom(typeJ) && sigLenI < sigLenJ)
				{
					candidates.RemoveAt(i);
					break;
				}

				if (!typeJ.IsAssignableFrom(typeI) || sigLenI > sigLenJ) continue;
				candidates.RemoveAt(j);
				i--;
				j--;
			}
		}

		candidates.Sort((x, y) =>
		{
			int yc = y is ICompoundStream
						? 1
						: 0;
			int xc = x is ICompoundStream
						? 1
						: 0;
			int cmb = yc.CompareTo(xc);
			if (cmb != 0) return cmb;
			cmb = (y.Signature?.Length ?? 0).CompareTo(x.Signature?.Length ?? 0);
			return cmb != 0
						? cmb
						: y.Offset.CompareTo(x.Offset);
		});
		return candidates;
	}

	[NotNull]
	public static IEnumerable<FileFormatBase> GetFormats([NotNull] Assembly assembly) { return GetFormats(assembly, false); }
	[NotNull]
	public static IEnumerable<FileFormatBase> GetFormats([NotNull] Assembly assembly, bool includeDefaults)
	{
		IEnumerable<FileFormatBase> assemblyFormats = FormatsOf(assembly);
		if (!includeDefaults) return assemblyFormats;
		return assembly == __assembly
					? assemblyFormats
					: assemblyFormats.Union(__formats);

		static IEnumerable<FileFormatBase> FormatsOf(Assembly assembly)
		{
			return assembly.GetTypes()
							.Where(e => !e.IsAbstract && typeof(FileFormatBase).IsAssignableFrom(e))
							.Select(Activator.CreateInstance)
							.Cast<FileFormatBase>();
		}
	}
}