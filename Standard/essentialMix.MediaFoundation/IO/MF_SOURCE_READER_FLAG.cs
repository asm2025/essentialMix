using System;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.IO
{
	[Flags]
	[UnmanagedName("MF_SOURCE_READER_FLAG")]
	public enum MF_SOURCE_READER_FLAG
	{
		None = 0,
		Error = 0x00000001,
		EndOfStream = 0x00000002,
		NewStream = 0x00000004,
		NativeMediaTypeChanged = 0x00000010,
		CurrentMediaTypeChanged = 0x00000020,
		AllEffectsRemoved = 0x00000200,
		StreamTick = 0x00000100
	}
}