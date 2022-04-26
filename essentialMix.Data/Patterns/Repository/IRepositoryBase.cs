using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Repository;

public interface IRepositoryBase : IDisposable
{
}

public interface IRepositoryBase<TEntity> : IRepositoryBase
	where TEntity : class, IEntity
{
	[NotNull]
	Type EntityType { get; }

	[NotNull]
	TEntity Create();
	ValueTask<TEntity> CreateAsync(CancellationToken token = default(CancellationToken));

	[NotNull]
	T Create<T>() where T : TEntity;
	ValueTask<T> CreateAsync<T>(CancellationToken token = default(CancellationToken)) where T : TEntity;

	int Count(IPagination settings = null);
	Task<int> CountAsync(IPagination settings = null, CancellationToken token = default(CancellationToken));

	long LongCount(IPagination settings = null);
	Task<long> LongCountAsync(IPagination settings = null, CancellationToken token = default(CancellationToken));

	IQueryable<TEntity> List(IPagination settings = null);
	Task<IList<TEntity>> ListAsync(IPagination settings = null, CancellationToken token = default(CancellationToken));
}

public interface IRepositoryBase<TEntity, TKey> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	TKey GetKeyValue([NotNull] TEntity entity);

	TEntity Get([NotNull] TKey key);
	Task<TEntity> GetAsync([NotNull] TKey key, CancellationToken token = default(CancellationToken));

	TEntity Get([NotNull] TKey key, [NotNull] IGetSettings settings);
	Task<TEntity> GetAsync([NotNull] TKey key, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IRepositoryBase<TEntity, TKey1, TKey2> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	(TKey1, TKey2) GetKeyValue([NotNull] TEntity entity);

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IRepositoryBase<TEntity, TKey1, TKey2, TKey3> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	(TKey1, TKey2, TKey3) GetKeyValue([NotNull] TEntity entity);

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IRepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	(TKey1, TKey2, TKey3, TKey4) GetKeyValue([NotNull] TEntity entity);

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}

public interface IRepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	(TKey1, TKey2, TKey3, TKey4, TKey5) GetKeyValue([NotNull] TEntity entity);

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));

	TEntity Get([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);
	Task<TEntity> GetAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
}
