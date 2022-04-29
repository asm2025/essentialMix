using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Repository;

public interface IRepository<TEntity> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	TEntity Add([NotNull] TEntity entity);
	ValueTask<TEntity> AddAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	TEntity Update([NotNull] TEntity entity);
	ValueTask<TEntity> UpdateAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	TEntity Delete([NotNull] TEntity entity);
	ValueTask<TEntity> DeleteAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
}

public interface IRepository<TEntity, TKey> : IRepository<TEntity>
	where TEntity : class, IEntity
{
	TEntity Delete([NotNull] TKey key);
	ValueTask<TEntity> DeleteAsync([NotNull] TKey key, CancellationToken token = default(CancellationToken));
}

public interface IRepository<TEntity, TKey1, TKey2> : IRepository<TEntity>
	where TEntity : class, IEntity
{
	TEntity Delete([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	ValueTask<TEntity> DeleteAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));
}

public interface IRepository<TEntity, TKey1, TKey2, TKey3> : IRepository<TEntity>
	where TEntity : class, IEntity
{
	TEntity Delete([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	ValueTask<TEntity> DeleteAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));
}

public interface IRepository<TEntity, TKey1, TKey2, TKey3, TKey4> : IRepository<TEntity>
	where TEntity : class, IEntity
{
	TEntity Delete([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	ValueTask<TEntity> DeleteAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));
}

public interface IRepository<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IRepository<TEntity>
	where TEntity : class, IEntity
{
	TEntity Delete([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	ValueTask<TEntity> DeleteAsync([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));
}
