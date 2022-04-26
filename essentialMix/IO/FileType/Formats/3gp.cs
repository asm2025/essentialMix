using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record ThreeGp() : IsoBMFF("3gp", MediaTypeNames.Video.ThreeGpp, new byte[] { 0x00, 0x00, 0x00, 0x14 });