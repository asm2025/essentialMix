using System.Diagnostics.CodeAnalysis;

namespace essentialMix.Media.ffmpeg
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum ConversionProfile
	{
		baseline,
		main,
		high,
		high10,
		high422,
		high444
	}
}