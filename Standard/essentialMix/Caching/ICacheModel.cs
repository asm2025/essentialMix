using System;

namespace essentialMix.Caching
{
	public interface ICacheModel
	{
		string CacheKey { get; set; }
		DateTime Expires { get; set; }
		ICacheObject CacheObject { get; set; }
		ICacheOptions CacheOptions { get; set; }
	}
}
