using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_PRELOAD")]
	public enum MF_MEDIA_ENGINE_PRELOAD
	{
		Missing = 0,
		Empty = 1,
		None = 2,
		Metadata = 3,
		Automatic = 4
	}
}