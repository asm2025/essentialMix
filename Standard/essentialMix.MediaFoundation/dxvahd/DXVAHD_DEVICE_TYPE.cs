using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_DEVICE_TYPE")]
	public enum DXVAHD_DEVICE_TYPE
	{
		Hardware = 0,
		Software = 1,
		Reference = 2,
		Other = 3
	}
}