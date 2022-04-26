using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Xps() : OfficeOpenXml("xps", MediaTypeNames.Application.Vnd.MsXpsdocument, "FixedDocSeq.fdseq");