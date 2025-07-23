using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Bmp() : ImageBase("bmp", MediaTypeNames.Image.Bmp, [0x42, 0x4D]);