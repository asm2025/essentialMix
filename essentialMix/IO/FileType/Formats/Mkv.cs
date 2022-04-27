using essentialMix.Web;

namespace essentialMix.IO.FileType.Formats;

// This is the same signature for mkv, mka, mks, mk3d, and webm
public record Mkv() : IsoSubEbeBase("mkv", MediaTypeNames.Video.Mkv, new byte[] { 0x1A, 0x45, 0xDF, 0xA3 });