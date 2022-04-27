using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Wmf() : ImageBase("wmf", MediaTypeNames.Image.Wmf, new byte[] { 0xD7, 0xCD, 0xC6, 0x9A });