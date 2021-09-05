using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_STREAM_STATE")]
	public enum DXVAHD_STREAM_STATE
	{
		D3DFormat = 0,
		FrameFormat = 1,
		InputColorSpace = 2,
		OutputRate = 3,
		SourceRect = 4,
		DestinationRect = 5,
		Alpha = 6,
		Palette = 7,
		LumaKey = 8,
		AspectRatio = 9,
		FilterBrightness = 100,
		FilterContrast = 101,
		FilterHue = 102,
		FilterSaturation = 103,
		FilterNoiseReduction = 104,
		FilterEdgeEnhancement = 105,
		FilterAnamorphicScaling = 106,
		Private = 1000
	}
}