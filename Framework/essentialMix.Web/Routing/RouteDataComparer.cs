using System.Collections.Generic;
using essentialMix.Comparers;

namespace essentialMix.Web.Routing
{
	public class RouteDataComparer : GenericComparer<RouteData>
	{
		public new static RouteDataComparer Default { get; } = new RouteDataComparer();

		/// <inheritdoc />
		public RouteDataComparer() 
		{
		}

		/// <inheritdoc />
		public RouteDataComparer(IComparer<RouteData> comparer)
			: base(comparer)
		{
		}

		/// <inheritdoc />
		public RouteDataComparer(IComparer<RouteData> comparer, IEqualityComparer<RouteData> equalityComparer)
			: base(comparer, equalityComparer)
		{
		}

		public override int Compare(RouteData x, RouteData y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;
			return x.CompareTo(y);
		}

		public override bool Equals(RouteData x, RouteData y)
		{
			if (ReferenceEquals(x, y)) return true;
			if (x == null || y == null) return false;
			return x.Equals(y);
		}
	}
}