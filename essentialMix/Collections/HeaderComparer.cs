using System;
using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Collections;

public class HeaderComparer : GenericComparer<IHeader>
{
	public new static HeaderComparer Default { get; } = new HeaderComparer();

	public override int Compare(IHeader x, IHeader y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;

		int n = StringComparer.OrdinalIgnoreCase.Compare(x.Name, y.Name);
		if (n != 0) return n;
		n = Comparer<Type>.Default.Compare(x.ValueType, y.ValueType);
		return n != 0 ? n : StringComparer.OrdinalIgnoreCase.Compare(x.Text, y.Text);
	}

	public override bool Equals(IHeader x, IHeader y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return x.GetType() == y.GetType() &&
				StringComparer.OrdinalIgnoreCase.Equals(x.Name, y.Name) &&
				x.ValueType == y.ValueType &&
				StringComparer.OrdinalIgnoreCase.Equals(x.Text, y.Text);
	}
}