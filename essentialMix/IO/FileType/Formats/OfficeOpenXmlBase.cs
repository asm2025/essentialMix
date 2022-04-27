using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public abstract record OfficeOpenXmlBase : ZipBase, ICompoundStream
{
	protected OfficeOpenXmlBase(string extension, string mimeType, [NotNull] string identifiableEntry)
		: base(extension, mimeType)
	{
		IdentifiableEntry = identifiableEntry;
	}

	[NotNull]
	public string IdentifiableEntry { get; }

	/// <inheritdoc />
	public bool HasGuid => false;

	/// <inheritdoc />
	public bool HasStorage => true;

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