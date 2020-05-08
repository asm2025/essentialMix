using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;

namespace asm.Data.Patterns.Table
{
	public interface ITableResultBase
	{
		string Name { get; set; }

		[NotNull]
		TableColumns Columns { get; }

		bool MapSchemaTable(DataTable schema, Func<string, bool> filter = null, Func<string, TableColumnSettings?> onGetSettings = null);
	}

	public interface ITableResultBase<T> : ITableResultBase
	{
		[NotNull]
		ICollection<T> Items { get; }
		bool MapSchemaTable<TInstance>(TInstance instance, Func<string, bool> filter = null, Func<string, TableColumnSettings?> onGetSettings = null);
	}
}