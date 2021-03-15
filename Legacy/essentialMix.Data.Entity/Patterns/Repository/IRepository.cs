using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Repository;
using JetBrains.Annotations;
using SystemDbContext = System.Data.Entity.DbContext;

namespace essentialMix.Data.Entity.Patterns.Repository
{
	public interface IRepository<out TContext, TEntity> : IRepositoryBase<TContext, TEntity>, IRepository<TEntity>
		where TContext : SystemDbContext
		where TEntity : class, IEntity
	{
		TEntity Attach([NotNull] TEntity entity);
		ValueTask<TEntity> AttachAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	}
}