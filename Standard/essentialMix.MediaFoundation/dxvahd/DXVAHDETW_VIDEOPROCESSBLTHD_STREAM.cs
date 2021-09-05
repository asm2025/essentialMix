using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.dxvahd
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("DXVAHDETW_VIDEOPROCESSBLTHD_STREAM")]
	public struct DXVAHDETW_VIDEOPROCESSBLTHD_STREAM
	{
		public long pObject;
		public long pInputSurface;
		public MFRect SourceRect;
		public MFRect DestinationRect;
		public int InputFormat; // D3DFORMAT
		public DXVAHD_FRAME_FORMAT FrameFormat;
		public int ColorSpace;
		public int StreamNumber;
		public int OutputIndex;
		public int InputFrameOrField;
		public int PastFrames;
		public int FutureFrames;
	}
}