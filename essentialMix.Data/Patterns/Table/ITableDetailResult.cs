using System.Collections.Generic;

namespace essentialMix.Data.Patterns.Table;

public interface ITableDetailResult<T> : ITableResultBase
{
	T Item { get; set; }
	int LayoutColumns { get; set; }
}

public interface ITableDetailResult : ITableDetailResult<IDictionary<string, object>>
{
}