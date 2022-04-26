using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Mp4() : IsoBMFF("mp4", MediaTypeNames.Video.Mp4, null, new byte[] { 0x4D, 0x53, 0x4E, 0x56 }, 4);
public record Mp41() : IsoBMFF("mp4", MediaTypeNames.Video.Mp4, null, new byte[] { 0x69, 0x73, 0x6F, 0x6D }, 4);
