using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[StructLayout(LayoutKind.Sequential, Pack = 4)]
	[UnmanagedName("MFT_OUTPUT_STREAM_INFO")]
	public struct MFTOutputStreamInfo
	{
		public MFTOutputStreamInfoFlags dwFlags;
		public int cbSize;
		public int cbAlignment;
	}
}