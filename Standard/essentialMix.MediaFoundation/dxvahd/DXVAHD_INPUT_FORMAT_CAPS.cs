using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[Flags]
	[UnmanagedName("DXVAHD_INPUT_FORMAT_CAPS")]
	public enum DXVAHD_INPUT_FORMAT_CAPS
	{
		RGBInterlaced = 0x1,
		RGBProcAmp = 0x2,
		RGBLumaKey = 0x4,
		PaletteInterlaced = 0x8
	}
}