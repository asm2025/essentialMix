using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using asm.Data.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asm.Data.Patterns.Repository
{
	public abstract class Repository<TEntity> : RepositoryBase<TEntity>, IRepository<TEntity>
		where TEntity : class, IEntity
	{
		/// <inheritdoc />
		protected Repository([NotNull] IConfiguration configuration) 
			: this(configuration, null)
		{
		}

		/// <inheritdoc />
		protected Repository([NotNull] IConfiguration configuration, ILogger logger)
			: base(configuration, logger)
		{
		}

		/// <inheritdoc />
		public TEntity Add(TEntity entity)
		{
			ThrowIfDisposed();
			return AddInternal(entity);
		}

		protected abstract TEntity AddInternal([NotNull] TEntity entity);

		/// <inheritdoc />
		public ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return AddAsyncInternal(entity, token);
		}
		
		protected abstract ValueTask<TEntity> AddAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public TEntity Update(TEntity entity)
		{
			ThrowIfDisposed();
			return UpdateInternal(entity);
		}
		
		protected abstract TEntity UpdateInternal([NotNull] TEntity entity);

		/// <inheritdoc />
		public ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return UpdateAsyncInternal(entity, token);
		}
		
		protected abstract ValueTask<TEntity> UpdateAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public void Delete(object[] key)
		{
			ThrowIfDisposed();
			DeleteInternal(key);
		}

		protected abstract void DeleteInternal([NotNull] object[] key);

		/// <inheritdoc />
		public ValueTask DeleteAsync(object[] key, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return DeleteAsyncInternal(key, token);
		}

		protected abstract ValueTask DeleteAsyncInternal([NotNull] object[] key, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public TEntity Delete(TEntity entity)
		{
			ThrowIfDisposed();
			return DeleteInternal(entity);
		}

		protected abstract TEntity DeleteInternal([NotNull] TEntity entity);

		public ValueTask<TEntity> DeleteAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return DeleteAsyncInternal(entity, token);
		}

		protected abstract ValueTask<TEntity> DeleteAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken));
	}
}