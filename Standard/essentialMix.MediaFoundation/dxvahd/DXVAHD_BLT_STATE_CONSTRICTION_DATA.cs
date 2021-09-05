using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_BLT_STATE_CONSTRICTION_DATA")]
	public struct DXVAHD_BLT_STATE_CONSTRICTION_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool Enable;

		public MFSize xSize;
	}
}