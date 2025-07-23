using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record RichTextFormat() : FileFormatBase("rtf", MediaTypeNames.Application.Rtf, [0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31
]);