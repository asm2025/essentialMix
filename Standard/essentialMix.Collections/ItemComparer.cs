using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class ItemComparer : GenericComparer<IItem>
{
	public new static ItemComparer Default { get; } = new ItemComparer();

	/// <inheritdoc />
	public ItemComparer()
	{
	}

	/// <inheritdoc />
	public ItemComparer(IComparer<IItem> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public ItemComparer(IComparer<IItem> comparer, IEqualityComparer<IItem> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(IItem x, IItem y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		if (x.IsContainer && !y.IsContainer) return -1;
		if (!x.IsContainer && y.IsContainer) return 1;
		return PropertyComparer.Default.Compare(x, y);
	}

	public override bool Equals(IItem x, IItem y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return x.IsContainer == y.IsContainer &&
				PropertyComparer.Default.Equals(x, y);
	}
}