using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_BLT_STATE_ALPHA_FILL_DATA")]
	public struct DXVAHD_BLT_STATE_ALPHA_FILL_DATA
	{
		public DXVAHD_ALPHA_FILL_MODE Mode;
		public int StreamNumber;
	}
}