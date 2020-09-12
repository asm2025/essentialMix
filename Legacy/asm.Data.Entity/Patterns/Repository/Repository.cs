using System.Collections.Generic;
using System.Data.Entity;
using System.Threading;
using System.Threading.Tasks;
using asm.Data.Model;
using asm.Threading.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemDbContext = System.Data.Entity.DbContext;

namespace asm.Data.Entity.Patterns.Repository
{
	public abstract class Repository<TContext, TEntity> : RepositoryBase<TContext, TEntity>, IRepository<TContext, TEntity>
		where TContext : SystemDbContext
		where TEntity : class, IEntity
	{
		/// <inheritdoc />
		protected Repository([NotNull] TContext context, [NotNull] IConfiguration configuration)
			: this(context, configuration, null, false)
		{
		}

		/// <inheritdoc />
		protected Repository([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger)
			: this(context, configuration, logger, false)
		{
		}

		/// <inheritdoc />
		protected Repository([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger, bool ownsContext)
			: base(context, configuration, logger, ownsContext)
		{
		}

		/// <inheritdoc />
		public TEntity Add(TEntity entity)
		{
			ThrowIfDisposed();
			return AddInternal(entity);
		}

		protected virtual TEntity AddInternal([NotNull] TEntity entity) { return DbSet.Add(entity); }

		/// <inheritdoc />
		public ValueTask<TEntity> AddAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return AddAsyncInternal(entity, token);
		}

		protected virtual ValueTask<TEntity> AddAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(AddInternal(entity));
		}

		/// <inheritdoc />
		public TEntity Attach(TEntity entity)
		{
			ThrowIfDisposed();
			return AttachInternal(entity);
		}

		protected virtual TEntity AttachInternal([NotNull] TEntity entity) { return DbSet.Attach(entity); }

		/// <inheritdoc />
		public ValueTask<TEntity> AttachAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return AttachAsyncInternal(entity, token);
		}

		protected virtual ValueTask<TEntity> AttachAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(AttachInternal(entity));
		}

		/// <inheritdoc />
		[NotNull]
		public TEntity Update(TEntity entity)
		{
			ThrowIfDisposed();
			return UpdateInternal(entity);
		}

		[NotNull]
		protected virtual TEntity UpdateInternal([NotNull] TEntity entity)
		{
			DbSet.Attach(entity);
			Context.Entry(entity).State = EntityState.Modified;
			return entity;
		}

		/// <inheritdoc />
		public ValueTask<TEntity> UpdateAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return UpdateAsyncInternal(entity, token);
		}

		protected virtual ValueTask<TEntity> UpdateAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(UpdateInternal(entity));
		}

		/// <inheritdoc />
		public void Delete(object[] key)
		{
			ThrowIfDisposed();
			DeleteInternal(key);
		}

		protected virtual void DeleteInternal([NotNull] object[] key)
		{
			TEntity entity = DbSet.Find(key) ?? throw new KeyNotFoundException();
			DeleteInternal(entity);
		}

		/// <inheritdoc />
		public ValueTask DeleteAsync(object[] key, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return DeleteAsyncInternal(key, token);
		}

		protected virtual ValueTask DeleteAsyncInternal([NotNull] object[] key, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask(DbSet.FindAsync(token, key)
									.Then(e => DeleteAsyncInternal(e.Result, token).AsTask(), token));
		}

		/// <inheritdoc />
		public TEntity Delete(TEntity entity)
		{
			ThrowIfDisposed();
			return DeleteInternal(entity);
		}

		protected virtual TEntity DeleteInternal([NotNull] TEntity entity)
		{
			if (Context.Entry(entity).State == EntityState.Detached) DbSet.Attach(entity);
			return DbSet.Remove(entity);
		}

		public ValueTask<TEntity> DeleteAsync(TEntity entity, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return DeleteAsyncInternal(entity, token);
		}

		protected virtual ValueTask<TEntity> DeleteAsyncInternal([NotNull] TEntity entity, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(DeleteInternal(entity));
		}
	}
}