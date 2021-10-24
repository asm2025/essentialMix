using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Extensions;
using essentialMix.Logging.Helpers;
using essentialMix.Patterns.Object;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace essentialMix.Data.Patterns.Repository
{
	public abstract class RepositoryBase : Disposable, IRepositoryBase
	{
		/// <inheritdoc />
		protected RepositoryBase([NotNull] IConfiguration configuration)
			: this(configuration, null)
		{
		}

		/// <inheritdoc />
		protected RepositoryBase([NotNull] IConfiguration configuration, ILogger logger)
		{
			Configuration = configuration;
			Logger = logger ?? LogHelper.Empty;
		}

		[NotNull]
		protected IConfiguration Configuration { get; }

		[NotNull]
		protected ILogger Logger { get; }
	}

	public abstract class RepositoryBase<TEntity> : RepositoryBase, IRepositoryBase<TEntity>
		where TEntity : class, IEntity
	{
		/// <inheritdoc />
		protected RepositoryBase([NotNull] IConfiguration configuration) 
			: this(configuration, null)
		{
		}

		/// <inheritdoc />
		protected RepositoryBase([NotNull] IConfiguration configuration, ILogger logger)
			: base(configuration, logger)
		{
		}

		/// <inheritdoc />
		public Type EntityType { get; } = typeof(TEntity);

		[NotNull]
		protected abstract PropertyInfo[] KeyProperties { get; }

		/// <inheritdoc />
		[NotNull]
		public object[] GetKeyValue(TEntity entity)
		{
			ThrowIfDisposed();
			return GetKeyValueInternal(entity);
		}

		[NotNull]
		protected virtual object[] GetKeyValueInternal([NotNull] TEntity entity) { return KeyProperties.Select(e => e.GetValue(entity)).ToArray(); }

		/// <inheritdoc />
		public TEntity Create()
		{
			ThrowIfDisposed();
			return CreateInternal();
		}

		protected virtual TEntity CreateInternal()
		{
			return EntityType.CreateInstance<TEntity>();
		}

		/// <inheritdoc />
		public ValueTask<TEntity> CreateAsync(CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return CreateAsyncInternal(token);
		}

		protected virtual ValueTask<TEntity> CreateAsyncInternal(CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(CreateInternal());
		}

		/// <inheritdoc />
		public T Create<T>()
			where T : TEntity
		{
			ThrowIfDisposed();
			return CreateInternal<T>();
		}

		protected virtual T CreateInternal<T>()
			where T : TEntity
		{
			return typeof(T).CreateInstance<T>();
		}

		/// <inheritdoc />
		public ValueTask<T> CreateAsync<T>(CancellationToken token = default(CancellationToken))
			where T : TEntity
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return CreateAsyncInternal<T>(token);
		}

		protected virtual ValueTask<T> CreateAsyncInternal<T>(CancellationToken token = default(CancellationToken))
			where T : TEntity
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<T>(CreateInternal<T>());
		}

		/// <inheritdoc />
		public IQueryable<TEntity> List(IPagination settings = null)
		{
			ThrowIfDisposed();
			return ListInternal(settings);
		}

		protected abstract IQueryable<TEntity> ListInternal(IPagination settings = null);

		/// <inheritdoc />
		public ValueTask<IList<TEntity>> ListAsync(IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return ListAsyncInternal(settings, token);
		}

		protected abstract ValueTask<IList<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public TEntity Get(params object[] keys)
		{
			ThrowIfDisposed();
			return GetInternal(keys);
		}

		protected abstract TEntity GetInternal([NotNull] params object[] keys);

		/// <inheritdoc />
		public ValueTask<TEntity> GetAsync(params object[] keys)
		{
			ThrowIfDisposed();
			return GetAsyncInternal(keys, CancellationToken.None);
		}

		/// <inheritdoc />
		public ValueTask<TEntity> GetAsync(CancellationToken token, params object[] keys)
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return GetAsyncInternal(keys, token);
		}

		/// <inheritdoc />
		public ValueTask<TEntity> GetAsync(object[] keys, CancellationToken token)
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return GetAsyncInternal(keys, token);
		}

		protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] object[] keys, CancellationToken token = default(CancellationToken));

		/// <inheritdoc />
		public TEntity Get(IGetSettings settings)
		{
			ThrowIfDisposed();
			return GetInternal(settings);
		}

		protected abstract TEntity GetInternal([NotNull] IGetSettings settings);

		/// <inheritdoc />
		public ValueTask<TEntity> GetAsync(IGetSettings settings, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return GetAsyncInternal(settings, token);
		}

		protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));

		[NotNull]
		protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] object[] keys);

		[NotNull]
		protected abstract IQueryable<TEntity> PrepareListQuery([NotNull] IQueryable<TEntity> query, IPagination settings);

		[NotNull]
		protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] IGetSettings settings);
	}
}