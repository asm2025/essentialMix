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

namespace essentialMix.Data.Patterns.Repository;

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
	public IQueryable<TEntity> Count(IPagination settings = null)
	{
		ThrowIfDisposed();
		return CountInternal(settings);
	}

	protected abstract IQueryable<TEntity> CountInternal(IPagination settings = null);

	/// <inheritdoc />
	public ValueTask<T> CountAsync<T>(IPagination settings = null, CancellationToken token = default(CancellationToken))
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return CountAsyncInternal<T>(settings, token);
	}

	protected abstract ValueTask<T> CountAsyncInternal<T>(IPagination settings = null, CancellationToken token = default(CancellationToken))
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable;

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

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareCountQuery([NotNull] IQueryable<TEntity> query, IPagination settings);

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareListQuery([NotNull] IQueryable<TEntity> query, IPagination settings);
}

public abstract class RepositoryBase<TEntity, TKey> : RepositoryBase<TEntity>, IRepositoryBase<TEntity, TKey>
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

	[NotNull]
	protected abstract PropertyInfo KeyProperty { get; }

	/// <inheritdoc />
	[NotNull]
	public TKey GetKeyValue(TEntity entity)
	{
		ThrowIfDisposed();
		return GetKeyValueInternal(entity);
	}

	[NotNull]
	protected virtual TKey GetKeyValueInternal([NotNull] TEntity entity) { return (TKey)KeyProperty.GetValue(entity); }

	/// <inheritdoc />
	public TEntity Get(TKey key)
	{
		ThrowIfDisposed();
		return GetInternal(key);
	}

	protected abstract TEntity GetInternal([NotNull] TKey key);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey key, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey key, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	public TEntity Get(TKey key, IGetSettings settings)
	{
		ThrowIfDisposed();
		return GetInternal(key, settings);
	}

	protected abstract TEntity GetInternal([NotNull] TKey key, [NotNull] IGetSettings settings);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey key, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key, settings, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey key, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey key);

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey key, [NotNull] IGetSettings settings);
}

public abstract class RepositoryBase<TEntity, TKey1, TKey2> : RepositoryBase<TEntity>, IRepositoryBase<TEntity, TKey1, TKey2>
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

	[NotNull]
	protected abstract PropertyInfo Key1Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key2Property { get; }

	/// <inheritdoc />
	public (TKey1, TKey2) GetKeyValue(TEntity entity)
	{
		ThrowIfDisposed();
		return GetKeyValueInternal(entity);
	}

	protected virtual (TKey1, TKey2) GetKeyValueInternal([NotNull] TEntity entity)
	{
		return ((TKey1)Key1Property.GetValue(entity),
				(TKey2)Key2Property.GetValue(entity));
	}

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, IGetSettings settings)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, settings);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, settings, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2);

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] IGetSettings settings);
}

public abstract class RepositoryBase<TEntity, TKey1, TKey2, TKey3> : RepositoryBase<TEntity>, IRepositoryBase<TEntity, TKey1, TKey2, TKey3>
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

	[NotNull]
	protected abstract PropertyInfo Key1Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key2Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key3Property { get; }

	/// <inheritdoc />
	public (TKey1, TKey2, TKey3) GetKeyValue(TEntity entity)
	{
		ThrowIfDisposed();
		return GetKeyValueInternal(entity);
	}

	protected virtual (TKey1, TKey2, TKey3) GetKeyValueInternal([NotNull] TEntity entity)
	{
		return ((TKey1)Key1Property.GetValue(entity),
				(TKey2)Key2Property.GetValue(entity),
				(TKey3)Key3Property.GetValue(entity));
	}

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, TKey3 key3)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, key3);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, TKey3 key3, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, key3, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, TKey3 key3, IGetSettings settings)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, key3, settings);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, TKey3 key3, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, key3, settings, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] IGetSettings settings);
}

public abstract class RepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4> : RepositoryBase<TEntity>, IRepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4>
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

	[NotNull]
	protected abstract PropertyInfo Key1Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key2Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key3Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key4Property { get; }

	/// <inheritdoc />
	public (TKey1, TKey2, TKey3, TKey4) GetKeyValue(TEntity entity)
	{
		ThrowIfDisposed();
		return GetKeyValueInternal(entity);
	}

	protected virtual (TKey1, TKey2, TKey3, TKey4) GetKeyValueInternal([NotNull] TEntity entity)
	{
		return ((TKey1)Key1Property.GetValue(entity),
				(TKey2)Key2Property.GetValue(entity),
				(TKey3)Key3Property.GetValue(entity),
				(TKey4)Key3Property.GetValue(entity));
	}

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, key3, key4);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, key3, key4, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, IGetSettings settings)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, key3, key4, settings);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, key3, key4, settings, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] IGetSettings settings);
}

public abstract class RepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : RepositoryBase<TEntity>, IRepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>
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

	[NotNull]
	protected abstract PropertyInfo Key1Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key2Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key3Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key4Property { get; }

	[NotNull]
	protected abstract PropertyInfo Key5Property { get; }

	/// <inheritdoc />
	public (TKey1, TKey2, TKey3, TKey4, TKey5) GetKeyValue(TEntity entity)
	{
		ThrowIfDisposed();
		return GetKeyValueInternal(entity);
	}

	protected virtual (TKey1, TKey2, TKey3, TKey4, TKey5) GetKeyValueInternal([NotNull] TEntity entity)
	{
		return ((TKey1)Key1Property.GetValue(entity),
				(TKey2)Key2Property.GetValue(entity),
				(TKey3)Key3Property.GetValue(entity),
				(TKey4)Key3Property.GetValue(entity),
				(TKey5)Key3Property.GetValue(entity));
	}

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, key3, key4, key5);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, key3, key4, key5, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));

	/// <inheritdoc />
	public TEntity Get(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, IGetSettings settings)
	{
		ThrowIfDisposed();
		return GetInternal(key1, key2, key3, key4, key5, settings);
	}

	protected abstract TEntity GetInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);

	/// <inheritdoc />
	public ValueTask<TEntity> GetAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return GetAsyncInternal(key1, key2, key3, key4, key5, settings, token);
	}

	protected abstract ValueTask<TEntity> GetAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings, CancellationToken token = default(CancellationToken));

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);

	[NotNull]
	protected abstract IQueryable<TEntity> PrepareGetQuery([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, [NotNull] IGetSettings settings);
}
