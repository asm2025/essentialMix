using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record Dicom() : FileFormatBase("dcm", MediaTypeNames.Application.Dicom, new byte[] { 0x44, 0x49, 0x43, 0x4D }, 128);
