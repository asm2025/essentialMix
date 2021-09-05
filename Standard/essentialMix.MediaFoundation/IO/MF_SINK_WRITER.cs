using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.IO
{
	[UnmanagedName("Unnamed enum")]
	public enum MF_SINK_WRITER
	{
		InvalidStreamIndex = unchecked((int)0xFFFFFFFF),
		AllStreams = unchecked((int)0xFFFFFFFE),
		MediaSink = unchecked((int)0xFFFFFFFF)
	}
}