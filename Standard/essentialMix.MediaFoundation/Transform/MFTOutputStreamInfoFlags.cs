using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[Flags]
	[UnmanagedName("_MFT_OUTPUT_STREAM_INFO_FLAGS")]
	public enum MFTOutputStreamInfoFlags
	{
		None = 0,
		WholeSamples = 0x00000001,
		SingleSamplePerBuffer = 0x00000002,
		FixedSampleSize = 0x00000004,
		Discardable = 0x00000008,
		Optional = 0x00000010,
		ProvidesSamples = 0x00000100,
		CanProvideSamples = 0x00000200,
		LazyRead = 0x00000400,
		Removable = 0x00000800
	}
}