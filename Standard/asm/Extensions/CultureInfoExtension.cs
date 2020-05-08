using System.Collections.Concurrent;
using System.Globalization;
using JetBrains.Annotations;

namespace asm.Extensions
{
	public static class CultureInfoExtension
	{
		private static readonly ConcurrentDictionary<int, RegionInfo> __regionCache = new ConcurrentDictionary<int, RegionInfo>();

		public static string ListSeparator(this CultureInfo thisValue) { return thisValue?.TextInfo.ListSeparator; }

		public static bool IsRightToLeft(this CultureInfo thisValue) { return thisValue != null && thisValue.TextInfo.IsRightToLeft; }

		[NotNull]
		public static CultureInfo Neutral([NotNull] this CultureInfo thisValue)
		{
			return thisValue.IsNeutralCulture
						? thisValue
						: thisValue.Parent;
		}

		public static RegionInfo Region([NotNull] this CultureInfo thisValue)
		{
			return __regionCache.GetOrAdd(thisValue.LCID, lcid => new RegionInfo(lcid));
		}
	}
}