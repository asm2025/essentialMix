using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record PowerPointLegacy() : CompoundFileBinary("ppt", MediaTypeNames.Application.Vnd.MsPowerpoint.Default, "PowerPointDocument");