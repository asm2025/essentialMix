using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("ASF_MUX_STATISTICS")]
	public struct ASFMuxStatistics
	{
		public int cFramesWritten;
		public int cFramesDropped;
	}
}