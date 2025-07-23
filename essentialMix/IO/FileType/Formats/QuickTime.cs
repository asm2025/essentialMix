using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record QuickTime : FileFormatBase
{
	public QuickTime()
		: this([0x6D, 0x6F, 0x6F, 0x76], 4)
	{
	}

	protected QuickTime([NotNull] byte[] signature)
		: this(signature, 0)
	{
	}

	protected QuickTime([NotNull] byte[] signature, int offset)
		: base("mov", MediaTypeNames.Video.Quicktime, signature, offset)
	{
	}
}

public record QuickTime_1() : QuickTime([0x66, 0x72, 0x65, 0x65], 4);
public record QuickTime_2() : QuickTime([0x6D, 0x64, 0x61, 0x74], 4);
public record QuickTime_3() : QuickTime([0x77, 0x69, 0x64, 0x65], 4);
public record QuickTime_4() : QuickTime([0x70, 0x6E, 0x6F, 0x74], 4);
public record QuickTime_5() : QuickTime([0x73, 0x6B, 0x69, 0x70], 4);
