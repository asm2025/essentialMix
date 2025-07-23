using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record WebP() : ImageBase("webp", MediaTypeNames.Image.WebP, [0x57, 0x45, 0x42, 0x50], 8);