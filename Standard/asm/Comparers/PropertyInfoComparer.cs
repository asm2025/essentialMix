using System;
using System.Reflection;
using asm.Extensions;

namespace asm.Comparers
{
	public class PropertyInfoComparer : ComparisonComparer<PropertyInfo>
	{
		public new static PropertyInfoComparer Default { get; } = new PropertyInfoComparer();

		/// <inheritdoc />
		public PropertyInfoComparer()
			: base(CompareInternal, EqualsInternal)
		{
		}

		private static int CompareInternal(PropertyInfo x, PropertyInfo y)
		{
			if (ReferenceEquals(x, y)) return 0;
			if (x == null) return 1;
			if (y == null) return -1;

			Type xd = x.DeclaringType;
			Type yd = y.DeclaringType;
			if (ReferenceEquals(xd, yd)) return string.CompareOrdinal(x.Name, y.Name);
			if (xd == null) return 1;
			if (yd == null) return -1;
			if (xd.IsAssignableTo(yd) || yd.IsAssignableTo(xd)) return string.CompareOrdinal(x.Name, y.Name);

			int compare = string.CompareOrdinal(xd.FullName, yd.FullName);
			return compare != 0
						? compare
						: string.CompareOrdinal(x.Name, y.Name);
		}

		private static bool EqualsInternal(PropertyInfo x, PropertyInfo y)
		{
			return CompareInternal(x, y) == 0;
		}
	}
}