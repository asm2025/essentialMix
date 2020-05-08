﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Threading;
using asm.Comparers;
using asm.Exceptions.Collections;
using asm.Extensions;
using asm.Helpers;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// A double-ended queue (deque), which provides O(1) indexed access, O(1) removals from the front and back, amortized
	/// O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting
	/// slower as the index approaches the middle).
	/// https://github.com/StephenCleary/Deque/blob/master/src/Nito.Collections.Deque/Deque.cs
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the deque.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[ComVisible(false)]
	[Serializable]
	public class Deque<T> : IList<T>, IList, IReadOnlyList<T>, IReadOnlyCollection<T>, ICollection, IEnumerable
	{
		[Serializable]
		internal class SynchronizedList : IList<T>
		{
			private readonly Deque<T> _deque;
			private readonly IList _list;
			private readonly object _root;

			internal SynchronizedList(Deque<T> deque)
			{
				_deque = deque;
				_list = deque;
				_root = _list.SyncRoot;
			}

			public T this[int index]
			{
				get
				{
					lock (_root)
					{
						return _deque[index];
					}
				}
				set
				{
					lock (_root)
					{
						_deque[index] = value;
					}
				}
			}

			public int Count
			{
				get
				{
					lock (_root)
					{
						return _deque.Count;
					}
				}
			}

			public bool IsReadOnly
			{
				get
				{
					lock (_root)
					{
						return _list.IsReadOnly;
					}
				}
			}

			public void Insert(int index, T item)
			{
				lock (_root)
				{
					_deque.Insert(index, item);
				}
			}

			public void Add(T item)
			{
				lock (_root)
				{
					_deque.Add(item);
				}
			}

			public void RemoveAt(int index)
			{
				lock (_root)
				{
					_deque.RemoveAt(index);
				}
			}

			public bool Remove(T item)
			{
				lock (_root)
				{
					return _deque.Remove(item);
				}
			}

			public void Clear()
			{
				lock (_root)
				{
					_deque.Clear();
				}
			}

			public int IndexOf(T item)
			{
				lock (_root)
				{
					return _deque.IndexOf(item);
				}
			}

			public bool Contains(T item)
			{
				lock (_root)
				{
					return _deque.Contains(item);
				}
			}

			public void CopyTo(T[] array, int arrayIndex)
			{
				lock (_root)
				{
					_deque.CopyTo(array, arrayIndex);
				}
			}

			IEnumerator IEnumerable.GetEnumerator()
			{
				lock (_root)
				{
					return _deque.GetEnumerator();
				}
			}

			public IEnumerator<T> GetEnumerator()
			{
				lock (_root)
				{
					return _deque.GetEnumerator();
				}
			}
		}

		[Serializable]
		public struct Enumerator : IEnumerator<T>, IEnumerator
		{
			private readonly Deque<T> _deque;
			private readonly int _version;

			private int _index;

			internal Enumerator([NotNull] Deque<T> deque)
			{
				_deque = deque;
				_index = 0;
				_version = deque._version;
				Current = default(T);
			}

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (_version == _deque._version && _index < _deque.Count)
				{
					Current = _deque.Items[_index];
					_index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (_version != _deque._version) throw new VersionChangedException();
				_index = _deque.Count + 1;
				Current = default(T);
				return false;
			}

			public T Current { get; private set; }

			object IEnumerator.Current
			{
				get
				{
					if (_index.InRangeRx(0, _deque.Count)) throw new InvalidOperationException();
					return Current;
				}
			}

			void IEnumerator.Reset()
			{
				if (_version != _deque._version) throw new VersionChangedException();
				_index = 0;
				Current = default(T);
			}
		}

		private int _version;

		[NonSerialized]
		private object _syncRoot;

		/// <summary>
		/// Initializes a new instance of the <see cref="Deque{T}" /> class.
		/// </summary>
		public Deque()
			: this(0)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Deque{T}" /> class with the specified capacity.
		/// </summary>
		/// <param name="capacity">The initial capacity. Must be greater than <c>0</c>.</param>
		public Deque(int capacity)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Items = capacity == 0
						? Array.Empty<T>()
						: new T[capacity];
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Deque{T}" /> class with the elements from the specified
		/// collection.
		/// </summary>
		/// <param name="collection">The collection. May not be <c>null</c>.</param>
		public Deque([NotNull] IEnumerable<T> collection)
		{
			Items = Array.Empty<T>();
			InsertRange(0, collection);
		}

		public int Capacity
		{
			get => Items.Length;
			set
			{
				if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
				if (value == Items.Length) return;

				if (value > 0)
				{
					T[] newItems = new T[value];
					CopyTo(newItems);
					Items = newItems;
				}
				else
				{
					Items = Array.Empty<T>();
				}

				Offset = 0;
				_version++;
			}
		}

		[field: ContractPublicPropertyName("Count")]
		public int Count { get; private set; }

		public T this[int index]
		{
			get => Items[IndexToBufferIndex(index, true)];
			set
			{
				index = IndexToBufferIndex(index, true);
				Items[index] = value;
				_version++;
			}
		}

		object IList.this[int index]
		{
			get => this[index];
			set
			{
				if (!ObjectHelper.IsCompatible<T>(value)) throw new ArgumentException("Value is of incorrect type.", nameof(value));
				this[index] = (T)value;
			}
		}

		bool IList.IsFixedSize => false;

		bool IList.IsReadOnly => false;

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

		bool ICollection.IsSynchronized => false;

		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		/// <summary>
		/// The circular Items that holds the view.
		/// </summary>
		[NotNull]
		protected T[] Items { get; private set; }

		/// <summary>
		/// The offset into <see cref="Items" /> where the view begins.
		/// </summary>
		protected int Offset { get; private set; }

		[NotNull] 
		public ReadOnlyCollection<T> AsReadOnly() { return new ReadOnlyCollection<T>(this); }

		public void Insert(int index, T item)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			MakeGapAt(index, 1);

			if (index == 0)
			{
				Items[PreDecrement(1)] = item;
				Count++;
			}
			else if (index == Count)
			{
				Items[IndexToBufferIndex(Count)] = item;
				Count++;
			}
			else
			{
				Items[IndexToBufferIndex(index)] = item;
				Count++;
			}

			_version++;
		}

		void IList.Insert(int index, object value)
		{
			if (!ObjectHelper.IsCompatible<T>(value)) throw new InvalidCastException();
			Insert(index, (T)value);
		}

		public void Add(T item) { Insert(Count, item); }

		int IList.Add(object value)
		{
			if (value == null && !typeof(T).IsClass) throw new ArgumentNullException(nameof(value), "Value cannot be null.");
			if (!ObjectHelper.IsCompatible<T>(value)) throw new ArgumentException("Value is of incorrect type.", nameof(value));
			AddToBack((T)value);
			return Count - 1;
		}

		public void AddToFront(T item) { Insert(0, item); }

		public void AddToBack(T item) { Insert(Count, item); }

		public void RemoveAt(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			if (index == 0 || index == Count - 1)
			{
				Count--;
				_version++;
			}
			else
			{
				RemoveGapAt(index, 1);
				Count--;
			}
		}

		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index == -1) return false;
			RemoveAt(index);
			return true;
		}

		void IList.Remove(object value)
		{
			if (!ObjectHelper.IsCompatible<T>(value)) return;
			Remove((T)value);
		}

		/// <summary>
		/// Removes and returns the last element of this deque.
		/// </summary>
		/// <returns>The former last element.</returns>
		/// <exception cref="InvalidOperationException">The deque is empty.</exception>
		public T RemoveFromBack()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			T item = Items[IndexToBufferIndex(Count - 1)];
			RemoveAt(Count - 1);
			return item;
		}

		/// <summary>
		/// Removes and returns the first element of this deque.
		/// </summary>
		/// <returns>The former first element.</returns>
		/// <exception cref="InvalidOperationException">The deque is empty.</exception>
		public T RemoveFromFront()
		{
			if (Count == 0) throw new InvalidOperationException("Collection is empty.");
			RemoveAt(0);
			return Items[PostIncrement(1)];
		}

		/// <summary>
		/// Removes all items from this deque.
		/// </summary>
		public void Clear()
		{
			Offset = 0;
			Count = 0;
		}

		/// <summary>
		/// Inserts a collection of elements into this deque.
		/// </summary>
		/// <param name="index">The index at which the collection is inserted.</param>
		/// <param name="collection">The collection of elements to insert.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// <paramref name="index" /> is not a valid index to an insertion point for
		/// the source.
		/// </exception>
		public void InsertRange(int index, [NotNull] IEnumerable<T> collection)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			int count = collection.FastCount();
			if (count == 0) return;
			if (count > 0) EnsureCapacity(checked(Count + count));

			if (collection is ICollection<T> c)
			{
				MakeGapAt(index, c.Count);

				foreach (T item in collection)
					Items[IndexToBufferIndex(index++)] = item;

				Count += c.Count;
				_version++;
				return;
			}

			using (IEnumerator<T> enumerator = collection.GetEnumerator())
			{
				while (enumerator.MoveNext())
				{
					Insert(index++, enumerator.Current);
				}
			}
		}

		/// <summary>
		/// Removes a range of elements from this deque.
		/// </summary>
		/// <param name="index">The index into the deque at which the range begins.</param>
		/// <param name="count">The number of elements to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Either <paramref name="index" /> or <paramref name="count" /> is less
		/// than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The range [<paramref name="index" />, <paramref name="index" /> +
		/// <paramref name="count" />) is not within the range [0, <see cref="Count" />).
		/// </exception>
		public void RemoveRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (count == 0) return;
			RemoveGapAt(index, count);
			Count -= count;
		}

		// This method removes all items which matches the predicate.
		// The complexity is O(n).   
		public int RemoveAll([NotNull] Predicate<T> match)
		{
			int freeIndex = 0;   // the first free slot in items array

			// Find the first item which needs to be removed.
			while (freeIndex < Count && !match(Items[IndexToBufferIndex(freeIndex)]))
				freeIndex++;

			if (freeIndex >= Count) return 0;
			int result = Count - freeIndex;
			RemoveRange(freeIndex, result);
			return result;
		}

		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>
		/// A <see cref="T:System.Collections.Generic.IEnumerator`1" /> that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.
		/// </returns>
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public int BinarySearch([NotNull] T item)
		{
			return BinarySearch(0, Count, item, null);
		}

		public int BinarySearch([NotNull] T item, IComparer<T> comparer)
		{
			return BinarySearch(0, Count, item, comparer);
		}

		// Searches a section of the list for a given element using a binary search
		// algorithm. Elements of the list are compared to the search value using
		// the given IComparer interface. If comparer is null, elements of
		// the list are compared to the search value using the IComparable
		// interface, which in that case must be implemented by all elements of the
		// list and the given search value. This method assumes that the given
		// section of the list is already sorted; if this is not the case, the
		// result will be incorrect.
		//
		// The method returns the index of the given value in the list. If the
		// list does not contain the given value, the method returns a negative
		// integer. The bitwise complement operator (~) can be applied to a
		// negative result to produce the index of the first element (if any) that
		// is larger than the given search value. This is also the index at which
		// the search value should be inserted into the list in order for the list
		// to remain sorted.
		// 
		// The method uses the Array.BinarySearch method to perform the
		// search.
		// 
		public int BinarySearch(int index, int count, [NotNull] T item, IComparer<T> comparer)
		{
			Count.ValidateRange(index, ref count);
			return Array.BinarySearch(Items, index, count, item, comparer);
		}

		// Returns the index of the first occurrence of a given value in a range of
		// this list. The list is searched forwards from beginning to end.
		// The elements of the list are compared to the given value using the
		// Object.Equals method.
		// 
		// This method uses the Array.IndexOf method to perform the
		// search.
		public int IndexOf(T item) { return IndexOf(item, 0, -1); }

		// Returns the index of the first occurrence of a given value in a range of
		// this list. The list is searched forwards, starting at index
		// index and ending at count number of elements. The
		// elements of the list are compared to the given value using the
		// Object.Equals method.
		// 
		// This method uses the Array.IndexOf method to perform the
		// search.
		public int IndexOf(T item, int index) { return IndexOf(item, index, -1); }

		// Returns the index of the first occurrence of a given value in a range of
		// this list. The list is searched forwards, starting at index
		// index and up to count number of elements. The
		// elements of the list are compared to the given value using the
		// Object.Equals method.
		// 
		// This method uses the Array.IndexOf method to perform the
		// search.
		public int IndexOf(T item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			return Array.IndexOf(Items, item, index, count);
		}

		int IList.IndexOf(object value)
		{
			return ObjectHelper.IsCompatible<T>(value)
						? IndexOf((T)value)
						: -1;
		}

		// Returns the index of the last occurrence of a given value in a range of
		// this list. The list is searched backwards, starting at the end 
		// and ending at the first element in the list. The elements of the list 
		// are compared to the given value using the Object.Equals method.
		// 
		// This method uses the Array.LastIndexOf method to perform the
		// search.
		public int LastIndexOf(T item)
		{
			return LastIndexOf(item, 0, -1);
		}

		// Returns the index of the last occurrence of a given value in a range of
		// this list. The list is searched backwards, starting at index
		// index and ending at the first element in the list. The 
		// elements of the list are compared to the given value using the 
		// Object.Equals method.
		// 
		// This method uses the Array.LastIndexOf method to perform the
		// search.
		public int LastIndexOf(T item, int index)
		{
			return LastIndexOf(item, index, -1);
		}

		// Returns the index of the last occurrence of a given value in a range of
		// this list. The list is searched backwards, starting at index
		// index and up to count elements. The elements of
		// the list are compared to the given value using the Object.Equals
		// method.
		// 
		// This method uses the Array.LastIndexOf method to perform the
		// search.
		public int LastIndexOf(T item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (index == 0) index = Count - 1;
			return Count == 0
						? -1
						: Array.LastIndexOf(Items, item, index, count);
		}

		/// <summary>
		/// Determines whether this list contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in this list.</param>
		/// <returns>
		/// true if <paramref name="item" /> is found in this list; otherwise, false.
		/// </returns>
		public bool Contains(T item)
		{
			return IndexOf(item, 0, -1) > -1;
		}

		bool IList.Contains(object value) { return ObjectHelper.IsCompatible<T>(value) && Contains((T)value); }

		public T Find([NotNull] Predicate<T> match)
		{
			for (int i = 0; i < Count; i++)
			{
				if (!match(Items[i])) continue;
				return Items[i];
			}
			return default(T);
		}

		public IEnumerable<T> FindAll([NotNull] Predicate<T> match)
		{
			for (int i = 0; i < Count; i++)
			{
				T item = Items[i];
				if (!match(item)) continue;
				yield return item;
			}
		}

		public int FindIndex([NotNull] Predicate<T> match)
		{
			return FindIndex(0, Count, match);
		}

		public int FindIndex(int startIndex, [NotNull] Predicate<T> match)
		{
			return FindIndex(startIndex, -1, match);
		}

		public int FindIndex(int startIndex, int count, [NotNull] Predicate<T> match)
		{
			Count.ValidateRange(startIndex, ref count);

			int endIndex = startIndex + count;

			for (int i = startIndex; i < endIndex; i++)
			{
				if (!match(Items[IndexToBufferIndex(i)])) continue;
				return i;
			}

			return -1;
		}

		public T FindLast([NotNull] Predicate<T> match)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				T item = Items[IndexToBufferIndex(i)];
				if (!match(item)) continue;
				return item;
			}
			return default(T);
		}

		public int FindLastIndex([NotNull] Predicate<T> match)
		{
			return FindLastIndex(0, -1, match);
		}

		public int FindLastIndex(int startIndex, [NotNull] Predicate<T> match)
		{
			return FindLastIndex(startIndex, -1, match);
		}

		public int FindLastIndex(int startIndex, int count, [NotNull] Predicate<T> match)
		{
			Count.ValidateRange(startIndex, ref count);
			if (startIndex == 0) startIndex = Count - 1;
			int endIndex = startIndex - count;

			for (int i = startIndex; i > endIndex; i--)
			{
				if (!match(Items[IndexToBufferIndex(i)])) continue;
				return i;
			}

			return -1;
		}

		public bool Exists([NotNull] Predicate<T> match) { return FindIndex(match) != -1; }

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter)
		{
			for (int i = 0; i < Count; i++)
			{
				yield return converter(Items[IndexToBufferIndex(i)]);
			}
		}

		// Copies this List into array, which must be of a 
		// compatible array type.  
		public void CopyTo([NotNull] T[] array) { CopyTo(array, 0); }
		public void CopyTo(T[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
		public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
		{
			Count.ValidateRange(arrayIndex, ref count);
			array.Length.ValidateRange(arrayIndex, ref count);

			if (Offset > Capacity - Count)
			{
				// The existing buffer is split, so we have to copy it in parts
				int length = Capacity - Offset;
				Array.Copy(Items, Offset, array, arrayIndex, length);
				Array.Copy(Items, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(Items, Offset, array, arrayIndex, Count);
			}
		}

		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));

			if (array is T[] tArray)
			{
				CopyTo(tArray, arrayIndex);
				return;
			}

			/*
			 * Catch the obvious case assignment will fail.
			 * We can find all possible problems by doing the check though.
			 * For example, if the element type of the Array is derived from T,
			 * we can't figure out if we can successfully copy the element beforehand.
			 */
			array.Length.ValidateRange(arrayIndex, Count);
			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(T);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[])) throw new ArgumentException("Invalid array type", nameof(array));

			if (Offset > Capacity - Count)
			{
				// The existing buffer is split, so we have to copy it in parts
				int length = Capacity - Offset;
				Array.Copy(Items, Offset, array, arrayIndex, length);
				Array.Copy(Items, 0, array, arrayIndex + length, Count - length);
			}
			else
			{
				// The existing buffer is whole
				Array.Copy(Items, Offset, array, arrayIndex, Count);
			}
		}

		/// <summary>
		/// Creates and returns a new array containing the elements in this deque.
		/// </summary>
		[NotNull]
		public T[] ToArray()
		{
			T[] result = new T[Count];
			CopyTo(result);
			return result;
		}

		public IEnumerable<T> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			int last = index + count;

			for (int i = index; i < last; i++)
			{
				yield return Items[IndexToBufferIndex(i)];
			}
		}

		public void ForEach([NotNull] Action<T> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i]);
			}

			if (version != _version) throw new VersionChangedException();
		}

		public void ForEach([NotNull] Action<T, int> action)
		{
			int version = _version;

			for (int i = 0; i < Count; i++)
			{
				if (version != _version) break;
				action(Items[i], i);
			}

			if (version != _version) throw new VersionChangedException();
		}

		// Reverses the elements in this list.
		public void Reverse() { Reverse(0, Count); }

		// Reverses the elements in a range of this list. Following a call to this
		// method, an element in the range given by index and count
		// which was previously located at index i will now be located at
		// index index + (index + count - i - 1).
		// 
		// This method uses the Array.Reverse method to reverse the
		// elements.
		public void Reverse(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (Count < 2 || count < 2) return;

			if (Offset > Capacity - Count)
			{
				int split = count / 2;

				for (int i = index; i < split; i++)
				{
					int x = IndexToBufferIndex(index + count - (i + 1));
					int y = IndexToBufferIndex(index + i);
					T tmp = Items[x];
					Items[x] = Items[y];
					Items[y] = tmp;
				}
			}
			else
			{
				Array.Reverse(Items, index, count);
			}

			_version++;
		}

		public void Sort([NotNull] Comparison<T> comparison)
		{
			if (Count == 0) return;
			IComparer<T> comparer = new FunctorComparer<T>(comparison);
			Sort(0, Count, comparer);
		}

		// Sorts the elements in this list.  Uses the default comparer and 
		// Array.Sort.
		public void Sort() { Sort(0, Count, null); }

		// Sorts the elements in this list.  Uses Array.Sort with the
		// provided comparer.
		public void Sort(IComparer<T> comparer) { Sort(0, Count, comparer); }

		// Sorts the elements in a section of this list. The sort compares the
		// elements to each other using the given IComparer interface. If
		// comparer is null, the elements are compared to each other using
		// the IComparable interface, which in that case must be implemented by all
		// elements of the list.
		// 
		// This method uses the Array.Sort method to sort the elements.
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			Count.ValidateRange(index, ref count);
			if (count < 2) return;

			if (Offset > Capacity - Count)
			{
				// will just do a bubble sort
				int last = index + count;

				for (int i = index; i < last; i++)
				{
					int x = IndexToBufferIndex(index + count - (i + 1));
					int y = IndexToBufferIndex(index + i);
					if (comparer.IsLessThanOrEqual(Items[x], Items[y])) continue;
					// swap
					T tmp = Items[x];
					Items[x] = Items[y];
					Items[y] = tmp;
				}
			}
			else
			{
				Array.Sort(Items, index, count, comparer);
			}

			_version++;
		}

		// Sets the capacity of this list to the size of the list. This method can
		// be used to minimize a list's memory overhead once it is known that no
		// new elements will be added to the list. To completely clear a list and
		// release all memory referenced by the list, execute the following
		// statements:
		// 
		// list.Clear();
		// list.TrimExcess();
		public void TrimExcess()
		{
			int threshold = (int)(Items.Length * 0.9);
			if (Count >= threshold) return;
			Capacity = Count;
		}

		public bool TrueForAll([NotNull] Predicate<T> match)
		{
			for (int i = 0; i < Count; i++)
			{
				if (match(Items[IndexToBufferIndex(i)])) continue;
				return false;
			}
			return true;
		}

		protected void EnsureCapacity(int min)
		{
			if (Items.Length >= min) return;
			Capacity = (Items.Length == 0 ? Constants.DEFAULT_CAPACITY : Items.Length * 2).NotBelow(min);
		}

		protected int IndexToBufferIndex(int index, bool validate = false)
		{
			int n = (index + Offset) % Capacity;
			if (validate && !n.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			return n;
		}

		/// <summary>
		/// Increments <see cref="Offset" /> by <paramref name="value" /> using modulo-<see cref="Capacity" /> arithmetic.
		/// </summary>
		/// <param name="value">The value by which to increase <see cref="Offset" />. May not be negative.</param>
		/// <returns>The value of <see cref="Offset" /> after it was incremented.</returns>
		private int PostIncrement(int value)
		{
			int ret = Offset;
			Offset += value;
			Offset %= Capacity;
			_version++;
			return ret;
		}

		/// <summary>
		/// Decrements <see cref="Offset" /> by <paramref name="value" /> using modulo-<see cref="Capacity" /> arithmetic.
		/// </summary>
		/// <param name="value">
		/// The value by which to reduce <see cref="Offset" />. May not be negative or greater than
		/// <see cref="Capacity" />.
		/// </param>
		/// <returns>The value of <see cref="Offset" /> before it was decremented.</returns>
		private int PreDecrement(int value)
		{
			Offset -= value;
			if (Offset < 0) Offset += Capacity;
			_version++;
			return Offset;
		}

		/// <summary>
		/// Make room for one or more elements to be inserted in the view.
		/// </summary>
		/// <param name="index">The index into the view at which the elements are to be inserted.</param>
		/// <param name="size">
		/// The gap size.
		/// </param>
		private void MakeGapAt(int index, int size)
		{
			if (Count + size >= Capacity) EnsureCapacity(Count + size);
			if (!index.InRangeEx(0, Count)) return;

			// Make room in the existing list
			if (index < Count / 2)
			{
				// Inserting into the first half of the list
				// Move lower items down: [0, index) -> [Capacity - size, Capacity - size + index)
				// This clears out the low "index" number of items, moving them by [size] places down;
				// after rotation, there will be a [size]-sized hole at "index".
				int writeIndex = Capacity - size;

				for (int i = 0; i < index; ++i)
					Items[IndexToBufferIndex(writeIndex + i)] = Items[IndexToBufferIndex(i)];

				// Rotate to the new view
				PreDecrement(1);
			}
			else
			{
				// Inserting into the second half of the list
				// Move higher items up: [index, count) -> [index + size, size + count)
				int copyCount = Count - index;
				int writeIndex = index + 1;

				for (int i = copyCount - 1; i > -1; --i)
					Items[IndexToBufferIndex(writeIndex + i)] = Items[IndexToBufferIndex(index + i)];

				_version++;
			}
		}

		/// <summary>
		/// Prepare for removal of elements from the view.
		/// </summary>
		/// <param name="index">The index into the view at which the range begins.</param>
		/// <param name="size">
		/// The number of elements in the range. This must be greater than 0 and less than or equal
		/// to <see cref="Count" />.
		/// </param>
		private void RemoveGapAt(int index, int size)
		{
			if (size == 0) return;

			if (index == 0)
			{
				// Removing from the beginning: rotate to the new view
				PostIncrement(size);
				return;
			}

			if (index == Count - size)
			{
				// Removing from the ending: nothing to be done
				return;
			}

			if (index + size / 2 < Count / 2)
			{
				// Removing from first half of list
				// Move lower items up: [0, index) -> [collectionCount, collectionCount + index)
				int copyCount = index;
				int writeIndex = size;

				for (int i = copyCount - 1; i != -1; --i)
					Items[IndexToBufferIndex(writeIndex + i)] = Items[IndexToBufferIndex(i)];

				// Rotate to new view
				PostIncrement(size);
			}
			else
			{
				// Removing from second half of list
				// Move higher items down: [index + collectionCount, count) -> [index, count - collectionCount)
				int copyCount = Count - size - index;
				int readIndex = index + size;

				for (int i = 0; i != copyCount; ++i)
					Items[IndexToBufferIndex(index + i)] = Items[IndexToBufferIndex(readIndex + i)];

				_version++;
			}
		}

		[NotNull]
		public static IList<T> Synchronized(Deque<T> list)
		{
			return new SynchronizedList(list);
		}
	}
}