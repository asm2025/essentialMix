using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Tiff() : Image("tiff", MediaTypeNames.Image.Tiff, new byte[] { 0x49, 0x20, 0x49 });
public record Tiff1() : Image("tiff", MediaTypeNames.Image.Tiff, new byte[] { 0x49, 0x49, 0x2A, 0x00 });
public record Tiff2() : Image("tiff", MediaTypeNames.Image.Tiff, new byte[] { 0x4D, 0x4D, 0x00, 0x2A });
public record Tiff3() : Image("tiff", MediaTypeNames.Image.Tiff, new byte[] { 0x4D, 0x4D, 0x00, 0x2B });