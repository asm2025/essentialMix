using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class ColumnsCollectionComparer : GenericComparer<Browsable.ColumnsCollection>
{
	public new static ColumnsCollectionComparer Default { get; } = new ColumnsCollectionComparer();

	/// <inheritdoc />
	public ColumnsCollectionComparer()
	{
	}

	/// <inheritdoc />
	public ColumnsCollectionComparer(IComparer<Browsable.ColumnsCollection> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public ColumnsCollectionComparer(IComparer<Browsable.ColumnsCollection> comparer, IEqualityComparer<Browsable.ColumnsCollection> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(Browsable.ColumnsCollection x, Browsable.ColumnsCollection y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		if (ReferenceEquals(x.Owner, y.Owner)) return 0;
		if (x.Owner == null) return 1;
		if (y.Owner == null) return -1;
		return DictionaryComparer<string, Column>.Default.Compare(x, y);
	}

	public override bool Equals(Browsable.ColumnsCollection x, Browsable.ColumnsCollection y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null) return false;
		if (y == null) return false;
		return x.GetType() == y.GetType() && ReferenceEquals(x.Owner, y.Owner) && DictionaryComparer<string, Column>.Default.Equals(x, y);
	}
}