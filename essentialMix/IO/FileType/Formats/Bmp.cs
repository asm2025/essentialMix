using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Bmp() : Image("bmp", MediaTypeNames.Image.Bmp, new byte[] { 0x42, 0x4D });