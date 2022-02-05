using System;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace essentialMix.Data.Patterns.Table;

[DebuggerDisplay("Count = {Count}")]
[Serializable]
public class TableCollection : KeyedCollection<string, ITableResult>
{
	/// <inheritdoc />
	public TableCollection()
		: base(StringComparer.OrdinalIgnoreCase)
	{
	}

	/// <inheritdoc />
	protected override string GetKeyForItem(ITableResult item) { return item.Name; }
}