using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[Flags]
	[UnmanagedName("DXVA2_ProcAmp_* defines")]
	public enum DXVA2ProcAmp
	{
		None = 0,
		Brightness = 0x0001,
		Contrast = 0x0002,
		Hue = 0x0004,
		Saturation = 0x0008
	}
}