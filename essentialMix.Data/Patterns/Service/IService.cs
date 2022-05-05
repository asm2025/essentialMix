using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Service;

public interface IService<TEntity> : IServiceBase<TEntity>
	where TEntity : class, IEntity
{
	T Add<T>([NotNull] TEntity entity);
	Task<T> AddAsync<T>([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	T Update<T>([NotNull] TEntity entity);
	Task<T> UpdateAsync<T>([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	T Delete<T>([NotNull] TEntity entity);
	Task<T> DeleteAsync<T>([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey> : IService<TEntity>, IServiceBase<TEntity, TKey>
	where TEntity : class, IEntity
{
	T Delete<T>([NotNull] TKey key);
	Task<T> DeleteAsync<T>([NotNull] TKey key, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2> : IService<TEntity>, IServiceBase<TEntity, TKey1, TKey2>
	where TEntity : class, IEntity
{
	T Delete<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	Task<T> DeleteAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2, TKey3> : IService<TEntity>, IServiceBase<TEntity, TKey1, TKey2, TKey3>
	where TEntity : class, IEntity
{
	T Delete<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	Task<T> DeleteAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2, TKey3, TKey4> : IService<TEntity>, IServiceBase<TEntity, TKey1, TKey2, TKey3, TKey4>
	where TEntity : class, IEntity
{
	T Delete<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	Task<T> DeleteAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IService<TEntity>, IServiceBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>
	where TEntity : class, IEntity
{
	T Delete<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	Task<T> DeleteAsync<T>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));
}
