using System.Net.Cache;
using JetBrains.Annotations;

namespace asm.Helpers
{
	public static class WebRequestHelper
	{
		public const string DIVIDER = "<!>";
		public const int TIMEOUT_DEF = 300000;

		[NotNull]
		public static HttpRequestCachePolicy CachePolicy => new HttpRequestCachePolicy(HttpRequestCacheLevel.BypassCache);
	}
}