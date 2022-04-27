using System.Linq;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

// https://en.wikipedia.org/wiki/ISO/IEC_base_media_file_format
public abstract record IsoBMFFBase : FileFormatBase
{
	private static readonly byte[] __signature = { 0x66, 0x74, 0x79, 0x70 };

	/// <inheritdoc />
	protected IsoBMFFBase(string extension, string mimeType, [NotNull] byte[] prefix)
		: this(extension, mimeType, prefix, null, 0)
	{
	}

	/// <inheritdoc />
	protected IsoBMFFBase(string extension, string mimeType, [NotNull] byte[] prefix, int offset)
		: this(extension, mimeType, prefix, null, offset)
	{
	}

	/// <inheritdoc />
	protected IsoBMFFBase(string extension, string mimeType, [NotNull] byte[] prefix, [NotNull] byte[] suffix)
		: this(extension, mimeType, prefix, suffix, 0)
	{
	}

	/// <inheritdoc />
	protected IsoBMFFBase(string extension, string mimeType, byte[] prefix, byte[] suffix, int offset)
		: base(extension, mimeType, prefix.DefaultIfNull().Concat(__signature).Concat(suffix.DefaultIfNull()).ToArray(), offset)
	{
	}
}