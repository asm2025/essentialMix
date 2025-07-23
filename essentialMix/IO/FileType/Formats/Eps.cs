using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record Eps : ImageBase
{
	public Eps()
		: this([0xC5, 0xD0, 0xD3, 0xC6])
	{
	}

	protected Eps([NotNull] byte[] signature)
		: base("eps", MediaTypeNames.Application.Postscript, signature)
	{
	}
}

public record Eps_1() : Eps([0x25, 0x21, 0x50, 0x53, 0x2D, 0x41, 0x64, 0x6F]);