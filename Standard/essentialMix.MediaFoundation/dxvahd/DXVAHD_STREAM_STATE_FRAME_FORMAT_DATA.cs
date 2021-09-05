using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_STREAM_STATE_FRAME_FORMAT_DATA")]
	public struct DXVAHD_STREAM_STATE_FRAME_FORMAT_DATA
	{
		public DXVAHD_FRAME_FORMAT FrameFormat;
	}
}