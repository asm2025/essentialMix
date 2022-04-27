using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record ThreeGp : IsoBMFFBase
{
	public ThreeGp()
		: this(new byte[] { 0x00, 0x00, 0x00, 0x14 })
	{
	}

	protected ThreeGp([NotNull] byte[] signature)
		: base("3gp", MediaTypeNames.Video.ThreeGpp, signature)
	{
	}
}

public record ThreeGp_1() : ThreeGp(new byte[] { 0x00, 0x00, 0x00, 0x20 });
public record ThreeGp5() : ThreeGp(new byte[] { 0x00, 0x00, 0x00, 0x18 });