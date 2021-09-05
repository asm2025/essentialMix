using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_FRAME_FORMAT")]
	public enum DXVAHD_FRAME_FORMAT
	{
		Progressive = 0,
		InterlacedTopFieldFirst = 1,
		InterlacedBottomFieldFirst = 2
	}
}