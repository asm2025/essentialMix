using essentialMix.ComponentModel.DataAnnotations;

namespace essentialMix.MediaFoundation.Infrastrcture
{
	[UnmanagedName("AUDIO_STREAM_CATEGORY")]
	public enum AUDIO_STREAM_CATEGORY
	{
		Other = 0,
		ForegroundOnlyMedia,
		BackgroundCapableMedia,
		Communications,
		Alerts,
		SoundEffects,
		GameEffects,
		GameMedia,
		GameChat,
		Speech,
		Movie,
		Media
	}
}