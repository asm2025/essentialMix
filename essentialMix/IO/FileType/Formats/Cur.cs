namespace essentialMix.IO.FileType.Formats;

// This is the same signature for cur, and wb2
public record Cur() : FileFormatBase("cur", "Windows cursor", new byte[] { 0x00, 0x00, 0x02, 0x00 });