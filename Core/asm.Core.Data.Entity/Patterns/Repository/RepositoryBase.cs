using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using asm.Data.Model;
using asm.Data.Patterns.Parameters;
using asm.Data.Patterns.Repository;
using asm.Extensions;
using asm.Data.Extensions;
using asm.Helpers;
using asm.Patterns.Pagination;
using asm.Patterns.Sorting;
using asm.Threading.Extensions;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace asm.Core.Data.Entity.Patterns.Repository
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

		protected override TEntity GetInternal(params object[] keys) { return PrepareGetQuery(keys).FirstOrDefault(); }

		/// <inheritdoc />
		protected override ValueTask<TEntity> GetAsyncInternal(object[] keys, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(PrepareGetQuery(keys).FirstOrDefaultAsync(token));
		}

		/// <inheritdoc />

		/// <inheritdoc />
		protected override TEntity GetInternal(IGetSettings settings) { return PrepareGetQuery(settings).FirstOrDefault(); }

		/// <inheritdoc />
		protected override ValueTask<TEntity> GetAsyncInternal(IGetSettings settings, CancellationToken token = default(CancellationToken))
		{
			token.ThrowIfCancellationRequested();
			return new ValueTask<TEntity>(PrepareGetQuery(settings).FirstOrDefaultAsync(token));
		}

		/// <inheritdoc />
		public IQueryable<TEntity> SqlQuery([NotNull] string sql, [NotNull] params object[] parameters)
		{
			ThrowIfDisposed();
			return SqlQueryInternal(sql, parameters);
		}

		protected virtual IQueryable<TEntity> SqlQueryInternal([NotNull] string sql, [NotNull] params object[] parameters)
		{
			return DbSet.FromSqlRaw(sql, parameters);
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
			
			if (settings is IFilterSettings filterSettings)
			{
				Expression<Func<TEntity, bool>> expression = BuildFilter(filterSettings);
				if (expression != null) query = query.Where(expression);
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

			StringBuilder sb = new StringBuilder();

			for (int i = 0; i < keys.Length; i++)
			{
				if (sb.Length > 0) sb.Append(" and ");
				PropertyInfo property = KeyProperties[i];
				sb.Append(property.Name);

				if (property.PropertyType.IsNumeric())
				{
					sb.Append(" == ");
					sb.Append(keys[i]);
				}
				else if (keys[i] == null)
				{
					sb.Append(" == null");
				}
				else
				{
					sb.Append(" == ");
					sb.Append(keys[i]);
				}
			}

			Expression<Func<TEntity, bool>> filter = BuildFilter(sb.ToString());
			if (filter != null) query = query.Where(filter);
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

			if (settings is IFilterSettings filterSettings)
			{
				Expression<Func<TEntity, bool>> expression = BuildFilter(filterSettings);
				if (expression != null) query = query.Where(expression);
			}

			return query;
		}
	}
}