using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Swf() : FileFormatBase("swf", MediaTypeNames.Application.Vnd.Adobe.Flash.Movie, [0x46, 0x57, 0x53]);