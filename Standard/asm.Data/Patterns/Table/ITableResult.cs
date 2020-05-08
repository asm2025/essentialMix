using System.Collections.Generic;
using asm.Patterns.Pagination;
using JetBrains.Annotations;

namespace asm.Data.Patterns.Table
{
	public interface ITableResult<T> : ITableResultBase<T>
	{
		[NotNull]
		IPagination Paging { get; }
	}

	public interface ITableResult : ITableResult<IDictionary<string, object>>
	{
	}
}
