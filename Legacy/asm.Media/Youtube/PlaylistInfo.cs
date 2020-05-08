using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Media.Youtube
{
	public class PlaylistInfo
	{
		public PlaylistInfo()
		{
		}

		public string Id { get; internal set; }
		public PlaylistType Type { get; internal set; }
		public string Title { get; internal set; }
		public string Author { get; internal set; }
		public string Description { get; internal set; }
		public long ViewCount { get; internal set; }
		public IReadOnlyList<VideoInfoSnippet> Videos { get; internal set; }

		protected static PlaylistType GetPlaylistType([NotNull] string id)
		{
			if (id == null) throw new ArgumentNullException(nameof(id));

			if (id.StartsWith("PL", StringComparison.OrdinalIgnoreCase)) return PlaylistType.Normal;

			if (id.StartsWith("RD", StringComparison.OrdinalIgnoreCase)) return PlaylistType.VideoMix;

			if (id.StartsWith("UL", StringComparison.OrdinalIgnoreCase)) return PlaylistType.ChannelVideoMix;

			if (id.StartsWith("UU", StringComparison.OrdinalIgnoreCase)) return PlaylistType.ChannelVideos;

			if (id.StartsWith("PU", StringComparison.OrdinalIgnoreCase)) return PlaylistType.PopularChannelVideos;

			if (id.StartsWith("LL", StringComparison.OrdinalIgnoreCase)) return PlaylistType.LikedVideos;

			if (id.StartsWith("FL", StringComparison.OrdinalIgnoreCase)) return PlaylistType.Favorites;

			if (id.StartsWith("WL", StringComparison.OrdinalIgnoreCase)) return PlaylistType.WatchLater;

			throw new NotSupportedException();
		}
	}
}