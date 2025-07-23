using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record SevenZip() : FileFormatBase("7z", MediaTypeNames.Application.SevenZip, [0x37, 0x7A, 0xBC, 0xAF, 0x27, 0x1C
]);