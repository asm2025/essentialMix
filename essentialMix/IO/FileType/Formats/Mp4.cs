using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Mp4() : IsoBMFF("mp4", MediaTypeNames.Video.Mp4, new byte[] { 0x6D, 0x70, 0x34, 0x32 });