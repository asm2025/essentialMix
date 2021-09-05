using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[UnmanagedName("MFP_MEDIAPLAYER_STATE")]
	public enum MFP_MEDIAPLAYER_STATE
	{
		Empty = 0x00000000,
		Stopped = 0x00000001,
		Playing = 0x00000002,
		Paused = 0x00000003,
		Shutdown = 0x00000004
	}
}