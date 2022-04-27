using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record Tiff : ImageBase
{
	public Tiff()
		: this(new byte[] { 0x49, 0x20, 0x49 })
	{
	}

	protected Tiff([NotNull] byte[] signature)
		: base("tiff", MediaTypeNames.Image.Tiff, signature)
	{
	}
}

public record Tiff_1() : Tiff(new byte[] { 0x49, 0x49, 0x2A, 0x00 });
public record Tiff_2() : Tiff(new byte[] { 0x4D, 0x4D, 0x00, 0x2A });
public record Tiff_3() : Tiff(new byte[] { 0x4D, 0x4D, 0x00, 0x2B });