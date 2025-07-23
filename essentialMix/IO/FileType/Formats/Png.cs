using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Png() : ImageBase("png", MediaTypeNames.Image.Png, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]);