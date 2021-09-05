using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHD_CONTENT_DESC")]
	public struct DXVAHD_CONTENT_DESC
	{
		public DXVAHD_FRAME_FORMAT InputFrameFormat;
		public DXVAHD_RATIONAL InputFrameRate;
		public int InputWidth;
		public int InputHeight;
		public DXVAHD_RATIONAL OutputFrameRate;
		public int OutputWidth;
		public int OutputHeight;
	}
}