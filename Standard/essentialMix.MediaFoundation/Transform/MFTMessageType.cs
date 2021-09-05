using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Transform
{
	[UnmanagedName("MFT_MESSAGE_TYPE")]
	public enum MFTMessageType
	{
		CommandDrain = 1,
		CommandFlush = 0,
		NotifyBeginStreaming = 0x10000000,
		NotifyEndOfStream = 0x10000002,
		NotifyEndStreaming = 0x10000001,
		NotifyStartOfStream = 0x10000003,
		SetD3DManager = 2,
		DropSamples = 0x00000003,
		CommandTick = 0x00000004,
		CommandMarker = 0x20000000,
		NotifyReleaseResources = 0x10000004,
		NotifyReacquireResources = 0x10000005
	}
}