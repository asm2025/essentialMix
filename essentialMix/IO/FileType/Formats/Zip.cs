using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public abstract record Zip : FileFormat
{
	/// <inheritdoc />
	protected Zip()
		: this("zip", MediaTypeNames.Application.Zip)
	{
	}

	/// <inheritdoc />
	protected Zip(string extension, string mimeType)
		: base(extension, mimeType, new byte[] { 0x50, 0x4B, 0x03, 0x04 })
	{
	}
}