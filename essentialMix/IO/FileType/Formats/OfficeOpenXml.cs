using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public abstract record OfficeOpenXml : Zip, ICompoundStream
{
	protected OfficeOpenXml(string extension, string mimeType, [NotNull] string identifiableEntry)
		: base(extension, mimeType)
	{
		IdentifiableEntry = identifiableEntry;
	}

	[NotNull]
	public string IdentifiableEntry { get; }

	/// <inheritdoc />
	public IDisposable GetStream(Stream stream)
	{
		try
		{
			return new ZipArchive(stream, ZipArchiveMode.Read, true);
		}
		catch (InvalidDataException)
		{
			return null;
		}
	}

	/// <inheritdoc />
	public bool IsMatch(IDisposable stream)
	{
		return stream is ZipArchive archive && archive.Entries.Any(e => e.FullName.Equals(IdentifiableEntry, StringComparison.OrdinalIgnoreCase));
	}
}