using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public abstract record ZipBase : FileFormatBase
{
	/// <inheritdoc />
	protected ZipBase([NotNull] byte[] signature)
		: this("zip", MediaTypeNames.Application.Zip, signature, 0)
	{
	}

	/// <inheritdoc />
	protected ZipBase([NotNull] byte[] signature, int offset)
		: this("zip", MediaTypeNames.Application.Zip, signature, offset)
	{
	}

	/// <inheritdoc />
	protected ZipBase(string mimeType, [NotNull] byte[] signature)
		: this("zip", mimeType, signature, 0)
	{
	}

	/// <inheritdoc />
	protected ZipBase(string extension, string mimeType)
		: this(extension, mimeType, new byte[] { 0x50, 0x4B, 0x03, 0x04 }, 0)
	{
	}

	/// <inheritdoc />
	protected ZipBase(string extension, string mimeType, [NotNull] byte[] signature)
		: this(extension, mimeType, signature, 0)
	{
	}

	/// <inheritdoc />
	protected ZipBase(string extension, string mimeType, [NotNull] byte[] signature, int offset)
		: base(extension, mimeType, signature, offset)
	{
	}
}