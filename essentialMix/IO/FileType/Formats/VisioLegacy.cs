using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record VisioLegacy() : CompoundFileBinary("vsd", MediaTypeNames.Application.Vnd.Visio, "VisioDocument");