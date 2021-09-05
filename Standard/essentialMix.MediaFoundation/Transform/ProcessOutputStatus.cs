using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_PROCESS_OUTPUT_STATUS")]
	public enum ProcessOutputStatus
	{
		None = 0,
		NewStreams = 0x00000100
	}
}