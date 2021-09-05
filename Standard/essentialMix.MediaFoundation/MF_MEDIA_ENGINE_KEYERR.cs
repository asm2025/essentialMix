using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation
{
	[UnmanagedName("MF_MEDIA_ENGINE_KEYERR")]
	public enum MF_MEDIA_ENGINE_KEYERR
	{
		Unknown = 1,
		Client = 2,
		Service = 3,
		Output = 4,
		HardwareChange = 5,
		Domain = 6,
	}
}