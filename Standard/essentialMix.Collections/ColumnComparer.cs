using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class ColumnComparer : GenericComparer<Column>
{
	public new static ColumnComparer Default { get; } = new ColumnComparer();

	/// <inheritdoc />
	public ColumnComparer()
	{
	}

	/// <inheritdoc />
	public ColumnComparer(IComparer<Column> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public ColumnComparer(IComparer<Column> comparer, IEqualityComparer<Column> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(Column x, Column y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		if (x.Order != y.Order) return x.Order - y.Order;
		return HeaderComparer.Default.Compare(x, y);
	}

	public override bool Equals(Column x, Column y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null) return false;
		return y != null && x.Order == y.Order && HeaderComparer.Default.Equals(x, y);
	}
}