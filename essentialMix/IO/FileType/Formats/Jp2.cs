using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Jp2() : ImageBase("jp2", MediaTypeNames.Image.Jp2, new byte[] { 0x00, 0x00, 0x00, 0x0C, 0x6A, 0x50, 0x20, 0x20 });