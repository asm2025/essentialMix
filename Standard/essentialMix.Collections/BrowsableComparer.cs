using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class BrowsableComparer : GenericComparer<IBrowsable>
{
	public new static BrowsableComparer Default { get; } = new BrowsableComparer();

	/// <inheritdoc />
	public BrowsableComparer() 
	{
	}

	/// <inheritdoc />
	public BrowsableComparer(IComparer<IBrowsable> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public BrowsableComparer(IComparer<IBrowsable> comparer, IEqualityComparer<IBrowsable> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(IBrowsable x, IBrowsable y)
	{
		int cmp = ItemComparer.Default.Compare(x, y);
		if (cmp != 0) return cmp;
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		return ColumnsCollectionComparer.Default.Compare(x.Columns, y.Columns);
	}

	public override bool Equals(IBrowsable x, IBrowsable y)
	{
		if (ItemComparer.Default.Equals(x, y)) return true;
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return ColumnsCollectionComparer.Default.Equals(x.Columns, y.Columns);
	}
}