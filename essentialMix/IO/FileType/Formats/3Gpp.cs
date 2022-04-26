using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record ThreeGpp() : IsoBMFF("3gp", MediaTypeNames.Video.ThreeGpp, new byte[] { 0x33, 0x67, 0x70 });