using System.Diagnostics.CodeAnalysis;

namespace essentialMix.Media.ffmpeg
{
	[SuppressMessage("ReSharper", "InconsistentNaming")]
	public enum ConversionPreset
	{
		ultrafast,
		superfast,
		veryfast,
		faster,
		fast,
		medium,
		slow,
		slower,
		veryslow,
		placebo
	}
}