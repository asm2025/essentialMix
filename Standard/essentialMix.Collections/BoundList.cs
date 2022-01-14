using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Comparers;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class BoundList<T> : IBoundList<T>, IReadOnlyBoundList<T>, IList
{
	[Serializable]
	internal class SynchronizedList : IBoundList<T>
	{
		private readonly BoundList<T> _list;

		private object _root;

		internal SynchronizedList(BoundList<T> list)
		{
			_list = list;
			_root = ((ICollection)list).SyncRoot;
		}

		public int Count
		{
			get
			{
				lock (_root)
				{
					return _list.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				lock (_root)
				{
					return ((ICollection<T>)_list).IsReadOnly;
				}
			}
		}

		/// <inheritdoc />
		public int Capacity
		{
			get
			{
				lock (_root)
				{
					return _list.Capacity;
				}
			}
			set
			{
				lock (_root)
				{
					_list.Capacity = value;
				}
			}
		}

		/// <inheritdoc />
		public int Limit
		{
			get
			{
				lock (_root)
				{
					return _list.Limit;
				}
			}
		}

		public T this[int index]
		{
			get
			{
				lock (_root)
				{
					return _list[index];
				}
			}
			set
			{
				lock (_root)
				{
					_list[index] = value;
				}
			}
		}

		public void Add(T item)
		{
			lock (_root)
			{
				_list.Add(item);
			}
		}

		public void Insert(int index, T item)
		{
			lock (_root)
			{
				_list.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (_root)
			{
				_list.RemoveAt(index);
			}
		}

		public bool Remove(T item)
		{
			lock (_root)
			{
				return _list.Remove(item);
			}
		}

		public void Clear()
		{
			lock (_root)
			{
				_list.Clear();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (_root)
			{
				return _list.GetEnumerator();
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			lock (_root)
			{
				return ((IEnumerable<T>)_list).GetEnumerator();
			}
		}

		public int IndexOf(T item)
		{
			lock (_root)
			{
				return _list.IndexOf(item);
			}
		}

		public bool Contains(T item)
		{
			lock (_root)
			{
				return _list.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_root)
			{
				_list.CopyTo(array, arrayIndex);
			}
		}
	}

	[Serializable]
	private struct Enumerator : IEnumerator<T>, IEnumerator
	{
		[NonSerialized]
		private readonly BoundList<T> _list;
		private readonly int _version;

		private int _index;
		private T _current;

		internal Enumerator([NotNull] BoundList<T> list)
		{
			_list = list;
			_version = list._version;
			_index = 0;
			_current = default(T);
		}

		public T Current
		{
			get
			{
				if (!_index.InRange(0, _list.Count)) throw new InvalidOperationException();
				return _current;
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_version == _list._version && _index < _list.Count)
			{
				_current = _list._items[_index];
				_index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _list._version) throw new VersionChangedException();
			_index = _list.Count + 1;
			_current = default(T);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _list._version) throw new VersionChangedException();
			_index = 0;
			_current = default(T);
		}
	}

	private int _version;

	[NonSerialized]
	private object _syncRoot;

	[NotNull]
	private T[] _items;

	public BoundList()
		: this(1, 0)
	{
	}

	public BoundList(int limit)
		: this(limit, 0)
	{
	}

	public BoundList(int limit, int capacity)
	{
		if (limit < 1) throw new ArgumentOutOfRangeException(nameof(limit));
		if (capacity < 0 || capacity > limit) throw new ArgumentOutOfRangeException(nameof(capacity));
		Limit = limit;
		_items = capacity == 0
					? Array.Empty<T>()
					: new T[capacity];
	}

	public BoundList(int limit, [NotNull] IEnumerable<T> enumerable)
		: this(limit, 0)
	{
		InsertRange(0, enumerable);
	}

	public int Capacity
	{
		get => _items.Length;
		set
		{
			if (value > Limit || value < Count) throw new ArgumentOutOfRangeException(nameof(value));
			if (value == _items.Length) return;

			if (value > 0)
			{
				T[] newItems = new T[value];
				if (Count > 0) Array.Copy(_items, 0, newItems, 0, Count);
				_items = newItems;
			}
			else
			{
				_items = Array.Empty<T>();
			}

			_version++;
		}
	}

	public int Limit { get; }

	// Read-only property describing how many elements are in the List.
	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	// Sets or Gets the element at the given index.
	public T this[int index]
	{
		get => _items[index];
		set => Insert(index, value, false);
	}

	object IList.this[int index]
	{
		get => this[index];
		set
		{
			if (!ObjectHelper.IsCompatible<T>(value)) throw new ArgumentException("Incompatible value.", nameof(value));
			Insert(index, (T)value, false);
		}
	}

	bool IList.IsFixedSize => false;

	// Is this List read-only?
	public bool IsReadOnly => false;

	bool IList.IsReadOnly => false;

	// Is this List synchronized (thread-safe)?
	bool ICollection.IsSynchronized => false;

	// Synchronization root for this object.
	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	[NotNull]
	protected T[] Items => _items;

	[NotNull]
	public ReadOnlyCollection<T> AsReadOnly()
	{
		return new ReadOnlyCollection<T>(this);
	}

	// Inserts an element into this list at a given index. The size of the list
	// is increased by one. If required, the capacity of the list is doubled
	// before inserting the new element.
	public void Insert(int index, T item)
	{
		Insert(index, item, true);
	}

	void IList.Insert(int index, object item)
	{
		if (!ObjectHelper.IsCompatible<T>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(index, (T)item, true);
	}

	// Adds the given object to the end of this list. The size of the list is
	// increased by one. If required, the capacity of the list is doubled
	// before adding the new element.
	public void Add(T item) { Insert(Count, item, true); }

	int IList.Add(object item)
	{
		if (item is null && !typeof(T).IsClass) throw new ArgumentNullException(nameof(item), "Value cannot be null.");
		if (!ObjectHelper.IsCompatible<T>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(Count, (T)item, true);
		return Count - 1;
	}

	// Removes the element at the given index. The size of the list is
	// decreased by one.
	public void RemoveAt(int index)
	{
		if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		if (index < Count - 1) Array.Copy(_items, index + 1, _items, index, Count - (index + 1));
		_items[Count] = default(T);
		Count--;
		_version++;
	}

	// Removes the element at the given index. The size of the list is
	// decreased by one.
	public bool Remove(T item)
	{
		int index = IndexOf(item);
		if (index < 0) return false;
		RemoveAt(index);
		return true;
	}

	void IList.Remove(object item)
	{
		if (!ObjectHelper.IsCompatible<T>(item)) return;
		Remove((T)item);
	}

	// Clears the contents of List.
	public void Clear()
	{
		if (Count == 0) return;
		Array.Clear(_items, 0, Count); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
		Count = 0;
		_version++;
	}

	// Adds the elements of the given collection to the end of this list. If
	// required, the capacity of the list is increased to twice the previous
	// capacity or the new size, whichever is larger.
	//
	public void AddRange([NotNull] IEnumerable<T> enumerable)
	{
		InsertRange(Count, enumerable);
	}

	// Inserts the elements of the given collection at a given index. If
	// required, the capacity of the list is increased to twice the previous
	// capacity or the new size, whichever is larger.  Ranges may be added
	// to the end of the list by setting index to the List's size.
	public void InsertRange(int index, [NotNull] IEnumerable<T> enumerable)
	{
		if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

		int count;

		switch (enumerable)
		{
			case IReadOnlyCollection<T> readOnlyCollection:
			{
				count = readOnlyCollection.Count;
				if (count == 0) return;
				if (Count + count > Limit) count = Limit - Count;
				EnsureCapacity(Count + count);
				if (index < Count) Array.Copy(_items, index, _items, index + count, Count - index);

				// If we're inserting a List into itself, we want to be able to deal with that.
				if (ReferenceEquals(this, readOnlyCollection))
				{
					// Copy first part of _items to insert location
					Array.Copy(_items, 0, _items, index, index);
					// Copy last part of _items back to inserted location
					Array.Copy(_items, index + count, _items, index * 2, Count - index);
				}
				else
				{
					foreach (T item in readOnlyCollection)
						_items[index++] = item;
				}

				Count += count;
				_version++;
				break;
			}
			case ICollection<T> collection:
			{
				count = collection.Count;
				if (count == 0) return;
				if (Count + count > Limit) count = Limit - Count;
				EnsureCapacity(Count + count);
				if (index < Count) Array.Copy(_items, index, _items, index + count, Count - index);

				// If we're inserting a List into itself, we want to be able to deal with that.
				if (ReferenceEquals(this, collection))
				{
					// Copy first part of _items to insert location
					Array.Copy(_items, 0, _items, index, index);
					// Copy last part of _items back to inserted location
					Array.Copy(_items, index + count, _items, index * 2, Count - index);
				}
				else
				{
					collection.CopyTo(_items, index);
				}

				Count += count;
				_version++;
				break;
			}
			default:
			{
				count = enumerable.FastCount();
				if (count > 0) EnsureCapacity(Count + count);

				using (IEnumerator<T> en = enumerable.GetEnumerator())
				{
					while (Count < Limit && en.MoveNext())
					{
						Insert(index++, en.Current, true);
					}
				}
				break;
			}
		}
	}

	// Removes a range of elements from this list.
	public void RemoveRange(int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (count == 0) return;
		if (index < Count) Array.Copy(_items, index + count, _items, index, Count - index);
		Array.Clear(_items, Count - count, count);
		Count -= count;
		_version++;
	}

	// This method removes all items which matches the predicate.
	// The complexity is O(n).   
	public int RemoveAll([NotNull] Predicate<T> match)
	{
		int freeIndex = 0;   // the first free slot in items array

		// Find the first item which needs to be removed.
		while (freeIndex < Count && !match(_items[freeIndex]))
			freeIndex++;

		if (freeIndex >= Count) return 0;
		int count = Count - freeIndex;
		RemoveRange(freeIndex, count);
		return count;
	}

	public IEnumerator<T> GetEnumerator()
	{
		return new Enumerator(this);
	}

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
		return Array.BinarySearch(_items, index, count, item, comparer);
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
		return Array.IndexOf(_items, item, index, count);
	}

	int IList.IndexOf(object item)
	{
		return ObjectHelper.IsCompatible<T>(item)
					? IndexOf((T)item)
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
					: Array.LastIndexOf(_items, item, index, count);
	}

	// Contains returns true if the specified element is in the List.
	// It does a linear, O(n) search.  Equality is determined by calling
	// item.Equals().
	public bool Contains(T item)
	{
		return IndexOf(item, 0, -1) > -1;
	}

	bool IList.Contains(object item)
	{
		return ObjectHelper.IsCompatible<T>(item) && Contains((T)item);
	}

	public T Find([NotNull] Predicate<T> match)
	{
		for (int i = 0; i < Count; i++)
		{
			if (!match(_items[i])) continue;
			return _items[i];
		}
		return default(T);
	}

	public T FindLast([NotNull] Predicate<T> match)
	{
		for (int i = Count - 1; i >= 0; i--)
		{
			T item = _items[i];
			if (!match(item)) continue;
			return item;
		}
		return default(T);
	}

	public IEnumerable<T> FindAll([NotNull] Predicate<T> match)
	{
		for (int i = 0; i < Count; i++)
		{
			T item = _items[i];
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
		return Array.FindIndex(_items, startIndex, count, match);
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
		return Array.FindLastIndex(_items, startIndex, count, match);
	}

	public bool Exists([NotNull] Predicate<T> match) { return FindIndex(match) != -1; }

	public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<T, TOutput> converter)
	{
		for (int i = 0; i < Count; i++)
		{
			yield return converter(_items[i]);
		}
	}

	// Copies this List into array, which must be of a 
	// compatible array type.  
	public void CopyTo([NotNull] T[] array) { CopyTo(array, 0); }
	public void CopyTo(T[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
	public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
	{
		if (Count == 0) return;
		array.Length.ValidateRange(arrayIndex, ref count);
		if (count == 0) return;
		Count.ValidateRange(arrayIndex, ref count);
		// Delegate rest of error checking to Array.Copy.
		Array.Copy(_items, 0, array, arrayIndex, count);
	}

	// Copies this List into array, which must be of a 
	// compatible array type.  
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

	// ToArray returns a new Object array containing the contents of the List.
	// This requires copying the List, which is an O(n) operation.
	[NotNull]
	public T[] ToArray()
	{
		T[] array = new T[Count];
		Array.Copy(_items, 0, array, 0, Count);
		return array;
	}

	public IEnumerable<T> GetRange(int index, int count)
	{
		Count.ValidateRange(index, ref count);
		int last = index + count;

		for (int i = index; i < last; i++)
		{
			yield return _items[i];
		}
	}

	public void ForEach([NotNull] Action<T> action)
	{
		int version = _version;

		for (int i = 0; i < Count; i++)
		{
			if (version != _version) break;
			action(_items[i]);
		}

		if (version != _version) throw new VersionChangedException();
	}

	public void ForEach([NotNull] Action<T, int> action)
	{
		int version = _version;

		for (int i = 0; i < Count; i++)
		{
			if (version != _version) break;
			action(_items[i], i);
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
		Array.Reverse(_items, index, count);
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
	public void Sort()
	{
		Sort(0, Count, null);
	}

	// Sorts the elements in this list.  Uses Array.Sort with the
	// provided comparer.
	public void Sort(IComparer<T> comparer)
	{
		Sort(0, Count, comparer);
	}

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
		Array.Sort(_items, index, count, comparer);
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
		int threshold = (int)(_items.Length * 0.9);
		if (Count >= threshold) return;
		Capacity = Count;
	}

	public bool TrueForAll([NotNull] Predicate<T> match)
	{
		for (int i = 0; i < Count; i++)
		{
			if (match(_items[i])) continue;
			return false;
		}
		return true;
	}

	[NotNull]
	public static IBoundList<T> Synchronized(BoundList<T> list)
	{
		return new SynchronizedList(list);
	}

	// Ensures that the capacity of this list is at least the given minimum
	// value. If the current capacity of the list is less than min, the
	// capacity is increased to twice the current capacity or to min,
	// whichever is larger.
	protected void EnsureCapacity(int min)
	{
		if (min < 0) throw new ArgumentOutOfRangeException(nameof(min));
		if (min > Limit) min = Limit;
		if (_items.Length >= min) return;
		Capacity = (_items.Length == 0
						? Constants.DEFAULT_CAPACITY
						: _items.Length * 2).Within(min, Limit);
	}

	protected void Insert(int index, T item, bool add)
	{
		if (add)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (Count == Limit) throw new LimitReachedException();
			if (Count == _items.Length) EnsureCapacity(Count + 1);
			if (index < Count) Array.Copy(_items, index, _items, index + 1, Count - index);
			Count++;
		}
		else
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		}

		_items[index] = item;
		_version++;
	}
}
