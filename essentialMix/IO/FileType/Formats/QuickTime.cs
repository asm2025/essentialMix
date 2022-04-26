using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record QuickTime() : IsoBMFF("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x71, 0x74, 0x20, 0x20 });