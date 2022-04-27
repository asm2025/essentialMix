using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record Mpg : FileFormatBase
{
	public Mpg()
		: this(new byte[] { 0x00, 0x00, 0x01, 0xBA })
	{
	}

	protected Mpg([NotNull] byte[] signature)
		: base("mpg", MediaTypeNames.Video.Mpeg, signature)
	{
	}
}

public record Mpg_1() : Mpg(new byte[] { 0x00, 0x00, 0x01, 0xB3 });