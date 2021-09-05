using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[Flags]
	[UnmanagedName("MFVideoAspectRatioMode")]
	public enum MFVideoAspectRatioMode
	{
		None = 0x00000000,
		PreservePicture = 0x00000001,
		PreservePixel = 0x00000002,
		NonLinearStretch = 0x00000004,
		Mask = 0x00000007
	}
}