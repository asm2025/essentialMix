using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[Flags]
	[UnmanagedName("DXVAHD_FILTER_CAPS")]
	public enum DXVAHD_FILTER_CAPS
	{
		Brightness = 0x1,
		Contrast = 0x2,
		Hue = 0x4,
		Saturation = 0x8,
		NoiseReduction = 0x10,
		EdgeEnhancement = 0x20,
		AnamorphicScaling = 0x40
	}
}