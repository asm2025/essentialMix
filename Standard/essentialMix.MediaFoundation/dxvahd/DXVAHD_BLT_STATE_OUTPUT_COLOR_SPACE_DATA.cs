using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_BLT_STATE_OUTPUT_COLOR_SPACE_DATA")]
	public struct DXVAHD_BLT_STATE_OUTPUT_COLOR_SPACE_DATA
	{
		public int Value;
	}
}