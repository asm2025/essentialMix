using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class ItemsCollectionComparer : GenericComparer<Browsable.ItemsCollection>
{
	public new static ItemsCollectionComparer Default { get; } = new ItemsCollectionComparer();

	/// <inheritdoc />
	public ItemsCollectionComparer()
	{
	}

	/// <inheritdoc />
	public ItemsCollectionComparer(IComparer<Browsable.ItemsCollection> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public ItemsCollectionComparer(IComparer<Browsable.ItemsCollection> comparer, IEqualityComparer<Browsable.ItemsCollection> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(Browsable.ItemsCollection x, Browsable.ItemsCollection y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		if (ReferenceEquals(x.Owner, y.Owner)) return 0;
		if (x.Owner == null) return 1;
		if (y.Owner == null) return -1;
		return DictionaryComparer<string, IItem>.Default.Compare(x, y);
	}

	public override bool Equals(Browsable.ItemsCollection x, Browsable.ItemsCollection y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null) return false;
		if (y == null) return false;
		return x.GetType() == y.GetType() && ReferenceEquals(x.Owner, y.Owner) && DictionaryComparer<string, IItem>.Default.Equals(x, y);
	}
}