using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_INPUT_STREAM_INFO_FLAGS")]
	public enum MFTInputStreamInfoFlags
	{
		None = 0,
		WholeSamples = 0x1,
		SingleSamplePerBuffer = 0x2,
		FixedSampleSize = 0x4,
		HoldsBuffers = 0x8,
		DoesNotAddRef = 0x100,
		Removable = 0x200,
		Optional = 0x400,
		ProcessesInPlace = 0x800
	}
}