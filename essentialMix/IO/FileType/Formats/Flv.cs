using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Flv() : FileFormatBase("flv", MediaTypeNames.Application.Vnd.Adobe.Flash.Movie, new byte[] { 0x46, 0x4C, 0x56 });