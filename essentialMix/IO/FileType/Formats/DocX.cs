using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record DocX() : FileFormatBase("docx", MediaTypeNames.Application.Vnd.MsWord.Default, [0x50, 0x4B, 0x03, 0x04, 0x14, 0x00, 0x06, 0x00
]);