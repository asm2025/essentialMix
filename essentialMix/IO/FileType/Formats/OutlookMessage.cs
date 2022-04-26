using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record OutlookMessage() : CompoundFileBinary("msg", MediaTypeNames.Application.Vnd.MsOutlook.Default, "__properties_version1.0");