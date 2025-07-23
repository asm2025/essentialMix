using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record M4a() : IsoBMFFBase("m4a", MediaTypeNames.Audio.Mp4, [0x00, 0x00, 0x00, 0x20], [0x4D, 0x34, 0x41]);