using System.Diagnostics.CodeAnalysis;

namespace essentialMix.Media.ffmpeg;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public enum Quality
{
	Default,
	best,
	good,
	realtime
}