using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record RichTextFormat() : FileFormat("rtf", MediaTypeNames.Application.Rtf, new byte[] { 0x7B, 0x5C, 0x72, 0x74, 0x66, 0x31 });