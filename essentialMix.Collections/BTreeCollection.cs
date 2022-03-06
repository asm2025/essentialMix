using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
public abstract class BTreeCollection<TBlock, TNode, T, TItem> : IList<TItem>, IReadOnlyList<TItem>, IList
	where TBlock : BTreeBase<TBlock, TNode, T>.BTreeBlockBase
	where TNode : class, ITreeNode<TNode, T>
{
	[Serializable]
	private class SynchronizedList : IList<TItem>
	{
		private readonly BTreeCollection<TBlock, TNode, T, TItem> _collection;

		private object _root;

		internal SynchronizedList(BTreeCollection<TBlock, TNode, T, TItem> collection)
		{
			_collection = collection;
			_root = ((ICollection)collection).SyncRoot;
		}

		public int Count
		{
			get
			{
				lock (_root)
				{
					return _collection.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				lock (_root)
				{
					return _collection.IsReadOnly;
				}
			}
		}

		public TItem this[int index]
		{
			get
			{
				lock (_root)
				{
					return _collection[index];
				}
			}
			set
			{
				lock (_root)
				{
					_collection[index] = value;
				}
			}
		}

		public void Add(TItem item)
		{
			lock (_root)
			{
				_collection.Add(item);
			}
		}

		public void Insert(int index, TItem item)
		{
			lock (_root)
			{
				_collection.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (_root)
			{
				_collection.RemoveAt(index);
			}
		}

		public bool Remove(TItem item)
		{
			lock (_root)
			{
				return _collection.Remove(item);
			}
		}

		public void Clear()
		{
			lock (_root)
			{
				_collection.Clear();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (_root)
			{
				return _collection.GetEnumerator();
			}
		}

		IEnumerator<TItem> IEnumerable<TItem>.GetEnumerator()
		{
			lock (_root)
			{
				return _collection.GetEnumerator();
			}
		}

		public int IndexOf(TItem item)
		{
			lock (_root)
			{
				return _collection.IndexOf(item);
			}
		}

		public bool Contains(TItem item)
		{
			lock (_root)
			{
				return _collection.Contains(item);
			}
		}

		public void CopyTo(TItem[] array, int arrayIndex)
		{
			lock (_root)
			{
				_collection.CopyTo(array, arrayIndex);
			}
		}
	}

	[Serializable]
	private struct Enumerator : IEnumerator<TItem>, IEnumerator
	{
		[NonSerialized]
		private readonly BTreeCollection<TBlock, TNode, T, TItem> _collection;
		private readonly int _version;

		private int _index;
		private TItem _current;

		internal Enumerator([NotNull] BTreeCollection<TBlock, TNode, T, TItem> collection)
		{
			_collection = collection;
			_version = collection._tree._version;
			_index = 0;
			_current = default(TItem);
		}

		public TItem Current
		{
			get
			{
				if (!_index.InRange(0, _collection.Count)) throw new InvalidOperationException();
				return _current;
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_version == _collection._tree._version && _index < _collection.Count)
			{
				_current = _collection._items[_index];
				_index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _collection._tree._version) throw new VersionChangedException();
			_index = _collection.Count + 1;
			_current = default(TItem);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _collection._tree._version) throw new VersionChangedException();
			_index = 0;
			_current = default(TItem);
		}
	}

	private BTreeBase<TBlock, TNode, T> _tree;
	private TItem[] _items;
	private object _root;

	protected BTreeCollection([NotNull] BTreeBase<TBlock, TNode, T> tree, int capacity)
	{
		if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
		_tree = tree;
		_root = ((ICollection)_tree).SyncRoot;
		_items = new TItem[capacity];
	}

	protected int InnerCapacity
	{
		get => _items.Length;
		set
		{
			if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
			if (value == _items.Length) return;

			if (value > 0)
			{
				TItem[] newItems = new TItem[value];
				if (Count > 0) Array.Copy(_items, 0, newItems, 0, Count);
				_items = newItems;
			}
			else
			{
				_items = Array.Empty<TItem>();
			}

			_tree._version++;
		}
	}

	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	public TItem this[int index]
	{
		get => _items[index];
		set => Insert(index, value, false);
	}

	object IList.this[int index]
	{
		get => this[index];
		set
		{
			if (!ObjectHelper.IsCompatible<TItem>(value)) throw new ArgumentException("Incompatible value.", nameof(value));
			Insert(index, (TItem)value, false);
		}
	}

	bool IList.IsFixedSize => false;

	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => _root;

	[NotNull]
	public ReadOnlyCollection<TItem> AsReadOnly() { return new ReadOnlyCollection<TItem>(this); }

	public void Insert(int index, TItem item) { Insert(index, item, true); }

	void IList.Insert(int index, object item)
	{
		if (!ObjectHelper.IsCompatible<TItem>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(index, (TItem)item, true);
	}

	public void Add(TItem item) { Insert(Count, item, true); }

	int IList.Add(object item)
	{
		if (!ObjectHelper.IsCompatible<TItem>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(Count, (TItem)item, true);
		return Count - 1;
	}

	public void RemoveAt(int index)
	{
		if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		if (index < Count - 1) Array.Copy(_items, index + 1, _items, index, Count - (index + 1));
		_items[Count - 1] = default(TItem);
		Count--;
		_tree.Count--;
		_tree._version++;
	}

	public bool Remove(TItem item)
	{
		int index = IndexOf(item);
		if (index < 0) return false;
		RemoveAt(index);
		return true;
	}

	void IList.Remove(object item)
	{
		if (!ObjectHelper.IsCompatible<TItem>(item)) return;
		Remove((TItem)item);
	}

	public void Clear()
	{
		if (Count == 0) return;
		Array.Clear(_items, 0, Count);
		Count = 0;
		_tree._version++;
	}

	public IEnumerator<TItem> GetEnumerator() { return new Enumerator(this); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public int BinarySearch([NotNull] TItem item) { return BinarySearch(0, Count, item, null); }
	public int BinarySearch([NotNull] TItem item, IComparer<TItem> comparer) { return BinarySearch(0, Count, item, comparer); }
	public int BinarySearch(int index, int count, [NotNull] TItem item, IComparer<TItem> comparer)
	{
		Count.ValidateRange(index, ref count);
		return Array.BinarySearch(_items, index, count, item, comparer);
	}

	public int IndexOf(TItem item) { return IndexOf(item, 0, -1); }
	public int IndexOf(TItem item, int index) { return IndexOf(item, index, -1); }
	public int IndexOf(TItem item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		return Array.IndexOf(_items, item, index, count);
	}

	int IList.IndexOf(object item)
	{
		return ObjectHelper.IsCompatible<TItem>(item)
					? IndexOf((TItem)item)
					: -1;
	}
	public int LastIndexOf(TItem item) { return LastIndexOf(item, 0, -1); }
	public int LastIndexOf(TItem item, int index) { return LastIndexOf(item, index, -1); }
	public int LastIndexOf(TItem item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (index == 0) index = Count - 1;
		return Count == 0
					? -1
					: Array.LastIndexOf(_items, item, index, count);
	}

	public bool Contains(TItem item) { return IndexOf(item, 0, -1) > -1; }
	bool IList.Contains(object item)
	{
		return ObjectHelper.IsCompatible<TItem>(item) && Contains((TItem)item);
	}

	public TItem Find([NotNull] Predicate<TItem> match)
	{
		for (int i = 0; i < Count; i++)
		{
			if (!match(_items[i])) continue;
			return _items[i];
		}
		return default(TItem);
	}

	public TItem FindLast([NotNull] Predicate<TItem> match)
	{
		for (int i = Count - 1; i >= 0; i--)
		{
			TItem item = _items[i];
			if (!match(item)) continue;
			return item;
		}
		return default(TItem);
	}

	public IEnumerable<TItem> FindAll([NotNull] Predicate<TItem> match)
	{
		for (int i = 0; i < Count; i++)
		{
			TItem item = _items[i];
			if (!match(item)) continue;
			yield return item;
		}
	}

	public int FindIndex([NotNull] Predicate<TItem> match) { return FindIndex(0, Count, match); }
	public int FindIndex(int startIndex, [NotNull] Predicate<TItem> match) { return FindIndex(startIndex, -1, match); }
	public int FindIndex(int startIndex, int count, [NotNull] Predicate<TItem> match)
	{
		Count.ValidateRange(startIndex, ref count);
		return Array.FindIndex(_items, startIndex, count, match);
	}

	public int FindLastIndex([NotNull] Predicate<TItem> match) { return FindLastIndex(0, -1, match); }
	public int FindLastIndex(int startIndex, [NotNull] Predicate<TItem> match) { return FindLastIndex(startIndex, -1, match); }
	public int FindLastIndex(int startIndex, int count, [NotNull] Predicate<TItem> match)
	{
		Count.ValidateRange(startIndex, ref count);
		return Array.FindLastIndex(_items, startIndex, count, match);
	}

	public void CopyTo([NotNull] TItem[] array) { CopyTo(array, 0); }
	public void CopyTo(TItem[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
	public void CopyTo([NotNull] TItem[] array, int arrayIndex, int count)
	{
		if (Count == 0) return;
		array.Length.ValidateRange(arrayIndex, ref count);
		if (count == 0) return;
		Count.ValidateRange(arrayIndex, ref count);
		Array.Copy(_items, 0, array, arrayIndex, count);
	}

	void ICollection.CopyTo(Array array, int arrayIndex)
	{
		if (array.Rank != 1) throw new RankException();
		if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
		if (Count == 0) return;

		if (array is TItem[] tArray)
		{
			CopyTo(tArray, arrayIndex);
			return;
		}

		array.Length.ValidateRange(arrayIndex, Count);

		Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
		Type sourceType = typeof(TItem);
		if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
		if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));

		try
		{
			foreach (TItem item in this)
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
	public TItem[] ToArray()
	{
		TItem[] array = new TItem[Count];
		Array.Copy(_items, 0, array, 0, Count);
		return array;
	}

	public IEnumerable<TItem> GetRange(int index, int count)
	{
		Count.ValidateRange(index, ref count);
		int last = index + count;

		for (int i = index; i < last; i++)
			yield return _items[i];
	}

	public void TrimExcess()
	{
		int threshold = (int)(_items.Length * 0.9);
		if (Count >= threshold) return;
		InnerCapacity = Count;
	}

	private void EnsureCapacity(int min)
	{
		if (min < 0) throw new ArgumentOutOfRangeException(nameof(min));
		if (_items.Length >= min) return;
		InnerCapacity = (_items.Length == 0
						? Constants.DEFAULT_CAPACITY
						: _items.Length * 2).NotBelow(min);
	}

	private void Insert(int index, TItem item, bool add)
	{
		if (add)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (Count == _items.Length) EnsureCapacity(Count + 1);
			if (index < Count) Array.Copy(_items, index, _items, index + 1, Count - index);
			Count++;
		}
		else
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		}

		_items[index] = item;
		_tree._version++;
	}

	[NotNull]
	public static IList<TItem> Synchronized(BTreeCollection<TBlock, TNode, T, TItem> block)
	{
		return new SynchronizedList(block);
	}
}
