using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

// This is the same signature for asf, wma, and wmv
public record Asf() : FileFormatBase("asf", MediaTypeNames.Application.Vnd.MsAsf, [0x30, 0x26, 0xB2, 0x75, 0x8E, 0x66, 0xCF, 0x11
]);