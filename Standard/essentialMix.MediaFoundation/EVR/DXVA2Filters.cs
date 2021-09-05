using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[UnmanagedName("Unnamed enum")]
	public enum DXVA2Filters
	{
		None = 0,
		NoiseFilterLumaLevel = 1,
		NoiseFilterLumaThreshold = 2,
		NoiseFilterLumaRadius = 3,
		NoiseFilterChromaLevel = 4,
		NoiseFilterChromaThreshold = 5,
		NoiseFilterChromaRadius = 6,
		DetailFilterLumaLevel = 7,
		DetailFilterLumaThreshold = 8,
		DetailFilterLumaRadius = 9,
		DetailFilterChromaLevel = 10,
		DetailFilterChromaThreshold = 11,
		DetailFilterChromaRadius = 12
	}
}