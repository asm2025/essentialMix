using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record ExcelLegacy() : CompoundFileBinary("xls", MediaTypeNames.Application.Vnd.MsExcel.Default, "Workbook");