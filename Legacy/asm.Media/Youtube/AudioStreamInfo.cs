namespace asm.Media.Youtube
{
	public class AudioStreamInfo : MediaStreamInfo
	{
		public AudioStreamInfo()
		{
		}

		public override int iTag
		{
			get => base.iTag;
			internal set
			{
				base.iTag = value;
				AudioEncoding = GetAudioEncoding(base.iTag);
			}
		}

		public long Bitrate { get; internal set; }
		public AudioEncoding AudioEncoding { get; internal set; }
	}
}