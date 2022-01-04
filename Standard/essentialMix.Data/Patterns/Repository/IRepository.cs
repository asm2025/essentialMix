using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using essentialMix.Data.Model;

namespace essentialMix.Data.Patterns.Repository;

public interface IRepository<TEntity> : IRepositoryBase<TEntity>
	where TEntity : class, IEntity
{
	TEntity Add([NotNull] TEntity entity);
	ValueTask<TEntity> AddAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	TEntity Update([NotNull] TEntity entity);
	ValueTask<TEntity> UpdateAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	void Delete([NotNull] object[] key);
	ValueTask DeleteAsync([NotNull] object[] key, CancellationToken token = default(CancellationToken));
	TEntity Delete([NotNull] TEntity entity);
	ValueTask<TEntity> DeleteAsync([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
}