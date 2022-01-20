using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using SystemDbContext = Microsoft.EntityFrameworkCore.DbContext;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class CoreDataEntityDbContextExtension
{
	public static IEntityType GetEntityType([NotNull] this SystemDbContext thisValue, [NotNull] Type type)
	{
		return thisValue.Model.FindEntityType(type);
	}

	public static string GetTableName([NotNull] this SystemDbContext thisValue, [NotNull] Type type)
	{
		return GetEntityType(thisValue, type)?.GetTableName();
	}

	public static string GetSchemaQualifiedTableName([NotNull] this SystemDbContext thisValue, [NotNull] Type type)
	{
		return GetEntityType(thisValue, type)?.GetSchemaQualifiedTableName();
	}

	public static IEnumerable<string> GetKeyNames([NotNull] this SystemDbContext thisValue, [NotNull] Type type)
	{
		return GetEntityType(thisValue, type)
				?.FindPrimaryKey()
				?.Properties
				.Select(e => e.Name);
	}

	public static void Reload<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity)
		where TEntity : class
	{
		thisValue.Entry(entity).Reload();
	}

	[NotNull]
	public static Task ReloadAsync<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity)
		where TEntity : class
	{
		return thisValue.Entry(entity).ReloadAsync();
	}

	[NotNull]
	public static Task ReloadAsync<TEntity>([NotNull] this SystemDbContext thisValue, [NotNull] TEntity entity, CancellationToken cancellationToken)
		where TEntity : class
	{
		return thisValue.Entry(entity).ReloadAsync(cancellationToken);
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