using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[Flags]
	[UnmanagedName("MFVideoMixPrefs")]
	public enum MFVideoMixPrefs
	{
		None = 0,
		ForceHalfInterlace = 0x00000001,
		AllowDropToHalfInterlace = 0x00000002,
		AllowDropToBob = 0x00000004,
		ForceBob = 0x00000008,
		Mask = 0x0000000f
	}
}