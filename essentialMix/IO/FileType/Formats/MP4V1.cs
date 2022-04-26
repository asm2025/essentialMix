using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record MP4V1() : IsoBMFF("mp4", MediaTypeNames.Video.Mp4, new byte[] { 0x69, 0x73, 0x6F, 0x6D });