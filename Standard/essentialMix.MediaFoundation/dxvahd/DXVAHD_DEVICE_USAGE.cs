using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_DEVICE_USAGE")]
	public enum DXVAHD_DEVICE_USAGE
	{
		PlaybackNormal = 0,
		OptimalSpeed = 1,
		OptimalQuality = 2
	}
}