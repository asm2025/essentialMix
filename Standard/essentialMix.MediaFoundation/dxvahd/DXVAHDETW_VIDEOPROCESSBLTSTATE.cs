using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDETW_VIDEOPROCESSBLTSTATE")]
	public struct DXVAHDETW_VIDEOPROCESSBLTSTATE
	{
		public long pObject;
		public DXVAHD_BLT_STATE State;
		public int DataSize;

		[MarshalAs(UnmanagedType.Bool)]
		public bool SetState;
	}
}