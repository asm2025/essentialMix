using System.Runtime.InteropServices;
using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.IO
{
	[StructLayout(LayoutKind.Sequential)]
	[UnmanagedName("MF_SINK_WRITER_STATISTICS")]
	public struct MF_SINK_WRITER_STATISTICS
	{
		public int cb;

		public long llLastTimestampReceived;
		public long llLastTimestampEncoded;
		public long llLastTimestampProcessed;
		public long llLastStreamTickReceived;
		public long llLastSinkSampleRequest;

		public long qwNumSamplesReceived;
		public long qwNumSamplesEncoded;
		public long qwNumSamplesProcessed;
		public long qwNumStreamTicksReceived;

		public int dwByteCountQueued;
		public long qwByteCountProcessed;

		public int dwNumOutstandingSinkSampleRequests;

		public int dwAverageSampleRateReceived;
		public int dwAverageSampleRateEncoded;
		public int dwAverageSampleRateProcessed;
	}
}