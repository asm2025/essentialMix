using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Repository;
using JetBrains.Annotations;
using SystemDbContext = System.Data.Entity.DbContext;

namespace essentialMix.Data.Entity.Patterns.Repository;

public interface IRepository<TContext, TEntity> : IRepositoryBase<TContext, TEntity>, IRepository<TEntity>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	TEntity Attach([NotNull] TEntity entity);
	ValueTask<TEntity> AttachAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
}

public interface IRepository<TContext, TEntity, TKey> : IRepository<TContext, TEntity>, IRepositoryBase<TContext, TEntity, TKey>, essentialMix.Data.Patterns.Repository.IRepository<TEntity, TKey>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepository<TContext, TEntity, TKey1, TKey2> : IRepository<TContext, TEntity>, IRepositoryBase<TContext, TEntity, TKey1, TKey2>, essentialMix.Data.Patterns.Repository.IRepository<TEntity, TKey1, TKey2>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepository<TContext, TEntity, TKey1, TKey2, TKey3> : IRepository<TContext, TEntity>, IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3>, essentialMix.Data.Patterns.Repository.IRepository<TEntity, TKey1, TKey2, TKey3>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepository<TContext, TEntity, TKey1, TKey2, TKey3, TKey4> : IRepository<TContext, TEntity>, IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4>, essentialMix.Data.Patterns.Repository.IRepository<TEntity, TKey1, TKey2, TKey3, TKey4>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}

public interface IRepository<TContext, TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : IRepository<TContext, TEntity>, IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>, essentialMix.Data.Patterns.Repository.IRepository<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
}
