using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record M4v() : IsoBMFFBase("m4v", MediaTypeNames.Video.Mp4, new byte[] { 0x4D, 0x34, 0x56, 0x20 });