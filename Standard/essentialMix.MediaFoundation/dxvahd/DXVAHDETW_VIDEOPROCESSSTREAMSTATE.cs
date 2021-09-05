using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDETW_VIDEOPROCESSSTREAMSTATE")]
	public struct DXVAHDETW_VIDEOPROCESSSTREAMSTATE
	{
		public long pObject;
		public int StreamNumber;
		public DXVAHD_STREAM_STATE State;
		public int DataSize;

		[MarshalAs(UnmanagedType.Bool)]
		public bool SetState;
	}
}