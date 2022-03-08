using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using essentialMix.Helpers;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc cref="IBTreeBlock{TBlock,TNode,T}" />
[Serializable]
[DebuggerDisplay("{Degree}, Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
public abstract class BTreeBlockBase<TBlock, TNode, T> : IBTreeBlockBase<TBlock, TNode, T>, IReadOnlyList<TNode>, IList
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[Serializable]
	private class SynchronizedList : IList<TNode>
	{
		private readonly BTreeBlockBase<TBlock, TNode, T> _block;

		private object _root;

		internal SynchronizedList(BTreeBlockBase<TBlock, TNode, T> block)
		{
			_block = block;
			_root = ((ICollection)block).SyncRoot;
		}

		public int Count
		{
			get
			{
				lock (_root)
				{
					return _block.Count;
				}
			}
		}

		public bool IsReadOnly
		{
			get
			{
				lock (_root)
				{
					return _block.IsReadOnly;
				}
			}
		}

		public TNode this[int index]
		{
			get
			{
				lock (_root)
				{
					return _block[index];
				}
			}
			set
			{
				lock (_root)
				{
					_block[index] = value;
				}
			}
		}

		public void Add(TNode item)
		{
			lock (_root)
			{
				_block.Add(item);
			}
		}

		public void Insert(int index, TNode item)
		{
			lock (_root)
			{
				_block.Insert(index, item);
			}
		}

		public void RemoveAt(int index)
		{
			lock (_root)
			{
				_block.RemoveAt(index);
			}
		}

		public bool Remove(TNode item)
		{
			lock (_root)
			{
				return _block.Remove(item);
			}
		}

		public void Clear()
		{
			lock (_root)
			{
				_block.Clear();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			lock (_root)
			{
				return _block.GetEnumerator();
			}
		}

		IEnumerator<TNode> IEnumerable<TNode>.GetEnumerator()
		{
			lock (_root)
			{
				return _block.GetEnumerator();
			}
		}

		public int IndexOf(TNode item)
		{
			lock (_root)
			{
				return _block.IndexOf(item);
			}
		}

		public bool Contains(TNode item)
		{
			lock (_root)
			{
				return _block.Contains(item);
			}
		}

		public void CopyTo(TNode[] array, int arrayIndex)
		{
			lock (_root)
			{
				_block.CopyTo(array, arrayIndex);
			}
		}
	}

	[Serializable]
	private struct Enumerator : IEnumerator<TNode>, IEnumerator
	{
		[NonSerialized]
		private readonly BTreeBlockBase<TBlock, TNode, T> _collection;
		private readonly int _version;

		private int _index;
		private TNode _current;

		internal Enumerator([NotNull] BTreeBlockBase<TBlock, TNode, T> collection)
		{
			_collection = collection;
			_version = collection._tree._version;
			_index = 0;
			_current = default(TNode);
		}

		public TNode Current
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
			_current = default(TNode);
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _collection._tree._version) throw new VersionChangedException();
			_index = 0;
			_current = default(TNode);
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

	private readonly int _minEntries;
	private readonly int _maxEntries;

	private TNode[] _items;
	private object _root;
	private BTreeBase<TBlock, TNode, T> _tree;
	private BTreeBlockCollection<TBlock, TNode, T> _children;

	protected BTreeBlockBase([NotNull] BTreeBase<TBlock, TNode, T> tree, int degree)
	{
		if (degree < BTree.MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
		Degree = degree;
		_tree = tree;
		_root = ((ICollection)_tree).SyncRoot;
		_items = new TNode[degree];
		_minEntries = BTree.FastMinimumEntries(degree);
		_maxEntries = BTree.FastMaximumEntries(degree);
	}

	[field: ContractPublicPropertyName("Count")]
	public int Count { get; private set; }

	public TNode this[int index]
	{
		get => _items[index];
		set => Insert(index, value, false);
	}

	object IList.this[int index]
	{
		get => this[index];
		set
		{
			if (!ObjectHelper.IsCompatible<TNode>(value)) throw new ArgumentException("Incompatible value.", nameof(value));
			Insert(index, (TNode)value, false);
		}
	}

	bool IList.IsFixedSize => false;

	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot => _root;

	/// <inheritdoc />
	public BTreeBlockCollection<TBlock, TNode, T> Children
	{
		get => _children;
		set => _children = value;
	}

	/// <inheritdoc />
	public int Degree { get; }

	/// <inheritdoc />
	public bool IsLeaf => _children == null || _children.Count == 0;

	/// <inheritdoc />
	public bool IsEmpty => Count == 0;

	/// <inheritdoc />
	public bool IsFull => Count >= _maxEntries;

	/// <inheritdoc />
	public bool HasMinimumEntries => Count >= _minEntries;

	/// <inheritdoc />
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public void EnsureChildren() { _children ??= new BTreeBlockCollection<TBlock, TNode, T>(_tree); }

	[NotNull]
	public ReadOnlyCollection<TNode> AsReadOnly() { return new ReadOnlyCollection<TNode>(this); }

	public void Insert(int index, TNode item) { Insert(index, item, true); }

	void IList.Insert(int index, object item)
	{
		if (!ObjectHelper.IsCompatible<TNode>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(index, (TNode)item, true);
	}

	public void Add(TNode item) { Insert(Count, item, true); }

	int IList.Add(object item)
	{
		if (!ObjectHelper.IsCompatible<TNode>(item)) throw new ArgumentException("Incompatible value.", nameof(item));
		Insert(Count, (TNode)item, true);
		return Count - 1;
	}

	public void RemoveAt(int index)
	{
		if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		if (index < Count - 1) Array.Copy(_items, index + 1, _items, index, Count - (index + 1));
		_items[Count - 1] = default(TNode);
		Count--;
		_tree.Count--;
		_tree._version++;
	}

	public bool Remove(TNode item)
	{
		int index = IndexOf(item);
		if (index < 0) return false;
		RemoveAt(index);
		return true;
	}

	void IList.Remove(object item)
	{
		if (!ObjectHelper.IsCompatible<TNode>(item)) return;
		Remove((TNode)item);
	}

	public void Clear()
	{
		if (Count == 0) return;
		Array.Clear(_items, 0, Count);
		_tree.Count -= Count;
		Count = 0;
		_tree._version++;
	}

	public void AddRange([NotNull] IEnumerable<TNode> enumerable) { InsertRange(Count, enumerable); }

	public void InsertRange(int index, [NotNull] IEnumerable<TNode> enumerable)
	{
		if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

		int count;

		switch (enumerable)
		{
			case IReadOnlyCollection<TNode> readOnlyCollection:
			{
				count = readOnlyCollection.Count;
				if (Count + count > Degree) count = Degree - Count;
				if (count <= 0) return;
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
					foreach (TNode item in readOnlyCollection)
						_items[index++] = item;
				}

				Count += count;
				_tree.Count += count;
				_tree._version++;
				break;
			}
			case ICollection<TNode> collection:
			{
				count = collection.Count;
				if (Count + count > Degree) count = Degree - Count;
				if (count <= 0) return;
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
				_tree.Count += count;
				_tree._version++;
				break;
			}
			default:
			{
				count = enumerable.FastCount();

				if (count > 0)
				{
					if (Count + count > Degree) count = Degree - Count;
					if (count <= 0) return;
				}

				using (IEnumerator<TNode> en = enumerable.GetEnumerator())
				{
					while (count > 0 && en.MoveNext())
					{
						Insert(index++, en.Current, true);
						count--;
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
		_tree._version++;
		_tree.Count -= count;
		Count -= count;
	}

	public int RemoveAll([NotNull] Predicate<TNode> match)
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

	public IEnumerator<TNode> GetEnumerator() { return new Enumerator(this); }

	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	public int BinarySearch([NotNull] TNode item) { return BinarySearch(0, Count, item, null); }
	public int BinarySearch([NotNull] TNode item, IComparer<TNode> comparer) { return BinarySearch(0, Count, item, comparer); }
	public int BinarySearch(int index, int count, [NotNull] TNode item, IComparer<TNode> comparer)
	{
		Count.ValidateRange(index, ref count);
		return Array.BinarySearch(_items, index, count, item, comparer);
	}

	public int IndexOf(TNode item) { return IndexOf(item, 0, -1); }
	public int IndexOf(TNode item, int index) { return IndexOf(item, index, -1); }
	public int IndexOf(TNode item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		return Array.IndexOf(_items, item, index, count);
	}

	int IList.IndexOf(object item)
	{
		return ObjectHelper.IsCompatible<TNode>(item)
					? IndexOf((TNode)item)
					: -1;
	}
	public int LastIndexOf(TNode item) { return LastIndexOf(item, 0, -1); }
	public int LastIndexOf(TNode item, int index) { return LastIndexOf(item, index, -1); }
	public int LastIndexOf(TNode item, int index, int count)
	{
		Count.ValidateRange(index, ref count);
		if (index == 0) index = Count - 1;
		return Count == 0
					? -1
					: Array.LastIndexOf(_items, item, index, count);
	}

	public bool Contains(TNode item) { return IndexOf(item, 0, -1) > -1; }
	bool IList.Contains(object item)
	{
		return ObjectHelper.IsCompatible<TNode>(item) && Contains((TNode)item);
	}

	public TNode Find(Predicate<TNode> match)
	{
		for (int i = 0; i < Count; i++)
		{
			if (!match(_items[i])) continue;
			return _items[i];
		}
		return default(TNode);
	}

	public TNode FindLast(Predicate<TNode> match)
	{
		for (int i = Count - 1; i >= 0; i--)
		{
			TNode item = _items[i];
			if (!match(item)) continue;
			return item;
		}
		return default(TNode);
	}

	public IEnumerable<TNode> FindAll([NotNull] Predicate<TNode> match)
	{
		for (int i = 0; i < Count; i++)
		{
			TNode item = _items[i];
			if (!match(item)) continue;
			yield return item;
		}
	}

	public int FindIndex(Predicate<TNode> match) { return FindIndex(0, Count, match); }
	public int FindIndex(int startIndex, Predicate<TNode> match) { return FindIndex(startIndex, -1, match); }
	public int FindIndex(int startIndex, int count, [NotNull] Predicate<TNode> match)
	{
		Count.ValidateRange(startIndex, ref count);
		return Array.FindIndex(_items, startIndex, count, match);
	}

	public int FindLastIndex(Predicate<TNode> match) { return FindLastIndex(0, -1, match); }
	public int FindLastIndex(int startIndex, Predicate<TNode> match) { return FindLastIndex(startIndex, -1, match); }
	public int FindLastIndex(int startIndex, int count, Predicate<TNode> match)
	{
		Count.ValidateRange(startIndex, ref count);
		return Array.FindLastIndex(_items, startIndex, count, match);
	}

	public void CopyTo([NotNull] TNode[] array) { CopyTo(array, 0); }
	public void CopyTo(TNode[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
	public void CopyTo([NotNull] TNode[] array, int arrayIndex, int count)
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

		if (array is TNode[] tArray)
		{
			CopyTo(tArray, arrayIndex);
			return;
		}

		array.Length.ValidateRange(arrayIndex, Count);

		Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
		Type sourceType = typeof(TNode);
		if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
		if (array is not object[] objects) throw new ArgumentException("Invalid array type", nameof(array));

		try
		{
			foreach (TNode item in this)
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
	public TNode[] ToArray()
	{
		TNode[] array = new TNode[Count];
		Array.Copy(_items, 0, array, 0, Count);
		return array;
	}

	public IEnumerable<TNode> GetRange(int index, int count)
	{
		Count.ValidateRange(index, ref count);

		int last = index + count;

		for (int i = index; i < last; i++)
			yield return _items[i];
	}

	private void Insert(int index, TNode item, bool add)
	{
		if (add)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (Count == _items.Length) throw new LimitReachedException();
			if (index < Count) Array.Copy(_items, index, _items, index + 1, Count - index);
			Count++;
			_tree.Count++;
		}
		else
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
		}

		_items[index] = item;
		_tree._version++;
	}

	[NotNull]
	public static IList<TNode> Synchronized(BTreeBlockBase<TBlock, TNode, T> block)
	{
		return new SynchronizedList(block);
	}
}

[Serializable]
public sealed class BTreeBlock<T> : BTreeBlockBase<BTreeBlock<T>, BTreeNode<T>, T>, IBTreeBlock<BTreeBlock<T>, BTreeNode<T>, T>
{
	/// <inheritdoc />
	internal BTreeBlock(BTreeBase<BTreeBlock<T>, BTreeNode<T>, T> tree, int degree)
		: base(tree, degree)
	{
	}
}

[Serializable]
public sealed class BTreeBlock<TKey, TValue> : BTreeBlockBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue>, IBTreeBlock<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TKey, TValue>
{
	/// <inheritdoc />
	internal BTreeBlock(BTreeBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue> tree, int degree)
		: base(tree, degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<TKey, TValue> MakeNode(TKey key, TValue value) { return new BTreeNode<TKey, TValue>(key, value); }
}
