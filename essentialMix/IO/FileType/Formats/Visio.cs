using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Visio() : OfficeOpenXml("vsdx", MediaTypeNames.Application.Vnd.Visio, "visio/document.xml");