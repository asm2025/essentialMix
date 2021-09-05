using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_BLT_STATE_BACKGROUND_COLOR_DATA")]
	public struct DXVAHD_BLT_STATE_BACKGROUND_COLOR_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool YCbCr;

		public DXVAHD_COLOR BackgroundColor;
	}
}