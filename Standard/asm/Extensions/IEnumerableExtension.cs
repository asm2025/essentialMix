using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using JetBrains.Annotations;
using asm.Collections;
using asm.Helpers;
using asm.Patterns.Pagination;
using asm.Threading;

namespace asm.Extensions
{
	public static class IEnumerableExtension
	{
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int FastCount([NotNull] this IEnumerable thisValue)
		{
			return FastCount(thisValue, out int count)
						? count
						: -1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool FastCount([NotNull] this IEnumerable thisValue, out int count)
		{
			switch (thisValue)
			{
				case ICollection collection:
					count = collection.Count;
					return true;
				default:
					count = -1;
					return false;
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static int FastCount<T>([NotNull] this IEnumerable<T> thisValue)
		{
			return FastCount(thisValue, out int count)
						? count
						: -1;
		}

		public static bool FastCount<T>([NotNull] this IEnumerable<T> thisValue, out int count)
		{
			switch (thisValue)
			{
				case ISet<T> set:
					count = set.Count;
					return true;
				case ICollection collection:
					count = collection.Count;
					return true;
				case ICollection<T> collection:
					count = collection.Count;
					return true;
				case IReadOnlyCollection<T> readOnlyCollection:
					count = readOnlyCollection.Count;
					return true;
				default:
					count = -1;
					return false;
			}
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> CastTo<T>([NotNull] this IEnumerable thisValue)
		{
			Type type = typeof(T);
			IEnumerable<T> e = thisValue as IEnumerable<T>;
			return e ?? thisValue.Cast<object>().Select(source => (T)Convert.ChangeType(source, type));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> CastTo<T>([NotNull] this IEnumerable thisValue, IFormatProvider provider)
		{
			Type type = typeof(T);
			IEnumerable<T> e = thisValue as IEnumerable<T>;
			return e ?? thisValue.Cast<object>().Select(source => (T)Convert.ChangeType(source, type, provider));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TTarget> CastTo<TSource, TTarget>([NotNull] this IEnumerable<TSource> thisValue)
		{
			Type type = typeof(TTarget);
			IEnumerable<TTarget> e = thisValue as IEnumerable<TTarget>;
			return e ?? thisValue.Select(source => (TTarget)Convert.ChangeType(source, type));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<TTarget> CastTo<TSource, TTarget>([NotNull] this IEnumerable<TSource> thisValue, IFormatProvider provider)
		{
			Type type = typeof(TTarget);
			IEnumerable<TTarget> e = thisValue as IEnumerable<TTarget>;
			return e ?? thisValue.Select(source => (TTarget)Convert.ChangeType(source, type, provider));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerator<T> CastEnumerator<T>([NotNull] this IEnumerable thisValue)
		{
			IEnumerable<T> e = thisValue as IEnumerable<T>;
			return new Enumerator<T>(e ?? thisValue.Cast<T>());
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerator<TTarget> CastEnumerator<TSource, TTarget>([NotNull] this IEnumerable<TSource> thisValue)
		{
			IEnumerable<TTarget> e = thisValue as IEnumerable<TTarget>;
			return new Enumerator<TTarget>(e ?? thisValue.Cast<TTarget>());
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<T> CastLister<T>([NotNull] this IEnumerable thisValue)
		{
			IEnumerable<T> e = thisValue as IEnumerable<T>;
			return new Lister<T>(e ?? thisValue.Cast<T>());
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<TTarget> CastLister<TSource, TTarget>([NotNull] this IEnumerable<TSource> thisValue)
		{
			IEnumerable<TTarget> e = thisValue as IEnumerable<TTarget>;
			return new Lister<TTarget>(e ?? thisValue.Cast<TTarget>());
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Enumerator Enumerate([NotNull] this IEnumerable thisValue) { return new Enumerator(thisValue); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Enumerator<T> Enumerate<T>([NotNull] this IEnumerable<T> thisValue) { return new Enumerator<T>(thisValue); }

		[NotNull]
		public static string Format<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] string format)
		{
			StringBuilder sb = new StringBuilder();
			sb.ConcatFormat(thisValue, format);
			return sb.ToString();
		}

		[NotNull]
		public static string Format<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] string format, char separator)
		{
			StringBuilder sb = new StringBuilder();
			sb.ConcatFormat(thisValue, format, separator);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Format<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] string format, string separator)
		{
			StringBuilder sb = new StringBuilder();
			sb.ConcatFormat(thisValue, format, separator);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Format<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue, [NotNull] string format)
		{
			StringBuilder sb = new StringBuilder();
			sb.ConcatFormat(thisValue, format);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Format<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue, [NotNull] string format, char group)
		{
			StringBuilder sb = new StringBuilder();
			sb.ConcatFormat(thisValue, format, group);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Format<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue, [NotNull] string format, char separator, char group)
		{
			StringBuilder sb = new StringBuilder();
			sb.JoinFormat(thisValue, format, separator, group);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Format<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue, [NotNull] string format, [NotNull] string group)
		{
			StringBuilder sb = new StringBuilder();
			sb.ConcatFormat(thisValue, format, group);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string Format<TKey, TValue>([NotNull] this IEnumerable<KeyValuePair<TKey, TValue>> thisValue, [NotNull] string format, [NotNull] string separator, [NotNull] string group)
		{
			StringBuilder sb = new StringBuilder();
			sb.JoinFormat(thisValue, format, separator, group);
			return sb.ToString();
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToString<T>([NotNull] this IEnumerable<T> thisValue, char separator) { return ToString(thisValue, separator.ToString()); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToString<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] string separator)
		{
			return separator.Length == 0
						? string.Concat(thisValue)
						: string.Join(separator, thisValue);
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static Type[] Types([NotNull] this IEnumerable thisValue)
		{
			return thisValue.Cast<object>()
							.Select(item => item.AsType())
							.ToArray();
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static bool IsDynamic<T>([NotNull] this IEnumerable<T> thisValue) { return thisValue is ICollection<T> collection && !collection.IsReadOnly; }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static string ToString<T>([NotNull] this IEnumerable<T> thisValue)
		{
			StringBuilder sb = new StringBuilder();
			sb.Concat(thisValue);
			return sb.ToString();
		}

		public static void ForEach([NotNull] this IEnumerable thisValue, [NotNull] Action<object> action)
		{
			using (Enumerator enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static void ForEach([NotNull] this IEnumerable thisValue, [NotNull] Action<object, int> action)
		{
			using (Enumerator enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current, enumerator.Position);
			}
		}

		public static void ForEach([NotNull] this IEnumerable thisValue, [NotNull] Func<object, bool> action)
		{
			using (Enumerator enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current)) break;
				}
			}
		}

		public static void ForEach([NotNull] this IEnumerable thisValue, [NotNull] Func<object, int, bool> action)
		{
			using (Enumerator enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current, enumerator.Position)) break;
				}
			}
		}

		public static void ForEach<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Action<T> action)
		{
			using (Enumerator<T> enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current);
			}
		}

		public static void ForEach<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Action<T, int> action)
		{
			using (Enumerator<T> enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
					action(enumerator.Current, enumerator.Position);
			}
		}

		public static void ForEach<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, bool> action)
		{
			using (Enumerator<T> enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current)) break;
				}
			}
		}

		public static void ForEach<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, int, bool> action)
		{
			using (Enumerator<T> enumerator = Enumerate(thisValue))
			{
				while (enumerator.MoveNext())
				{
					if (!action(enumerator.Current, enumerator.Position)) break;
				}
			}
		}

		public static IEnumerable GetRange([NotNull] this IEnumerable thisValue, int startIndex, int count)
		{
			switch (thisValue)
			{
				case IList list:
					list.Count.ValidateRange(startIndex, ref count);
					if (count == 0) yield break;

					for (int i = startIndex; count > 0; i++, count--)
						yield return list[i];

					yield break;
				case ICollection collection:
					collection.Count.ValidateRange(startIndex, ref count);
					break;
				default:
					if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
					if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
					break;
			}

			if (count == 0) yield break;

			IEnumerator enumerator = thisValue.GetEnumerator();
			
			while (startIndex > 0)
			{
				if (!enumerator.MoveNext()) yield break;
				startIndex--;
			}

			while (count > 0 && enumerator.MoveNext())
			{
				--count;
				yield return enumerator.Current;
			}
		}

		public static IEnumerable<T> GetRange<T>([NotNull] this IEnumerable<T> thisValue, int startIndex, int count)
		{
			switch (thisValue)
			{
				case IList<T> list:
					list.Count.ValidateRange(startIndex, ref count);
					if (count == 0) yield break;

					for (int i = startIndex; count > 0; i++, count--)
						yield return list[i];

					yield break;
				case IReadOnlyList<T> readOnlyList:
					readOnlyList.Count.ValidateRange(startIndex, ref count);
					if (count == 0) yield break;

					for (int i = startIndex; count > 0; i++, count--)
						yield return readOnlyList[i];

					yield break;
				case ICollection<T> collection:
					collection.Count.ValidateRange(startIndex, ref count);
					break;
				case IReadOnlyCollection<T> readOnlyCollection:
					readOnlyCollection.Count.ValidateRange(startIndex, ref count);
					break;
				default:
					if (startIndex < 0) throw new ArgumentOutOfRangeException(nameof(startIndex));
					if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
					break;
			}

			if (count == 0) yield break;

			using (IEnumerator<T> enumerator = thisValue.GetEnumerator())
			{
				while (startIndex > 0)
				{
					if (!enumerator.MoveNext()) yield break;
					startIndex--;
				}

				while (count > 0 && enumerator.MoveNext())
				{
					--count;
					yield return enumerator.Current;
				}
			}
		}

		[ItemNotNull]
		public static IEnumerable<IReadOnlyCollection<T>> Partition<T>([NotNull] this IEnumerable<T> thisValue, int size)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0) yield break;

			List<T> list = new List<T>(size);

			foreach (T item in thisValue)
			{
				list.Add(item);
				if (list.Count < size) continue;
				yield return list.AsReadOnly();
				list.Clear();
			}

			// the last loop had less items than size and the list has items, so return them
			if (list.Count > 0) yield return list.ToArray();
		}

		[ItemNotNull]
		public static IEnumerable<IReadOnlyCollection<T>> PartitionUnique<T>([NotNull] this IEnumerable<T> thisValue, int size, IEqualityComparer<T> comparer = null)
		{
			if (size < 0) throw new ArgumentOutOfRangeException(nameof(size));
			if (size == 0) yield break;

			HashSet<T> hashSet = new HashSet<T>(thisValue, comparer ?? EqualityComparer<T>.Default);

			if (hashSet.Count <= size)
			{
				yield return hashSet;
			}
			else
			{
				T[] values = new T[size];
				int i = 0, remaining = hashSet.Count;

				foreach (T item in hashSet)
				{
					values[i++] = item;
					if (i < size) continue;
					yield return values;
					// the last loop had less items than size and the set has items, so return them
					i = 0;
					remaining -= values.Length;
					if (!remaining.InRangeRx(1, values.Length)) continue;
					size = remaining;
					Array.Resize(ref values, size);
				}
			}
		}

		[NotNull]
		public static IEnumerable<byte> ReverseIfIsLittleEndian([NotNull] this IEnumerable<byte> thisValue)
		{
			return BitConverter.IsLittleEndian
						? thisValue.Reverse()
						: thisValue;
		}

		/// <summary>
		/// Can be used to get all permutations at a certain level. A modified copy from Linq internal implementation
		/// </summary>
		[NotNull]
		[ItemNotNull]
		public static IEnumerable<IEnumerable<T>> Combinations<T>([NotNull] this IEnumerable<T> thisValue, int count)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			
			switch (count)
			{
				case 0:
					return Array.Empty<IEnumerable<T>>();
				case 1:
				{
					T t = thisValue.FirstOrDefault();
					return t.IsNull() ? Array.Empty<IEnumerable<T>>() : new[] { new[] { t } };
				}
				default:
				{
					ICollection<T> collection = thisValue as ICollection<T> ?? thisValue.ToArray();

					switch (collection.Count)
					{
						case 0:
							return Array.Empty<IEnumerable<T>>();
						case 1:
							return new[] { new[] { collection.First() } };
						default:
							List<IEnumerable<T>> result = new List<IEnumerable<T>>();
							collection.ForEach((e, i) =>
							{
								int skip = i + 1;
								if (skip >= collection.Count) return;

								IEnumerable<IEnumerable<T>> combinations = CombinationsLocal(collection.Skip(skip), count - 1);
								result.AddRange(combinations.Select(combination => new[] { e }.Union(combination)));
							});

							return result;
					}
				}
			}

			static IEnumerable<IEnumerable<T>> CombinationsLocal(IEnumerable<T> enumerable, int k)
			{
				if (k <= 0) yield break;

				if (k == 1)
				{
					T t = enumerable.FirstOrDefault();
					if (t.IsNull()) yield break;
					yield return new[] { t };
					yield break;
				}

				ICollection<T> collection = enumerable as ICollection<T> ?? enumerable.ToArray();

				switch (collection.Count)
				{
					case 0:
						yield break;
					case 1:
						yield return new[] { collection.First() };
						break;
					default:
						int i = 0;

						foreach (T e in collection)
						{
							if (i + 1 >= collection.Count) break;
							IEnumerable<IEnumerable<T>> combinations = CombinationsLocal(collection.GetRange(i + 1, -1), k - 1);

							foreach (IEnumerable<T> combination in combinations)
								yield return new[] { e }.Concat(combination);

							i++;
						}

						break;
				}
			}
		}

		[NotNull]
		[ItemNotNull]
		public static IEnumerable<IEnumerable<T>> PermutationsHeap<T>([NotNull] this IEnumerable<T> thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// https://www.geeksforgeeks.org/heaps-algorithm-for-generating-permutations/
			IList<T> values = thisValue as IList<T> ?? new List<T>(thisValue);
			if (values.Count < 2) return new[] { values };

			List<IList<T>> results = new List<IList<T>>();
			Permute(values, results, results.Count);
			return results;

			static void Permute(IList<T> list, ICollection<IList<T>> results, int size)
			{
				if (size <= 1) results.Add(new List<T>(list));

				for (int i = 0; i < size; i++)
				{
					Permute(list, results, size - 1);

					/*
					 * if size is odd, swap first and last
					 * else (is even) swap item at i index and last.
					 */
					list.FastSwap(size % 2 == 1
									? 0
									: i, list.Count - 1);
				}
			}
		}

		/// <summary>
		/// Generates all permutations of a sequence. The sequence needs to be sorted if it's going to
		/// start from the first permutation.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <returns></returns>
		[NotNull]
		[ItemNotNull]
		public static IEnumerable<IEnumerable<T>> Permutations<T>([NotNull] this IEnumerable<T> thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// https://leetcode.com/problems/permutations-ii/
			// AlgoExpert - Become An Expert In Algorithms
			IList<T> values = thisValue as IList<T> ?? new List<T>(thisValue);
			if (values.Count < 2) return new[] { values };

			List<IList<T>> results = new List<IList<T>>();
			PermutationsLocal(values, 0, results);
			return results;

			static void PermutationsLocal(IList<T> list, int index, List<IList<T>> permutations)
			{
				if (index == list.Count - 1)
				{
					T[] permutation = new T[list.Count];
					list.CopyTo(permutation, 0);
					permutations.Add(permutation);
					return;
				}

				for (int i = index; i < list.Count; i++)
				{
					list.FastSwap(index, i);
					PermutationsLocal(list, index + 1, permutations);
					list.FastSwap(index, i);
				}
			}
		}

		/// <summary>
		/// Check if the current sequence is a permutation. The sequence needs to be sorted.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <returns></returns>
		public static bool IsPermutation<T>([NotNull] this IEnumerable<T> thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			Lister<T> values = new Lister<T>(thisValue);
			HashSet<T> set = new HashSet<T>();
			T expected = values[0], prev = expected;
			
			foreach (T value in values)
			{
				if (!set.Add(value)) continue;
				if (prev.CompareTo(value) > 0) return false;
				prev = value;
				if (expected.CompareTo(value) < 1) expected = value;
			}

			return set.Count == values.Count && expected.CompareTo(values[values.Count - 1]) == 0;
		}

		[NotNull]
		[ItemNotNull]
		public static IEnumerable<IEnumerable<T>> Subsets<T>([NotNull] this IEnumerable<T> thisValue)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// https://leetcode.com/problems/subsets-ii/
			// https://leetcode.com/problems/permutations/discuss/18239/a-general-approach-to-backtracking-questions-in-java-subsets-permutations-combination-sum-palindrome-partioning
			List<T> values = thisValue as List<T> ?? new List<T>(thisValue);
			if (values.Count < 2) return new[] { values };

			List<IList<T>> results = new List<IList<T>>();
			values.Sort();
			Backtrack(values, results, new List<T>(), 0);
			return results;

			static void Backtrack(IReadOnlyList<T> list, ICollection<IList<T>> results, ICollection<T> tmp, int start)
			{
				results.Add(new List<T>(tmp));

				for (int i = start; i < list.Count; i++)
				{
					tmp.Add(list[i]);
					Backtrack(list, results, tmp, i + 1);
					tmp.Remove(list[i]);
			
					while (i < list.Count - 1 && list[i].Equals(list[i + 1]))
						i++;
				}
			}
		}

		[NotNull]
		[ItemNotNull]
		public static IEnumerable<IEnumerable<T>> CombinationSum<T>([NotNull] this IEnumerable<T> thisValue, T target)
			where T : struct, IComparable, IComparable<T>, IEquatable<T>, IConvertible, IFormattable
		{
			// https://leetcode.com/problems/combination-sum/
			// https://leetcode.com/problems/permutations/discuss/18239/a-general-approach-to-backtracking-questions-in-java-subsets-permutations-combination-sum-palindrome-partioning
			List<T> values = thisValue as List<T> ?? new List<T>(thisValue);
			if (values.Count < 2) return new[] { values };

			T defaultValue = default(T);
			List<IList<T>> results = new List<IList<T>>();
			values.Sort();
			Backtrack(values, results, new List<T>(), target, defaultValue, 0);
			return results;

			static void Backtrack(IReadOnlyList<T> list, ICollection<IList<T>> results, IList<T> tmp, T remain, T def, int start)
			{
				int cmp = remain.CompareTo(def);

				if (cmp < 0) return;

				if (cmp == 0)
				{
					results.Add(new List<T>(tmp));
				}
				else
				{
					for (int i = start; i < list.Count; i++)
					{
						tmp.Add(list[i]);
						// not i + 1 because we can reuse same elements
						Backtrack(list, results, tmp, remain.Subtract(list[i]), def, i);
						tmp.RemoveAt(tmp.Count - 1);
					}
				}
			}
		}

		public static IEnumerable<TSource> Distinct<TSource, TKey>([NotNull] this IEnumerable<TSource> thisValue, [NotNull] Func<TSource, TKey> selector)
		{
			ISet<TKey> existing = new HashSet<TKey>();

			foreach (TSource element in thisValue)
			{
				if (existing.Add(selector(element)))
					yield return element;
			}
		}

		public static IEnumerable Flatten([NotNull] this IEnumerable thisValue, [NotNull] Func<object, IEnumerable> getChildren, Predicate<object> selector = null)
		{
			bool useSelector = selector != null;
			Stack<IEnumerator> stack = new Stack<IEnumerator>();
			stack.Push(thisValue.GetEnumerator());

			while (stack.Count > 0)
			{
				IEnumerator e = stack.Pop();

				while (e.MoveNext())
				{
					object current = e.Current;
					if (useSelector && !selector(current)) continue;
					yield return current;

					IEnumerable children = getChildren(current);
					if (children == null) continue;
					stack.Push(e);
					e = children.GetEnumerator();
				}
			}
		}

		public static IEnumerable<T> Flatten<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, IEnumerable<T>> getChildren, Predicate<T> selector = null)
		{
			bool useSelector = selector != null;
			IEnumerator<T> e = null;
			Stack<IEnumerator<T>> stack = new Stack<IEnumerator<T>>();
			stack.Push(thisValue.GetEnumerator());

			try
			{
				while (stack.Count > 0)
				{
					e = stack.Pop();

					while (e.MoveNext())
					{
						T current = e.Current;
						if (useSelector && !selector(current)) continue;
						yield return current;

						IEnumerable<T> children = getChildren(current);
						if (children == null) continue;
						stack.Push(e);
						e = children.GetEnumerator();
					}

					ObjectHelper.Dispose(ref e);
				}
			}
			finally
			{
				ObjectHelper.Dispose(ref e);

				while (stack.Count > 0)
				{
					IEnumerator<T> item = stack.Pop();
					ObjectHelper.Dispose(ref item);
				}
			}
		}

		public static bool AnyChild<T>([NotNull] this T thisValue, [NotNull] Predicate<T> selector, [NotNull] Func<T, IEnumerable<T>> getChildren)
		{
			IEnumerable<T> topChildren = getChildren.Invoke(thisValue);
			if (topChildren == null) return false;

			Stack<IEnumerator<T>> stack = new Stack<IEnumerator<T>>();
			IEnumerator<T> e = topChildren.GetEnumerator();

			try
			{
				while (true)
				{
					while (e.MoveNext())
					{
						T current = e.Current;
						if (selector(current)) return true;

						IEnumerable<T> children = getChildren.Invoke(current);
						if (children == null) continue;
						stack.Push(e);
						e = children.GetEnumerator();
					}

					if (stack.Count == 0) break;
					ObjectHelper.Dispose(ref e);
					e = stack.Pop();
				}
			}
			finally
			{
				ObjectHelper.Dispose(ref e);

				while (stack.Count > 0)
				{
					IEnumerator<T> item = stack.Pop();
					ObjectHelper.Dispose(ref item);
				}
			}

			return false;
		}

		public static IEnumerable Traverse([NotNull] this IEnumerable thisValue, [NotNull] Func<object, IEnumerable> getChildren, DequeuePriority dequeuePriority = DequeuePriority.FIFO)
		{
			switch (dequeuePriority)
			{
				case DequeuePriority.FIFO:
					Queue<IEnumerable> queue = new Queue<IEnumerable>();

					queue.Enqueue(thisValue);

					while (queue.Count > 0)
					{
						IEnumerable enumerable = queue.Dequeue();

						foreach (object child in enumerable)
						{
							yield return child;
							IEnumerable children = getChildren(child);
							if (children != null) queue.Enqueue(children);
						}
					}
					break;
				case DequeuePriority.LIFO:
					Stack<IEnumerable> stack = new Stack<IEnumerable>();

					stack.Push(thisValue);

					while (stack.Count > 0)
					{
						IEnumerable enumerable = stack.Pop();

						foreach (object child in enumerable)
						{
							yield return child;
							IEnumerable children = getChildren(child);
							if (children != null) stack.Push(children);
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(dequeuePriority), dequeuePriority, null);
			}
		}

		public static IEnumerable<T> Traverse<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, IEnumerable<T>> getChildren, DequeuePriority dequeuePriority = DequeuePriority.FIFO)
		{
			switch (dequeuePriority)
			{
				case DequeuePriority.FIFO:
					Queue<IEnumerable<T>> queue = new Queue<IEnumerable<T>>();
					queue.Enqueue(thisValue);

					while (queue.Count > 0)
					{
						IEnumerable<T> enumerable = queue.Dequeue();

						foreach (T child in enumerable)
						{
							yield return child;
							IEnumerable<T> children = getChildren(child);
							if (children != null) queue.Enqueue(children);
						}
					}
					break;
				case DequeuePriority.LIFO:
					Stack<IEnumerable<T>> stack = new Stack<IEnumerable<T>>();
					stack.Push(thisValue);

					while (stack.Count > 0)
					{
						IEnumerable<T> enumerable = stack.Pop();

						foreach (T child in enumerable)
						{
							yield return child;
							IEnumerable<T> children = getChildren(child);
							if (children != null) stack.Push(children);
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(dequeuePriority), dequeuePriority, null);
			}
		}

		[NotNull]
		public static ISet<T> AsHashSet<T>([NotNull] this IEnumerable<T> thisValue, IEqualityComparer<T> comparer = null)
		{
			return thisValue as HashSet<T> ?? new HashSet<T>(thisValue, comparer ?? EqualityComparer<T>.Default);
		}

		[NotNull]
		public static IReadOnlySet<T> AsReadOnlySet<T>([NotNull] this IEnumerable<T> thisValue, IEqualityComparer<T> comparer = null)
		{
			return thisValue as IReadOnlySet<T> ?? AsHashSet(thisValue, comparer).AsReadOnly();
		}

		[ItemNotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> SkipNull<T>([NotNull] this IEnumerable<T> thisValue)
		{
			foreach (T item in thisValue.Where(e => !e.IsNull()))
				yield return item;
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<string> SkipNullOrEmpty([NotNull] this IEnumerable<string> thisValue) { return thisValue.Where(e => !string.IsNullOrEmpty(e)); }

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<string> SkipNullOrWhitespace([NotNull] this IEnumerable<string> thisValue)
		{
			return thisValue.Where(e => !string.IsNullOrWhiteSpace(e));
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<string> SkipNullOrEmptyTrim([NotNull] this IEnumerable<string> thisValue)
		{
			return thisValue.Where(e => !string.IsNullOrWhiteSpace(e))
							.Select(item => item.Trim());
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T FirstNotNullOrDefault<T>([NotNull] this IEnumerable<T> thisValue) { return thisValue.FirstOrDefault(e => !e.IsNull()); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T LastNotNullOrDefault<T>([NotNull] this IEnumerable<T> thisValue) { return thisValue.LastOrDefault(e => !e.IsNull()); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T FirstNotNull<T>([NotNull] this IEnumerable<T> thisValue) { return thisValue.First(e => !e.IsNull()); }
		
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T LastNotNull<T>([NotNull] this IEnumerable<T> thisValue) { return thisValue.Last(e => !e.IsNull()); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T SingleNotNullOrDefault<T>([NotNull] this IEnumerable<T> thisValue) { return thisValue.SingleOrDefault(e => !e.IsNull()); }

		public static (T First, T Last) FirstAndLast<T>([NotNull] this IEnumerable<T> thisValue)
		{
			if (thisValue is IList<T> list)
			{
				if (list.Count > 0) return (list[0], list[list.Count - 1]);
			}
			else
			{
				using (IEnumerator<T> enumerator = thisValue.GetEnumerator())
				{
					T first = default(T), last = default(T);

					if (enumerator.MoveNext())
						first = last = enumerator.Current;

					while (enumerator.MoveNext())
						last = enumerator.Current;

					return (first, last);
				}
			}

			throw new InvalidOperationException("Enumerable has no elements.");
		}

		public static (T First, T Last) FirstAndLastOrDefault<T>([NotNull] this IEnumerable<T> thisValue)
		{
			if (thisValue is IList<T> list)
			{
				if (list.Count > 0) return (list[0], list[list.Count - 1]);
			}
			else
			{
				using (IEnumerator<T> enumerator = thisValue.GetEnumerator())
				{
					T first = default(T), last = default(T);

					if (enumerator.MoveNext())
						first = last = enumerator.Current;

					while (enumerator.MoveNext())
						last = enumerator.Current;

					return (first, last);
				}
			}

			return (default(T), default(T));
		}

		public static T RearElementAt<T>([NotNull] this IEnumerable<T> thisValue, int offset)
		{
			if (offset < 1) throw new ArgumentOutOfRangeException(nameof(offset));

			try
			{
				return TakeLast(thisValue, offset, true).First();
			}
			catch (ArgumentOutOfRangeException)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}
			catch (InvalidOperationException)
			{
				throw new ArgumentOutOfRangeException(nameof(offset));
			}
		}

		public static IEnumerable<T> TakeLast<T>([NotNull] this IEnumerable<T> thisValue, int count, bool exact = false)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
			if (count == 0) yield break;
	
			switch (thisValue)
			{
				case IList<T> list:
				{
					if (exact && count > list.Count) throw new ArgumentOutOfRangeException(nameof(count));

					int listCount = list.Count;

					for (int idx = listCount - Math.Min(count, listCount); idx < listCount; idx++)
						yield return list[idx];

					break;
				}
				case IReadOnlyList<T> readOnlyList:
				{
					if (exact && count > readOnlyList.Count) throw new ArgumentOutOfRangeException(nameof(count));

					int listCount = readOnlyList.Count;

					for (int idx = listCount - Math.Min(count, listCount); idx < listCount; idx++)
						yield return readOnlyList[idx];

					break;
				}
				default:
				{
					using (IEnumerator<T> enumerator = thisValue.GetEnumerator())
					{
						int numOfItems = 0;
						T[] buffer = new T[count];

						while (numOfItems < count && enumerator.MoveNext()) 
							buffer[numOfItems++] = enumerator.Current;

						// if numOfItems < count, it means we got all what we possibly could
						if (numOfItems < count)
						{
							if (exact) throw new ArgumentOutOfRangeException(nameof(count));

							for (int i = 0; i < numOfItems; i++)
								yield return buffer[i];

							yield break;
						}

						// if we got here, then we'll keep going and replace the element circularly using the modulus operator
						int index;
				
						for (index = 0; enumerator.MoveNext(); index = (index + 1) % count) 
							buffer[index] = enumerator.Current;

						/*
						 * from here, we have the latest value of the index.
						 * For example:
						 * if thisValue = {, 2, 3, ..., 10} and count = 3 then
						 * buffer = [9, 7, 8]
						 * index = 1
						 * numOfItems = 3
						 *
						 * with the next loop
						 * buffer[index] = 7
						 *
						 * moving on: index = (index + 1) % count
						 * index = (1 + 1) % 3 = 2
						 * buffer[index] = 8
						 *
						 * next: index = (index + 1) % count
						 * index = (2 + 1) % 3 = 0
						 * buffer[index] = 9
						 *
						 * this is the circular buffer technique
						 */
						for (; numOfItems > 0; index = (index + 1) % count, numOfItems--)
							yield return buffer[index];
					}
					break;
				}
			}
		}

		public static IEnumerable<T> TakeLastWhile<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, bool> predicate)
		{
			List<T> buffer = new List<T>();

			foreach (T item in thisValue)
			{
				if (predicate(item))
					buffer.Add(item);
				else
					buffer.Clear();
			}

			foreach (T item in buffer)
			{
				yield return item;
			}
		}

		public static IEnumerable<T> TakeLastWhile<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, int, bool> predicate)
		{
			List<T> buffer = new List<T>();
			int index = 0;

			foreach (T item in thisValue)
			{
				if (predicate(item, index++))
					buffer.Add(item);
				else
					buffer.Clear();
			}

			foreach (T item in buffer)
			{
				yield return item;
			}
		}

		public static IEnumerable<T> SkipLast<T>([NotNull] this IEnumerable<T> thisValue, int count)
		{
			if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
	
			switch (thisValue)
			{
				case IList<T> list:
				{
					if (count > list.Count) yield break;

					int returnCount = list.Count - count;

					for (int i = 0; i < returnCount; i++)
					{
						yield return list[i];
					}
					break;
				}
				case IReadOnlyList<T> readOnlyList:
				{
					if (count > readOnlyList.Count) yield break;

					int returnCount = readOnlyList.Count - count;

					for (int i = 0; i < returnCount; i++)
					{
						yield return readOnlyList[i];
					}
					break;
				}
				default:
				{
					// circular buffer
					T[] buffer = new T[count];

					using (IEnumerator<T> enumerator = thisValue.GetEnumerator())
					{
						int index;

						for (index = 0; index < count && enumerator.MoveNext(); index++)
						{
							buffer[index] = enumerator.Current;
						}

						index = 0;

						while (enumerator.MoveNext())
						{
							T item = buffer[index];
							buffer[index] = enumerator.Current;
							index = (index + 1) % count;
							yield return item;
						}
					}
					break;
				}
			}
		}

		public static IEnumerable<T> SkipLastWhile<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, bool> predicate)
		{
			List<T> buffer = new List<T>();

			foreach (T item in thisValue)
			{
				if (predicate(item))
				{
					buffer.Add(item);
				}
				else
				{
					if (buffer.Count > 0)
					{
						foreach (T value in buffer)
						{
							yield return value;
						}

						buffer.Clear();
					}

					yield return item;
				}
			}

			foreach (T item in buffer)
			{
				yield return item;
			}
		}

		public static IEnumerable<T> SkipLastWhile<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] Func<T, int, bool> predicate)
		{
			List<T> buffer = new List<T>();
			int index = 0;

			foreach (T item in thisValue)
			{
				if (predicate(item, index++))
				{
					buffer.Add(item);
				}
				else
				{
					if (buffer.Count > 0)
					{
						foreach (T value in buffer)
						{
							yield return value;
						}

						buffer.Clear();
					}

					yield return item;
				}
			}

			foreach (T item in buffer)
			{
				yield return item;
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static IndexedEnumerable AsIndexed([NotNull] this IEnumerable thisValue) { return new IndexedEnumerable(thisValue); }

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		[NotNull]
		public static IndexedEnumerable<T> AsIndexed<T>([NotNull] this IEnumerable<T> thisValue) { return new IndexedEnumerable<T>(thisValue); }

		/// <summary>
		/// Groups and executes a pipeline for a single result per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult Result)> GroupWithPipeline<TElement, TKey, TResult>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult>> pipeline)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline);
		}

		/// <summary>
		/// Groups and executes a pipeline for a single result per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult Result)> GroupWithPipeline<TElement, TKey, TResult>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector, IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult>> pipeline)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult> Result)> results = new List<(TKey, IFuture<TResult>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.End();

			return results.Select(e => (e.Key, e.Result.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for two results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2)> GroupWithPipeline<TElement, TKey, TResult1, TResult2>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2);
		}

		/// <summary>
		/// Groups and executes a pipeline for two results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2)> GroupWithPipeline<TElement, TKey, TResult1, TResult2>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.End();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for three results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3);
		}

		/// <summary>
		/// Groups and executes a pipeline for three results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2, IFuture<TResult3> Result3)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);
				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer), pipeline3(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.End();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value, e.Result3.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for four results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3, pipeline4);
		}

		/// <summary>
		/// Groups and executes a pipeline for four results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2, IFuture<TResult3> Result3, IFuture<TResult4> Result4)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>, IFuture<TResult4>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer), pipeline3(producer), pipeline4(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.End();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value, e.Result3.Value, e.Result4.Value));
		}

		/// <summary>
		/// Groups and executes a pipeline for five results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4, TResult5 Result5)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4, TResult5>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			[NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult5>> pipeline5)
		{
			return GroupWithPipeline(thisValue, keySelector, EqualityComparer<TKey>.Default, pipeline1, pipeline2, pipeline3, pipeline4, pipeline5);
		}

		/// <summary>
		/// Groups and executes a pipeline for five results per group
		/// </summary>
		[NotNull]
		public static IEnumerable<(TKey Key, TResult1 Result1, TResult2 Result2, TResult3 Result3, TResult4 Result4, TResult5 Result5)> GroupWithPipeline<TElement, TKey, TResult1, TResult2, TResult3, TResult4, TResult5>([NotNull] this IEnumerable<TElement> thisValue, [NotNull] Func<TElement, TKey> keySelector,
			IEqualityComparer<TKey> comparer, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult1>> pipeline1, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult2>> pipeline2, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult3>> pipeline3, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult4>> pipeline4, [NotNull] Func<IDataProducer<TElement>, IFuture<TResult5>> pipeline5)
		{
			IDictionary<TKey, DataProducer<TElement>> keyMap = new Dictionary<TKey, DataProducer<TElement>>(comparer ?? EqualityComparer<TKey>.Default);
			IList<(TKey Key, IFuture<TResult1> Result1, IFuture<TResult2> Result2, IFuture<TResult3> Result3, IFuture<TResult4> Result4, IFuture<TResult5> Result5)> results = new List<(TKey, IFuture<TResult1>, IFuture<TResult2>, IFuture<TResult3>, IFuture<TResult4>, IFuture<TResult5>)>();

			foreach (TElement element in thisValue)
			{
				TKey key = keySelector(element);

				if (!keyMap.TryGetValue(key, out DataProducer<TElement> producer))
				{
					producer = new DataProducer<TElement>();
					keyMap[key] = producer;
					results.Add((key, pipeline1(producer), pipeline2(producer), pipeline3(producer), pipeline4(producer), pipeline5(producer)));
				}
				producer.Produce(element);
			}

			foreach (DataProducer<TElement> producer in keyMap.Values)
				producer.End();

			return results.Select(e => (e.Key, e.Result1.Value, e.Result2.Value, e.Result3.Value, e.Result4.Value, e.Result5.Value));
		}

		[NotNull]
		public static ParallelQuery<TSource> AsParallel<TSource>([NotNull] this IEnumerable<TSource> thisValue, ParallelMergeOptions mergeOptions)
		{
			return thisValue.AsParallel()
							.WithMergeOptions(mergeOptions);
		}

		[NotNull]
		public static ParallelQuery<TSource> AsForcedParallel<TSource>([NotNull] this IEnumerable<TSource> thisValue)
		{
			return thisValue.AsParallel()
							.WithExecutionMode(ParallelExecutionMode.ForceParallelism);
		}

		[NotNull]
		public static ParallelQuery<TSource> AsForcedParallel<TSource>([NotNull] this IEnumerable<TSource> thisValue, ParallelMergeOptions mergeOptions)
		{
			return thisValue.AsParallel()
							.WithMergeOptions(mergeOptions)
							.WithExecutionMode(ParallelExecutionMode.ForceParallelism);
		}

		public static IEnumerable<TResult> Zip<T1, T2, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] Func<T1, T2, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					while (e1.MoveNext() && e2.MoveNext())
						yield return resultSelector(e1.Current, e2.Current);
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] Func<T1, T2, T3, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext())
							yield return resultSelector(e1.Current, e2.Current, e3.Current);
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] Func<T1, T2, T3, T4, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext())
								yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current);
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] Func<T1, T2, T3, T4, T5, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext())
									yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current);
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] Func<T1, T2, T3, T4, T5, T6, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext())
										yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current);
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext())
											yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current);
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext())
												yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current);
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext())
													yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current);
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext())
														yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current);
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] IEnumerable<T11> values11, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													using (IEnumerator<T11> e11 = values11.GetEnumerator())
													{
														while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext() && e11.MoveNext())
															yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current);
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] IEnumerable<T11> values11, [NotNull] IEnumerable<T12> values12, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													using (IEnumerator<T11> e11 = values11.GetEnumerator())
													{
														using (IEnumerator<T12> e12 = values12.GetEnumerator())
														{
															while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext() && e11.MoveNext() && e12.MoveNext())
																yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current);
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] IEnumerable<T11> values11, [NotNull] IEnumerable<T12> values12, [NotNull] IEnumerable<T13> values13, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													using (IEnumerator<T11> e11 = values11.GetEnumerator())
													{
														using (IEnumerator<T12> e12 = values12.GetEnumerator())
														{
															using (IEnumerator<T13> e13 = values13.GetEnumerator())
															{
																while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext() && e11.MoveNext() && e12.MoveNext() && e13.MoveNext())
																	yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current);
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] IEnumerable<T11> values11, [NotNull] IEnumerable<T12> values12, [NotNull] IEnumerable<T13> values13, [NotNull] IEnumerable<T14> values14, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													using (IEnumerator<T11> e11 = values11.GetEnumerator())
													{
														using (IEnumerator<T12> e12 = values12.GetEnumerator())
														{
															using (IEnumerator<T13> e13 = values13.GetEnumerator())
															{
																using (IEnumerator<T14> e14 = values14.GetEnumerator())
																{
																	while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext() && e11.MoveNext() && e12.MoveNext() && e13.MoveNext() && e14.MoveNext())
																		yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current);
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] IEnumerable<T11> values11, [NotNull] IEnumerable<T12> values12, [NotNull] IEnumerable<T13> values13, [NotNull] IEnumerable<T14> values14, [NotNull] IEnumerable<T15> values15, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													using (IEnumerator<T11> e11 = values11.GetEnumerator())
													{
														using (IEnumerator<T12> e12 = values12.GetEnumerator())
														{
															using (IEnumerator<T13> e13 = values13.GetEnumerator())
															{
																using (IEnumerator<T14> e14 = values14.GetEnumerator())
																{
																	using (IEnumerator<T15> e15 = values15.GetEnumerator())
																	{
																		while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext() && e11.MoveNext() && e12.MoveNext() && e13.MoveNext() && e14.MoveNext() && e15.MoveNext())
																			yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current);
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		public static IEnumerable<TResult> Zip<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult>([NotNull] this IEnumerable<T1> thisValue, [NotNull] IEnumerable<T2> values2, [NotNull] IEnumerable<T3> values3, [NotNull] IEnumerable<T4> values4, [NotNull] IEnumerable<T5> values5, [NotNull] IEnumerable<T6> values6, [NotNull] IEnumerable<T7> values7, [NotNull] IEnumerable<T8> values8, [NotNull] IEnumerable<T9> values9, [NotNull] IEnumerable<T10> values10, [NotNull] IEnumerable<T11> values11, [NotNull] IEnumerable<T12> values12, [NotNull] IEnumerable<T13> values13, [NotNull] IEnumerable<T14> values14, [NotNull] IEnumerable<T15> values15, [NotNull] IEnumerable<T16> values16, [NotNull] Func<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, TResult> resultSelector)
		{
			using (IEnumerator<T1> e1 = thisValue.GetEnumerator())
			{
				using (IEnumerator<T2> e2 = values2.GetEnumerator())
				{
					using (IEnumerator<T3> e3 = values3.GetEnumerator())
					{
						using (IEnumerator<T4> e4 = values4.GetEnumerator())
						{
							using (IEnumerator<T5> e5 = values5.GetEnumerator())
							{
								using (IEnumerator<T6> e6 = values6.GetEnumerator())
								{
									using (IEnumerator<T7> e7 = values7.GetEnumerator())
									{
										using (IEnumerator<T8> e8 = values8.GetEnumerator())
										{
											using (IEnumerator<T9> e9 = values9.GetEnumerator())
											{
												using (IEnumerator<T10> e10 = values10.GetEnumerator())
												{
													using (IEnumerator<T11> e11 = values11.GetEnumerator())
													{
														using (IEnumerator<T12> e12 = values12.GetEnumerator())
														{
															using (IEnumerator<T13> e13 = values13.GetEnumerator())
															{
																using (IEnumerator<T14> e14 = values14.GetEnumerator())
																{
																	using (IEnumerator<T15> e15 = values15.GetEnumerator())
																	{
																		using (IEnumerator<T16> e16 = values16.GetEnumerator())
																		{
																			while (e1.MoveNext() && e2.MoveNext() && e3.MoveNext() && e4.MoveNext() && e5.MoveNext() && e6.MoveNext() && e7.MoveNext() && e8.MoveNext() && e9.MoveNext() && e10.MoveNext() && e11.MoveNext() && e12.MoveNext() && e13.MoveNext() && e14.MoveNext() && e15.MoveNext() && e16.MoveNext())
																				yield return resultSelector(e1.Current, e2.Current, e3.Current, e4.Current, e5.Current, e6.Current, e7.Current, e8.Current, e9.Current, e10.Current, e11.Current, e12.Current, e13.Current, e14.Current, e15.Current, e16.Current);
																		}
																	}
																}
															}
														}
													}
												}
											}
										}
									}
								}
							}
						}
					}
				}
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void Consume<T>([NotNull] this IEnumerable<T> thisValue)
		{
			foreach (T _ in thisValue)
			{
			}
		}

		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IEnumerable<T> Paginate<T>([NotNull] this IEnumerable<T> thisValue, [NotNull] IPagination settings)
		{
			if (settings.PageSize < 1) settings.PageSize = Pagination.PAGE_SIZE;
			if (settings.Page < 1) settings.Page = 1;
			int start = (settings.Page - 1) * settings.PageSize;
			return thisValue.Skip(start).Take(settings.PageSize);
		}
	}
}