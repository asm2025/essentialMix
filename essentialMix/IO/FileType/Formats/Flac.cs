using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Flac() : FileFormatBase("flac", MediaTypeNames.Audio.Flac, [0x66, 0x4C, 0x61, 0x43, 0x00, 0x00, 0x00, 0x22
]);