using System.Linq;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

// https://en.wikipedia.org/wiki/ISO/IEC_base_media_file_format
public abstract record IsoBMFF : FileFormat
{
	private static readonly byte[] __prefix = { 0x66, 0x74, 0x79, 0x70 };

	/// <inheritdoc />
	protected IsoBMFF(string extension, string mimeType, [NotNull] byte[] signature)
		: base(extension, mimeType, __prefix.Concat(signature).ToArray(), __prefix.Length)
	{
	}
}