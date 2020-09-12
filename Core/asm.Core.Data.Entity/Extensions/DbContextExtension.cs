using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using asm.Extensions;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

namespace asm.Core.Data.Entity.Extensions
{
	public static class DbContextExtension
	{
		public static void Reload<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity)
			where TEntity : class
		{
			thisValue.Entry(entity).Reload();
		}

		public static async Task ReloadAsync<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity)
			where TEntity : class
		{
			await thisValue.Entry(entity).ReloadAsync().ConfigureAwait();
		}

		public static async Task ReloadAsync<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity, CancellationToken cancellationToken)
			where TEntity : class
		{
			await thisValue.Entry(entity).ReloadAsync(cancellationToken).ConfigureAwait();
		}

		public static void ReloadNavigationProperty<TEntity, TElement>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity, [NotNull] Expression<Func<TEntity, IEnumerable<TElement>>> navigationProperty)
			where TEntity : class
			where TElement : class
		{
			thisValue.Entry(entity).Collection(navigationProperty).Query();
		}

		public static void ForceRefresh<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity)
			where TEntity : class
		{
			thisValue.Entry(entity).State = EntityState.Detached;
		}

		public static void ForceUpdate<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity)
			where TEntity : class
		{
			thisValue.Entry(entity).State = EntityState.Modified;
		}
	}
}