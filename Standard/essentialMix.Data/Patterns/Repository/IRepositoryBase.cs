using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Repository
{
	public interface IRepositoryBase : IDisposable
	{
	}

	public interface IRepositoryBase<TEntity> : IRepositoryBase
		where TEntity : class, IEntity
	{
		[NotNull]
		Type EntityType { get; }
		
		object[] GetKeyValue([NotNull] TEntity entity);

		[NotNull]
		TEntity Create();
		ValueTask<TEntity> CreateAsync(CancellationToken token = default(CancellationToken));

		[NotNull]
		T Create<T>() where T : TEntity;
		ValueTask<T> CreateAsync<T>(CancellationToken token = default(CancellationToken)) where T : TEntity;

		IQueryable<TEntity> List(IPagination settings = null);
		ValueTask<IList<TEntity>> ListAsync(IPagination settings = null, CancellationToken token = default(CancellationToken));

		TEntity Get([NotNull] params object[] keys);
		ValueTask<TEntity> GetAsync([NotNull] params object[] keys);
		ValueTask<TEntity> GetAsync(CancellationToken token, [NotNull] params object[] keys);
		ValueTask<TEntity> GetAsync([NotNull] object[] keys, CancellationToken token);

		TEntity Get([NotNull] IGetSettings settings);
		ValueTask<TEntity> GetAsync([NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));
	}
}
