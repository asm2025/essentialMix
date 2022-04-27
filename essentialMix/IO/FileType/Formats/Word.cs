using System;
using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Word() : CompoundFileBinaryBase("doc", MediaTypeNames.Application.Vnd.MsWord.Default, Guid.Parse("{00020906-0000-0000-c000-000000000046}"));

public record Word_1() : OfficeOpenXmlBase("docx", MediaTypeNames.Application.Vnd.OpenxmlformatsOfficedocument.Wordprocessingml.Document.Default, "word/document.xml");