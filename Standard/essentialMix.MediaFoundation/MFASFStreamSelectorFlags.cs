using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[Flags]
	[UnmanagedName("MFASF_STREAMSELECTORFLAGS")]
	public enum MFASFStreamSelectorFlags
	{
		None = 0x00000000,
		DisableThinning = 0x00000001,
		UseAverageBitrate = 0x00000002
	}
}