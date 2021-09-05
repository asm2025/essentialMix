using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Explicit)]
	[UnmanagedName("DXVAHD_COLOR")]
	public struct DXVAHD_COLOR
	{
		[FieldOffset(0)]
		public DXVAHD_COLOR_RGBA RGB;

		[FieldOffset(0)]
		public DXVAHD_COLOR_YCbCrA YCbCr;
	}
}