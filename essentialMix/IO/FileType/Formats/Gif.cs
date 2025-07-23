using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Gif() : ImageBase("gif", MediaTypeNames.Image.Gif, [0x47, 0x49, 0x46, 0x38]);