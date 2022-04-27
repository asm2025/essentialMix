using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Xps() : OfficeOpenXmlBase("xps", MediaTypeNames.Application.Vnd.MsXpsdocument, "FixedDocSeq.fdseq");