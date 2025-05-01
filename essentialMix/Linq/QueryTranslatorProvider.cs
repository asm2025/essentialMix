using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using NotNullAttribute = JetBrains.Annotations.NotNullAttribute;

namespace essentialMix.Linq;

public abstract class QueryTranslatorProvider([NotNull] IQueryable source) : ExpressionVisitor
{
	internal IQueryable Source { get; } = source;
}

[SuppressMessage("ReSharper", "UnusedTypeParameter")]
public class QueryTranslatorProvider<T>([NotNull] IQueryable source, [NotNull] IEnumerable<ExpressionVisitor> visitors)
	: QueryTranslatorProvider(source), IQueryProvider
{
	public IQueryable<TElement> CreateQuery<TElement>(Expression expression) { return new QueryTranslator<TElement>(Source, expression, visitors); }

	public IQueryable CreateQuery(Expression expression)
	{
		Type elementType = expression.Type.GetGenericArguments().First();
		IQueryable result = (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, visitors);
		return result;
	}

	public TResult Execute<TResult>(Expression expression)
	{
		object result = (this as IQueryProvider).Execute(expression);
		return (TResult)result;
	}

	public object Execute(Expression expression)
	{
		Expression translated = VisitAll(expression);
		return Source.Provider.Execute(translated);
	}

	internal IEnumerable ExecuteEnumerable([NotNull] Expression expression)
	{
		Expression translated = VisitAll(expression);
		return Source.Provider.CreateQuery(translated);
	}

	private Expression VisitAll(Expression expression)
	{
		// Run all visitors in order
		IEnumerable<ExpressionVisitor> visitors1 = new ExpressionVisitor[] { this }.Concat(visitors);
		Expression result = expression;

		foreach (ExpressionVisitor visitor in visitors1)
			result = visitor.Visit(result);

		return result;
	}

	protected override Expression VisitConstant(ConstantExpression node)
	{
		// Fix up the Expression tree to work with the underlying LINQ provider
		if (node.Type.IsGenericType && node.Type.GetGenericTypeDefinition() == typeof(QueryTranslator<>))
		{
			if (((IQueryable)node.Value).Provider is QueryTranslatorProvider provider)
				return provider.Source.Expression;

			return Source.Expression;
		}

		return base.VisitConstant(node);
	}
}