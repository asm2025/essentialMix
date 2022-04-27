using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record VisioLegacy() : CompoundFileBinaryBase("vsd", MediaTypeNames.Application.Vnd.Visio, "VisioDocument");
public record Visio() : OfficeOpenXmlBase("vsdx", MediaTypeNames.Application.Vnd.Visio, "visio/document.xml");