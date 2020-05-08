using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Runtime.Caching;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Caching
{
	public class MemoryCacheProvider : ProviderBase, ICacheProvider
	{
		private MemoryCache _cache;
		private TimeSpan _expirationTime;

		/// <inheritdoc />
		public MemoryCacheProvider()
			: this(MemoryCache.Default)
		{
		}

		public MemoryCacheProvider([NotNull] string name)
			: this(new MemoryCache(name))
		{
		}

		private MemoryCacheProvider(MemoryCache cache)
		{
			SyncRoot = new object();
			_cache = cache;
			ExpirationTime = CacheOptions.ExpirationTimeDefault;
			Enabled = true;
		}

		[NotNull]
		public object SyncRoot { get; }

		public TimeSpan ExpirationTime
		{
			get => _expirationTime;
			set => _expirationTime = value.NotBelow(TimeSpan.Zero);
		}

		public bool Enabled { get; set; }

		/// <inheritdoc cref="ProviderBase" />
		public override void Initialize(string name, [NotNull] NameValueCollection config)
		{
			base.Initialize(name, config);
			if (config.ContainsKey("timeout")) ExpirationTime = config["timeout"].IfNullOrEmpty(ExpirationTime.ToString).To(ExpirationTime);
			if (config.ContainsKey("enable")) Enabled = config["enable"].ToBoolean(Enabled);
		}

		public virtual object Get(string key)
		{
			if (!Enabled) return null;

			object item;

			lock (SyncRoot)
			{
				item = _cache.Get(Convert.ToString(key));
			}

			switch (item)
			{
				case CacheItem model:
					return model.Value.Deserialize();
				case CacheObject value:
					return value.Item.Deserialize();
				default:
					return null;
			}
		}

		public virtual T Get<T>(string key)
		{
			if (!Enabled) return default(T);

			object item;

			lock (SyncRoot)
			{
				item = _cache.Get(key);
			}

			switch (item)
			{
				case CacheItem model:
					return model.Value.Deserialize<T>();
				case CacheObject cacheObject:
					return cacheObject.Item.Deserialize<T>();
				default:
					return default(T);
			}
		}

		public virtual bool Contains(string key)
		{
			if (!Enabled) return false;

			lock(SyncRoot)
			{
				return _cache[key] != null;
			}
		}

		public virtual long Count()
		{
			if (!Enabled)
				return 0;

			lock (SyncRoot)
			{
				return _cache.GetCount();
			}
		}

		public virtual bool Add(string key, object value) { return Add(key, value, CacheOptions.Default); }

		public virtual bool Add(string key, object value, ICacheOptions options)
		{
			if (!Enabled) 
				return true;

			if (Contains(key))
				Remove(key);

			if (options.ExpirationInMilliSeconds < 1)
				options.ExpirationInMilliSeconds = CacheOptions.EXPIRATION_TIME_DEFAULT;

			CacheItemPolicy cacheItemPolicy = new CacheItemPolicy();

			if (options.AllowSlidingTime)
				cacheItemPolicy.SlidingExpiration = TimeSpan.FromMilliseconds(options.ExpirationInMilliSeconds);
			else
				cacheItemPolicy.AbsoluteExpiration = DateTimeOffset.UtcNow.AddMilliseconds(options.ExpirationInMilliSeconds);

			return AddOrUpdateItem(key, value, cacheItemPolicy);
		}

		public virtual bool AddPermanent(string key, object value, ICacheOptions options)
		{
			if (!Enabled)
				return true;

			if (Contains(key))
				Remove(key);

			return AddOrUpdateItem(key, value, new CacheItemPolicy());
		}

		public virtual bool Remove(string key)
		{
			if (!Enabled)
				return true;

			lock (SyncRoot)
			{
				_cache.Remove(key);
			}

			return true;
		}

		/// <inheritdoc />
		public bool RemoveExpired() { return Clear(); }

		/// <summary>
		/// Remove from cache.(region not supported in memorycache)
		/// </summary>
		/// <returns>True if successful else false.</returns>
		public virtual bool Clear()
		{
			if (!Enabled)
				return true;

			lock (SyncRoot)
			{
				_cache.Dispose();
				_cache = MemoryCache.Default;
			}

			return true;
		}

		private bool AddOrUpdateItem([NotNull] string key, object value, CacheItemPolicy cacheItemPolicy)
		{
			ICacheObject cacheItem = new CacheObject
			{
				Item = value.SerializeBinary()
			};
			
			bool results;

			lock (SyncRoot)
			{
				results = _cache.Add(key, cacheItem, cacheItemPolicy);
			}

			return results;
		}

		protected bool AddOrUpdateItem([NotNull] string key, object value, int expireCacheTime, CacheItemPolicy cacheItemPolicy, bool allowSlidingTime = false)
		{
			if (expireCacheTime < 0) expireCacheTime = CacheOptions.EXPIRATION_TIME_DEFAULT;
			
			DateTime expireTime = DateTime.UtcNow.AddMilliseconds(expireCacheTime);

			CacheItem item = new CacheItem
			{
				Key = key,
				Expires = expireTime,
				Value = value.SerializeBinary(),
				AllowSlidingTime = allowSlidingTime
			};

			bool results;

			lock (SyncRoot)
			{
				results = _cache.Add(key, item, cacheItemPolicy);
			}

			return results;
		}
	}
}