using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace essentialMix.Linq
{
	public class ExpandableQuery<T> : IQueryable<T>, IOrderedQueryable<T>, IOrderedQueryable
	{
		private readonly ExpandableQueryProvider<T> _provider;

		internal ExpandableQuery(IQueryable<T> inner)
		{
			InnerQuery = inner;
			_provider = new ExpandableQueryProvider<T>(this);
		}

		public override string ToString() { return InnerQuery.ToString(); }

		internal IQueryable<T> InnerQuery { get; }

		Expression IQueryable.Expression => InnerQuery.Expression;
		Type IQueryable.ElementType => typeof(T);
		IQueryProvider IQueryable.Provider => _provider;

		public IEnumerator<T> GetEnumerator() { return InnerQuery.GetEnumerator(); }

		IEnumerator IEnumerable.GetEnumerator() { return InnerQuery.GetEnumerator(); }
	}
}