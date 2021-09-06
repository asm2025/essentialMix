using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_NETWORK")]
	public enum MF_MEDIA_ENGINE_NETWORK : short
	{
		Empty = 0,
		Idle = 1,
		Loading = 2,
		NoSource = 3
	}
}