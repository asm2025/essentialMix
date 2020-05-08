using System.Diagnostics.CodeAnalysis;

namespace asm.Media.ffmpeg
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum TargetStandard
	{
		Default,
		pal,
		ntsc,
		film
	}
}