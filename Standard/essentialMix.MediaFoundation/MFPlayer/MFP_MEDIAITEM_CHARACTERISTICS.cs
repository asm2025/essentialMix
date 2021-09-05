using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[Flags]
	[UnmanagedName("MFP_MEDIAITEM_CHARACTERISTICS")]
	public enum MFP_MEDIAITEM_CHARACTERISTICS
	{
		None = 0x00000000,
		IsLive = 0x00000001,
		CanSeek = 0x00000002,
		CanPause = 0x00000004,
		HasSlowSeek = 0x00000008
	}
}