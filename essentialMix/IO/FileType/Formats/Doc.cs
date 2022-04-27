using essentialMix.Web;
using JetBrains.Annotations;

namespace essentialMix.IO.FileType.Formats;

public record Doc : FileFormatBase
{
	public Doc()
		: this(new byte[] { 0x0D, 0x44, 0x4F, 0x43 }, 0)
	{
	}

	protected Doc([NotNull] byte[] signature)
		: this(signature, 0)
	{
	}

	protected Doc([NotNull] byte[] signature, int offset)
		: base("doc", MediaTypeNames.Application.OctetStream, signature, offset)
	{
	}
}

public record Doc_1() : Doc(new byte[] { 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1, 0x00 });
public record Doc_2() : Doc(new byte[] { 0xDB, 0xA5, 0x2D, 0x00 });
public record Doc_3() : Doc(new byte[] { 0xEC, 0xA5, 0xC1, 0x00 }, 512);
public record Doc_4() : Doc(new byte[] { 0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1 });