using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[Flags]
	[UnmanagedName("MF_MEDIA_ENGINE_FRAME_PROTECTION_FLAGS")]
	public enum MF_MEDIA_ENGINE_FRAME_PROTECTION_FLAGS
	{
		None = 0x0,
		Protected = 0x01,
		RequiresSurfaceProtection = 0x02,
		RequiresAntiScreenScrapeProtection = 0x04
	}
}