using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[Flags]
	[UnmanagedName("MFMEDIASOURCE_CHARACTERISTICS")]
	public enum MFMEDIASOURCE_CHARACTERISTICS
	{
		None = 0,
		IsLive = 0x1,
		CanSeek = 0x2,
		CanPause = 0x4,
		HasSlowSeek = 0x8,
		HasMultiplePresentations = 0x10,
		CanSkipForward = 0x20,
		CanSkipBackward = 0x40,
		DoesNotUseNetwork = 0x80,
	}
}