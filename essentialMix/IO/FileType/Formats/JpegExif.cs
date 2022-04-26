namespace essentialMix.IO.FileType.Formats;

public record JpegExif() : Jpeg(new byte[] { 0xFF, 0xE1 });