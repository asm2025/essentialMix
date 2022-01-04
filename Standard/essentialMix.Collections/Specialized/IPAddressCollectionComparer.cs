using System.Collections.Generic;
using System.Linq;
using essentialMix.Comparers;

namespace essentialMix.Collections.Specialized;

public class IPAddressCollectionComparer : GenericComparer<ICollection<IPAddressEntry>>
{
	public new static IPAddressCollectionComparer Default { get; } = new IPAddressCollectionComparer();

	/// <inheritdoc />
	public IPAddressCollectionComparer()
	{
	}

	/// <inheritdoc />
	public IPAddressCollectionComparer(IComparer<ICollection<IPAddressEntry>> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public IPAddressCollectionComparer(IComparer<ICollection<IPAddressEntry>> comparer, IEqualityComparer<ICollection<IPAddressEntry>> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(ICollection<IPAddressEntry> x, ICollection<IPAddressEntry> y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		if (x.Count != y.Count) return y.Count - x.Count;
		return x.Select((t, i) => IPAddressEntryComparer.Default.Compare(t, y.ElementAt(i))).Sum();
	}

	public override bool Equals(ICollection<IPAddressEntry> x, ICollection<IPAddressEntry> y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		if (x.Count != y.Count) return false;
		return !x.Where((t, i) => !IPAddressEntryComparer.Default.Equals(t, y.ElementAt(i))).Any();
	}
}