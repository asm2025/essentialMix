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

// ReSharper disable once UnusedTypeParameter
public interface IServiceBase<TEntity> : IServiceBase
	where TEntity : class
{
	[NotNull]
	Type EntityType { get; }
	[NotNull]
	IPaginated<T> List<T>(IPagination settings = null);
	[NotNull]
	[ItemNotNull]
	Task<IPaginated<T>> ListAsync<T>(IPagination settings = null, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey> : IServiceBase<TEntity>
	where TEntity : class
{
	T Get<T>([NotNull] TKey key);
	Task<T> GetAsync<T>([NotNull] TKey key, CancellationToken token = default(CancellationToken));

	T Get<T>([NotNull] TKey key, [NotNull] IGetSettings settings);
	Task<T> GetAsync<T>([NotNull] TKey key, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2> : IServiceBase<TEntity>
	where TEntity : class
{
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));

	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2, TKey3> : IServiceBase<TEntity>
	where TEntity : class
{
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));

	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2, TKey3, TKey4> : IServiceBase<TEntity>
	where TEntity : class
{
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));

	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IServiceBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IServiceBase<TEntity>
	where TEntity : class
{
	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));

	T Get<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);
	Task<T> GetAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}
