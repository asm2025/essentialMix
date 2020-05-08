using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class IEqualityComparerExtension
	{
		public static bool In<T>([NotNull] this IEqualityComparer<T> thisValue, T value, [NotNull] params T[] list)
		{
			return list.Contains(value, thisValue);
		}

		public static bool In<T>([NotNull] this IEqualityComparer<T> thisValue, T value, [NotNull] IEnumerable<T> list)
		{
			return list.Contains(value, thisValue);
		}
	}
}