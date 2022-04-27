using System;
using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Excel() : CompoundFileBinaryBase("xls", MediaTypeNames.Application.Vnd.MsExcel.Default, Guid.Parse("{00020810-0000-0000-c000-000000000046}"));
public record Excel_1() : CompoundFileBinaryBase("xls", MediaTypeNames.Application.Vnd.MsExcel.Default, Guid.Parse("{00020820-0000-0000-c000-000000000046}"));
public record Excel_2() : OfficeOpenXmlBase("xlsx", MediaTypeNames.Application.Vnd.OpenxmlformatsOfficedocument.Spreadsheetml.Sheet.Default, "xl/workbook.xml");