using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace asm.Web.Routing
{
	public class RouteData : IComparable<RouteData>, IComparable, IEquatable<RouteData>
	{
		public RouteData()
		{
		}

		public int CompareTo(object obj) { return CompareTo(obj as RouteData); }

		public int CompareTo(RouteData other)
		{
			if (other == null) return -1;
			return RouteComparer.Default.Compare(Route, other.Route);
		}

		public bool Equals(RouteData other) { return ReferenceEquals(this, other) || other != null && GetType() == other.GetType() && CompareTo(other) == 0; }

		public override string ToString() { return Data; }

		public string Data { get; private set; }
		public RouteBase Route { get; private set; }

		[ItemNotNull]
		public static IEnumerable<RouteData> ParseRoutes(string value)
		{
			if (string.IsNullOrEmpty(value)) yield break;

			foreach ((RouteBase Route, string Value) route in RouteBase.ParseRoutesInternal(value))
			{
				yield return new RouteData
				{
					Route = route.Route,
					Data = route.Value
				};
			}
		}
	}
}
