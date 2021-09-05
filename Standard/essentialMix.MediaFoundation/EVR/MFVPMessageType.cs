using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.EVR
{
	[UnmanagedName("MFVP_MESSAGE_TYPE")]
	public enum MFVPMessageType
	{
		Flush = 0,
		InvalidateMediaType,
		ProcessInputNotify,
		BeginStreaming,
		EndStreaming,
		EndOfStream,
		Step,
		CancelStep
	}
}