using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_OUTPUT_RATE_DATA")]
	public struct DXVAHD_STREAM_STATE_OUTPUT_RATE_DATA
	{
		[MarshalAs(UnmanagedType.Bool)]
		public bool RepeatFrame;

		public DXVAHD_OUTPUT_RATE OutputRate;
		public DXVAHD_RATIONAL CustomRate;
	}
}