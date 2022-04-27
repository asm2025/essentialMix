using System;
using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record PowerPoint() : CompoundFileBinaryBase("ppt", MediaTypeNames.Application.Vnd.MsPowerpoint.Default, Guid.Parse("{64818d10-4f9b-11cf-86ea-00aa00b929e8}"));
public record PowerPoint_1() : OfficeOpenXmlBase("pptx", MediaTypeNames.Application.Vnd.OpenxmlformatsOfficedocument.Presentationml.Presentation.Default, "ppt/presentation.xml");