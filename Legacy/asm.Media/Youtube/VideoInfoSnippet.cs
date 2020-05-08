using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Media.Youtube
{
	public class VideoInfoSnippet
	{
		public VideoInfoSnippet()
		{
		}

		public string Id { get; internal set; }
		public string Title { get; internal set; }
		public TimeSpan Duration { get; internal set; }
		public string Description { get; internal set; }
		public IReadOnlyList<string> Keywords { get; internal set; }
		[NotNull]
		public string ImageThumbnailUrl => $"https://img.youtube.com/vi/{Id}/default.jpg";

		[NotNull]
		public string ImageMediumResUrl => $"https://img.youtube.com/vi/{Id}/mqdefault.jpg";

		[NotNull]
		public string ImageHighResUrl => $"https://img.youtube.com/vi/{Id}/hqdefault.jpg";

		[NotNull]
		public string ImageStandardResUrl => $"https://img.youtube.com/vi/{Id}/sddefault.jpg";

		[NotNull]
		public string ImageMaxResUrl => $"https://img.youtube.com/vi/{Id}/maxresdefault.jpg";

		public long ViewCount { get; internal set; }
		public long LikeCount { get; internal set; }
		public long DislikeCount { get; internal set; }
		public double AverageRating => 1 + 4.0 * LikeCount / (LikeCount + DislikeCount);
	}
}