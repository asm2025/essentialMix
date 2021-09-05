using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[UnmanagedName("_MFT_DRAIN_TYPE")]
	public enum MFTDrainType
	{
		ProduceTails = 0x00000000,
		NoTails = 0x00000001
	}
}