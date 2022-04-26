using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

public record QuickTime() : FileFormat("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x6D, 0x6F, 0x6F, 0x76 }, 4);
public record QuickTime1() : FileFormat("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x66, 0x72, 0x65, 0x65 }, 4);
public record QuickTime2() : FileFormat("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x6D, 0x64, 0x61, 0x74 }, 4);
public record QuickTime3() : FileFormat("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x77, 0x69, 0x64, 0x65 }, 4);
public record QuickTime4() : FileFormat("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x70, 0x6E, 0x6F, 0x74 }, 4);
public record QuickTime5() : FileFormat("mov", MediaTypeNames.Video.Quicktime, new byte[] { 0x73, 0x6B, 0x69, 0x70 }, 4);
