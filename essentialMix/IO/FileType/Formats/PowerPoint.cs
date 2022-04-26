using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record PowerPoint() : OfficeOpenXml("pptx", MediaTypeNames.Application.Vnd.OpenxmlformatsOfficedocument.Presentationml.Presentation.Default, "ppt/presentation.xml");