using System;
using System.Globalization;
using JetBrains.Annotations;

namespace essentialMix.Comparers
{
	[Serializable]
	public abstract class StringFunctionalComparer : StringComparer, IGenericComparer<string>
	{
		protected StringFunctionalComparer()
		{
		}

		public static StringComparer StartsWithCurrentCulture { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.CurrentCulture, false, FunctionalComparerMethod.StartsWith);
		public static StringComparer StartsWithCurrentCultureIgnoreCase { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.CurrentCulture, true, FunctionalComparerMethod.StartsWith);
		public static StringComparer StartsWithInvariantCulture { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.InvariantCulture, false, FunctionalComparerMethod.StartsWith);
		public static StringComparer StartsWithInvariantCultureIgnoreCase { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.InvariantCulture, true, FunctionalComparerMethod.StartsWith);
		public static StringComparer StartsWithOrdinal { get; } = new OrdinalFunctionalComparer(false, FunctionalComparerMethod.StartsWith);
		public static StringComparer StartsWithOrdinalIgnoreCase { get; } = new OrdinalFunctionalComparer(true, FunctionalComparerMethod.StartsWith);

		public static StringComparer ContainsCurrentCulture { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.CurrentCulture, false, FunctionalComparerMethod.Contains);
		public static StringComparer ContainsCurrentCultureIgnoreCase { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.CurrentCulture, true, FunctionalComparerMethod.Contains);
		public static StringComparer ContainsInvariantCulture { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.InvariantCulture, false, FunctionalComparerMethod.Contains);
		public static StringComparer ContainsInvariantCultureIgnoreCase { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.InvariantCulture, true, FunctionalComparerMethod.Contains);
		public static StringComparer ContainsOrdinal { get; } = new OrdinalFunctionalComparer(false, FunctionalComparerMethod.Contains);
		public static StringComparer ContainsOrdinalIgnoreCase { get; } = new OrdinalFunctionalComparer(true, FunctionalComparerMethod.Contains);

		public static StringComparer EndsWithCurrentCulture { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.CurrentCulture, false, FunctionalComparerMethod.EndsWith);
		public static StringComparer EndsWithCurrentCultureIgnoreCase { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.CurrentCulture, true, FunctionalComparerMethod.EndsWith);
		public static StringComparer EndsWithInvariantCulture { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.InvariantCulture, false, FunctionalComparerMethod.EndsWith);
		public static StringComparer EndsWithInvariantCultureIgnoreCase { get; } = new StringCultureAwareFunctionalComparer(CultureInfo.InvariantCulture, true, FunctionalComparerMethod.EndsWith);
		public static StringComparer EndsWithOrdinal { get; } = new OrdinalFunctionalComparer(false, FunctionalComparerMethod.EndsWith);
		public static StringComparer EndsWithOrdinalIgnoreCase { get; } = new OrdinalFunctionalComparer(true, FunctionalComparerMethod.EndsWith);

		[NotNull]
		public static StringComparer Create([NotNull] CultureInfo culture, bool ignoreCase, FunctionalComparerMethod method)
		{
			return new StringCultureAwareFunctionalComparer(culture, ignoreCase, method);
		}
	}
}