using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace essentialMix.Linq
{
	public class QueryTranslator<T> : IOrderedQueryable<T>
	{
		private readonly QueryTranslatorProvider<T> _provider;

		public QueryTranslator([NotNull] IQueryable source, [NotNull] IEnumerable<ExpressionVisitor> visitors)
		{
			Expression = Expression.Constant(this);
			_provider = new QueryTranslatorProvider<T>(source, visitors);
		}

		public QueryTranslator([NotNull] IQueryable source, [NotNull] Expression expression, [NotNull] IEnumerable<ExpressionVisitor> visitors)
		{
			Expression = expression;
			_provider = new QueryTranslatorProvider<T>(source, visitors);
		}

		public IEnumerator<T> GetEnumerator() { return ((IEnumerable<T>)_provider.ExecuteEnumerable(Expression)).GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return _provider.ExecuteEnumerable(Expression).GetEnumerator(); }

		public Type ElementType => typeof(T);

		public Expression Expression { get; }

		public IQueryProvider Provider => _provider;
	}
}