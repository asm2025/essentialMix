using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Data.Patterns.Table
{
	public interface ITableMasterDetailResult<T> : ITableDetailResult<T>
	{
		[NotNull]
		TableCollection Details { get; }

		[NotNull]
		TableCollection Lookups { get; }
	}

	public interface ITableMasterDetailResult : ITableMasterDetailResult<IDictionary<string, object>>
	{
	}
}