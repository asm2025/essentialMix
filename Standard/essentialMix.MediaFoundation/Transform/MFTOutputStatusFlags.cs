using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_OUTPUT_STATUS_FLAGS")]
	public enum MFTOutputStatusFlags
	{
		None = 0,
		SampleReady = 0x00000001
	}
}