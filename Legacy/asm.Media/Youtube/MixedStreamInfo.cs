using asm.Extensions;

namespace asm.Media.Youtube
{
	/// <summary>
	///     Mixed (video and audio) stream info
	/// </summary>
	public class MixedStreamInfo : MediaStreamInfo
	{
		/// <inheritdoc />
		public MixedStreamInfo()
		{
		}

		public override int iTag
		{
			get => base.iTag;
			internal set
			{
				base.iTag = value;
				AudioEncoding = GetAudioEncoding(base.iTag);
				VideoEncoding = GetVideoEncoding(base.iTag);
				VideoQuality = GetVideoQuality(base.iTag);
			}
		}

		public AudioEncoding AudioEncoding { get; internal set; }
		public VideoEncoding VideoEncoding { get; internal set; }
		public VideoSizeEnum VideoQuality { get; internal set; }
		public string VideoQualityLabel => VideoQuality.GetDisplayName();
	}
}