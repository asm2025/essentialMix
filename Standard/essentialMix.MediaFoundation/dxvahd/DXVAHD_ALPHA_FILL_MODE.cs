using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_ALPHA_FILL_MODE")]
	public enum DXVAHD_ALPHA_FILL_MODE
	{
		Opaque = 0,
		Background = 1,
		Destination = 2,
		SourceStream = 3
	}
}