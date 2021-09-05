using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[UnmanagedName("MF_MSE_READY")]
	public enum MF_MSE_READY
	{
		Closed = 1,
		Open = 2,
		Ended = 3,
	}
}