using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using essentialMix.Exceptions.Collections;
using essentialMix.Helpers;
using essentialMix.Numeric;
using JetBrains.Annotations;

namespace essentialMix.Extensions;

public static class IReadOnlyListExtension
{
	[NotNull]
	public static Type[] Types([NotNull] this IReadOnlyList<object> thisValue)
	{
		return thisValue.Count == 0 ? Type.EmptyTypes : thisValue.Select(item => item.AsType()).ToArray();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining | MethodImplOptions.ForwardRef)]
	public static int IndexOf<T>([NotNull] this IReadOnlyList<T> thisValue, T value) { return IndexOf(thisValue, value, null); }
	public static int IndexOf<T>([NotNull] this IReadOnlyList<T> thisValue, T value, IEqualityComparer<T> comparer)
	{
		if (thisValue.Count == 0) return -1;

		comparer ??= EqualityComparer<T>.Default;

		for (int i = 0; i < thisValue.Count; i++)
		{
			if (!comparer.Equals(thisValue[i], value)) continue;
			return i;
		}

		return -1;
	}

	public static int BinarySearch<T>([NotNull] this IReadOnlyList<T> thisValue, T value) { return BinarySearch(thisValue, value, 0, -1, null); }
	public static int BinarySearch<T>([NotNull] this IReadOnlyList<T> thisValue, T value, int index) { return BinarySearch(thisValue, value, index, -1, null); }
	public static int BinarySearch<T>([NotNull] this IReadOnlyList<T> thisValue, T value, int index, int count) { return BinarySearch(thisValue, value, index, count, null); }
	public static int BinarySearch<T>([NotNull] this IReadOnlyList<T> thisValue, T value, IComparer<T> comparer) { return BinarySearch(thisValue, value, 0, -1, comparer); }
	public static int BinarySearch<T>([NotNull] this IReadOnlyList<T> thisValue, T value, int index, IComparer<T> comparer) { return BinarySearch(thisValue, value, index, -1, comparer); }
	public static int BinarySearch<T>([NotNull] this IReadOnlyList<T> thisValue, T value, int index, int count, IComparer<T> comparer)
	{
		thisValue.Count.ValidateRange(index, ref count);
		comparer ??= Comparer<T>.Default;

		int lo = index;
		int hi = index + count - 1;

		while (lo <= hi)
		{
			// get median
			int mid = lo + ((hi - lo) >> 1);
			int c = comparer.Compare(thisValue[mid], value);
			if (c == 0)
				return mid;

			if (c < 0)
				lo = mid + 1;
			else
				hi = mid - 1;
		}

		return ~lo;
	}

	public static T PickRandom<T>([NotNull] this IReadOnlyList<T> thisValue, int startIndex = 0, int count = -1)
	{
		thisValue.Count.ValidateRange(startIndex, ref count);
		if (thisValue.Count == 0) throw new CollectionIsEmptyException();

		int max;
		int n;

		if (thisValue is ICollection collection)
		{
			lock (collection.SyncRoot)
			{
				max = count - 1;
				if (max < 0) throw new CollectionIsEmptyException();
				n = RNGRandomHelper.Next(startIndex, max);
				return thisValue[n];
			}
		}

		lock (thisValue)
		{
			max = count - 1;
			n = RNGRandomHelper.Next(startIndex, max);
			return thisValue[n];
		}
	}

	/// <summary>
	/// This method takes a list of numbers and tries to find the maximum sum the can be obtained by summing
	/// up the longest sequence of adjacent elements in the list.
	/// </summary>
	/// <typeparam name="T">The value type</typeparam>
	/// <param name="thisValue">List of numbers</param>
	/// <returns>The maximum sum that can be obtained by summing up the longest sequence of adjacent elements in the list.</returns>
	public static T KadaneMaximumSum<T>([NotNull] this IReadOnlyList<T> thisValue)
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
	{
		// AlgoExpert - Become An Expert In Algorithms
		/*
		* Write a function that takes in a non-empty array of integers and returns the
		* maximum sum that can be obtained by summing up all the numbers in a non-empty
		* sub-array of the input array. A sub-array must only contain adjacent numbers.
		*/
		if (thisValue.Count == 0) return default(T);
		if (thisValue.Count == 1) return thisValue[0];

		T maxEndingHere = thisValue[0];
		T maxSoFar = maxEndingHere;

		for (int i = 1; i < thisValue.Count; i++)
		{
			T value = thisValue[i];
			T newMaxEndingHere = maxEndingHere.Add(value);
			maxEndingHere = newMaxEndingHere.CompareTo(value) > 0
								? newMaxEndingHere
								: value;
			if (maxSoFar.CompareTo(maxEndingHere) < 0) maxSoFar = maxEndingHere;
		}

		return maxSoFar;
	}

	public static T DeepestPit<T>([NotNull] this IReadOnlyList<T> thisValue)
		where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
	{
		// https://medium.com/@spylogsster/deepest-pit-of-an-array-solution-ee7fd0b4d1c7
		/*
		* A non-empty zero-indexed array A consisting of N integers is given.
		* A pit in this array is any triplet of integers (P, Q, R) such that:
		* 0 <= P < Q < R < N
		*
		* sequence [A[P], A[P+1], ..., A[Q]] is strictly decreasing:
		* i.e. A[P] > A[P + 1] > ... > A[Q]
		*
		* sequence A[Q], A[Q+1], ..., A[R] is strictly increasing:
		* i.e. A[Q] < A[Q + 1] < ... < A[R]
		*
		* Triplet (2, 3, 4) is one of pits in this array, because sequence [A[2], A[3]] is strictly
		* decreasing (3 > −2) and sequence [A[3], A[4]] is strictly increasing (−2 < 0). Its depth
		* is min{A[2] − A[3], A[4] − A[3]} = 2. Triplet (2, 3, 5) is another pit with depth 3.
		* Triplet (5, 7, 8) is yet another pit with depth 4. There is no pit in this array deeper
		* i.e. having depth greater than 4.
		*
		* Complexity:
		*  expected worst-case time complexity is O(N);
		*  expected worst-case space complexity is O(N), beyond input storage (not counting the
		*  storage required for input arguments)
		*
		* Elements of input arrays can be modified.
		*/
		T def = default(T), minus = def.Decrement(), deepest = minus;
		if (thisValue.Count < 3) return deepest;

		int p = 0, q = -1;

		for (int i = 1; i < thisValue.Count; i++)
		{
			if (q < 0 && thisValue[i].CompareTo(thisValue[i - 1]) >= 0) q = i - 1;
			if (q < 0 || thisValue[i].CompareTo(thisValue[i - 1]) > 0 && i + 1 != thisValue.Count) continue;
			int r = thisValue[i].CompareTo(thisValue[i - 1]) <= 0
						? i - 1
						: i;
			deepest = Math2.Max(deepest, Math2.Min(thisValue[p].Subtract(thisValue[q]), thisValue[r].Subtract(thisValue[q])));
			p = i - 1;
			q = -1;
		}

		if (deepest.Equals(def)) deepest = minus;
		return deepest;
	}
}