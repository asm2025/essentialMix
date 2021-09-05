using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[Flags]
	[UnmanagedName("MFVideoAlphaBitmapFlags")]
	public enum MFVideoAlphaBitmapFlags
	{
		None = 0,
		EntireDDS = 0x00000001,
		SrcColorKey = 0x00000002,
		SrcRect = 0x00000004,
		DestRect = 0x00000008,
		FilterMode = 0x00000010,
		Alpha = 0x00000020,
		BitMask = 0x0000003f
	}
}