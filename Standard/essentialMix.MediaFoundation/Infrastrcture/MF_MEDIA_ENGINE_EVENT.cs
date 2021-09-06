using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_EVENT")]
	public enum MF_MEDIA_ENGINE_EVENT
	{
		LoadStart = 1,
		Progress = 2,
		Suspend = 3,
		Abort = 4,
		Error = 5,
		Emptied = 6,
		Stalled = 7,
		Play = 8,
		Pause = 9,
		LoadedMetadata = 10,
		LoadedData = 11,
		Waiting = 12,
		Playing = 13,
		CanPlay = 14,
		CanPlayThrough = 15,
		Seeking = 16,
		Seeked = 17,
		TimeUpdate = 18,
		Ended = 19,
		RateChange = 20,
		DurationChange = 21,
		VolumeChange = 22,

		FormatChange = 1000,
		PurgeQueuedEvents = 1001,
		TimelineMarker = 1002,
		BalanceChange = 1003,
		DownloadComplete = 1004,
		BufferingStarted = 1005,
		BufferingEnded = 1006,
		FrameStepCompleted = 1007,
		NotifyStableState = 1008,
		FirstFrameReady = 1009,
		TracksChange = 1010,
		OpmInfo = 1011,

		ResourceLost = 1012,
		DelayLoadEventChanged = 1013,
		StreamRenderingError = 1014,
	}
}