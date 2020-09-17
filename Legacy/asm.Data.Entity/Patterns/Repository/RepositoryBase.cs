using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using asm.Data.Extensions;
using asm.Data.Model;
using asm.Data.Patterns.Parameters;
using asm.Data.Patterns.Repository;
using asm.Extensions;
using asm.Helpers;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using asm.Threading.Extensions;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemDbContext = System.Data.Entity.DbContext;

namespace asm.Data.Entity.Patterns.Repository
{
	public abstract class RepositoryBase<TContext, TEntity> : RepositoryBase<TEntity>, IRepositoryBase<TContext, TEntity>
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
		}

		/// <inheritdoc />
		public TContext Context => _context;

		/// <inheritdoc />
		public DbSet<TEntity> DbSet { get; }
		
		protected bool OwnsContext { get; }

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
			return new ValueTask<IList<TEntity>>(PrepareListQuery(settings).Paginate(settings).ToListAsync(token).As<List<TEntity>, IList<TEntity>>());
		}

		/// <inheritdoc />
		protected override TEntity GetInternal(IGetSettings settings)
		{
			return PrepareGetQuery(settings).FirstOrDefault();
		}

		/// <inheritdoc />
		protected override ValueTask<TEntity> GetAsyncInternal(IGetSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(PrepareGetQuery(settings).FirstOrDefaultAsync(token));
		}

		/// <inheritdoc />
		public DbSqlQuery<TEntity> SqlQuery(string sql, params object[] parameters)
		{
			ThrowIfDisposed();
			return SqlQueryInternal(sql, parameters);
		}

		protected virtual DbSqlQuery<TEntity> SqlQueryInternal(string sql, params object[] parameters)
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
			if (settings is IIncludeSettings includeSettings && includeSettings.Include?.Count > 0)
			{
				query = includeSettings.Include.SkipNullOrEmpty()
										.Aggregate(query, (current, path) => current.Include(path));
			}
			
			if (settings is IFilterSettings filterSettings && !string.IsNullOrWhiteSpace(filterSettings.FilterExpression))
			{
				query = query.Where(filterSettings.FilterExpression);
			}
			
			if (settings is ISortable sortable && sortable.OrderBy?.Count > 0)
			{
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
			}

			return query;
		}

		protected override IQueryable<TEntity> PrepareGetQuery(object[] keys)
		{
			IQueryable<TEntity> query = DbSet;
			if (keys.Length != KeyProperties.Length) throw new ArgumentException("Wrong number of key values.", nameof(keys));

			StringBuilder filter = new StringBuilder();

			for (int i = 0; i < keys.Length; i++)
			{
				if (filter.Length > 0) filter.Append(" and ");
				PropertyInfo property = KeyProperties[i];
				filter.Append($"{property.Name} == ");

				if (property.PropertyType.IsNumeric())
					filter.Append(keys[i]);
				else if (keys[i] == null)
					filter.Append("null");
				else
					filter.Append($"\"{keys[i]}\"");
			}

			query = query.Where(filter.ToString());
			return query;
		}

		protected override IQueryable<TEntity> PrepareGetQuery(IGetSettings settings)
		{
			IQueryable<TEntity> query = PrepareGetQuery(settings.KeyValue);
			
			if (settings is IIncludeSettings includeSettings && includeSettings.Include?.Count > 0)
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
}