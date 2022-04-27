using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public abstract record ImageBase : FileFormatBase
{
	/// <inheritdoc />
	protected ImageBase(string extension, string mimeType, [NotNull] byte[] signature)
		: base(extension, mimeType, signature)
	{
	}

	/// <inheritdoc />
	protected ImageBase(string extension, string mimeType, [NotNull] byte[] signature, int offset)
		: base(extension, mimeType, signature, offset)
	{
	}
}