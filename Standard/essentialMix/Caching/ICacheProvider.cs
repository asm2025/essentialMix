using System.Collections.Specialized;
using JetBrains.Annotations;

namespace essentialMix.Caching
{
	public interface ICacheProvider
	{
		string Name { get; }
		string Description { get; }
        
		/// <summary>
		/// Initialize from config
		/// </summary>
		/// <param name="name"></param>
		/// <param name="config">Config properties</param>
		void Initialize(string name, NameValueCollection config);

		/// <summary>
        /// Get from cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>
        /// An object instance with the Cache Value corresponding to the entry if found, else null
        /// </returns>
        object Get([NotNull] string key);
        
        /// <summary>
        /// Gets the specified cache key.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The cache key.</param>
        /// <returns>An Instance of T if the entry is found, else null.</returns>
        T Get<T>([NotNull] string key);
        
        /// <summary>
        /// Check if the item exist
        /// </summary>
        /// <param name="key"></param>
        /// <returns>true false</returns>
        bool Contains([NotNull] string key);

        /// <summary>
        /// Gets the cache count by region
        /// </summary>
        long Count();

		/// <summary>
        /// Add to cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache object.</param>
        /// <returns>True if successful else false.</returns>
        bool Add([NotNull] string key, object value);

        /// <summary>
        /// Add to cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache object.</param>
        /// <param name="options">Options that can be set for the cache</param>
        /// <returns>True if successful else false.</returns>
        bool Add([NotNull] string key, object value, ICacheOptions options);

        /// <summary>
        /// Add an item to the cache and will need to be removed manually
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <param name="value">The cache object.</param>
        /// <param name="options">Options that can be set for the cache</param>
        /// <returns>true or false</returns>
        bool AddPermanent([NotNull] string key, object value, ICacheOptions options);

		/// <summary>
        /// Remove from cache.
        /// </summary>
        /// <param name="key">The cache key.</param>
        /// <returns>True if successful else false.</returns>
        bool Remove([NotNull] string key);

        /// <summary>
        /// Remove everything that has expired from cache.
        /// </summary>
        bool RemoveExpired();

		/// <summary>
		/// Remove everything from cache.
		/// </summary>
		bool Clear();
    }
}