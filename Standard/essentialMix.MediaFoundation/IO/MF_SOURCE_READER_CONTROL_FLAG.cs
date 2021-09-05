using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.IO
{
	[UnmanagedName("MF_SOURCE_READER_CONTROL_FLAG")]
	public enum MF_SOURCE_READER_CONTROL_FLAG
	{
		None = 0,
		Drain = 0x00000001
	}
}