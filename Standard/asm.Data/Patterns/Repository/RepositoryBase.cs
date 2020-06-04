using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using asm.Data.Model;
using asm.Data.Patterns.Parameters;
using asm.Extensions;
using asm.Logging.Helpers;
using asm.Patterns.Object;
using asm.Patterns.Pagination;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace asm.Data.Patterns.Repository
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
		private PropertyInfo[] _keyProperties;

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
		protected virtual PropertyInfo[] KeyProperties => _keyProperties ??= GetKeyProperties();

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
		public ValueTask<IQueryable<TEntity>> ListAsync(IPagination settings = null, CancellationToken token = default(CancellationToken))
		{
			ThrowIfDisposed();
			token.ThrowIfCancellationRequested();
			return ListAsyncInternal(settings, token);
		}

		protected abstract ValueTask<IQueryable<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken));

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
		private static PropertyInfo[] GetKeyProperties()
		{
			Type entityType = typeof(TEntity);
			PropertyInfo[] properties = entityType.GetProperties(asm.Constants.BF_PUBLIC_INSTANCE);
			List<(PropertyInfo Property, int Order)> list = new List<(PropertyInfo, int)>();
			ISet<int> columnOrders = new HashSet<int>();

			foreach (PropertyInfo property in properties)
			{
				object[] attributes = property.GetCustomAttributes(true);
				if (attributes.Length == 0 || !attributes.Exists(e => e is KeyAttribute)) continue;

				ColumnAttribute column = attributes.OfType<ColumnAttribute>().FirstOrDefault();
				int order = column?.Order ?? list.Count;

				while (columnOrders.Contains(order))
					order++;

				columnOrders.Add(order);
				list.Add((property, order));
			}

			if (list.Count > 0) list.Sort((x, y) => x.Order.CompareTo(y.Order));
			return list.OrderBy(e => e.Order).Select(e => e.Property).ToArray();
		}
	}
}