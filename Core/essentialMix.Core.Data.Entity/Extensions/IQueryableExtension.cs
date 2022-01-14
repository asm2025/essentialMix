// ReSharper disable once CheckNamespace
namespace essentialMix.Extensions;

public static class IQueryableExtension
{
	//private class DbContextBinder : ExpressionVisitor
	//{
	//	private readonly ObjectContext _context;

	//	public DbContextBinder([NotNull] IObjectContextAdapter adapter)
	//	{
	//		_context = adapter.ObjectContext;
	//	}

	//	public IQueryProvider TargetProvider { get; private set; }

	//	protected override Expression VisitConstant(ConstantExpression node)
	//	{
	//		if (node.Value is ObjectQuery objectQuery && objectQuery.Context != _context) return Expression.Constant(CreateObjectQuery((dynamic)objectQuery));
	//		return base.VisitConstant(node);
	//	}

	//	[NotNull]
	//	private ObjectQuery<T> CreateObjectQuery<T>([NotNull] ObjectQuery<T> source)
	//	{
	//		ObjectParameter[] parameters = source.Parameters
	//											.Select(p => new ObjectParameter(p.Name, p.ParameterType) { Value = p.Value })
	//											.ToArray();
	//		ObjectQuery<T> query = _context.CreateQuery<T>(source.CommandText, parameters);
	//		query.MergeOption = source.MergeOption;
	//		query.Streaming = source.Streaming;
	//		query.EnablePlanCaching = source.EnablePlanCaching;
	//		TargetProvider ??= ((IQueryable)query).Provider;
	//		return query;
	//	}
	//}

	//[NotNull]
	//public static IQueryable<T> BindTo<T>([NotNull] this IQueryable<T> thisValue, [NotNull] DbContext context)
	//{
	//	DbContextBinder binder = new DbContextBinder(context);
	//	Expression expression = binder.Visit(thisValue.Expression);
	//	IQueryProvider provider = binder.TargetProvider;
	//	return provider != null
	//				? provider.CreateQuery<T>(expression)
	//				: thisValue;
	//}
}