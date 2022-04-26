namespace essentialMix.IO.FileType.Formats;

public record Jfif() : Jpeg(new byte[] { 0xFF, 0xE0 });