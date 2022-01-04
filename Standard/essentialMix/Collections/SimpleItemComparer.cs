using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class SimpleItemComparer : GenericComparer<ISimpleItem>
{
	public new static PropertyComparer Default { get; } = new PropertyComparer();

	/// <inheritdoc />
	public SimpleItemComparer()
	{
	}

	/// <inheritdoc />
	public SimpleItemComparer(IComparer<ISimpleItem> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public SimpleItemComparer(IComparer<ISimpleItem> comparer, IEqualityComparer<ISimpleItem> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(ISimpleItem x, ISimpleItem y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;

		int n = HeaderComparer.Default.Compare(x, y);
		return n != 0 ? n : Comparer<object>.Default.Compare(x.Value, y.Value);
	}

	public override bool Equals(ISimpleItem x, ISimpleItem y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return HeaderComparer.Default.Equals(x, y) && x.Value == y.Value;
	}
}