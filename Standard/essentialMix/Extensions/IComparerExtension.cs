using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using essentialMix.Comparers;
using Other.JonSkeet.MiscUtil.Collections;
using Other.MarcGravell;

namespace essentialMix.Extensions
{
	public static class IComparerExtension
	{
		/// <summary>
		/// Reverses the original comparer; if it was already a reverse comparer,
		/// the previous version was reversed (rather than reversing twice).
		/// In other words, for any comparer X, X==X.Reverse().Reverse().
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static IComparer<T> Reverse<T>([NotNull] this IComparer<T> thisValue)
		{
			return thisValue is ReverseComparer<T> reverse
						? reverse.Original
						: new ReverseComparer<T>(thisValue);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static IEqualityComparer<T> AsEqualityComparer<T>([NotNull] this IComparer<T> thisValue)
		{
			return new ComparerForEquality<T>(thisValue);
		}
			
		/// <summary>
		/// Combines a comparer with a second comparer to implement composite sort
		/// behaviour.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static IComparer<T> ThenBy<T>([NotNull] this IComparer<T> thisValue, [NotNull] IComparer<T> secondComparer, IEqualityComparer<T> firstEqualityComparer = null, IEqualityComparer<T> secondEqualityComparer = null)
		{
			return new LinkedComparer<T>(thisValue, secondComparer, firstEqualityComparer ?? EqualityComparer<T>.Default, secondEqualityComparer);
		}

		/// <summary>
		/// Combines a comparer with a projection to implement composite sort behaviour.
		/// </summary>
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static IComparer<T> ThenBy<T, TKey>([NotNull] this IComparer<T> thisValue, [NotNull] Func<T, TKey> projection)
		{
			return new LinkedComparer<T>(thisValue, new ProjectionComparer<T, TKey>(projection));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsEqual<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) == 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsLessThan<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsLessThanOrEqual<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) <= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsGreaterThan<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) > 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsGreaterThanOrEqual<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) >= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRange<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
		{
			return thisValue.Compare(value, minimum) >= 0 && thisValue.Compare(value, maximum) <= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeEx<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
		{
			return thisValue.Compare(value, minimum) > 0 && thisValue.Compare(value, maximum) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeLx<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
		{
			return thisValue.Compare(value, minimum) > 0 && thisValue.Compare(value, maximum) <= 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool InRangeRx<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
		{
			return thisValue.Compare(value, minimum) >= 0 && thisValue.Compare(value, maximum) < 0;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Within<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
		{
			if (thisValue.Compare(minimum, maximum) > 0) throw new InvalidOperationException($"{nameof(minimum)} cannot be greater than {nameof(maximum)}.");
			return thisValue.Compare(value, minimum) < 0
						? minimum
						: thisValue.Compare(value, maximum) > 0
							? maximum
							: value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T WithinEx<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
			where T : struct, IComparable
		{
			return Within(thisValue, value, Operator<T>.Increment(minimum), Operator<T>.Decrement(maximum));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T WithinLx<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
			where T : struct, IComparable
		{
			return Within(thisValue, value, Operator<T>.Increment(minimum), maximum);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T WithinRx<T>([NotNull] this IComparer<T> thisValue, T value, T minimum, T maximum)
			where T : struct, IComparable
		{
			return Within(thisValue, value, minimum, Operator<T>.Decrement(maximum));
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T NotBelow<T>([NotNull] this IComparer<T> thisValue, T value, T minimum)
		{
			return thisValue.Compare(value, minimum) < 0
						? minimum
						: value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T NotAbove<T>([NotNull] this IComparer<T> thisValue, T value, T maximum)
		{
			return thisValue.Compare(value, maximum) > 0
						? maximum
						: value;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Minimum<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) < 0
						? x
						: y;
		}

		public static T Minimum<T>([NotNull] this IComparer<T> thisValue, T value, [NotNull] params T[] values)
		{
			if (values.Length == 0) return value;

			T v = value;

			foreach (T m in values)
			{
				// skip of v <= m
				if (thisValue.Compare(v, m) <= 0) continue;
				v = m;
			}

			return v;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T Maximum<T>([NotNull] this IComparer<T> thisValue, T x, T y)
		{
			return thisValue.Compare(x, y) > 0
						? x
						: y;
		}

		public static T Maximum<T>([NotNull] this IComparer<T> thisValue, T value, [NotNull] params T[] values)
		{
			if (values.Length == 0) return value;

			T v = value;

			foreach (T m in values)
			{
				// skip of v >= m
				if (thisValue.Compare(v, m) >= 0) continue;
				v = m;
			}

			return v;
		}

		public static T MinimumNotBelow<T>([NotNull] this IComparer<T> thisValue, T value, T bound, [NotNull] params T[] values)
		{
			T v = NotBelow(thisValue, value, bound);
			if (values.IsNullOrEmpty() || thisValue.Compare(v, bound) == 0) return v;

			foreach (T m in values)
			{
				// skip if v <= m
				if (thisValue.Compare(v, m) <= 0) continue;
				v = m;
				// stop if at bound
				if (thisValue.Compare(v, bound) == 0) break;
			}

			return v;
		}

		public static T MaximumNotAbove<T>([NotNull] this IComparer<T> thisValue, T value, T bound, [NotNull] params T[] values)
		{
			T v = NotBelow(thisValue, value, bound);
			if (values.IsNullOrEmpty() || thisValue.Compare(v, bound) == 0) return v;

			foreach (T m in values)
			{
				// skip if v >= m
				if (thisValue.Compare(v, m) >= 0) continue;
				v = m;
				// stop if at bound
				if (thisValue.Compare(v, bound) == 0) break;
			}

			return v;
		}
	}
}