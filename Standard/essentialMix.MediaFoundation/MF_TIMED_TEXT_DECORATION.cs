using System;

namespace essentialMix.MediaFoundation
{
	[Flags]
	public enum MF_TIMED_TEXT_DECORATION
	{
		None = 0,
		Underline = 1,
		LineThrough = 2,
		Overline = 4,
	}
}