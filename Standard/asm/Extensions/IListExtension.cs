using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using asm.Collections;
using JetBrains.Annotations;
using asm.Comparers;
using asm.Helpers;
using asm.Patterns.Collections;

namespace asm.Extensions
{
	public static class IListExtension
	{
		[NotNull]
		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static IReadOnlyList<T> AsReadOnly<T>([NotNull] this IList<T> thisValue)
		{
			return thisValue switch
			{
				List<T> list => list.AsReadOnly(),
				IReadOnlyList<T> readOnlyList => readOnlyList,
				_ => new ReadOnlyList<T>(thisValue)
			};
		}

		public static T GetSafe<T>([NotNull] this IList<T> thisValue, int index)
		{
			if (!index.InRangeRx(0, thisValue.Count))
				throw new ArgumentOutOfRangeException(nameof(index));
			if (!(thisValue is ICollection collection))
				return thisValue[index];

			lock (collection.SyncRoot)
			{
				return thisValue[index];
			}
		}

		public static T PickRandom<T>([NotNull] this IList<T> thisValue)
		{
			int max;
			int n;

			if (thisValue is ICollection collection)
			{
				lock (collection.SyncRoot)
				{
					max = thisValue.Count - 1;
					if (max < 0)
						throw new InvalidOperationException("List is empty.");
					n = RNGRandomHelper.Next(0, max);
					return thisValue[n];
				}
			}

			lock (thisValue)
			{
				max = thisValue.Count - 1;
				n = RNGRandomHelper.Next(0, max);
				return thisValue[n];
			}
		}

		public static T PopRandom<T>([NotNull] this IList<T> thisValue)
		{
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			int max;
			int n;
			T result;

			if (thisValue is ICollection collection)
			{
				lock (collection.SyncRoot)
				{
					max = thisValue.Count - 1;
					n = RNGRandomHelper.Next(0, max);
					result = thisValue[n];
					thisValue.RemoveAt(n);
					return result;
				}
			}

			lock (thisValue)
			{
				max = thisValue.Count - 1;
				n = RNGRandomHelper.Next(0, max);
				result = thisValue[n];
				thisValue.RemoveAt(n);
				return result;
			}
		}

		public static T PopFirst<T>([NotNull] this IList<T> thisValue)
		{
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			T result;

			if (thisValue is ICollection collection && collection.IsSynchronized)
			{
				lock (collection.SyncRoot)
				{
					result = thisValue[0];
					thisValue.RemoveAt(0);
					return result;
				}
			}

			lock (thisValue)
			{
				result = thisValue[0];
				thisValue.RemoveAt(0);
				return result;
			}
		}

		public static T PopLast<T>([NotNull] this IList<T> thisValue)
		{
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			T result;

			if (thisValue is ICollection collection && collection.IsSynchronized)
			{
				lock (collection.SyncRoot)
				{
					result = thisValue[thisValue.Count - 1];
					thisValue.RemoveAt(thisValue.Count - 1);
					return result;
				}
			}

			lock (thisValue)
			{
				result = thisValue[thisValue.Count - 1];
				thisValue.RemoveAt(thisValue.Count - 1);
				return result;
			}
		}

		public static void Initialize([NotNull] this IList thisValue, object value, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue[i] = value;
		}

		public static void Initialize<T>([NotNull] this IList<T> thisValue, T value, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue[i] = value;
		}

		public static void Initialize([NotNull] this IList thisValue, [NotNull] Func<int, object> func, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue[i] = func(i);
		}

		public static void Initialize<T>([NotNull] this IList<T> thisValue, [NotNull] Func<int, T> func, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = startIndex; i < lastPos; i++)
				thisValue[i] = func(i);
		}

		public static Array ToArray([NotNull] this IList thisValue)
		{
			MethodInfo method = thisValue.GetType().GetMethod("ToArray");
			return (Array)method?.Invoke(thisValue, null);
		}

		public static void CloneTo([NotNull] this IList thisValue, [NotNull] IList destination, int sourceIndex, int length)
		{
			CloneTo(thisValue, destination, sourceIndex, 0, length);
		}

		public static void CloneTo([NotNull] this IList thisValue, [NotNull] IList destination, int sourceIndex, int destinationIndex, int length)
		{
			if (!sourceIndex.InRangeRx(0, thisValue.Count))
				throw new ArgumentOutOfRangeException(nameof(sourceIndex));
			if (length == -1)
				length = thisValue.Count - sourceIndex;
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length));
			if (sourceIndex + length > thisValue.Count)
				length = thisValue.Count - sourceIndex;
			if (!destinationIndex.InRange(0, destination.Count))
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			if (thisValue.Count == 0)
			{
				if (destinationIndex > 0 && length <= destination.Count)
					destination.Initialize((object)null, destinationIndex, length);
				return;
			}

			int si = sourceIndex, di = destinationIndex;

			while (si < length && di < destination.Count)
			{
				ICloneable cloneable = thisValue[si] as ICloneable;
				destination[di] = cloneable?.Clone() ?? thisValue[si];
				si++;
				di++;
			}

			if (si >= length)
				return;

			for (int i = si; i < length; i++)
			{
				ICloneable cloneable = thisValue[si] as ICloneable;
				destination.Add(cloneable?.Clone() ?? thisValue[si]);
				si++;
				di++;
			}
		}

		public static void CloneTo<T>([NotNull] this IList<T> thisValue, [NotNull] IList<T> destination, int sourceIndex, int length)
		{
			CloneTo(thisValue, destination, sourceIndex, 0, length);
		}

		public static void CloneTo<T>([NotNull] this IList<T> thisValue, [NotNull] IList<T> destination, int sourceIndex, int destinationIndex, int length)
		{
			if (!sourceIndex.InRangeRx(0, thisValue.Count))
				throw new ArgumentOutOfRangeException(nameof(sourceIndex));
			if (length == -1)
				length = thisValue.Count - sourceIndex;
			if (length < 0)
				throw new ArgumentOutOfRangeException(nameof(length));
			if (sourceIndex + length > thisValue.Count)
				length = thisValue.Count - sourceIndex;
			if (!destinationIndex.InRange(0, destination.Count))
				throw new ArgumentOutOfRangeException(nameof(destinationIndex));

			if (thisValue.Count == 0)
			{
				if (destinationIndex > 0 && length <= destination.Count)
					destination.Initialize(default(T), destinationIndex, length);
				return;
			}

			int si = sourceIndex, di = destinationIndex;

			if (typeof(T) is ICloneable)
			{
				while (si < length && di < destination.Count)
				{
					destination[di] = (T)((ICloneable)thisValue[si]).Clone();
					si++;
					di++;
				}

				if (si >= length)
					return;

				for (int i = si; i < length; i++)
				{
					destination.Add((T)((ICloneable)thisValue[si]).Clone());
					si++;
					di++;
				}
			}
			else
			{
				while (si < length && di < destination.Count)
				{
					destination[di] = thisValue[si];
					si++;
					di++;
				}

				if (si >= length)
					return;

				for (int i = si; i < length; i++)
				{
					destination.Add(thisValue[si]);
					si++;
					di++;
				}
			}
		}

		[NotNull]
		public static object[] GetRange([NotNull] this IList thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return Array.Empty<object>();

			object[] range = new object[count];
			CopyTo(thisValue, startIndex, range);
			return range;
		}

		[NotNull]
		public static T[] GetRange<T>([NotNull] this IList<T> thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return Array.Empty<T>();

			T[] range = new T[count];
			CopyTo(thisValue, startIndex, range);
			return range;
		}

		public static void Add([NotNull] this IList thisValue, [NotNull] params object[] values) { AddRange(thisValue, values); }

		public static void AddRange([NotNull] this IList thisValue, [NotNull] IEnumerable values)
		{
			if (values == null)
				throw new ArgumentNullException(nameof(values));

			foreach (object value in values)
				thisValue.Add(value);
		}

		public static void RemoveAt([NotNull] this IList thisValue, [NotNull] params int[] indices)
		{
			if (thisValue.Count == 0 || indices.Length == 0)
				return;

			Array.Sort(indices);

			foreach (int i in indices.Reverse())
			{
				if (!i.InRangeRx(0, thisValue.Count))
				{
					if (i > 0)
						continue;
					break;
				}

				thisValue.RemoveAt(i);
			}
		}

		public static void RemoveAt<T>([NotNull] this IList<T> thisValue, [NotNull] params int[] indices)
		{
			if (thisValue.Count == 0 || indices.Length == 0)
				return;

			Array.Sort(indices);

			foreach (int i in indices.Reverse())
			{
				if (!i.InRangeRx(0, thisValue.Count))
				{
					if (i > 0)
						continue;
					break;
				}

				thisValue.RemoveAt(i);
			}
		}

		public static void RemoveRange([NotNull] this IList thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = lastPos - 1; i >= startIndex; i--)
				thisValue.RemoveAt(i);
		}

		public static void RemoveRange<T>([NotNull] this IList<T> thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastPos = startIndex + count;

			for (int i = lastPos - 1; i >= startIndex; i--)
				thisValue.RemoveAt(i);
		}

		public static int RemoveAll([NotNull] this IList thisValue, [NotNull] Predicate<object> match)
		{
			int index = 0, remove = 0;

			while (index < thisValue.Count)
			{
				if (!match(thisValue[index]))
				{
					index++;
					continue;
				}

				thisValue.RemoveAt(index);
				remove++;
			}

			return remove;
		}

		public static int RemoveAll<T>([NotNull] this IList<T> thisValue, [NotNull] Predicate<T> match)
		{
			int index = 0, remove = 0;

			while (index < thisValue.Count)
			{
				if (!match(thisValue[index]))
				{
					index++;
					continue;
				}

				thisValue.RemoveAt(index);
				remove++;
			}

			return remove;
		}

		public static void CopyTo([NotNull] this IList thisValue, int sourceIndex, [NotNull] object[] destination) { CopyTo(thisValue, sourceIndex, destination, 0, destination.Length); }
		public static void CopyTo([NotNull] this IList thisValue, int sourceIndex, [NotNull] object[] destination, int destinationIndex, int count)
		{
			thisValue.Count.ValidateRange(sourceIndex, ref count);
			destination.Length.ValidateRange(destinationIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastIndex = sourceIndex + count - 1;
			int lastArrayIndex = destinationIndex + count - 1;

			for (int i = sourceIndex, j = destinationIndex; i <= lastIndex && j <= lastArrayIndex; i++, j++)
				destination[j] = thisValue[i];
		}

		public static void CopyTo<T>([NotNull] this IList<T> thisValue, int sourceIndex, [NotNull] T[] destination) { CopyTo(thisValue, sourceIndex, destination, 0, destination.Length); }
		public static void CopyTo<T>([NotNull] this IList<T> thisValue, int sourceIndex, [NotNull] T[] destination, int destinationIndex, int count)
		{
			thisValue.Count.ValidateRange(sourceIndex, ref count);
			destination.Length.ValidateRange(destinationIndex, ref count);
			if (thisValue.Count == 0 || count == 0)
				return;

			int lastIndex = sourceIndex + count - 1;
			int lastArrayIndex = destinationIndex + count - 1;

			for (int i = sourceIndex, j = destinationIndex; i <= lastIndex && j <= lastArrayIndex; i++, j++)
				destination[j] = thisValue[i];
		}

		public static void Reverse([NotNull] this IList thisValue, int index, int count)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (thisValue.Count < 2 || count < 2)
				return;

			int split = count / 2;

			for (int i = index; i < split; i++)
			{
				int x = index + count - (i + 1);
				int y = index + i;
				object tmp = thisValue[x];
				thisValue[x] = thisValue[y];
				thisValue[y] = tmp;
			}
		}

		public static void Reverse<T>([NotNull] this IList<T> thisValue, int index, int count)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (thisValue.Count < 2 || count < 2)
				return;

			int split = count / 2;

			for (int i = index; i < split; i++)
			{
				int x = index + count - (i + 1);
				int y = index + i;
				T tmp = thisValue[x];
				thisValue[x] = thisValue[y];
				thisValue[y] = tmp;
			}
		}

		public static int BinarySearch<T>([NotNull] this IList<T> thisValue, T value) { return BinarySearch(thisValue, value, 0, -1, null); }
		public static int BinarySearch<T>([NotNull] this IList<T> thisValue, T value, int index) { return BinarySearch(thisValue, value, index, -1, null); }
		public static int BinarySearch<T>([NotNull] this IList<T> thisValue, T value, int index, int count) { return BinarySearch(thisValue, value, index, count, null); }
		public static int BinarySearch<T>([NotNull] this IList<T> thisValue, T value, IComparer<T> comparer) { return BinarySearch(thisValue, value, 0, -1, comparer); }
		public static int BinarySearch<T>([NotNull] this IList<T> thisValue, T value, int index, IComparer<T> comparer) { return BinarySearch(thisValue, value, index, -1, comparer); }
		public static int BinarySearch<T>([NotNull] this IList<T> thisValue, T value, int index, int count, IComparer<T> comparer)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (comparer == null)
				comparer = Comparer<T>.Default;

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

		public static void Sort<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2)
				return;
			if (comparer == null)
				comparer = Comparer<T>.Default;
			if (descending)
				comparer = comparer.Reverse();

			switch (thisValue)
			{
				case List<T> list:
					list.Sort(index, count, comparer);
					break;
				case T[] array:
					Array.Sort(array, index, count, comparer);
					break;
				case IList list when comparer is IComparer comp:
					ArrayList.Adapter(list).Sort(index, count, comp);
					break;
				default:
					for (int i = index, j = i + 1; i < count && j < count;)
					{
						T tmp = thisValue[i];

						if (comparer.Compare(tmp, thisValue[j]) > 0)
						{
							thisValue[i] = thisValue[j];
							thisValue[j] = tmp;
							i = index;
							j = i + 1;
						}
						else
						{
							i++;
							j++;
						}
					}
					break;
			}
		}
		public static void Sort<T>([NotNull] this IList<T> thisValue, [NotNull] Comparison<T> comparison, int index = 0, int count = -1, bool descending = false)
		{
			IComparer<T> comparer = Comparer<T>.Create(comparison);
			Sort(thisValue, index, count, comparer, descending);
		}

		/// <summary>
		/// Sorts the elements in the entire System.Collections.Generic.List{T} using
		/// a projection.
		/// </summary>
		/// <param name="thisValue">Data source</param>
		/// <param name="selector">The projection to use to obtain values for comparison</param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		/// <param name="comparer">The comparer to use to compare projected values (on null to use the default comparer)</param>
		/// <param name="descending">Should the list be sorted ascending or descending?</param>
		public static void Sort<T, TValue>([NotNull] this IList<T> thisValue, [NotNull] Func<T, TValue> selector, int index = 0, int count = -1, IComparer<TValue> comparer = null, bool descending = false)
		{
			IComparer<T> itemComparer = new ProjectionComparer<T, TValue>(selector, comparer ?? Comparer<TValue>.Default);
			Sort(thisValue, index, count, itemComparer, descending);
		}

		public static void Sort<T>([NotNull] this IList<T> thisValue, ListSortAlgorithm algorithm = ListSortAlgorithm.Default, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			if (comparer == null) comparer = Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();
			
			switch (algorithm)
			{
				case ListSortAlgorithm.Bubble:
					BubbleSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Selection:
					SelectionSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Insertion:
					InsertionSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Heap:
					HeapSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Merge:
					MergeSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Quick:
					QuickSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Shell:
					ShellSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Comb:
					CombSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Tim:
					TimSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Cycle:
					CycleSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Cocktail:
					CocktailSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Bitonic:
					BitonicSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Pancake:
					PancakeSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Default:
				case ListSortAlgorithm.Binary:
					BinarySort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Gnome:
					GnomeSort(thisValue, index, count, comparer);
					break;
				case ListSortAlgorithm.Brick:
					BrickSort(thisValue, index, count, comparer);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(algorithm), algorithm, null);
			}

			// todo O(?) best, worst
			static void BubbleSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.researchgate.net/publication/2434273_A_Comparative_Study_Of_Linked_List_Sorting_Algorithms
				// https://www.geeksforgeeks.org/bubble-sort/
				// a stable optimized version.
				// Best: O(n - 1), Worst: O(n ^ 2)
				int hi = n - 1;
		
				// swapped is for detecting sorted arrays
				for (int i = x, swapped = -1; (i == x || swapped > -1) && i < n - 1; i++, swapped = -1)
				{
					for (int j = x; j < hi; j++)
					{
						if (c.IsLessThanOrEqual(list[j], list[j + 1])) continue;
						list.FastSwap(j, j + 1);
						swapped = j;
					}

					if (swapped > -1) hi = swapped;
				}
			}

			static void SelectionSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/selection-sort/
				// an optimized version.
				for (int i = x; i < n; i++)
				{
					int min_key = i;

					for (int j = i + 1; j < n; j++)
					{
						if (c.IsGreaterThanOrEqual(list[j], list[min_key])) continue;
						min_key = j;
					}
			
					if (min_key == i) continue;
					list.FastSwap(min_key, i);
				}
			}

			static void InsertionSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/insertion-sort/
				for (int i = x + 1; i < n; i++)
				{
					int j = i - 1;

					while (j >= 0 && c.IsGreaterThan(list[j], list[i]))
					{
						list.FastSwap(j + 1, j--);
					}

					list[j + 1] = list[i];
				}
			}

			static void HeapSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/heap-sort/
				// Build heap (rearrange array) 
				for (int i = n / 2 - 1; i >= x; i--)
					Heap_Heapify(list, i, n, c);

				// One by one extract an element from heap 
				for (int i = n - 1; i >= x; i--)
				{
					// Move current root to end 
					list.FastSwap(x, i);
					// call max heapify on the reduced heap 
					Heap_Heapify(list, x, i, c);
				}
			}

			static void MergeSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/iterative-merge-sort/
				/* Merge sub arrays in bottom up manner. First merge sub arrays
				 * of size 1 to create sorted sub arrays of size 2, then merge
				 * sub arrays of size 2 to create sorted sub arrays of size 4,
				 * and so on.
				 */
				// i = current size
				for (int i = x + 1; i < n; i *= 2)
				{
					// Pick starting point of different sub arrays of current size
					for (int j = x; j < n - 1; j += i * 2)
					{
						// Find ending point of left sub array.
						int m = j + i - 1;
						// m + 1 is starting point of right.
						int r = Math.Min(j + 2 * i - 1, n - 1);
						// Merge sub arrays list[i...m] and arr[m + 1...r] 
						Merge_Merge(list, j, m, r, c);
					}
				}
			}

			static void QuickSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/iterative-quick-sort/
				if (n > list.Count - 1)
					n = list.Count - 1;

				// Create an auxiliary stack 
				int[] stack = new int[n - x + 1];

				// initialize top of stack 
				int top = -1;

				// push initial values of l and h to stack 
				stack[++top] = x;
				stack[++top] = n;

				// Keep popping from stack while is not empty 
				while (top >= 0)
				{
					// Pop h and l 
					n = stack[top--];
					x = stack[top--];

					// Set pivot element at its 
					// correct position in 
					// sorted array 
					int p = Quick_Partition(list, x, n, c);

					// If there are elements on 
					// left side of pivot, then 
					// push left side to stack 
					if (p - 1 > x)
					{
						stack[++top] = x;
						stack[++top] = p - 1;
					}

					if (p + 1 >= n) continue;
					// If there are elements on 
					// right side of pivot, then 
					// push right side to stack 
					stack[++top] = p + 1;
					stack[++top] = n;
				}
			}

			static void ShellSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/shellsort/
				// Start with a big gap, then reduce the gap 
				for (int gap = n / 2; gap > 0; gap /= 2)
				{
					// Do a gaped insertion sort for this gap size. 
					// The first gap elements a[0..gap-1] are already in gaped order 
					// keep adding one more element until the entire array is gap sorted  
					for (int i = gap; i < n; i += 1)
					{
						// add a[i] to the elements that have been gap sorted 
						// save a[i] in temp and make a hole at position i 
						T temp = list[x + i];

						// shift earlier gap-sorted elements up until the correct  
						// location for a[i] is found 
						int j;

						for (j = i; j >= gap && c.IsGreaterThan(list[x + j - gap], temp); j -= gap)
							list[x + j] = list[x + j - gap];

						// put temp (the original a[i]) in its correct location 
						list[x + j] = temp;
					}
				}
			}

			static void CombSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/comb-sort/
				// Initialize gap 
				int gap = n;
				// Initialize swapped as true to make sure the loop runs 
				bool swapped = true;

				// Keep running while gap is more than 1 and last 
				// iteration caused a swap 
				while (gap != 1 || swapped)
				{
					// Find next gap 
					gap = Comb_GetNextGap(gap);
					// Initialize swapped as false so that we can check if swap happened
					swapped = false;

					// Compare all elements with current gap 
					for (int i = x; i < n - gap; i++)
					{
						if (c.IsLessThanOrEqual(list[i], list[i + gap])) continue;
						list.FastSwap(i, i + gap);
						swapped = true;
					}
				}
			}

			static void TimSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/timsort/
				/*
				* This algorithm is based on InsertionSort and MergeSort.
				* It will sort individual sub arrays of size TIM using InsertionSort,
				* then will use MergeSort to form size 64, then 128, 256 and so on.
				*/
				const int TIM = 32;

				// Sort individual sub arrays of size TIM  
				for (int i = x; i < n; i += TIM)
					InsertionSort(list, i, Math.Min(i + TIM, n), c);

				// start merging from size TIM, and multiplications
				for (int i = TIM; i < n; i *= 2)
				{
					/*
					* Pick starting point of left sub array.
					* Merging list[left..left + size] and list[left + size, left + 2 * size]
					*/
					for (int j = x; j < n; j++)
					{
						// find ending point of left sub array  
						// mid+1 is starting point of right sub array  
						int mid = i + j - 1;
						int right = Math.Min(i + 2 * j, n);

						// merge sub array arr[left.....mid] &  
						// arr[mid+1....right]  
						Merge_Merge(list, i, mid, right - 1, c);
					}
				}
			}

			static void CycleSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/cycle-sort/
				// traverse array elements
				for (int i = x; i < n - 2; i++)
				{
					// Find position where we put the item.  
					// We basically count all smaller elements  
					// on right side of item. 
					int pos = i;

					for (int j = i + 1; j < n; j++)
					{
						if (c.IsGreaterThanOrEqual(list[j], list[i]))
							continue;
						pos++;
					}

					// If item is already in correct position 
					if (pos == i)
						continue;

					// ignore all duplicate elements 
					while (c.IsEqual(list[i], list[pos]))
						pos++;

					// put the item to it's right position 
					if (pos != i) list.FastSwap(i, pos);

					// Rotate rest of the cycle 
					while (pos != i)
					{
						pos = i;

						// Find position where we put the element 
						for (int j = i + 1; j < n; j++)
						{
							if (c.IsGreaterThanOrEqual(list[j], list[i])) continue;
							pos++;
						}

						// ignore all duplicate elements 
						while (c.IsEqual(list[i], list[pos]))
							pos++;

						if (c.IsEqual(list[i], list[pos])) continue;
						// put the item to it's right position 
						list.FastSwap(i, pos);
					}
				}
			}

			static void CocktailSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/cocktail-sort/
				bool swapped;

				do
				{
					// reset the swapped flag
					swapped = false;

					for (int i = x; i < n - 1; ++i)
					{
						if (c.IsLessThanOrEqual(list[i], list[i + 1])) continue;
						list.FastSwap(i, i + 1);
						swapped = true;
					}

					// if nothing moved, then array is sorted.
					if (!swapped) break;
					// otherwise, reset the swapped flag
					swapped = false;
					// move the end point back by one,
					n--;

					// same comparison but backwards
					for (int i = n - 1; i >= x; i--)
					{
						if (c.IsLessThanOrEqual(list[i], list[i + 1])) continue;
						list.FastSwap(i, i + 1);
						swapped = true;
					}

					// increase the starting point
					x++;
				}
				while (swapped);
			}

			static void BitonicSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/bitonic-sort/
				if (n - x < 2) return;

				int k = n / 2;

				// sort in given order
				BitonicSort(list, x, k, c);

				// sort in reverse order
				BitonicSort(list, x + k, k, c.Reverse());

				// Will merge whole sequence in ascending order  
				Bitonic_Merge(list, x, n, c);
			}

			static void PancakeSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/pancake-sorting/
				for (int i = n; i > x; i--)
				{
					int mi = 0;

					for (int j = x; j < i; j++)
					{
						if (c.IsGreaterThan(list[j], list[mi]))
							mi = j;
					}

					if (mi == i - 1) continue;

					if (mi > 0)
					{
						mi++;

						for (int j = x; j < --mi; j++)
						{
							list.FastSwap(j, mi);
						}
					}

					// flip
					int m = i;

					for (int j = x; j < --m; j++)
					{
						list.FastSwap(j, m);
					}
				}
			}

			static void BinarySort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://stackoverflow.com/questions/26454911/how-does-binary-insertion-sort-work
				for (int i = x + 1; i < n; i++)
				{
					T item = list[i];

					// Find location to insert using binary search
					int lo = x;
					int hi = i - 1;

					// this works a little different than the normal binary search
					// It's the same basic general idea but the start and end values are different
					// So we can't use Array.BinarySearch or IListExtension.BinarySearch
					while (lo <= hi)
					{
						// Same as (l + r) / 2, but avoids overflow
						int mid = lo + (hi - lo) / 2;

						if (c.IsLessThan(item, list[mid]))
							hi = mid - 1;
						else
							lo = mid + 1;
					}

					// Shifting array to one location right
					for (int j = i - 1; j >= lo; j--)
					{
						list[j + 1] = list[j];
					}

					// Placing element at its correct location 
					list[lo] = item;
				}
			}

			static void GnomeSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/gnome-sort-a-stupid-one/
				int i = x + 1;

				while (i < n)
				{
					if (i <= x || c.IsGreaterThanOrEqual(list[i], list[i - 1]))
					{
						i++;
						continue;
					}

					list.FastSwap(i, --i);
				}
			}

			static void BrickSort(IList<T> list, int x, int n, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/odd-even-sort-brick-sort/
				bool isSorted = false;

				while (!isSorted)
				{
					isSorted = true;

					// Perform Bubble sort on odd indexed element 
					for (int i = x + 1; i <= n - 2; i += 2) 
					{ 
						if (c.IsLessThanOrEqual(list[i], list[i + 1])) continue;
						list.FastSwap(i, i + 1);
						isSorted = false;
					} 
  
					// Perform Bubble sort on even indexed element 
					for (int i = x; i <= n - 2; i += 2) 
					{ 
						if (c.IsLessThanOrEqual(list[i], list[i + 1])) continue;
						list.FastSwap(i, i + 1);
						isSorted = false;
					} 
				}
			}

			// Helper functions
			static void Heap_Heapify(IList<T> list, int x, int n, IComparer<T> c)
			{
				int largest = x; // Initialize largest as root 
				int l = 2 * x + 1; // left = 2 * x + 1 
				int r = 2 * x + 2; // right = 2 * x + 2 

				// If left child is larger than root 
				if (l < n && c.IsGreaterThan(list[l], list[largest]))
					largest = l;
				// If right child is larger than largest so far 
				if (r < n && c.IsGreaterThan(list[r], list[largest]))
					largest = r;
				if (largest == x)
					return;

				// If largest is not root 
				list.FastSwap(x, largest);
				// Recursively heapify the affected sub-tree 
				Heap_Heapify(list, largest, n, c);
			}

			static int Quick_Partition(IList<T> list, int lo, int hi, IComparer<T> c)
			{
				T pivot = list[hi];
				// index of smaller element 
				int i = lo - 1;

				for (int j = lo; j < hi; j++)
				{
					if (c.IsGreaterThanOrEqual(list[j], pivot))
						continue;
					// If current element is smaller than the pivot 
					i++;
					// swap list[i] and list[j] 
					list.FastSwap(i, j);
				}

				// swap list[i + 1] and list[hi] (or pivot) 
				list.FastSwap(i + 1, hi);
				return i + 1;
			}

			static void Merge_Merge(IList<T> list, int l, int m, int r, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/in-place-merge-sort/
				/*
				 * l = start of the first range, l2 is the 2nd pointer to maintain the start
				 * of the 2nd range (array in the original implementation).
				 */
				int l2 = m + 1;
				// if the direct merge is already sorted
				if (c.IsLessThanOrEqual(list[m], list[l2])) return;

				while (l <= m && l2 <= r)
				{
					if (c.IsLessThanOrEqual(list[l], list[l2]))
					{
						l++;
						continue;
					}

					T value = list[l2];

					// Shift all the elements between element 1 and element 2 to the right by 1.
					for (int i = l2; i >= l; --i)
					{
						list[i] = list[i - 1];
					}

					list[l] = value;

					// Update all the pointers
					l++;
					l2++;
					m++;
				}
			}

			static int Comb_GetNextGap(int gap)
			{
				// Shrink gap by Shrink factor 
				gap *= 10 / 13;
				return gap < 1
							? 1
							: gap;
			}

			static void Bitonic_Merge(IList<T> list, int x, int n, IComparer<T> c)
			{
				if (n - x < 2) return;

				int k = n / 2;

				for (int i = x; i < x + k; i++)
				{
					if (c.IsGreaterThan(list[i], list[i + k])) continue;
					list.FastSwap(i, i + k);
				}

				Bitonic_Merge(list, x, k, c);
				Bitonic_Merge(list, x + k, k, c);
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void Swap<T>([NotNull] this IList<T> thisValue, int index1, int index2)
		{
			if (!index1.InRangeRx(0, thisValue.Count)) throw new ArgumentOutOfRangeException(nameof(index1));
			if (!index2.InRangeRx(0, thisValue.Count)) throw new ArgumentOutOfRangeException(nameof(index2));
			if (index1 == index2) return;

			T tmp = thisValue[index1];
			thisValue[index1] = thisValue[index2];
			thisValue[index2] = tmp;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void FastSwap<T>([NotNull] this IList<T> thisValue, int index1, int index2)
		{
			if (index1 == index2) return;
			T tmp = thisValue[index1];
			thisValue[index1] = thisValue[index2];
			thisValue[index2] = tmp;
		}
	}
}