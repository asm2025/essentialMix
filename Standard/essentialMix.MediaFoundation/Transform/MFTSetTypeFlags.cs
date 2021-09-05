using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_SET_TYPE_FLAGS")]
	public enum MFTSetTypeFlags
	{
		None = 0,
		TestOnly = 0x00000001
	}
}