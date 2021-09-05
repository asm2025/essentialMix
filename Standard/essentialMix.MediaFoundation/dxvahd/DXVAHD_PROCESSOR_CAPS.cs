using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[Flags]
	[UnmanagedName("DXVAHD_PROCESSOR_CAPS")]
	public enum DXVAHD_PROCESSOR_CAPS
	{
		DeinterlaceBland = 0x1,
		DeinterlaceBob = 0x2,
		DeinterlaceAdaptive = 0x4,
		DeinterlaceMotionCompensation = 0x8,
		InverseTelecine = 0x10,
		FrameRateConversion = 0x20
	}
}