using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_BLT_STATE_TARGET_RECT_DATA")]
	public struct DXVAHD_BLT_STATE_TARGET_RECT_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public MFRect TargetRect;
	}
}