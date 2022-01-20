using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using essentialMix.Data.Model;
using essentialMix.Data.Patterns.Parameters;
using essentialMix.Data.Patterns.Repository;
using essentialMix.Extensions;
using essentialMix.Helpers;
using essentialMix.Patterns.Pagination;
using essentialMix.Patterns.Sorting;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemDbContext = System.Data.Entity.DbContext;

namespace essentialMix.Data.Entity.Patterns.Repository;

public abstract class RepositoryBase<TContext, TEntity, TKey> : RepositoryBase<TEntity, TKey>, IRepositoryBase<TContext, TEntity, TKey>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	private TContext _context;

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration)
		: this(context, configuration, null, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger)
		: this(context, configuration, logger, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger, bool ownsContext)
		: base(configuration, logger)
	{
		_context = context;
		DbSet = _context.Set<TEntity>();
		OwnsContext = ownsContext;

		Type type = typeof(TEntity);
		string keyName = _context.GetKeyNames(type).Single();
		if (string.IsNullOrEmpty(keyName)) throw new MissingPrimaryKeyException();
		KeyProperty = type.GetProperty(keyName) ?? throw new MissingPrimaryKeyException();
	}

	/// <inheritdoc />
	public TContext Context => _context;

	/// <inheritdoc />
	public DbSet<TEntity> DbSet { get; }

	protected bool OwnsContext { get; }

	/// <inheritdoc />
	protected override PropertyInfo KeyProperty { get; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing && OwnsContext) ObjectHelper.Dispose(ref _context);
		base.Dispose(disposing);
	}

	[NotNull]
	protected override IQueryable<TEntity> ListInternal(IPagination settings = null) { return PrepareListQuery(settings); }

	protected override ValueTask<IList<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= new Pagination();
		return new ValueTask<IList<TEntity>>(PrepareListQuery(settings).Paginate(settings).ToListAsync(token).As<List<TEntity>, IList<TEntity>>(token));
	}

	protected override TEntity GetInternal(TKey key) { return PrepareGetQuery(key).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey key, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	protected override TEntity GetInternal(TKey key, IGetSettings settings) { return PrepareGetQuery(key, settings).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey key, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key, settings).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	[NotNull]
	public DbSqlQuery<TEntity> SqlQuery([NotNull] string sql, [NotNull] params object[] parameters)
	{
		ThrowIfDisposed();
		return SqlQueryInternal(sql, parameters);
	}

	[NotNull]
	protected virtual DbSqlQuery<TEntity> SqlQueryInternal([NotNull] string sql, [NotNull] params object[] parameters)
	{
		return DbSet.SqlQuery(sql, parameters);
	}

	[NotNull]
	protected IQueryable<TEntity> PrepareListQuery(IPagination settings)
	{
		return PrepareListQuery(DbSet, settings);
	}

	protected override IQueryable<TEntity> PrepareListQuery(IQueryable<TEntity> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		if (settings is not ISortable { OrderBy.Count: > 0 } sortable) return query;

		bool addedFirst = false;

		foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
		{
			if (!addedFirst)
			{
				query = query.OrderBy(field.Name, field.Type);
				addedFirst = true;
				continue;
			}

			query = query.ThenBy(field.Name, field.Type);
		}

		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey key)
	{
		IQueryable<TEntity> query = DbSet;
		StringBuilder filter = new StringBuilder();
		filter.Append($"({KeyProperty.Name} == ");

		if (KeyProperty.PropertyType.IsNumeric())
			filter.Append(key);
		else
			filter.Append($"\"{key}\"");

		filter.Append(")");
		query = query.Where(filter.ToString());
		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey key, IGetSettings settings)
	{
		IQueryable<TEntity> query = PrepareGetQuery(key);

		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		return query;
	}
}

public abstract class RepositoryBase<TContext, TEntity, TKey1, TKey2> : essentialMix.Data.Patterns.Repository.RepositoryBase<TEntity, TKey1, TKey2>, IRepositoryBase<TContext, TEntity, TKey1, TKey2>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	private TContext _context;

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration)
		: this(context, configuration, null, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger)
		: this(context, configuration, logger, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger, bool ownsContext)
		: base(configuration, logger)
	{
		_context = context;
		DbSet = _context.Set<TEntity>();
		OwnsContext = ownsContext;

		Type type = typeof(TEntity);
		string[] keyNames = _context.GetKeyNames(type).ToArray();
		if (keyNames is not { Length: 2 }) throw new MissingPrimaryKeyException();
		Key1Property = type.GetProperty(keyNames[0]) ?? throw new MissingPrimaryKeyException();
		Key2Property = type.GetProperty(keyNames[1]) ?? throw new MissingPrimaryKeyException();
	}

	/// <inheritdoc />
	public TContext Context => _context;

	/// <inheritdoc />
	public DbSet<TEntity> DbSet { get; }

	protected bool OwnsContext { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key1Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key2Property { get; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing && OwnsContext) ObjectHelper.Dispose(ref _context);
		base.Dispose(disposing);
	}

	[NotNull]
	protected override IQueryable<TEntity> ListInternal(IPagination settings = null) { return PrepareListQuery(settings); }

	protected override ValueTask<IList<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= new Pagination();
		return new ValueTask<IList<TEntity>>(PrepareListQuery(settings).Paginate(settings).ToListAsync(token).As<List<TEntity>, IList<TEntity>>(token));
	}

	protected override TEntity GetInternal(TKey1 key1, TKey2 key2) { return PrepareGetQuery(key1, key2).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, IGetSettings settings) { return PrepareGetQuery(key1, key2, settings).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, settings).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	[NotNull]
	public DbSqlQuery<TEntity> SqlQuery([NotNull] string sql, [NotNull] params object[] parameters)
	{
		ThrowIfDisposed();
		return SqlQueryInternal(sql, parameters);
	}

	[NotNull]
	protected virtual DbSqlQuery<TEntity> SqlQueryInternal([NotNull] string sql, [NotNull] params object[] parameters)
	{
		return DbSet.SqlQuery(sql, parameters);
	}

	[NotNull]
	protected IQueryable<TEntity> PrepareListQuery(IPagination settings)
	{
		return PrepareListQuery(DbSet, settings);
	}

	protected override IQueryable<TEntity> PrepareListQuery(IQueryable<TEntity> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		if (settings is not ISortable { OrderBy.Count: > 0 } sortable) return query;

		bool addedFirst = false;

		foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
		{
			if (!addedFirst)
			{
				query = query.OrderBy(field.Name, field.Type);
				addedFirst = true;
				continue;
			}

			query = query.ThenBy(field.Name, field.Type);
		}

		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2)
	{
		IQueryable<TEntity> query = DbSet;
		StringBuilder filter = new StringBuilder();
		filter.Append($"({Key1Property.Name} == ");

		if (Key1Property.PropertyType.IsNumeric())
			filter.Append(key1);
		else
			filter.Append($"\"{key1}\"");

		filter.Append($" AND {Key2Property.Name} == ");

		if (Key2Property.PropertyType.IsNumeric())
			filter.Append(key2);
		else
			filter.Append($"\"{key2}\"");

		filter.Append(")");
		query = query.Where(filter.ToString());
		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, IGetSettings settings)
	{
		IQueryable<TEntity> query = PrepareGetQuery(key1, key2);

		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		return query;
	}
}

public abstract class RepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3> : essentialMix.Data.Patterns.Repository.RepositoryBase<TEntity, TKey1, TKey2, TKey3>, IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	private TContext _context;

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration)
		: this(context, configuration, null, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger)
		: this(context, configuration, logger, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger, bool ownsContext)
		: base(configuration, logger)
	{
		_context = context;
		DbSet = _context.Set<TEntity>();
		OwnsContext = ownsContext;

		Type type = typeof(TEntity);
		string[] keyNames = _context.GetKeyNames(type).ToArray();
		if (keyNames is not { Length: 3 }) throw new MissingPrimaryKeyException();
		Key1Property = type.GetProperty(keyNames[0]) ?? throw new MissingPrimaryKeyException();
		Key2Property = type.GetProperty(keyNames[1]) ?? throw new MissingPrimaryKeyException();
		Key3Property = type.GetProperty(keyNames[2]) ?? throw new MissingPrimaryKeyException();
	}

	/// <inheritdoc />
	public TContext Context => _context;

	/// <inheritdoc />
	public DbSet<TEntity> DbSet { get; }

	protected bool OwnsContext { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key1Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key2Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key3Property { get; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing && OwnsContext) ObjectHelper.Dispose(ref _context);
		base.Dispose(disposing);
	}

	[NotNull]
	protected override IQueryable<TEntity> ListInternal(IPagination settings = null) { return PrepareListQuery(settings); }

	protected override ValueTask<IList<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= new Pagination();
		return new ValueTask<IList<TEntity>>(PrepareListQuery(settings).Paginate(settings).ToListAsync(token).As<List<TEntity>, IList<TEntity>>(token));
	}

	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, TKey3 key3) { return PrepareGetQuery(key1, key2, key3).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, TKey3 key3, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, key3).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, TKey3 key3, IGetSettings settings) { return PrepareGetQuery(key1, key2, key3, settings).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, TKey3 key3, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, key3, settings).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	[NotNull]
	public DbSqlQuery<TEntity> SqlQuery([NotNull] string sql, [NotNull] params object[] parameters)
	{
		ThrowIfDisposed();
		return SqlQueryInternal(sql, parameters);
	}

	[NotNull]
	protected virtual DbSqlQuery<TEntity> SqlQueryInternal([NotNull] string sql, [NotNull] params object[] parameters)
	{
		return DbSet.SqlQuery(sql, parameters);
	}

	[NotNull]
	protected IQueryable<TEntity> PrepareListQuery(IPagination settings)
	{
		return PrepareListQuery(DbSet, settings);
	}

	protected override IQueryable<TEntity> PrepareListQuery(IQueryable<TEntity> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		if (settings is not ISortable { OrderBy.Count: > 0 } sortable) return query;

		bool addedFirst = false;

		foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
		{
			if (!addedFirst)
			{
				query = query.OrderBy(field.Name, field.Type);
				addedFirst = true;
				continue;
			}

			query = query.ThenBy(field.Name, field.Type);
		}

		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, TKey3 key3)
	{
		IQueryable<TEntity> query = DbSet;
		StringBuilder filter = new StringBuilder();
		filter.Append($"({Key1Property.Name} == ");

		if (Key1Property.PropertyType.IsNumeric())
			filter.Append(key1);
		else
			filter.Append($"\"{key1}\"");

		filter.Append($" AND {Key2Property.Name} == ");

		if (Key2Property.PropertyType.IsNumeric())
			filter.Append(key2);
		else
			filter.Append($"\"{key2}\"");

		filter.Append($" AND {Key3Property.Name} == ");

		if (Key3Property.PropertyType.IsNumeric())
			filter.Append(key3);
		else
			filter.Append($"\"{key3}\"");

		filter.Append(")");
		query = query.Where(filter.ToString());
		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, TKey3 key3, IGetSettings settings)
	{
		IQueryable<TEntity> query = PrepareGetQuery(key1, key2, key3);

		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		return query;
	}
}

public abstract class RepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4> : essentialMix.Data.Patterns.Repository.RepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4>, IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	private TContext _context;

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration)
		: this(context, configuration, null, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger)
		: this(context, configuration, logger, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger, bool ownsContext)
		: base(configuration, logger)
	{
		_context = context;
		DbSet = _context.Set<TEntity>();
		OwnsContext = ownsContext;

		Type type = typeof(TEntity);
		string[] keyNames = _context.GetKeyNames(type).ToArray();
		if (keyNames is not { Length: 4 }) throw new MissingPrimaryKeyException();
		Key1Property = type.GetProperty(keyNames[0]) ?? throw new MissingPrimaryKeyException();
		Key2Property = type.GetProperty(keyNames[1]) ?? throw new MissingPrimaryKeyException();
		Key3Property = type.GetProperty(keyNames[2]) ?? throw new MissingPrimaryKeyException();
		Key4Property = type.GetProperty(keyNames[3]) ?? throw new MissingPrimaryKeyException();
	}

	/// <inheritdoc />
	public TContext Context => _context;

	/// <inheritdoc />
	public DbSet<TEntity> DbSet { get; }

	protected bool OwnsContext { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key1Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key2Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key3Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key4Property { get; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing && OwnsContext) ObjectHelper.Dispose(ref _context);
		base.Dispose(disposing);
	}

	[NotNull]
	protected override IQueryable<TEntity> ListInternal(IPagination settings = null) { return PrepareListQuery(settings); }

	protected override ValueTask<IList<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= new Pagination();
		return new ValueTask<IList<TEntity>>(PrepareListQuery(settings).Paginate(settings).ToListAsync(token).As<List<TEntity>, IList<TEntity>>(token));
	}

	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4) { return PrepareGetQuery(key1, key2, key3, key4).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, key3, key4).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, IGetSettings settings) { return PrepareGetQuery(key1, key2, key3, key4, settings).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, key3, key4, settings).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	[NotNull]
	public DbSqlQuery<TEntity> SqlQuery([NotNull] string sql, [NotNull] params object[] parameters)
	{
		ThrowIfDisposed();
		return SqlQueryInternal(sql, parameters);
	}

	[NotNull]
	protected virtual DbSqlQuery<TEntity> SqlQueryInternal([NotNull] string sql, [NotNull] params object[] parameters)
	{
		return DbSet.SqlQuery(sql, parameters);
	}

	[NotNull]
	protected IQueryable<TEntity> PrepareListQuery(IPagination settings)
	{
		return PrepareListQuery(DbSet, settings);
	}

	protected override IQueryable<TEntity> PrepareListQuery(IQueryable<TEntity> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		if (settings is not ISortable { OrderBy.Count: > 0 } sortable) return query;

		bool addedFirst = false;

		foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
		{
			if (!addedFirst)
			{
				query = query.OrderBy(field.Name, field.Type);
				addedFirst = true;
				continue;
			}

			query = query.ThenBy(field.Name, field.Type);
		}

		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4)
	{
		IQueryable<TEntity> query = DbSet;
		StringBuilder filter = new StringBuilder();
		filter.Append($"({Key1Property.Name} == ");

		if (Key1Property.PropertyType.IsNumeric())
			filter.Append(key1);
		else
			filter.Append($"\"{key1}\"");

		filter.Append($" AND {Key2Property.Name} == ");

		if (Key2Property.PropertyType.IsNumeric())
			filter.Append(key2);
		else
			filter.Append($"\"{key2}\"");

		filter.Append($" AND {Key3Property.Name} == ");

		if (Key3Property.PropertyType.IsNumeric())
			filter.Append(key3);
		else
			filter.Append($"\"{key3}\"");

		filter.Append($" AND {Key4Property.Name} == ");

		if (Key4Property.PropertyType.IsNumeric())
			filter.Append(key4);
		else
			filter.Append($"\"{key4}\"");

		filter.Append(")");
		query = query.Where(filter.ToString());
		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, IGetSettings settings)
	{
		IQueryable<TEntity> query = PrepareGetQuery(key1, key2, key3, key4);

		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		return query;
	}
}

public abstract class RepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4, TKey5> : essentialMix.Data.Patterns.Repository.RepositoryBase<TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>, IRepositoryBase<TContext, TEntity, TKey1, TKey2, TKey3, TKey4, TKey5>
	where TContext : SystemDbContext
	where TEntity : class, IEntity
{
	private TContext _context;

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration)
		: this(context, configuration, null, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger)
		: this(context, configuration, logger, false)
	{
	}

	/// <inheritdoc />
	protected RepositoryBase([NotNull] TContext context, [NotNull] IConfiguration configuration, ILogger logger, bool ownsContext)
		: base(configuration, logger)
	{
		_context = context;
		DbSet = _context.Set<TEntity>();
		OwnsContext = ownsContext;

		Type type = typeof(TEntity);
		string[] keyNames = _context.GetKeyNames(type).ToArray();
		if (keyNames is not { Length: 5 }) throw new MissingPrimaryKeyException();
		Key1Property = type.GetProperty(keyNames[0]) ?? throw new MissingPrimaryKeyException();
		Key2Property = type.GetProperty(keyNames[1]) ?? throw new MissingPrimaryKeyException();
		Key3Property = type.GetProperty(keyNames[2]) ?? throw new MissingPrimaryKeyException();
		Key4Property = type.GetProperty(keyNames[3]) ?? throw new MissingPrimaryKeyException();
		Key5Property = type.GetProperty(keyNames[4]) ?? throw new MissingPrimaryKeyException();
	}

	/// <inheritdoc />
	public TContext Context => _context;

	/// <inheritdoc />
	public DbSet<TEntity> DbSet { get; }

	protected bool OwnsContext { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key1Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key2Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key3Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key4Property { get; }

	/// <inheritdoc />
	protected override PropertyInfo Key5Property { get; }

	/// <inheritdoc />
	protected override void Dispose(bool disposing)
	{
		if (disposing && OwnsContext) ObjectHelper.Dispose(ref _context);
		base.Dispose(disposing);
	}

	[NotNull]
	protected override IQueryable<TEntity> ListInternal(IPagination settings = null) { return PrepareListQuery(settings); }

	protected override ValueTask<IList<TEntity>> ListAsyncInternal(IPagination settings = null, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		settings ??= new Pagination();
		return new ValueTask<IList<TEntity>>(PrepareListQuery(settings).Paginate(settings).ToListAsync(token).As<List<TEntity>, IList<TEntity>>(token));
	}

	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5) { return PrepareGetQuery(key1, key2, key3, key4, key5).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, key3, key4, key5).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	protected override TEntity GetInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, IGetSettings settings) { return PrepareGetQuery(key1, key2, key3, key4, key5, settings).FirstOrDefault(); }

	/// <inheritdoc />
	protected override ValueTask<TEntity> GetAsyncInternal(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, IGetSettings settings, CancellationToken token = default(CancellationToken))
	{
		token.ThrowIfCancellationRequested();
		return new ValueTask<TEntity>(PrepareGetQuery(key1, key2, key3, key4, key5, settings).FirstOrDefaultAsync(token));
	}

	/// <inheritdoc />
	[NotNull]
	public DbSqlQuery<TEntity> SqlQuery([NotNull] string sql, [NotNull] params object[] parameters)
	{
		ThrowIfDisposed();
		return SqlQueryInternal(sql, parameters);
	}

	[NotNull]
	protected virtual DbSqlQuery<TEntity> SqlQueryInternal([NotNull] string sql, [NotNull] params object[] parameters)
	{
		return DbSet.SqlQuery(sql, parameters);
	}

	[NotNull]
	protected IQueryable<TEntity> PrepareListQuery(IPagination settings)
	{
		return PrepareListQuery(DbSet, settings);
	}

	protected override IQueryable<TEntity> PrepareListQuery(IQueryable<TEntity> query, IPagination settings)
	{
		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		if (settings is not ISortable { OrderBy.Count: > 0 } sortable) return query;

		bool addedFirst = false;

		foreach (SortField field in sortable.OrderBy.Where(e => e.Type != SortType.None))
		{
			if (!addedFirst)
			{
				query = query.OrderBy(field.Name, field.Type);
				addedFirst = true;
				continue;
			}

			query = query.ThenBy(field.Name, field.Type);
		}

		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5)
	{
		IQueryable<TEntity> query = DbSet;
		StringBuilder filter = new StringBuilder();
		filter.Append($"({Key1Property.Name} == ");

		if (Key1Property.PropertyType.IsNumeric())
			filter.Append(key1);
		else
			filter.Append($"\"{key1}\"");

		filter.Append($" AND {Key2Property.Name} == ");

		if (Key2Property.PropertyType.IsNumeric())
			filter.Append(key2);
		else
			filter.Append($"\"{key2}\"");

		filter.Append($" AND {Key3Property.Name} == ");

		if (Key3Property.PropertyType.IsNumeric())
			filter.Append(key3);
		else
			filter.Append($"\"{key3}\"");

		filter.Append($" AND {Key4Property.Name} == ");

		if (Key4Property.PropertyType.IsNumeric())
			filter.Append(key4);
		else
			filter.Append($"\"{key4}\"");

		filter.Append($" AND {Key5Property.Name} == ");

		if (Key5Property.PropertyType.IsNumeric())
			filter.Append(key5);
		else
			filter.Append($"\"{key5}\"");

		filter.Append(")");
		query = query.Where(filter.ToString());
		return query;
	}

	protected override IQueryable<TEntity> PrepareGetQuery(TKey1 key1, TKey2 key2, TKey3 key3, TKey4 key4, TKey5 key5, IGetSettings settings)
	{
		IQueryable<TEntity> query = PrepareGetQuery(key1, key2, key3, key4, key5);

		if (settings is IIncludeSettings { Include.Count: > 0 } includeSettings)
		{
			query = includeSettings.Include.SkipNullOrEmpty()
									.Aggregate(query, (current, path) => current.Include(path));
		}

		if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
		{
			query = query.Where(filterSettings.FilterExpression);
		}

		return query;
	}
}
