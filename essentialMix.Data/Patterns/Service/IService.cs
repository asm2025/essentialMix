using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Service;

public interface IService<TEntity> : IServiceBase
	where TEntity : class, IEntity
{
	TDetailsDto Add<TDetailsDto>([NotNull] TEntity entity);
	Task<TDetailsDto> AddAsync<TDetailsDto>([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	TDetailsDto Update<TDetailsDto>([NotNull] TEntity entity);
	Task<TDetailsDto> UpdateAsync<TDetailsDto>([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	TDto Delete<TDto>([NotNull] TEntity entity);
	Task<TDto> DeleteAsync<TDto>([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey> : IService<TEntity>
	where TEntity : class, IEntity
{
	TDto Delete<TDto>([NotNull] TKey key);
	Task<TDto> DeleteAsync<TDto>([NotNull] TKey key, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2> : IService<TEntity>
	where TEntity : class, IEntity
{
	TDto Delete<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2);
	Task<TDto> DeleteAsync<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2, TKey3> : IService<TEntity>
	where TEntity : class, IEntity
{
	TDto Delete<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);
	Task<TDto> DeleteAsync<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2, TKey3, TKey4> : IService<TEntity>
	where TEntity : class, IEntity
{
	TDto Delete<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);
	Task<TDto> DeleteAsync<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));
}

public interface IService<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IService<TEntity>
	where TEntity : class, IEntity
{
	TDto Delete<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);
	Task<TDto> DeleteAsync<TDto>([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));
}
