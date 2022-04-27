using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Rar() : FileFormatBase("rar", MediaTypeNames.Application.Vnd.Rar, new byte[] { 0x52, 0x61, 0x72, 0x21, 0x1A, 0x7, 0x0 });