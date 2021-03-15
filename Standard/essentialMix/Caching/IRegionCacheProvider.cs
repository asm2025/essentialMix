using JetBrains.Annotations;

namespace essentialMix.Caching
{
	/// <inheritdoc />
	public interface IRegionCacheProvider : ICacheProvider
	{
		/// <summary>
		/// Get from cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="region">If region is supported by cache, it will separate the lookups</param>
		/// <returns>
		/// An object instance with the Cache Value corresponding to the entry if found, else null
		/// </returns>
		object Get([NotNull] string key, string region);
        
		/// <summary>
		/// Gets the specified cache key.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="key">The cache key.</param>
		/// <param name="region">If region is supported by cache , it will separate the lookups</param>
		/// <returns>An Instance of T if the entry is found, else null.</returns>
		T Get<T>([NotNull] string key, string region);
        
		/// <summary>
		/// Check if the item exist
		/// </summary>
		/// <param name="key"></param>
		/// <param name="region"></param>
		/// <returns>true false</returns>
		bool Exist([NotNull] string key, string region);

		/// <summary>
		/// Gets the cache count by region
		/// </summary>
		long Count(string region);

		/// <summary>
		/// Add to cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The cache object.</param>
		/// <param name="region"></param>
		/// <returns>True if successful else false.</returns>
		bool Add([NotNull] string key, object value, string region);

		/// <summary>
		/// Add to cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The cache object.</param>
		/// <param name="region">If region is supported by cache , it will separate the lookups</param>
		/// <param name="options">Options that can be set for the cache</param>
		/// <returns>True if successful else false.</returns>
		bool Add([NotNull] string key, object value, string region, ICacheOptions options);

		/// <summary>
		/// Add an item to the cache and will need to be removed manually
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="value">The cache object.</param>
		/// <param name="region">If region is supported by cache , it will separate the lookups</param>
		/// <param name="options">Options that can be set for the cache</param>
		/// <returns>true or false</returns>
		bool AddPermanent([NotNull] string key, object value, string region, ICacheOptions options);

		/// <summary>
		/// Remove from cache.
		/// </summary>
		/// <param name="key">The cache key.</param>
		/// <param name="region"></param>
		/// <returns>True if successful else false.</returns>
		bool Remove([NotNull] string key, string region);

		/// <summary>
		/// Remove everything from cache.
		/// </summary>
		bool RemoveAll(string region);

		/// <summary>
		/// Remove everything that has expired from cache.
		/// </summary>
		bool RemoveExpired(string region);
	}
}