using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Pdf() : FileFormatBase("pdf", MediaTypeNames.Application.Pdf, new byte[] { 0x25, 0x50, 0x44, 0x46 });