using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Excel() : OfficeOpenXml("xlsx", MediaTypeNames.Application.Vnd.OpenxmlformatsOfficedocument.Spreadsheetml.Sheet.Default, "xl/workbook.xml");