namespace essentialMix.IO.FileType.Formats;

public record Exif() : Jpeg([0xFF, 0xE1]);