namespace essentialMix.Caching
{
	/// <summary>
	/// This interface allows custom types to be cache keys
	/// </summary>
	public interface ICacheKey
	{
		void BuildCacheKey(CacheKeyBuilder builder);
	}
}