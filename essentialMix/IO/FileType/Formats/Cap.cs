using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

// This is the same signature for cab, snp, and ppz
public record Cap() : FileFormatBase("cap", MediaTypeNames.Application.Vnd.MsCabCompressed, new byte[] { 0x4D, 0x53, 0x43, 0x46 });