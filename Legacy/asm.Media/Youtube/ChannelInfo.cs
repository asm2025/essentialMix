using JetBrains.Annotations;

namespace asm.Media.Youtube
{
	public class ChannelInfo
	{
		public ChannelInfo()
		{
		}

		public string Id { get; internal set; }
		[NotNull]
		public string Url => $"https://www.youtube.com/channel/{Id}";

		public string Name { get; internal set; }
		public string Title { get; internal set; }
		public bool IsPaid { get; internal set; }
		public string LogoUrl { get; internal set; }
		public string BannerUrl { get; internal set; }
	}
}