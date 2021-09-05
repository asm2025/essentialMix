using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[UnmanagedName("DXVAHD_BLT_STATE")]
	public enum DXVAHD_BLT_STATE
	{
		TargetRect = 0,
		BackgroundColor = 1,
		OutputColorSpace = 2,
		AlphaFill = 3,
		Constriction = 4,
		Private = 1000
	}
}