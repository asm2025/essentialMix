using System.Collections.Generic;

namespace asm.Media.Youtube
{
	public class VideoInfo : VideoInfoSnippet
	{
		public VideoInfo()
		{
		}

		public ChannelInfo Author { get; internal set; }
		public IReadOnlyList<string> Watermarks { get; internal set; }
		public bool IsListed { get; internal set; }
		public bool IsRatingAllowed { get; internal set; }
		public bool IsMuted { get; internal set; }
		public bool IsEmbeddingAllowed { get; internal set; }
		public IReadOnlyList<MixedStreamInfo> MixedStreams { get; internal set; }
		public IReadOnlyList<AudioStreamInfo> AudioStreams { get; internal set; }
		public IReadOnlyList<VideoStreamInfo> VideoStreams { get; internal set; }
		public IReadOnlyList<ClosedCaptionTrackInfo> ClosedCaptionTracks { get; internal set; }
	}
}