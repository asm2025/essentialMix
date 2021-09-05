using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_FILTER")]
	public enum DXVAHD_FILTER
	{
		Brightness = 0,
		Contrast = 1,
		Hue = 2,
		Saturation = 3,
		NoiseReduction = 4,
		EdgeEnhancement = 5,
		AnamorphicScaling = 6
	}
}