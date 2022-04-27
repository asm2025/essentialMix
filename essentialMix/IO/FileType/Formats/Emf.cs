using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Emf() : ImageBase("emf", MediaTypeNames.Image.Emf, new byte[] { 0x10, 0x00, 0x00, 0x00 });