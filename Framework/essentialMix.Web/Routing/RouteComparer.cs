using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Web.Routing;

public class RouteComparer : GenericComparer<RouteBase>
{
	public new static RouteComparer Default { get; } = new RouteComparer();

	/// <inheritdoc />
	public RouteComparer() 
	{
	}

	/// <inheritdoc />
	public RouteComparer(IComparer<RouteBase> comparer)
		: base(comparer)
	{
	}

	/// <inheritdoc />
	public RouteComparer(IComparer<RouteBase> comparer, IEqualityComparer<RouteBase> equalityComparer)
		: base(comparer, equalityComparer)
	{
	}

	public override int Compare(RouteBase x, RouteBase y)
	{
		if (ReferenceEquals(x, y)) return 0;
		if (x == null) return 1;
		if (y == null) return -1;
		return x.CompareTo(y);
	}

	public override bool Equals(RouteBase x, RouteBase y)
	{
		if (ReferenceEquals(x, y)) return true;
		if (x == null || y == null) return false;
		return x.Equals(y);
	}
}