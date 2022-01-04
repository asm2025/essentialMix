using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using essentialMix.Threading;
using JetBrains.Annotations;

namespace essentialMix.Collections;

// based on https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/list.cs
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
[Serializable]
public class ObservableList<T> : IList<T>, IReadOnlyList<T>, IList, INotifyPropertyChanged, INotifyCollectionChanged
{
	protected const string ITEMS_NAME = "Item[]";

	[Serializable]
	internal class SynchronizedList : IList<T>
	{
		private readonly ObservableList<T> _list;

		private object _root;

		internal SynchronizedList(ObservableList<T> list)
		{
			_list = list;
			_root = ((ICollection)list).SyncRoot;
		}

		public int Count
		{
			get
			{
				lock(_root)
				{
					return _list.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				lock(_root)
				{
					return ((ICollection<T>)_list).IsReadOnly;
				}
			}
		}

		public void Add(T item)
		{
			lock(_root)
			{
				_list.Add(item);
			}
		}

		public void Clear()
		{
			lock(_root)
			{
				_list.Clear();
			}
		}

		public bool Contains(T item)
		{
			lock(_root)
			{
				return _list.Contains(item);
			}
		}

		public void CopyTo(T[] array, int arrayIndex)
		{
			lock(_root)
			{
				_list.CopyTo(array, arrayIndex);
			}
		}

		public bool Remove(T item)
		{
			lock(_root)
			{
				return _list.Remove(item);
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock(_root)
			{
				return _list.GetEnumerator();
			}
		}

		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			lock(_root)
			{
				return ((IEnumerable<T>)_list).GetEnumerator();
			}
		}

		public T this[int index]
		{
			get
			{
				lock(_root)
				{
					return _list[index];
				}
			}
			set
			{
				lock(_root)
				{
					_list[index] = value;
				}
			}
		}

		public int IndexOf(T item)
		{
			lock(_root)
			{
				return _list.IndexOf(item);
			}
		}

		public void Insert(int index, T item)
		{
			lock(_root)
			{
				_list.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock(_root)
			{
				_list.RemoveAt(index);
			}
		}
	}

	[Serializable]
	public struct Enumerator : IEnumerator<T>, IEnumerator
	{
		[NonSerialized]
		private readonly ObservableList<T> _list;
		private readonly int _version;

		private int _index;

		internal Enumerator([NotNull] ObservableList<T> list)
		{
			_list = list;
			_version = list._version;
			_index = 0;
			Current = default(T);
		}

		public T Current { get; private set; }

		object IEnumerator.Current
		{
			get
			{
				if (!_index.InRange(0, _list.Count)) throw new InvalidOperationException();
				return Current;
			}
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_version == _list._version && _index < _list.Count)
			{
				Current = _list.Items[_index];
				_index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _list._version) throw new VersionChangedException();
			_index = _list.Count + 1;
			Current = default(T);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _list._version) throw new VersionChangedException();
			_index = 0;
			Current = default(T);
		}
	}

	private int _version;

	private SimpleMonitor _monitor = new SimpleMonitor();

	[NonSerialized]
	private object _syncRoot;

	public ObservableList()
		: this(0)
	{
	}

	public ObservableList(int capacity)
	{
		if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
		Items = capacity == 0
					? Array.Empty<T>()
					: new T[capacity];
	}

	public ObservableList([NotNull] IEnumerable<T> enumerable)
	{
		Items = Array.Empty<T>();
		InsertRange(0, enumerable);
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
				if (Count > 0) Array.Copy(Items, 0, newItems, 0, Count);
				Items = newItems;
			}
			else
			{
				Items = Array.Empty<T>();
			}

			_version++;
		}
	}

	// Read-only property describing how many elements are in the List.
	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	public T this[int index]
	{
		get => Items[index];
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

	public bool IsReadOnly => false;

	bool IList.IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	protected bool SuppressCollectionEvents { get; set; }

	[NotNull]
	protected T[] Items { get; private set; }

	event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
	{
		add => PropertyChanged += value;
		remove => PropertyChanged -= value;
	}

	public event PropertyChangedEventHandler PropertyChanged;
	public event NotifyCollectionChangedEventHandler CollectionChanged;

	[NotNull]
	public ReadOnlyCollection<T> AsReadOnly()
	{
		return new ReadOnlyCollection<T>(this);
	}

	public void Insert(int index, T item)
	{
		Insert(index, item, true);
	}

	void IList.Insert(int index, object item)
	{
		if (!ObjectHelper.IsCompatible<T>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(index, (T)item, true);
	}

	public void Add(T item) { Insert(Count, item, true); }

	int IList.Add(object item)
	{
		if (!ObjectHelper.IsCompatible<T>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(Count, (T)item, true);
		return Count - 1;
	}

	public void RemoveAt(int index)
	{
		CheckReentrancy();
		if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		T item = Items[index];
		if (index < Count - 1) Array.Copy(Items, index + 1, Items, index, Count - (index + 1));
		Items[Count] = default(T);
		Count--;
		_version++;
		if (SuppressCollectionEvents) return;
		OnPropertyChanged(nameof(Count));
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, item, index);
	}

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

	public void Clear()
	{
		CheckReentrancy();
		if (Count == 0) return;
		Array.Clear(Items, 0, Count); // Don't need to doc this but we clear the elements so that the gc can reclaim the references.
		Count = 0;
		_version++;
		if (SuppressCollectionEvents) return;
		OnPropertyChanged(nameof(Count));
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged();
	}

	public void AddRange([NotNull] IEnumerable<T> enumerable)
	{
		InsertRange(Count, enumerable);
	}

	public void InsertRange(int index, [NotNull] IEnumerable<T> enumerable)
	{
		CheckReentrancy();
		if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

		List<T> newItems = enumerable.ToList();
		if (newItems.Count == 0) return;
		EnsureCapacity(Count + newItems.Count);
		if (index < Count) Array.Copy(Items, index, Items, index + newItems.Count, Count - index);

		// If we're inserting a List into itself, we want to be able to deal with that.
		if (ReferenceEquals(this, enumerable))
		{
			// Copy first part of _items to insert location
			Array.Copy(Items, 0, Items, index, index);
			// Copy last part of _items back to inserted location
			Array.Copy(Items, index + newItems.Count, Items, index * 2, Count - index);
		}
		else
		{
			newItems.CopyTo(Items, index);
		}

		Count += newItems.Count;
		_version++;
		OnPropertyChanged(nameof(Count));
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged(NotifyCollectionChangedAction.Add, newItems, index);
	}

	public void RemoveRange(int index, int count)
	{
		CheckReentrancy();
		Count.ValidateRange(index, ref count);
		if (count == 0) return;
		T[] removed = Items.GetRange(index, count);
		if (index < Count) Array.Copy(Items, index + count, Items, index, Count - index);
		Array.Clear(Items, Count - count, count);
		Count -= count;
		_version++;
		OnPropertyChanged(nameof(Count));
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged(NotifyCollectionChangedAction.Remove, removed, index);
	}

	public int RemoveAll([NotNull] Predicate<T> match)
	{
		int freeIndex = 0;   // the first free slot in items array

		// Find the first item which needs to be removed.
		while (freeIndex < Count && !match(Items[freeIndex]))
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

	public int BinarySearch(int index, int count, [NotNull] T item, IComparer<T> comparer)
	{
		Count.ValidateRange(index, ref count);
		return Array.BinarySearch(Items, index, count, item, comparer);
	}

	public int IndexOf(T item) { return IndexOf(item, 0, -1); }
	public int IndexOf(T item, int index) { return IndexOf(item, index, -1); }
	public int IndexOf(T item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		return Array.IndexOf(Items, item, index, count);
	}

	int IList.IndexOf(object item)
	{
		return ObjectHelper.IsCompatible<T>(item)
					? IndexOf((T)item)
					: -1;
	}

	public int LastIndexOf(T item) { return LastIndexOf(item, 0, -1); }
	public int LastIndexOf(T item, int index) { return LastIndexOf(item, index, -1); }
	public int LastIndexOf(T item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (index == 0) index = Count - 1;
		return Count == 0
					? -1
					: Array.LastIndexOf(Items, item, index, count);
	}

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
			if (!match(Items[i])) continue;
			return Items[i];
		}
		return default(T);
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
		return Array.FindIndex(Items, startIndex, count, match);
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
		return Array.FindLastIndex(Items, startIndex, count, match);
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

	[NotNull]
	public T[] ToArray()
	{
		T[] array = new T[Count];
		Array.Copy(Items, 0, array, 0, Count);
		return array;
	}

	[NotNull]
	public T[] GetRange(int index, int count)
	{
		return Items.GetRange(index, count);
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
	public void Reverse(int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (Count < 2 || count < 2) return;
		Array.Reverse(Items, index, count);
		_version++;
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged();
	}

	public void Sort([NotNull] Comparison<T> comparison)
	{
		if (Count == 0) return;
		IComparer<T> comparer = new FunctorComparer<T>(comparison);
		Sort(0, Count, comparer);
	}
	public void Sort() { Sort(0, Count, null); }
	public void Sort(IComparer<T> comparer) { Sort(0, Count, comparer); }
	public void Sort(int index, int count, IComparer<T> comparer)
	{
		Count.ValidateRange(index, ref count);
		Array.Sort(Items, index, count, comparer);
		_version++;
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged();
	}

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

	[NotNull]
	public static IList<T> Synchronized(ObservableList<T> list)
	{
		return new SynchronizedList(list);
	}

	[NotifyPropertyChangedInvocator]
	protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
	{
		if (SuppressCollectionEvents) return;
		OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
	}

	protected virtual void OnPropertyChanged([NotNull] PropertyChangedEventArgs e)
	{
		if (SuppressCollectionEvents) return;
		PropertyChanged?.Invoke(this, e);
	}

	protected void OnCollectionChanged()
	{
		if (SuppressCollectionEvents) return;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
	}

	protected void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
	{
		if (SuppressCollectionEvents) return;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
	}

	protected void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
	{
		if (SuppressCollectionEvents) return;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
	}

	protected void OnCollectionChanged(NotifyCollectionChangedAction action, IList items, int startingIndex)
	{
		if (SuppressCollectionEvents) return;
		OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, items, startingIndex));
	}

	protected virtual void OnCollectionChanged([NotNull] NotifyCollectionChangedEventArgs e)
	{
		if (SuppressCollectionEvents) return;
		CollectionChanged?.Invoke(this, e);
	}

	protected IDisposable BlockReentrancy()
	{
		_monitor.Enter();
		return _monitor;
	}

	protected void CheckReentrancy()
	{
		if (!_monitor.Busy) return;
		// we can allow changes if there's only one listener - the problem
		// only arises if reentrant changes make the original event args
		// invalid for later listeners.  This keeps existing code working
		// (e.g. Selector.SelectedItems).
		if (CollectionChanged != null && CollectionChanged.GetInvocationList().Length > 1)
			throw new InvalidOperationException("Reentrancy not allowed.");
	}

	protected void EnsureCapacity(int min)
	{
		if (Items.Length >= min) return;
		Capacity = (Items.Length == 0 ? Constants.DEFAULT_CAPACITY : Items.Length * 2).NotBelow(min);
	}

	protected void Insert(int index, T item, bool add)
	{
		if (add)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (Count == Items.Length) EnsureCapacity(Count + 1);
			if (index < Count) Array.Copy(Items, index, Items, index + 1, Count - index);
			Items[index] = item;
			Count++;
			_version++;
			OnPropertyChanged(nameof(Count));
			OnPropertyChanged(ITEMS_NAME);
			OnCollectionChanged(NotifyCollectionChangedAction.Add, item, index);
			return;
		}

		if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		T oldItem = Items[index];
		Items[index] = item;
		_version++;
		OnPropertyChanged(nameof(Count));
		OnPropertyChanged(ITEMS_NAME);
		OnCollectionChanged(NotifyCollectionChangedAction.Replace, oldItem, item, index);
	}
}