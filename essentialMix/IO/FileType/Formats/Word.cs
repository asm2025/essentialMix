using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Word() : OfficeOpenXml("docx", MediaTypeNames.Application.Vnd.OpenxmlformatsOfficedocument.Wordprocessingml.Document.Default, "word/document.xml");