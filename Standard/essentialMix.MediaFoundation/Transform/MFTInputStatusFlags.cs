using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_INPUT_STATUS_FLAGS")]
	public enum MFTInputStatusFlags
	{
		None = 0,
		AcceptData = 0x00000001
	}
}