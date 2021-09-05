using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[UnmanagedName("_MFT_OUTPUT_DATA_BUFFER_FLAGS")]
	public enum MFTOutputDataBufferFlags
	{
		None = 0,
		Incomplete = 0x01000000,
		FormatChange = 0x00000100,
		StreamEnd = 0x00000200,
		NoSample = 0x00000300
	}
}