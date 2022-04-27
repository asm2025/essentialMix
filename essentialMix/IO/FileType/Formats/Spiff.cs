namespace essentialMix.IO.FileType.Formats;

public record Spiff() : Jpeg(new byte[] { 0xFF, 0xE8 });