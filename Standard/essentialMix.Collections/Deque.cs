using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Comparers;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <summary>
	/// A double-ended queue (deque), which provides O(1) indexed access, O(1) removals from the front and back, amortized
	/// O(1) insertions to the front and back, and O(N) insertions and removals anywhere else (with the operations getting
	/// slower as the index approaches the middle).
	/// <para>Based on <see href="https://github.com/StephenCleary/Deque/blob/master/src/Nito.Collections.Deque/Deque.cs">Stephen Cleary's Nito.Collections.Deque</see></para>
	/// </summary>
	/// <typeparam name="T">The type of elements contained in the deque.</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
	[Serializable]
	public class Deque<T> : IDeque<T>, IList<T>, IList, IReadOnlyList<T>, IReadOnlyCollection<T>, ICollection, IEnumerable
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

			/// <inheritdoc />
			public T this[int index]
			{
				get
				{
					lock(_root)
					{
						return _deque[index];
					}
				}
				set
				{
					lock(_root)
					{
						_deque[index] = value;
					}
				}
			}

			/// <inheritdoc />
			public int Count
			{
				get
				{
					lock(_root)
					{
						return _deque.Count;
					}
				}
			}

			/// <inheritdoc />
			public bool IsReadOnly
			{
				get
				{
					lock(_root)
					{
						return _list.IsReadOnly;
					}
				}
			}

			/// <inheritdoc />
			public void Insert(int index, T item)
			{
				lock(_root)
				{
					_deque.Insert(index, item);
				}
			}

			/// <inheritdoc />
			public void Add(T item)
			{
				lock(_root)
				{
					_deque.Insert(_deque.Count, item);
				}
			}

			/// <inheritdoc />
			public void RemoveAt(int index)
			{
				lock(_root)
				{
					_deque.RemoveAt(index);
				}
			}

			/// <inheritdoc />
			public bool Remove(T item)
			{
				lock(_root)
				{
					return _deque.Remove(item);
				}
			}

			/// <inheritdoc />
			public void Clear()
			{
				lock(_root)
				{
					_deque.Clear();
				}
			}

			/// <inheritdoc />
			public int IndexOf(T item)
			{
				lock(_root)
				{
					return _deque.IndexOf(item);
				}
			}

			/// <inheritdoc />
			public bool Contains(T item)
			{
				lock(_root)
				{
					return _deque.Contains(item);
				}
			}

			/// <inheritdoc />
			public void CopyTo(T[] array, int arrayIndex)
			{
				lock(_root)
				{
					_deque.CopyTo(array, arrayIndex);
				}
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator()
			{
				lock(_root)
				{
					return _deque.GetEnumerator();
				}
			}

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
			{
				lock(_root)
				{
					return _deque.GetEnumerator();
				}
			}
		}

		[Serializable]
		private struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
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

			/// <inheritdoc />
			public void Dispose() { }

			/// <inheritdoc />
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

			/// <inheritdoc />
			public T Current { get; private set; }

			/// <inheritdoc />
			object IEnumerator.Current
			{
				get
				{
					if (!_index.InRange(0, _deque.Count)) throw new InvalidOperationException();
					return Current;
				}
			}

			/// <inheritdoc />
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
			: this(Constants.DEFAULT_CAPACITY)
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

		public Deque([NotNull] IEnumerable<T> collection)
		{
			if (!collection.FastCount(out int capacity)) capacity = Constants.DEFAULT_CAPACITY;
			Items = new T[capacity];

			foreach (T item in collection)
				Insert(Count, item, true);

			_version = 0;
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
					CopyTo(newItems, 0);
					Items = newItems;
				}
				else
				{
					Items = Array.Empty<T>();
				}

				_version++;
			}
		}

		/// <inheritdoc cref="ICollection{T}" />
		[field: ContractPublicPropertyName("Count")]
		public int Count { get; private set; }

		/// <inheritdoc cref="IList{T}" />
		public T this[int index]
		{
			get => Items[index];
			set => Insert(index, value, false);
		}

		/// <inheritdoc />
		object IList.this[int index]
		{
			get => this[index];
			set
			{
				if (!ObjectHelper.IsCompatible<T>(value)) throw new ArgumentException("Value is of incorrect type.", nameof(value));
				Insert(index, (T)value, false);
			}
		}

		/// <inheritdoc />
		bool IList.IsFixedSize => false;

		/// <inheritdoc />
		bool IList.IsReadOnly => false;

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => false;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => false;

		/// <inheritdoc />
		object ICollection.SyncRoot
		{
			get
			{
				if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
				return _syncRoot;
			}
		}

		[NotNull]
		protected T[] Items { get; private set; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return new Enumerator(this); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		[NotNull]
		public ReadOnlyCollection<T> AsReadOnly() { return new ReadOnlyCollection<T>(this); }

		/// <inheritdoc />
		public void Insert(int index, T item)
		{
			Insert(index, item, true);
		}

		/// <inheritdoc />
		void IList.Insert(int index, object value)
		{
			if (!ObjectHelper.IsCompatible<T>(value)) throw new InvalidCastException();
			Insert(index, (T)value, true);
		}

		/// <inheritdoc />
		void ICollection<T>.Add(T item) { Insert(Count, item, true); }

		/// <inheritdoc />
		int IList.Add(object value)
		{
			if (value is null && !typeof(T).IsClass) throw new ArgumentNullException(nameof(value), "Value cannot be null.");
			if (!ObjectHelper.IsCompatible<T>(value)) throw new ArgumentException("Value is of incorrect type.", nameof(value));
			Insert(Count, (T)value, true);
			return Count - 1;
		}

		/// <inheritdoc cref="IList{T}" />
		public void RemoveAt(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			RemoveAtInternal(index);
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			int index = IndexOf(item);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		/// <inheritdoc />
		void IList.Remove(object value)
		{
			if (!ObjectHelper.IsCompatible<T>(value)) return;
			Remove((T)value);
		}

		public void Enqueue(T item) { Insert(Count, item, true); }

		public void Enqueue([NotNull] IEnumerable<T> enumerable) { InsertRange(Count, enumerable); }

		public void EnqueueBefore(int index, T value)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			Insert(index, value, true);
		}

		public void EnqueueAfter(int index, T value)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			Insert(++index, value, true);
		}

		public void EnqueueLast(T item) { Insert(0, item, true); }

		public T Dequeue()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			return RemoveAtInternal(0);
		}

		/// <inheritdoc />
		public bool TryDequeue(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			return TryRemoveAtInternal(0, out item);
		}

		public void Push(T item) { Insert(Count, item, true); }

		public void Push([NotNull] IEnumerable<T> enumerable) { InsertRange(Count, enumerable.Reverse()); }

		public void PushBefore(int index, T value)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			Insert(++index, value, true);
		}

		public void PushAfter(int index, T value)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			Insert(index, value, true);
		}

		public void PushLast(T item) { Insert(0, item, true); }

		public void PushLast([NotNull] IEnumerable<T> enumerable) { InsertRange(0, enumerable.Reverse()); }

		public T Pop()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			return RemoveAtInternal(Count - 1);
		}

		/// <inheritdoc />
		public bool TryPop(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			return TryRemoveAtInternal(Count - 1, out item);
		}

		public T Peek()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			return Items[0];
		}

		/// <inheritdoc />
		public bool TryPeek(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			item = Items[0];
			return true;
		}

		public T PeekTail()
		{
			if (Count == 0) throw new CollectionIsEmptyException();
			return Items[Count - 1];
		}

		/// <inheritdoc />
		public bool TryPeekTail(out T item)
		{
			if (Count == 0)
			{
				item = default(T);
				return false;
			}
			item = Items[Count - 1];
			return true;
		}

		/// <inheritdoc cref="ICollection{T}" />
		public void Clear()
		{
			Count = 0;
			_version++;
		}

		/// <summary>
		/// Removes a range of elements from this deque.
		/// </summary>
		/// <param name="index">The index into the deque at which the range begins.</param>
		/// <param name="count">The number of elements to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">
		/// Either <paramref name="index" /> or <paramref name="count" /> is less than 0.
		/// </exception>
		/// <exception cref="ArgumentException">
		/// The range [<paramref name="index" />, <paramref name="index" /> +
		/// <paramref name="count" />) is not within the range [0, <see cref="Count" />).
		/// </exception>
		public void RemoveRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (count == 0) return;
			if (index < Count) Array.Copy(Items, index + count, Items, index, Count - index);
			Array.Clear(Items, Count - count, count);
			Count -= count;
			_version++;
		}

		/// <summary>
		/// Removes all items which matches the predicate.
		/// </summary>
		/// <param name="match"></param>
		/// <returns></returns>
		public int RemoveAll([NotNull] Predicate<T> match)
		{
			int freeIndex = 0;   // the first free slot in items array

			// Find the first item which needs to be removed.
			while (freeIndex < Count && !match(Items[freeIndex]))
				freeIndex++;

			if (freeIndex >= Count) return 0;
			int result = Count - freeIndex;
			RemoveRange(freeIndex, result);
			return result;
		}

		public int BinarySearch([NotNull] T item)
		{
			return BinarySearch(0, Count, item, null);
		}

		public int BinarySearch([NotNull] T item, IComparer<T> comparer)
		{
			return BinarySearch(0, Count, item, comparer);
		}

		/// <summary>
		/// Searches a section of the list for a given element using a binary search
		/// algorithm. Elements of the list are compared to the search value using
		/// the given IComparer interface. If comparer is null, elements of
		/// the list are compared to the search value using the IComparable
		/// interface, which in that case must be implemented by all elements of the
		/// list and the given search value. This method assumes that the given
		/// section of the list is already sorted; if this is not the case, the
		/// result will be incorrect.
		///
		/// The method returns the index of the given value in the list. If the
		/// list does not contain the given value, the method returns a negative
		/// integer. The bitwise complement operator (~) can be applied to a
		/// negative result to produce the index of the first element (if any) that
		/// is larger than the given search value. This is also the index at which
		/// the search value should be inserted into the list in order for the list
		/// to remain sorted.
		/// 
		/// The method uses the Array.BinarySearch method to perform the search.
		/// </summary>
		public int BinarySearch(int index, int count, [NotNull] T item, IComparer<T> comparer)
		{
			Count.ValidateRange(index, ref count);
			return Array.BinarySearch(Items, index, count, item, comparer);
		}

		/// <inheritdoc />
		public int IndexOf(T item) { return IndexOf(item, 0, -1); }

		/// <summary>
		/// Returns the index of the first occurrence of a given value in a range of
		/// this list. The list is searched forwards, starting at index
		/// index and ending at count number of elements. The
		/// elements of the list are compared to the given value using the
		/// Object.Equals method.
		/// 
		/// This method uses the Array.IndexOf method to perform the search.
		/// </summary>
		public int IndexOf(T item, int index) { return IndexOf(item, index, -1); }

		/// <summary>
		/// Returns the index of the first occurrence of a given value in a range of
		/// this list. The list is searched forwards, starting at index
		/// index and up to count number of elements. The
		/// elements of the list are compared to the given value using the
		/// Object.Equals method.
		/// 
		/// This method uses the Array.IndexOf method to perform the search.
		/// </summary>
		public int IndexOf(T item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			return Array.IndexOf(Items, item, index, count);
		}

		/// <inheritdoc />
		int IList.IndexOf(object value)
		{
			return ObjectHelper.IsCompatible<T>(value)
						? IndexOf((T)value)
						: -1;
		}

		/// <summary>
		/// Returns the index of the last occurrence of a given value in a range of
		/// this list. The list is searched backwards, starting at the end 
		/// and ending at the first element in the list. The elements of the list 
		/// are compared to the given value using the Object.Equals method.
		/// 
		/// This method uses the Array.LastIndexOf method to perform the search.
		/// </summary>
		public int LastIndexOf(T item)
		{
			return LastIndexOf(item, 0, -1);
		}

		/// <summary>
		/// Returns the index of the last occurrence of a given value in a range of
		/// this list. The list is searched backwards, starting at index
		/// index and ending at the first element in the list. The 
		/// elements of the list are compared to the given value using the 
		/// Object.Equals method.
		/// 
		/// This method uses the Array.LastIndexOf method to perform the search.
		/// </summary>
		public int LastIndexOf(T item, int index)
		{
			return LastIndexOf(item, index, -1);
		}

		/// <summary>
		/// Returns the index of the last occurrence of a given value in a range of
		/// this list. The list is searched backwards, starting at index
		/// index and up to count elements. The elements of
		/// the list are compared to the given value using the Object.Equals
		/// method.
		/// 
		/// This method uses the Array.LastIndexOf method to perform the search.
		/// </summary>
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

		/// <inheritdoc />
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
				if (!match(Items[i])) continue;
				return i;
			}

			return -1;
		}

		public T FindLast([NotNull] Predicate<T> match)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				T item = Items[i];
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
				if (!match(Items[i])) continue;
				return i;
			}

			return -1;
		}

		public bool Exists([NotNull] Predicate<T> match) { return FindIndex(match) != -1; }

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter)
		{
			for (int i = 0; i < Count; i++)
			{
				yield return converter(Items[i]);
			}
		}

		public void CopyTo([NotNull] T[] array) { CopyTo(array, 0); }
		public void CopyTo(T[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
		public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, ref count);
			if (count == 0) return;
			Count.ValidateRange(arrayIndex, ref count);
			// Delegate rest of error checking to Array.Copy.
			Array.Copy(Items, 0, array, arrayIndex, count);
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int arrayIndex)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

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
			if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));

			try
			{
				foreach (T item in this)
				{
					objects[arrayIndex++] = item;
				}
			}
			catch (ArrayTypeMismatchException)
			{
				throw new ArgumentException("Invalid array type", nameof(array));
			}
		}

		/// <summary>
		/// Creates and returns a new array containing the elements in this deque.
		/// </summary>
		[NotNull]
		public T[] ToArray()
		{
			T[] result = new T[Count];
			CopyTo(result, 0);
			return result;
		}

		public IEnumerable<T> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			int last = index + count;

			for (int i = index; i < last; i++)
			{
				yield return Items[i];
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

		public void Reverse() { Reverse(0, Count); }

		/// <summary>
		/// Reverses the elements in a range of this list. Following a call to this
		/// method, an element in the range given by index and count
		/// which was previously located at index i will now be located at
		/// index index + (index + count - i - 1).
		/// 
		/// This method uses the Array.Reverse method to reverse the elements.
		/// </summary>
		public void Reverse(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (Count < 2 || count < 2) return;
			Array.Reverse(Items, index, count);
			_version++;
		}

		public void Sort([NotNull] Comparison<T> comparison)
		{
			if (Count == 0) return;
			IComparer<T> comparer = new FunctorComparer<T>(comparison);
			Sort(0, Count, comparer);
		}

		public void Sort() { Sort(0, Count, null); }

		public void Sort(IComparer<T> comparer) { Sort(0, Count, comparer); }

		/// <summary>
		/// Sorts the elements in a section of this list. The sort compares the
		/// elements to each other using the given IComparer interface. If
		/// comparer is null, the elements are compared to each other using
		/// the IComparable interface, which in that case must be implemented by all
		/// elements of the list.
		/// 
		/// This method uses the Array.Sort method to sort the elements.
		/// </summary>
		public void Sort(int index, int count, IComparer<T> comparer)
		{
			Count.ValidateRange(index, ref count);
			Array.Sort(Items, index, count, comparer);
			_version++;
		}

		/// <summary>
		/// Sets the capacity of this list to the size of the list. This method can
		/// be used to minimize a list's memory overhead once it is known that no
		/// new elements will be added to the list. To completely clear a list and
		/// release all memory referenced by the list, execute the following
		/// statements:
		/// 
		/// list.Clear();
		/// list.TrimExcess();
		/// </summary>
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
				if (match(Items[i])) continue;
				return false;
			}
			return true;
		}

		protected void Insert(int index, T item, bool add)
		{
			if (add)
			{
				if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
				if (Count == Items.Length) EnsureCapacity(Count + 1);
				if (index < Count) Array.Copy(Items, index, Items, index + 1, Count - index);
				Count++;
			}
			else
			{
				if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			}

			Items[index] = item;
			_version++;
		}

		protected void InsertRange(int index, [NotNull] IEnumerable<T> enumerable)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			int count;

			switch (enumerable)
			{
				case IReadOnlyCollection<T> readOnlyCollection:
				{
					count = readOnlyCollection.Count;
					if (count == 0) return;
					EnsureCapacity(Count + count);
					if (index < Count) Array.Copy(Items, index, Items, index + count, Count - index);

					// If we're inserting a List into itself, we want to be able to deal with that.
					if (ReferenceEquals(this, readOnlyCollection))
					{
						// Copy first part of _items to insert location
						Array.Copy(Items, 0, Items, index, index);
						// Copy last part of _items back to inserted location
						Array.Copy(Items, index + count, Items, index * 2, Count - index);
					}
					else
					{
						foreach (T item in readOnlyCollection) 
							Items[index++] = item;
					}

					Count += count;
					_version++;
					break;
				}
				case ICollection<T> collection:
				{
					count = collection.Count;
					if (count == 0) return;
					EnsureCapacity(Count + count);
					if (index < Count) Array.Copy(Items, index, Items, index + count, Count - index);

					// If we're inserting a List into itself, we want to be able to deal with that.
					if (ReferenceEquals(this, collection))
					{
						// Copy first part of _items to insert location
						Array.Copy(Items, 0, Items, index, index);
						// Copy last part of _items back to inserted location
						Array.Copy(Items, index + count, Items, index * 2, Count - index);
					}
					else
					{
						collection.CopyTo(Items, index);
					}

					Count += count;
					_version++;
					break;
				}
				default:
				{
					count = enumerable.FastCount();
					if (count > 0) EnsureCapacity(Count + count);

					using (IEnumerator<T> enumerator = enumerable.GetEnumerator())
					{
						while (enumerator.MoveNext())
						{
							Insert(index++, enumerator.Current, true);
						}
					}
					break;
				}
			}
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected void EnsureCapacity(int min)
		{
			if (Items.Length >= min) return;
			Capacity = (Items.Length == 0 ? Constants.DEFAULT_CAPACITY : Items.Length * 2).NotBelow(min);
		}

		private T RemoveAtInternal(int index)
		{
			T item = Items[index];
			if (index < Count - 1) Array.Copy(Items, index + 1, Items, index, Count - (index + 1));
			Items[--Count] = default(T);
			_version++;
			return item;
		}

		private bool TryRemoveAtInternal(int index, out T item)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			item = Items[index];
			if (index < Count - 1) Array.Copy(Items, index + 1, Items, index, Count - (index + 1));
			Items[--Count] = default(T);
			_version++;
			return true;
		}

		[NotNull]
		public static IList<T> Synchronized(Deque<T> list)
		{
			return new SynchronizedList(list);
		}
	}
}