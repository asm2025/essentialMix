using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record WordLegacy() : CompoundFileBinary("doc", MediaTypeNames.Application.Vnd.MsWord.Default, "WordDocument");