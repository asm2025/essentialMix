using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record Mp4 : IsoBMFFBase
{
	public Mp4()
		: this(new byte[] { 0x69, 0x73 }, 4)
	{
	}

	protected Mp4([NotNull] byte[] signature)
		: this(signature, 0)
	{
	}

	protected Mp4([NotNull] byte[] signature, int offset)
		: base("mp4", MediaTypeNames.Video.Mp4, null, signature, offset)
	{
	}
}

public record Mp4_1() : Mp4(new byte[] { 0x6F, 0x6D }, 4);
