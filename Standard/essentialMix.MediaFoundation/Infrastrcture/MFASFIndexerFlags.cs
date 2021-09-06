using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[Flags]
	[UnmanagedName("MFASF_INDEXERFLAGS")]
	public enum MFASFIndexerFlags
	{
		None = 0x0,
		WriteNewIndex = 0x00000001,
		ReadForReversePlayback = 0x00000004,
		WriteForLiveRead = 0x00000008
	}
}