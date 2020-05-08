using System.Threading;
using System.Threading.Tasks;
using asm.Data.Model;
using asm.Data.Patterns.Repository;
using JetBrains.Annotations;
using SystemDbContext = System.Data.Entity.DbContext;

namespace asm.Data.Entity.Patterns.Repository
{
	public interface IRepository<out TContext, TEntity> : IRepositoryBase<TContext, TEntity>, IRepository<TEntity>
		where TContext : SystemDbContext
		where TEntity : class, IEntity
	{
		TEntity Attach([NotNull] TEntity entity);
		ValueTask<TEntity> AttachAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	}
}