using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Executable() : FileFormatBase("exe", MediaTypeNames.Application.OctetStream, [0x4D, 0x5A]);