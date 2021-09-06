using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_READY")]
	public enum MF_MEDIA_ENGINE_READY : short
	{
		HaveNothing = 0,
		HaveMetadata = 1,
		HaveCurrentData = 2,
		HaveFutureData = 3,
		HaveEnoughData = 4
	}
}