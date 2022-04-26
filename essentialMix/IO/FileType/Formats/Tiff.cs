using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Tiff() : Image("tif", MediaTypeNames.Image.Tiff, new byte[] { 0x2A, 0x00 }, 2);