namespace essentialMix.IO.FileType.Formats;

public record JpegSpiff() : Jpeg(new byte[] { 0xFF, 0xE8 });