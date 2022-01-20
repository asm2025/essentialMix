using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace essentialMix.Data.Patterns.Repository;

public abstract class Repository<TEntity, TKey> : RepositoryBase<TEntity, TKey>, IRepository<TEntity, TKey>
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
	public void Delete(TKey key)
	{
		ThrowIfDisposed();
		DeleteInternal(key);
	}

	protected abstract void DeleteInternal([NotNull] TKey key);

	/// <inheritdoc />
	public ValueTask DeleteAsync(TKey key, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return DeleteAsyncInternal(key, token);
	}

	protected abstract ValueTask DeleteAsyncInternal([NotNull] TKey key, CancellationToken token = default(CancellationToken));

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

public abstract class Repository<TEntity, TKey1, TKey2> : RepositoryBase<TEntity, TKey1, TKey2>, IRepository<TEntity, TKey1, TKey2>
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
	public void Delete(TKey1 key1, TKey2 key2)
	{
		ThrowIfDisposed();
		DeleteInternal(key1, key2);
	}

	protected abstract void DeleteInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2);

	/// <inheritdoc />
	public ValueTask DeleteAsync(TKey1 key1, TKey2 key2, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return DeleteAsyncInternal(key1, key2, token);
	}

	protected abstract ValueTask DeleteAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, CancellationToken token = default(CancellationToken));

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

public abstract class Repository<TEntity, TKey1, TKey2, TKey3> : RepositoryBase<TEntity, TKey1, TKey2, TKey3>, IRepository<TEntity, TKey1, TKey2, TKey3>
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
	public void Delete(TKey1 key1, TKey2 key2, TKey3 key3)
	{
		ThrowIfDisposed();
		DeleteInternal(key1, key2, key3);
	}

	protected abstract void DeleteInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3);

	/// <inheritdoc />
	public ValueTask DeleteAsync(TKey1 key1, TKey2 key2, TKey3 key3, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return DeleteAsyncInternal(key1, key2, key3, token);
	}

	protected abstract ValueTask DeleteAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, CancellationToken token = default(CancellationToken));

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

public abstract class Repository<TEntity, TKey1, TKey2, TKey3, TKey4> : RepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4>, IRepository<TEntity, TKey1, TKey2, TKey3, TKey4>
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
	public void Delete(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
	{
		ThrowIfDisposed();
		DeleteInternal(key1, key2, key3, key4);
	}

	protected abstract void DeleteInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4);

	/// <inheritdoc />
	public ValueTask DeleteAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return DeleteAsyncInternal(key1, key2, key3, key4, token);
	}

	protected abstract ValueTask DeleteAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, CancellationToken token = default(CancellationToken));

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

public abstract class Repository<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : RepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>, IRepository<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>
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
	public void Delete(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
	{
		ThrowIfDisposed();
		DeleteInternal(key1, key2, key3, key4, key5);
	}

	protected abstract void DeleteInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5);

	/// <inheritdoc />
	public ValueTask DeleteAsync(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, CancellationToken token = default(CancellationToken))
	{
		ThrowIfDisposed();
		token.ThrowIfCancellationRequested();
		return DeleteAsyncInternal(key1, key2, key3, key4, key5, token);
	}

	protected abstract ValueTask DeleteAsyncInternal([NotNull] TKey1 key1, [NotNull] TKey2 key2, [NotNull] TKey3 key3, [NotNull] TKey4 key4, [NotNull] TKey5 key5, CancellationToken token = default(CancellationToken));

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
