using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class PropertiesComparer : GenericComparer<IProperties>
{
	public new static PropertiesComparer Default { get; } = new PropertiesComparer();

	/// <inheritdoc />
	public PropertiesComparer() 
	{
	}

	/// <inheritdoc />
	public PropertiesComparer(IComparer<IProperties> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public PropertiesComparer(IComparer<IProperties> comparer, IEqualityComparer<IProperties> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(IProperties x, IProperties y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		return DictionaryComparer<string, IProperty>.Default.Compare(x, y);
	}

	public override bool Equals(IProperties x, IProperties y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null || x.GetType() != y.GetType()) return false;
		return DictionaryComparer<string, IProperty>.Default.Equals(x, y);
	}
}