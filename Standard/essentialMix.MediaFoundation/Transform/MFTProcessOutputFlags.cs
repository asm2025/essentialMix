using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_PROCESS_OUTPUT_FLAGS")]
	public enum MFTProcessOutputFlags
	{
		None = 0,
		DiscardWhenNoBuffer = 0x00000001,
		RegenerateLastOutput = 0x00000002
	}
}