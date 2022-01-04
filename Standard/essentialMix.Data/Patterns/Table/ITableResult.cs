using System.Collections.Generic;
using essentialMix.Patterns.Pagination;
using JetBrains.Annotations;

namespace essentialMix.Data.Patterns.Table;

public interface ITableResult<T> : ITableResultBase<T>
{
	[NotNull]
	IPagination Paging { get; }
}

public interface ITableResult : ITableResult<IDictionary<string, object>>
{
}