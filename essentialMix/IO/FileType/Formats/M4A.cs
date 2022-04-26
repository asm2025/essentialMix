using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record M4A() : IsoBMFF("m4a", MediaTypeNames.Audio.Mp4, new byte[] { 0x00, 0x00, 0x00, 0x20 }, new byte[] { 0x4D, 0x34, 0x41 });