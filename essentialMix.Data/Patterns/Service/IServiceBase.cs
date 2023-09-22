using System;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Service;

public interface IServiceBase : IDisposable
{
}

public interface IServiceBase<TEntity> : IServiceBase
	where TEntity : class
{
	[NotNull]
	Type EntityType { get; }
	IPaginated<TEntity> List(IPagination settings = null);
	IPaginated<T> List<T>(IPagination settings = null);
	[NotNull]
	Task<IPaginated<TEntity>> ListAsync(CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<IPaginated<TEntity>> ListAsync(IPagination settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<IPaginated<T>> ListAsync<T>(CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<IPaginated<T>> ListAsync<T>(IPagination settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey> : IServiceBase<TEntity>
	where TEntity : class
{
	TEntity Get([NotNull] TKey key);
	TEntity Get([NotNull] TKey key, [NotNull] IGetSettings settings);
	T Get<T>([NotNull] TKey key);
	T Get<T>([NotNull] TKey key, [NotNull] IGetSettings settings);
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey key, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey key, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey key, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey key, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2> : IServiceBase<TEntity>
	where TEntity : class
{
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2, TKey3> : IServiceBase<TEntity>
	where TEntity : class
{
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2, TKey3, TKey4> : IServiceBase<TEntity>
	where TEntity : class
{
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IServiceBase<TEntity>
	where TEntity : class
{
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));
	[NotNull]
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}
