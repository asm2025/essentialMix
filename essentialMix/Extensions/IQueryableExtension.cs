using System;
using System.Linq;
using System.Linq.Expressions;
using essentialMix.Linq;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class IQueryableExtension
{
	[NotNull]
	public static IQueryable<T> InterceptWith<T>([NotNull] this IQueryable<T> thisValue, [NotNull] params ExpressionVisitor[] visitors)
	{
		return new QueryTranslator<T>(thisValue, visitors);
	}

	[NotNull]
	public static IQueryable<T> AsExpandable<T>(this IQueryable<T> thisValue)
	{
		ExpandableQuery<T> query = thisValue as ExpandableQuery<T>;
		return query ?? new ExpandableQuery<T>(thisValue);
	}

	[NotNull]
	public static IQueryable<T> Paginate<T>([NotNull] this IQueryable<T> thisValue, IPagination settings)
	{
		if (settings == null) return thisValue;
		if (settings.PageSize < 1) settings.PageSize = Pagination.PAGE_SIZE;
		if (settings.Page < 1) settings.Page = 1;
		int start = (settings.Page - 1) * settings.PageSize;
		return thisValue.Skip(start).Take(settings.PageSize);
	}

	[NotNull]
	public static IQueryable<T> WhereIf<T>([NotNull] this IQueryable<T> query, bool condition, [NotNull] Expression<Func<T, bool>> predicate)
	{
		return condition
					? query.Where(predicate)
					: query;
	}
}