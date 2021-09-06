using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_CANPLAY")]
	public enum MF_MEDIA_ENGINE_CANPLAY
	{
		NotSupported = 0,
		Maybe = 1,
		Probably = 2,
	}
}