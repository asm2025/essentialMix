using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using asm.Extensions;
using SystemDbContext = System.Data.Entity.DbContext;

namespace asm.Data.Entity.Extensions
{
	public static class DbContextExtension
	{
		public static void Reload<TEntity>([NotNull] this SystemDbContext thisValue, TEntity entity)
			where TEntity : class
		{
			thisValue.Entry(entity).Reload();
		}

		public static async Task ReloadAsync<TEntity>([NotNull] this SystemDbContext thisValue, TEntity entity)
			where TEntity : class
		{
			await thisValue.Entry(entity).ReloadAsync().ConfigureAwait();
		}

		public static async Task ReloadAsync<TEntity>([NotNull] this SystemDbContext thisValue, TEntity entity, CancellationToken cancellationToken)
			where TEntity : class
		{
			await thisValue.Entry(entity).ReloadAsync(cancellationToken).ConfigureAwait();
		}

		public static void ReloadNavigationProperty<TEntity, TElement>([NotNull] this SystemDbContext thisValue, TEntity entity, Expression<Func<TEntity, ICollection<TElement>>> navigationProperty)
			where TEntity : class
			where TElement : class
		{
			thisValue.Entry(entity).Collection(navigationProperty).Query();
		}

		public static void ForceRefresh<TEntity>([NotNull] this SystemDbContext thisValue, TEntity entity)
			where TEntity : class
		{
			thisValue.Entry(entity).State = EntityState.Detached;
		}

		public static void ForceUpdate<TEntity>([NotNull] this SystemDbContext thisValue, TEntity entity)
			where TEntity : class
		{
			thisValue.Entry(entity).State = EntityState.Modified;
		}
	}
}