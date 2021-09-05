using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[UnmanagedName("MF_MEDIA_ENGINE_OPM_STATUS")]
	public enum MF_MEDIA_ENGINE_OPM_STATUS
	{
		NotRequested = 0,
		Established = 1,
		FailedVM = 2,
		FailedBDA = 3,
		FailedUnsignedDriver = 4,
		Failed = 5,
	}
}