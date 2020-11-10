using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;

namespace asm.Linq
{
	public abstract class QueryTranslatorProvider : ExpressionVisitor
	{
		protected QueryTranslatorProvider([JetBrains.Annotations.NotNull] IQueryable source)
		{
			Source = source;
		}

		internal IQueryable Source { get; }
	}

	[SuppressMessage("ReSharper", "UnusedTypeParameter")]
	public class QueryTranslatorProvider<T> : QueryTranslatorProvider, IQueryProvider
	{
		private readonly IEnumerable<ExpressionVisitor> _visitors;

		public QueryTranslatorProvider([JetBrains.Annotations.NotNull] IQueryable source, [JetBrains.Annotations.NotNull] IEnumerable<ExpressionVisitor> visitors)
			: base(source)
		{
		   _visitors = visitors;
		}

		public IQueryable<TElement> CreateQuery<TElement>(Expression expression) { return new QueryTranslator<TElement>(Source, expression, _visitors); }

		public IQueryable CreateQuery(Expression expression)
		{
			Type elementType = expression.Type.GetGenericArguments().First();
			IQueryable result = (IQueryable)Activator.CreateInstance(typeof(QueryTranslator<>).MakeGenericType(elementType), Source, expression, _visitors);
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

		internal IEnumerable ExecuteEnumerable([JetBrains.Annotations.NotNull] Expression expression)
		{
			Expression translated = VisitAll(expression);
			return Source.Provider.CreateQuery(translated);
		}

		private Expression VisitAll(Expression expression)
		{
			// Run all visitors in order
			IEnumerable<ExpressionVisitor> visitors = new ExpressionVisitor[] { this }.Concat(_visitors);
			Expression result = expression;
			
			foreach (ExpressionVisitor visitor in visitors) 
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
}