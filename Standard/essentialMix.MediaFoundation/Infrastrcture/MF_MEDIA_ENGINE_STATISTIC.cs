using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("MF_MEDIA_ENGINE_STATISTIC")]
	public enum MF_MEDIA_ENGINE_STATISTIC
	{
		FramesRendered = 0,
		FramesDropped = 1,
		BytesDownloaded = 2,
		BufferProgress = 3,
		FramesPerSecond = 4,
		PlaybackJitter = 5,
		FramesCorrupted = 6,
		TotalFrameDelay = 7,
	}
}