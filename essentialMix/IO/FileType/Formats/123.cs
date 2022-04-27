using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record OneTwoThree() : FileFormatBase("123", MediaTypeNames.Application.Vnd.Lotus1_2_3, new byte[] { 0x00, 0x00, 0x1A, 0x00, 0x05, 0x10, 0x04 });