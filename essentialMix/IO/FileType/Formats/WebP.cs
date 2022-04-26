using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record WebP() : Image("webp", MediaTypeNames.Image.WebP, new byte[] { 0x57, 0x45, 0x42, 0x50 }, 8);