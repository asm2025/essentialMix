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
public class BTreeBlockCollection<TBlock, TNode, T> : IList<TBlock>, IReadOnlyList<TBlock>, IList
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[Serializable]
	private class SynchronizedList : IList<TBlock>
	{
		private readonly BTreeBlockCollection<TBlock, TNode, T> _collection;

		private object _root;

		internal SynchronizedList(BTreeBlockCollection<TBlock, TNode, T> collection)
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

		public TBlock this[int index]
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

		public void Add(TBlock item)
		{
			lock (_root)
			{
				_collection.Add(item);
			}
		}

		public void Insert(int index, TBlock item)
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

		public bool Remove(TBlock item)
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

		IEnumerator<TBlock> IEnumerable<TBlock>.GetEnumerator()
		{
			lock (_root)
			{
				return _collection.GetEnumerator();
			}
		}

		public int IndexOf(TBlock item)
		{
			lock (_root)
			{
				return _collection.IndexOf(item);
			}
		}

		public bool Contains(TBlock item)
		{
			lock (_root)
			{
				return _collection.Contains(item);
			}
		}

		public void CopyTo(TBlock[] array, int arrayIndex)
		{
			lock (_root)
			{
				_collection.CopyTo(array, arrayIndex);
			}
		}
	}

	[Serializable]
	private struct Enumerator : IEnumerator<TBlock>, IEnumerator
	{
		[NonSerialized]
		private readonly BTreeBlockCollection<TBlock, TNode, T> _collection;
		private readonly int _version;

		private int _index;
		private TBlock _current;

		internal Enumerator([NotNull] BTreeBlockCollection<TBlock, TNode, T> collection)
		{
			_collection = collection;
			_version = collection._tree._version;
			_index = 0;
			_current = default(TBlock);
		}

		public TBlock Current
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
			_current = default(TBlock);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _collection._tree._version) throw new VersionChangedException();
			_index = 0;
			_current = default(TBlock);
		}
	}

	[Serializable]
	private struct LevelOrderEnumerator : IEnumerableEnumerator<TNode>
	{

	}

	[Serializable]
	private struct PreOrderEnumerator : IEnumerableEnumerator<TNode>
	{

	}

	[Serializable]
	private struct InOrderEnumerator : IEnumerableEnumerator<TNode>
	{

	}

	[Serializable]
	private struct PostOrderEnumerator : IEnumerableEnumerator<TNode>
	{

	}

	private BTreeBase<TBlock, TNode, T> _tree;
	private TBlock[] _items;
	private object _root;

	/// <inheritdoc />
	public BTreeBlockCollection([NotNull] BTreeBase<TBlock, TNode, T> tree)
		: this(tree, 0)
	{
	}

	public BTreeBlockCollection([NotNull] BTreeBase<TBlock, TNode, T> tree, int capacity)
	{
		if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
		_tree = tree;
		_root = ((ICollection)_tree).SyncRoot;
		_items = new TBlock[capacity];
	}

	public int Capacity
	{
		get => _items.Length;
		set
		{
			if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
			if (value == _items.Length) return;

			if (value > 0)
			{
				TBlock[] newItems = new TBlock[value];
				if (Count > 0) Array.Copy(_items, 0, newItems, 0, Count);
				_items = newItems;
			}
			else
			{
				_items = Array.Empty<TBlock>();
			}

			_tree._version++;
		}
	}

	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	public TBlock this[int index]
	{
		get => _items[index];
		set => Insert(index, value, false);
	}

	object IList.this[int index]
	{
		get => this[index];
		set
		{
			if (!ObjectHelper.IsCompatible<TBlock>(value)) throw new ArgumentException("Incompatible value.", nameof(value));
			Insert(index, (TBlock)value, false);
		}
	}

	bool IList.IsFixedSize => false;

	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => _root;

	[NotNull]
	public ReadOnlyCollection<TBlock> AsReadOnly() { return new ReadOnlyCollection<TBlock>(this); }

	public void Insert(int index, TBlock item) { Insert(index, item, true); }

	void IList.Insert(int index, object item)
	{
		if (!ObjectHelper.IsCompatible<TBlock>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(index, (TBlock)item, true);
	}

	public void Add(TBlock item) { Insert(Count, item, true); }

	int IList.Add(object item)
	{
		if (!ObjectHelper.IsCompatible<TBlock>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(Count, (TBlock)item, true);
		return Count - 1;
	}

	public void RemoveAt(int index)
	{
		if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		if (index < Count - 1) Array.Copy(_items, index + 1, _items, index, Count - (index + 1));
		_items[Count - 1] = default(TBlock);
		Count--;
		_tree.Count--;
		_tree._version++;
	}

	public bool Remove(TBlock item)
	{
		int index = IndexOf(item);
		if (index < 0) return false;
		RemoveAt(index);
		return true;
	}

	void IList.Remove(object item)
	{
		if (!ObjectHelper.IsCompatible<TBlock>(item)) return;
		Remove((TBlock)item);
	}

	public void Clear()
	{
		if (Count == 0) return;
		Array.Clear(_items, 0, Count);
		Count = 0;
		_tree._version++;
	}

	public void AddRange([NotNull] IEnumerable<TBlock> enumerable) { InsertRange(Count, enumerable); }

	public void InsertRange(int index, [NotNull] IEnumerable<TBlock> enumerable)
	{
		if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

		int count;

		switch (enumerable)
		{
			case IReadOnlyCollection<TBlock> readOnlyCollection:
			{
				count = readOnlyCollection.Count;
				if (count == 0) return;
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
					foreach (TBlock item in readOnlyCollection)
						_items[index++] = item;
				}

				Count += count;
				_tree._version++;
				break;
			}
			case ICollection<TBlock> collection:
			{
				count = collection.Count;
				if (count == 0) return;
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
				_tree._version++;
				break;
			}
			default:
			{
				count = enumerable.FastCount();
				if (count > 0) EnsureCapacity(Count + count);

				using (IEnumerator<TBlock> en = enumerable.GetEnumerator())
				{
					while (en.MoveNext())
					{
						Insert(index++, en.Current, true);
					}
				}
				break;
			}
		}
	}

	public void RemoveRange(int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (count == 0) return;
		if (index < Count) Array.Copy(_items, index + count, _items, index, Count - index);
		Array.Clear(_items, Count - count, count);
		Count -= count;
		_tree._version++;
	}

	public int RemoveAll([NotNull] Predicate<TBlock> match)
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

	public IEnumerator<TBlock> GetEnumerator() { return new Enumerator(this); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public int BinarySearch([NotNull] TBlock item) { return BinarySearch(0, Count, item, null); }
	public int BinarySearch([NotNull] TBlock item, IComparer<TBlock> comparer) { return BinarySearch(0, Count, item, comparer); }
	public int BinarySearch(int index, int count, [NotNull] TBlock item, IComparer<TBlock> comparer)
	{
		Count.ValidateRange(index, ref count);
		return Array.BinarySearch(_items, index, count, item, comparer);
	}

	public int IndexOf(TBlock item) { return IndexOf(item, 0, -1); }
	public int IndexOf(TBlock item, int index) { return IndexOf(item, index, -1); }
	public int IndexOf(TBlock item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		return Array.IndexOf(_items, item, index, count);
	}

	int IList.IndexOf(object item)
	{
		return ObjectHelper.IsCompatible<TBlock>(item)
					? IndexOf((TBlock)item)
					: -1;
	}
	public int LastIndexOf(TBlock item) { return LastIndexOf(item, 0, -1); }
	public int LastIndexOf(TBlock item, int index) { return LastIndexOf(item, index, -1); }
	public int LastIndexOf(TBlock item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (index == 0) index = Count - 1;
		return Count == 0
					? -1
					: Array.LastIndexOf(_items, item, index, count);
	}

	public bool Contains(TBlock item) { return IndexOf(item, 0, -1) > -1; }
	bool IList.Contains(object item)
	{
		return ObjectHelper.IsCompatible<TBlock>(item) && Contains((TBlock)item);
	}

	public TBlock Find([NotNull] Predicate<TBlock> match)
	{
		for (int i = 0; i < Count; i++)
		{
			if (!match(_items[i])) continue;
			return _items[i];
		}
		return default(TBlock);
	}

	public TBlock FindLast([NotNull] Predicate<TBlock> match)
	{
		for (int i = Count - 1; i >= 0; i--)
		{
			TBlock item = _items[i];
			if (!match(item)) continue;
			return item;
		}
		return default(TBlock);
	}

	public IEnumerable<TBlock> FindAll([NotNull] Predicate<TBlock> match)
	{
		for (int i = 0; i < Count; i++)
		{
			TBlock item = _items[i];
			if (!match(item)) continue;
			yield return item;
		}
	}

	public int FindIndex([NotNull] Predicate<TBlock> match) { return FindIndex(0, Count, match); }
	public int FindIndex(int startIndex, [NotNull] Predicate<TBlock> match) { return FindIndex(startIndex, -1, match); }
	public int FindIndex(int startIndex, int count, [NotNull] Predicate<TBlock> match)
	{
		Count.ValidateRange(startIndex, ref count);
		return Array.FindIndex(_items, startIndex, count, match);
	}

	public int FindLastIndex([NotNull] Predicate<TBlock> match) { return FindLastIndex(0, -1, match); }
	public int FindLastIndex(int startIndex, [NotNull] Predicate<TBlock> match) { return FindLastIndex(startIndex, -1, match); }
	public int FindLastIndex(int startIndex, int count, [NotNull] Predicate<TBlock> match)
	{
		Count.ValidateRange(startIndex, ref count);
		return Array.FindLastIndex(_items, startIndex, count, match);
	}

	public void CopyTo([NotNull] TBlock[] array) { CopyTo(array, 0); }
	public void CopyTo(TBlock[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
	public void CopyTo([NotNull] TBlock[] array, int arrayIndex, int count)
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

		if (array is TBlock[] tArray)
		{
			CopyTo(tArray, arrayIndex);
			return;
		}

		array.Length.ValidateRange(arrayIndex, Count);

		Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
		Type sourceType = typeof(TBlock);
		if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
		if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));

		try
		{
			foreach (TBlock item in this)
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
	public TBlock[] ToArray()
	{
		TBlock[] array = new TBlock[Count];
		Array.Copy(_items, 0, array, 0, Count);
		return array;
	}

	public IEnumerable<TBlock> GetRange(int index, int count)
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
		Capacity = Count;
	}

	private void EnsureCapacity(int min)
	{
		if (min < 0 || min < Count) throw new ArgumentOutOfRangeException(nameof(min));
		if (_items.Length >= min) return;
		Capacity = (_items.Length == 0
						? Constants.DEFAULT_CAPACITY
						: _items.Length * 2).NotBelow(min);
	}

	private void Insert(int index, TBlock item, bool add)
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
	public static IList<TBlock> Synchronized(BTreeBlockCollection<TBlock, TNode, T> block) { return new SynchronizedList(block); }
}