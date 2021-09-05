using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[StructLayout(LayoutKind.Sequential, Pack = 8)]
	[UnmanagedName("MFT_INPUT_STREAM_INFO")]
	public struct MFTInputStreamInfo
	{
		public long hnsMaxLatency;
		public MFTInputStreamInfoFlags dwFlags;
		public int cbSize;
		public int cbMaxLookahead;
		public int cbAlignment;
	}
}
