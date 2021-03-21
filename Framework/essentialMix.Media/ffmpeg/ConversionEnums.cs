using System.Diagnostics.CodeAnalysis;

namespace essentialMix.Media.ffmpeg
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