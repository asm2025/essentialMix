namespace asm.Media.Youtube
{
	public class VideoStreamInfo : MediaStreamInfo
	{
		public VideoStreamInfo()
		{
		}

		public long Bitrate { get; internal set; }
		public VideoEncoding VideoEncoding { get; internal set; }
		public VideoSize VideoSize { get; internal set; }
		public double VideoFrameRate { get; internal set; }
	}
}