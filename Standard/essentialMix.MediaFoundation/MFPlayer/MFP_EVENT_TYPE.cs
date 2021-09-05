using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.MFPlayer
{
	[UnmanagedName("MFP_EVENT_TYPE")]
	public enum MFP_EVENT_TYPE
	{
		Play = 0,
		Pause = 1,
		Stop = 2,
		PositionSet = 3,
		RateSet = 4,
		MediaItemCreated = 5,
		MediaItemSet = 6,
		FrameStep = 7,
		MediaItemCleared = 8,
		MF = 9,
		Error = 10,
		PlaybackEnded = 11,
		AcquireUserCredential = 12
	}
}