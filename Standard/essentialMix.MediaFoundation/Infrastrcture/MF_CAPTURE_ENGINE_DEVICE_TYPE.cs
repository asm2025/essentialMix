using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_CAPTURE_ENGINE_DEVICE_TYPE")]
	public enum MF_CAPTURE_ENGINE_DEVICE_TYPE
	{
		Audio = 0x00000000,
		Video = 0x00000001
	}
}