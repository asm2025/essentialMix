using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Mpg() : FileFormat("mpg", MediaTypeNames.Video.Mpeg, new byte[] { 0x00, 0x00, 0x01, 0xBA });
public record Mpg1() : FileFormat("mpg", MediaTypeNames.Video.Mpeg, new byte[] { 0x00, 0x00, 0x01, 0xB3 });