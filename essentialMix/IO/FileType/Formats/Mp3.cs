using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Mp3() : FileFormatBase("mp3", MediaTypeNames.Audio.Mpeg, new byte[] { 0x49, 0x44, 0x33 });