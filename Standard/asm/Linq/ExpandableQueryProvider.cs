using System.Linq;
using System.Linq.Expressions;
using asm.Extensions;

namespace asm.Linq
{
	internal class ExpandableQueryProvider<T> : IQueryProvider
	{
		private readonly ExpandableQuery<T> _query;

		internal ExpandableQueryProvider(ExpandableQuery<T> query)
		{
			_query = query;
		}

		IQueryable<TElement> IQueryProvider.CreateQuery<TElement>(Expression expression)
		{
			return new ExpandableQuery<TElement>(_query.InnerQuery.Provider.CreateQuery<TElement>(expression.Expand()));
		}

		IQueryable IQueryProvider.CreateQuery(Expression expression) { return _query.InnerQuery.Provider.CreateQuery(expression.Expand()); }

		TResult IQueryProvider.Execute<TResult>(Expression expression) { return _query.InnerQuery.Provider.Execute<TResult>(expression.Expand()); }

		object IQueryProvider.Execute(Expression expression) { return _query.InnerQuery.Provider.Execute(expression.Expand()); }
	}
}