using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class DbSetExtension
{
	[NotNull]
	public static DbContext GetContext<T>([NotNull] this DbSet<T> thisValue)
		where T : class
	{
		return thisValue.GetService<ICurrentDbContext>().Context;
	}

	public static string GetTableName<T>([NotNull] this DbSet<T> thisValue)
		where T : class
	{
		DbContext context = GetContext(thisValue);
		return context.GetTableName(thisValue.GetType());
	}
}