using System;

namespace essentialMix.Web
{
	public class UrlSearchResult
	{
		internal UrlSearchResult()
		{
		}

		public UrlSearchStatus Status { get; internal set; }
		public string Title { get; internal set; }
		public Uri RedirectUri { get; internal set; }
		public string Buffer { get; internal set; }
		public Exception Exception { get; internal set; }
	}
}