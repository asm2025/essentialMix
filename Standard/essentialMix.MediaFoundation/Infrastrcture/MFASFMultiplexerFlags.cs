using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[Flags]
	[UnmanagedName("MFASF_MULTIPLEXERFLAGS")]
	public enum MFASFMultiplexerFlags
	{
		None = 0,
		AutoAdjustBitrate = 0x00000001
	}
}