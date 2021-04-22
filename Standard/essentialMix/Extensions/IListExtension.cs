using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using essentialMix.Collections;
using JetBrains.Annotations;
using essentialMix.Helpers;
using Other.JonSkeet.MiscUtil.Collections;
using essentialMixMath = essentialMix.Numeric.Math;

namespace essentialMix.Extensions
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

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static T SyncGet<T>([NotNull] this IList<T> thisValue, int index)
		{
			if (!index.InRangeRx(0, thisValue.Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (thisValue is not ICollection collection) return thisValue[index];

			lock (collection.SyncRoot)
			{
				return thisValue[index];
			}
		}

		public static T PickRandom<T>([NotNull] this IList<T> thisValue, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			int max;
			int n;

			if (thisValue is ICollection collection)
			{
				lock (collection.SyncRoot)
				{
					max = count - 1;
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

		public static T PopRandom<T>([NotNull] this IList<T> thisValue, int startIndex = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0) throw new InvalidOperationException("List is empty.");

			int max;
			int n;
			T result;

			if (thisValue is ICollection collection)
			{
				lock (collection.SyncRoot)
				{
					max = count - 1;
					n = RNGRandomHelper.Next(startIndex, max);
					result = thisValue[n];
					thisValue.RemoveAt(n);
					return result;
				}
			}

			lock (thisValue)
			{
				max = count - 1;
				n = RNGRandomHelper.Next(startIndex, max);
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

		[NotNull]
		public static object[] GetRange([NotNull] this IList thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0) return Array.Empty<object>();

			int x = startIndex;
			object[] range = new object[count];

			for (int i = startIndex; i < count; i++) 
				range[x++] = thisValue[i];

			return range;
		}

		[NotNull]
		public static T[] GetRange<T>([NotNull] this IList<T> thisValue, int startIndex, int count)
		{
			thisValue.Count.ValidateRange(startIndex, ref count);
			if (thisValue.Count == 0 || count == 0) return Array.Empty<T>();

			int x = startIndex;
			T[] range = new T[count];

			for (int i = startIndex; i < count; i++) 
				range[x++] = thisValue[i];

			return range;
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

		public static void Sort<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

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
		/// Sorts the elements in the entire System.Collections.Generic.List{T} using a projection.
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

		#region Sort Algorithms
		public static void SortBubble<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// Udemy - Data Structures and Algorithms Deep Dive Using Java
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			for (int lastUnsorted = count - 1; lastUnsorted > index; lastUnsorted--)
			{
				for (int i = index; i < lastUnsorted; i++)
				{
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[i + 1])) continue;
					thisValue.FastSwap(i, i + 1);
				}
			}
		}

		public static void SortSelection<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// Udemy - Data Structures and Algorithms Deep Dive Using Java
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			for (int lastUnsorted = count - 1; lastUnsorted > index; lastUnsorted--)
			{
				int largest = index;

				for (int i = index + 1; i <= lastUnsorted; i++)
				{
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[largest])) continue;
					largest = i;
				}

				thisValue.FastSwap(largest, lastUnsorted);
			}
		}

		public static void SortInsertion<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// Udemy - Data Structures and Algorithms Deep Dive Using Java
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			for (int firstUnsorted = index + 1; firstUnsorted < count; firstUnsorted++)
			{
				T value = thisValue[firstUnsorted];
				int i;

				for (i = firstUnsorted; i > 0 && comparer.IsGreaterThan(thisValue[i - 1], value); i--)
				{
					thisValue.FastSwap(i, i - 1);
				}

				thisValue[i] = value;
			}
		}

		public static void SortShell<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/shellsort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// Start with a big gap, then reduce the gap 
			for (int gap = count / 2; gap > 0; gap /= 2)
			{
				// Do a gaped insertion sort for this gap size. 
				// The first gap elements a[0..gap-1] are already in gaped order 
				// keep adding one more element until the entire array is gap sorted  
				for (int i = gap; i < count; i++)
				{
					// add a[i] to the elements that have been gap sorted 
					// save a[i] in temp and make a hole at position i 
					T value = thisValue[index + i];

					// shift earlier gap-sorted elements up until the correct  
					// location for a[i] is found 
					int j;

					for (j = i; j >= gap && comparer.IsGreaterThan(thisValue[index + j - gap], value); j -= gap)
						thisValue[index + j] = thisValue[index + j - gap];

					// put temp (the original a[i]) in its correct location 
					thisValue[index + j] = value;
				}
			}
		}

		public static void SortMerge<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/in-place-merge-sort/
			// https://www.geeksforgeeks.org/iterative-merge-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			/* Merge sub arrays in bottom up manner. First merge sub arrays
			* of size 1 to create sorted sub arrays of size 2, then merge
			* sub arrays of size 2 to create sorted sub arrays of size 4,
			* and so on.
			*/
			// i = current size
			for (int size = index + 1; size < count; size *= 2)
			{
				// Pick starting point of different sub arrays of current size
				for (int left = index; left < count - 1; left += size * 2)
				{
					// Find ending point of left sub array.
					int mid = Math.Min(left + size - 1, count - 1);
					// mid + 1 is starting point of right.
					int right = Math.Min(left + 2 * size - 1, count - 1);
					// Merge sub arrays thisValue[i...m] and arr[m + 1...r] 
					Merge(thisValue, left, mid, right, comparer);
				}
			}

			static void Merge(IList<T> list, int l, int m, int r, IComparer<T> c)
			{
				// start of the second range
				int l2 = Math.Min(m + 1, r);
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
					for (int i = l2; i > l; i--)
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
		}

		public static void SortHeap<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/iterative-heap-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// Build heap (rearrange array) starting from the last parent
			for (int i = index + 1; i < count; i++)
			{
				// swap child and parent until parent is smaller
				for (int child = i, parent = (child - 1) / 2;
					parent >= index && comparer.IsGreaterThan(thisValue[child], thisValue[parent]);
					child = parent, parent = (child - 1) / 2)
				{
					thisValue.FastSwap(child, parent);
				}
			}

			for (int i = count - 1; i > index; i--)
			{
				// swap value of first indexed with last indexed
				thisValue.FastSwap(index, i);
				
				// maintaining heap property after each swapping
				for (int parent = 0, child = parent * 2 + 1; child < i; parent = child, child = parent * 2 + 1)
				{
					// if left is smaller than right point index to right
					if (child < i - 1 && comparer.IsLessThan(thisValue[child], thisValue[child + 1])) child++;
					// if parent is smaller than child, swap them
					if (child < i && comparer.IsLessThan(thisValue[parent], thisValue[child])) thisValue.FastSwap(parent, child);
				}
			}
		}

		public static void SortQuick<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// Fastest algorithm
			// https://www.geeksforgeeks.org/iterative-quick-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			if (count > thisValue.Count - 1) count = thisValue.Count - 1;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// Create an auxiliary stack 
			int[] stack = new int[count - index + 1];

			// initialize top of stack 
			int top = -1;

			// push initial values of l and h to stack 
			stack[++top] = index;
			stack[++top] = count;

			// Keep popping from stack while is not empty 
			while (top >= 0)
			{
				// Pop h and l 
				count = stack[top--];
				index = stack[top--];

				// Set pivot element at its 
				// correct position in 
				// sorted array 
				int p = Partition(thisValue, index, count, comparer);

				// If there are elements on 
				// left side of pivot, then 
				// push left side to stack 
				if (p - 1 > index)
				{
					stack[++top] = index;
					stack[++top] = p - 1;
				}

				if (p + 1 >= count) continue;
				// If there are elements on 
				// right side of pivot, then 
				// push right side to stack 
				stack[++top] = p + 1;
				stack[++top] = count;
			}

			static int Partition(IList<T> list, int lo, int hi, IComparer<T> c)
			{
				T pivot = list[hi];
				// index of smaller element 
				int i = lo - 1;

				for (int j = lo; j < hi; j++)
				{
					if (c.IsGreaterThanOrEqual(list[j], pivot)) continue;
					// If current element is smaller than the pivot 
					i++;
					// swap thisValue[i] and thisValue[j] 
					list.FastSwap(i, j);
				}

				// swap thisValue[i + 1] and thisValue[hi] (or pivot) 
				list.FastSwap(i + 1, hi);
				return i + 1;
			}
		}

		public static void SortComb<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/comb-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();
			// Initialize gap 
			int gap = count;
			// Initialize swapped as true to make sure the loop runs 
			bool swapped = true;

			// Keep running while gap is more than 1 and last 
			// iteration caused a swap 
			while (gap != 1 || swapped)
			{
				// Find next gap 
				gap = GetNextGap(gap);
				// Initialize swapped as false so that we can check if swap happened
				swapped = false;

				// Compare all elements with current gap 
				for (int i = index; i < count - gap; i++)
				{
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[i + gap])) continue;
					thisValue.FastSwap(i, i + gap);
					swapped = true;
				}
			}

			static int GetNextGap(int gap)
			{
				// Shrink gap by Shrink factor 
				gap = gap * 10 / 13;
				return gap < 1
							? 1
							: gap;
			}
		}

		public static void SortTim<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			const int TIM = 32;

			// https://www.geeksforgeeks.org/timsort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			/*
			* This algorithm is based on InsertionSort and MergeSort.
			* It will sort individual sub arrays of size TIM using InsertionSort,
			* then will use MergeSort to form size 64, then 128, 256 and so on.
			*/
			// Sort individual sub arrays of size TIM  
			for (int i = index; i < count; i += TIM)
				SortInsertion(thisValue, i, Math.Min(i + TIM, count), comparer);

			// start merging from size TIM, and multiplications
			for (int i = TIM; i < count; i *= 2)
			{
				/*
				* Pick starting point of left sub array.
				* Merging thisValue[left..left + size] and thisValue[left + size, left + 2 * size]
				*/
				for (int j = index; j < count; j++)
				{
					// find ending point of left sub array  
					// mid+1 is starting point of right sub array  
					int mid = i + j - 1;
					int right = Math.Min(i + 2 * j, count);

					// merge sub array arr[left.....mid] &  
					// arr[mid+1....right]  
					Merge(thisValue, i, mid, right - 1, comparer);
				}
			}

			static void Merge(IList<T> list, int l, int m, int r, IComparer<T> c)
			{
				// https://www.geeksforgeeks.org/in-place-merge-sort/
				/*
				* l = start of the first range, l2 is the 2nd pointer to maintain the start
				* of the 2nd range (array in the original implementation).
				*/
				int l2 = m + 1;
				if (l2 > r) return;

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
		}

		public static void SortCocktail<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/cocktail-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			bool swapped;

			do
			{
				// reset the swapped flag
				swapped = false;

				for (int i = index; i < count - 1; ++i)
				{
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[i + 1])) continue;
					thisValue.FastSwap(i, i + 1);
					swapped = true;
				}

				// if nothing moved, then array is sorted.
				if (!swapped) break;
				// otherwise, reset the swapped flag
				swapped = false;
				// move the end point back by one,
				count--;

				// same comparison but backwards
				for (int i = count - 1; i >= index; i--)
				{
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[i + 1])) continue;
					thisValue.FastSwap(i, i + 1);
					swapped = true;
				}

				// increase the starting point
				index++;
			}
			while (swapped);
		}

		public static void SortBitonic<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/bitonic-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			if (count - index < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			// sort in given order
			SortTheList(thisValue, index, count, comparer, descending ? -1 : 1);

			static void SortTheList(IList<T> list, int x, int n, IComparer<T> c, int dir)
			{
				if (n < 2) return;

				int mid = n / 2;

				// sort in given order
				SortTheList(list, x, mid, c, 1 /* ascending */);
				// sort in reverse order
				SortTheList(list, x + mid, mid, c, -1 /* descending */);
				// merge whole sequence in given order  
				Merge(list, x, n, c, dir);
			}

			static void Merge(IList<T> list, int x, int n, IComparer<T> c, int dir)
			{
				if (n < 2) return;

				int k = n / 2;

				for (int i = x; i < x + k; i++)
				{
					if (c.Compare(list[i], list[i + k]) != dir) continue;
					list.FastSwap(i, i + k);
				}

				Merge(list, x, k, c, dir);
				Merge(list, x + k, k, c, dir);
			}
		}

		public static void SortPancake<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/pancake-sorting/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			for (int i = count; i > index; i--)
			{
				int mi = 0;

				for (int j = index; j < i; j++)
				{
					if (comparer.IsGreaterThan(thisValue[j], thisValue[mi]))
						mi = j;
				}

				if (mi == i - 1) continue;

				if (mi > 0)
				{
					mi++;

					for (int j = index; j < --mi; j++)
					{
						thisValue.FastSwap(j, mi);
					}
				}

				// flip
				int m = i;

				for (int j = index; j < --m; j++)
				{
					thisValue.FastSwap(j, m);
				}
			}
		}

		public static void SortBinary<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://stackoverflow.com/questions/26454911/how-does-binary-insertion-sort-work
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			for (int i = index + 1; i < count; i++)
			{
				T value = thisValue[i];

				// Find location to insert using binary search
				int lo = index;
				int hi = i - 1;

				// this works a little different than the normal binary search
				// It's the same basic general idea but the start and end values are different
				// So we can't use Array.BinarySearch or IListExtension.BinarySearch
				while (lo <= hi)
				{
					// Same as (l + r) / 2, but avoids overflow
					int mid = lo + (hi - lo) / 2;

					if (comparer.IsLessThan(value, thisValue[mid]))
						hi = mid - 1;
					else
						lo = mid + 1;
				}

				// Shifting array to one location right
				for (int j = i - 1; j >= lo; j--)
				{
					thisValue[j + 1] = thisValue[j];
				}

				// Placing element at its correct location 
				thisValue[lo] = value;
			}
		}

		public static void SortGnome<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/gnome-sort-a-stupid-one/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			int i = index + 1;

			while (i < count)
			{
				if (i <= index || comparer.IsGreaterThanOrEqual(thisValue[i], thisValue[i - 1]))
				{
					i++;
					continue;
				}

				thisValue.FastSwap(i, --i);
			}
		}

		public static void SortBrick<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1, IComparer<T> comparer = null, bool descending = false)
		{
			// https://www.geeksforgeeks.org/odd-even-sort-brick-sort/
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;
			comparer ??= Comparer<T>.Default;
			if (descending) comparer = comparer.Reverse();

			bool isSorted = false;

			while (!isSorted)
			{
				isSorted = true;

				// Perform Bubble sort on odd indexed element 
				for (int i = index + 1; i <= count - 2; i += 2) 
				{ 
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[i + 1])) continue;
					thisValue.FastSwap(i, i + 1);
					isSorted = false;
				} 

				// Perform Bubble sort on even indexed element 
				for (int i = index; i <= count - 2; i += 2) 
				{ 
					if (comparer.IsLessThanOrEqual(thisValue[i], thisValue[i + 1])) continue;
					thisValue.FastSwap(i, i + 1);
					isSorted = false;
				} 
			}
		}
		#endregion

		/// <summary>
		/// Shuffles the list using Fisher-Yates shuffle algorithm
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="thisValue"></param>
		/// <param name="index"></param>
		/// <param name="count"></param>
		public static void Shuffle<T>([NotNull] this IList<T> thisValue, int index = 0, int count = -1)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (count < 2 || thisValue.Count < 2) return;

			for (int i = count - 1; i > index; i--)
			{
				int pick = (int)Math.Floor((i + 1) * RandomHelper.NextDouble());
				FastSwap(thisValue, i, pick);
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public static void Swap<T>([NotNull] this IList<T> thisValue, int index1, int index2)
		{
			thisValue.Count.ValidateRange(index1);
			thisValue.Count.ValidateRange(index2);
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

		public static void LeftShift<T>([NotNull] this IList<T> thisValue, int index, int count)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (count == 0) return;

			for (int i = count - 1; i > index; i--)
				thisValue[i - 1] = thisValue[i];
		}

		public static void FastLeftShift<T>([NotNull] this IList<T> thisValue, int index, int count)
		{
			for (int i = count - 1; i > index; i--)
				thisValue[i - 1] = thisValue[i];
		}

		public static void RightShift<T>([NotNull] this IList<T> thisValue, int index, int count)
		{
			thisValue.Count.ValidateRange(index, ref count);
			if (count == 0) return;

			for (int i = index; i < count - 1; i++)
				thisValue[i + 1] = thisValue[i];
		}

		public static void FastRightShift<T>([NotNull] this IList<T> thisValue, int index, int count)
		{
			for (int i = index; i < count - 1; i++)
				thisValue[i + 1] = thisValue[i];
		}

		/// <summary>
		/// This method takes a list of numbers and tries to find the maximum sum the can be obtained by summing
		/// up the longest sequence of adjacent elements in the list.
		/// </summary>
		/// <typeparam name="T">The value type</typeparam>
		/// <param name="thisValue">List of numbers</param>
		/// <returns>The maximum sum that can be obtained by summing up the longest sequence of adjacent elements in the list.</returns>
		public static T KadaneMaximumSum<T>([NotNull] this IList<T> thisValue)
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

		public static T DeepestPit<T>([NotNull] this IList<T> thisValue)
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
				deepest = essentialMixMath.Max(deepest, essentialMixMath.Min(thisValue[p].Subtract(thisValue[q]), thisValue[r].Subtract(thisValue[q])));
				p = i - 1;
				q = -1;
			}

			if (deepest.Equals(def)) deepest = minus;
			return deepest;
		}
	}
}