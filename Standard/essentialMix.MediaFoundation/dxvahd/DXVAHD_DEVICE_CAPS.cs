using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[Flags]
	[UnmanagedName("DXVAHD_DEVICE_CAPS")]
	public enum DXVAHD_DEVICE_CAPS
	{
		LinearSpace = 0x1,
		xvYCC = 0x2,
		RGBRangeConversion = 0x4,
		YCbCrMatrixConversion = 0x8
	}
}