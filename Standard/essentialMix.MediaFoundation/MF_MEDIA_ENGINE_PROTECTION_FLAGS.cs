using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[Flags]
	[UnmanagedName("MF_MEDIA_ENGINE_PROTECTION_FLAGS")]
	public enum MF_MEDIA_ENGINE_PROTECTION_FLAGS
	{
		EnableProtectedContent = 1,
		UsePMPForAllContent = 2,
		UseUnprotectedPMP = 4

	}
}