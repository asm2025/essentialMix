using System.Linq;
using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record Jpeg : ImageBase
{
	private static readonly byte[] __soi = { 0xFF, 0xD8 };

	public Jpeg()
		: base("jpg", MediaTypeNames.Image.Jpeg, __soi)
	{
	}

	protected Jpeg([NotNull] byte[] signature)
		: base("jpg", MediaTypeNames.Image.Jpeg, __soi.Concat(signature).ToArray())
	{
	}
}