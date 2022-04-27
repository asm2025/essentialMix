namespace essentialMix.IO.FileType.Formats;

public record Exif() : Jpeg(new byte[] { 0xFF, 0xE1 });