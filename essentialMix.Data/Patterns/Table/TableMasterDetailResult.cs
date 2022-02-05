using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace essentialMix.Data.Patterns.Table;

[DebuggerDisplay("Columns = {Columns.Count}")]
[Serializable]
public class TableMasterDetailResult<T> : TableDetailResult<T>, ITableMasterDetailResult<T>
{
	/// <inheritdoc />
	public TableMasterDetailResult()
		: this(null, default(T))
	{
	}

	/// <inheritdoc />
	public TableMasterDetailResult(string name)
		: this(name, default(T))
	{
	}

	/// <inheritdoc />
	public TableMasterDetailResult(T item)
		: this(null, item)
	{
	}

	/// <inheritdoc />
	public TableMasterDetailResult(string name, T item)
	{
		Name = name;
		Item = item;
	}

	/// <inheritdoc />
	public TableCollection Details { get; } = new TableCollection();

	/// <inheritdoc />
	public TableCollection Lookups { get; } = new TableCollection();
}

[Serializable]
public class TableMasterDetailResult : TableMasterDetailResult<IDictionary<string, object>>, ITableMasterDetailResult
{
	/// <inheritdoc />
	public TableMasterDetailResult()
		: this(null, null)
	{
	}

	/// <inheritdoc />
	public TableMasterDetailResult(string name)
		: this(name, null)
	{
	}

	/// <inheritdoc />
	public TableMasterDetailResult(IDictionary<string, object> item)
		: this(null, item)
	{
	}

	/// <inheritdoc />
	public TableMasterDetailResult(string name, IDictionary<string, object> item)
		: base(name, item ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase))
	{
	}
}