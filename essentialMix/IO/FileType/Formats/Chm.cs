namespace essentialMix.IO.FileType.Formats;

// This is the same signature for chi, and chm
public record Chm() : FileFormatBase("chm", "MS Compiled HTML Help File", [0x49, 0x54, 0x53, 0x46]);