using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using essentialMix.Collections.DebugView;
using essentialMix.Comparers;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/*
* A B-tree of order m is a tree which satisfies the following properties:
* 1. The nodes in a B-tree of order m can have a maximum of m children.
* 2. Each internal node (non-leaf and non-root) can have at least (m/2) children.
* 3. The root has at least two children if it is not a leaf node.
* 4. A non-leaf node with k children contains k − 1 keys.
* 5. All leaves appear in the same level and carry no information.
*
* For a B-Tree of order 3
*	     ___ D ______
*	    /          \ \
*	   B            F H __
*	 /   \        /  |  \ \
*	A     C      E   G   I J
*
*	    BFS                  DFS                  DFS                   DFS
*	  LevelOrder           PreOrder              InOrder              PostOrder
*	    1 _____               1 _____                4 _____               10 ____
*	   /     \ \             /     \ \              /     \ \             /     \ \
*	  2       3 4 __        2       5 8 __         2       6 8 __        3       6 9 __
*	 /  \    / |  \ \      /  \    / |  \ \       /  \    / |  \ \      /  \    / |  \ \
*	5    6  7  8   9 10   3    4  6  7  9  10    1    3  5  7   9 10   1    2  4  5   7 8
*
* BFS (LevelOrder): DBFHACEGIJ => Root-Left-Right (Queue)
* DFS [PreOrder]:   DBACFEGHIJ => Root-Left-Right (Stack)
* DFS [InOrder]:    ABCDEFGHIJ => Left-Root-Right (Stack)
* DFS [PostOrder]:  ACBEGFIJHD => Left-Right-Root (Stack)
*/
/// <summary>
/// <see href="https://en.wikipedia.org/wiki/B-tree">B-tree</see> using the linked representation.
/// See a brief overview at <see href="https://www.youtube.com/watch?v=aZjYr87r1b8">10.2  B Trees and B+ Trees. How they are useful in Databases</see>.
/// <para>Based on BTree chapter in "Introduction to Algorithms", by Thomas Cormen, Charles Leiserson, Ronald Rivest.</para>
/// <para>This uses the same abstract pattern similar to <see cref="LinkedBinaryTree{Entry,T}"/></para>
/// <para>
/// A B-tree of order m is a tree which satisfies the following properties:
/// <list type="number">
/// <item>Every node has at most m children.</item>
/// <item>Every internal node except the root has at least ⌈m/2⌉ children.</item>
/// <item>Every non-leaf node has at least two children.</item>
/// <item>All leaves appear on the same level and carry no information.</item>
/// <item>A non-leaf node with k children contains k−1 nodes.</item>
/// </list>
/// </para>
/// </summary>
/// <typeparam name="T">The element type of the tree</typeparam>
[Serializable]
[DebuggerDisplay("Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_BTreeDebugView<>))]
public class BTree<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection
{
	[Serializable]
	[DebuggerDisplay("{Degree}, Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
	public sealed class Node : IReadOnlyList<Entry>
	{
		[Serializable]
		private struct Enumerator : IEnumerator<Entry>, IEnumerator
		{
			private readonly Node _node;
			private readonly int _version;

			private int _index;
			private Entry _current;

			internal Enumerator([NotNull] Node node)
			{
				_node = node;
				_version = node._tree._version;
				_index = 0;
				_current = default(Entry);
			}

			public Entry Current
			{
				get
				{
					if (!_index.InRange(0, _node.Count)) throw new InvalidOperationException();
					return _current;
				}
			}

			object IEnumerator.Current => Current;

			public void Dispose()
			{
			}

			public bool MoveNext()
			{
				if (_version == _node._tree._version && _index < _node.Count)
				{
					_current = _node._items[_index];
					_index++;
					return true;
				}
				return MoveNextRare();
			}

			private bool MoveNextRare()
			{
				if (_version != _node._tree._version) throw new VersionChangedException();
				_index = _node.Count + 1;
				_current = default(Entry);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (_version != _node._tree._version) throw new VersionChangedException();
				_index = 0;
				_current = default(Entry);
			}
		}

		private readonly int _minEntries;
		private readonly int _maxEntries;
		private readonly BTree<T> _tree;

		private Entry[] _items;
		private NodeCollection _children;

		internal Node(BTree<T> tree, int degree)
		{
			if (degree < BTree.MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
			_tree = tree;
			Degree = degree;
			_items = new Entry[degree];
			_minEntries = BTree.FastMinimumEntries(degree);
			_maxEntries = BTree.FastMaximumEntries(degree);
		}

		[field: ContractPublicPropertyName("Count")]
		public int Count { get; private set; }

		public Entry this[int index]
		{
			get => _items[index];
			internal set => Insert(index, value, false);
		}

		public NodeCollection Children
		{
			get => _children;
			internal set => _children = value;
		}

		public int Degree { get; }

		public bool IsLeaf => _children == null || _children.Count == 0;

		public bool IsEmpty => Count == 0;

		public bool IsFull => Count >= _maxEntries;

		public bool HasMinimumEntries => Count >= _minEntries;

		public IEnumerator<Entry> GetEnumerator() { return new Enumerator(this); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		internal void Insert(int index, Entry item) { Insert(index, item, true); }

		internal void Add(Entry item) { Insert(Count, item, true); }

		internal void RemoveAt(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (index < Count - 1) Array.Copy(_items, index + 1, _items, index, Count - (index + 1));
			_items[Count - 1] = default(Entry);
			Count--;
			_tree.Count--;
			_tree._version++;
		}

		internal bool Remove(Entry item)
		{
			int index = IndexOf(item);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		internal void Clear()
		{
			if (Count == 0) return;
			Array.Clear(_items, 0, Count);
			_tree.Count -= Count;
			Count = 0;
			_tree._version++;
		}

		internal void AddRange([NotNull] IEnumerable<Entry> enumerable) { InsertRange(Count, enumerable); }

		internal void InsertRange(int index, [NotNull] IEnumerable<Entry> enumerable)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			int count;

			switch (enumerable)
			{
				case IReadOnlyCollection<Entry> readOnlyCollection:
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
						foreach (Entry item in readOnlyCollection)
							_items[index++] = item;
					}

					Count += count;
					_tree.Count += count;
					_tree._version++;
					break;
				}
				case ICollection<Entry> collection:
				{
					count = collection.Count;
					if (Count + count > Degree) count = Degree - Count;
					if (count <= 0) return;
					if (index < Count) Array.Copy(_items, index, _items, index + count, Count - index);
					collection.CopyTo(_items, index);
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

					using (IEnumerator<Entry> en = enumerable.GetEnumerator())
					{
						while (count > 0 && Count < Degree && en.MoveNext())
						{
							Insert(index++, en.Current, true);
							count--;
						}
					}
					break;
				}
			}
		}

		internal void RemoveRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (count == 0) return;
			if (index < Count) Array.Copy(_items, index + count, _items, index, Count - index);
			Array.Clear(_items, Count - count, count);
			_tree._version++;
			_tree.Count -= count;
			Count -= count;
		}

		public int BinarySearch([NotNull] Entry item) { return BinarySearch(0, Count, item, null); }
		public int BinarySearch([NotNull] Entry item, IComparer<Entry> comparer) { return BinarySearch(0, Count, item, comparer); }
		public int BinarySearch(int index, int count, [NotNull] Entry item, IComparer<Entry> comparer)
		{
			Count.ValidateRange(index, ref count);
			return Array.BinarySearch(_items, index, count, item, comparer);
		}

		public int IndexOf(Entry item) { return IndexOf(item, 0, -1); }
		public int IndexOf(Entry item, int index) { return IndexOf(item, index, -1); }
		public int IndexOf(Entry item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			return Array.IndexOf(_items, item, index, count);
		}

		public int LastIndexOf(Entry item) { return LastIndexOf(item, 0, -1); }
		public int LastIndexOf(Entry item, int index) { return LastIndexOf(item, index, -1); }
		public int LastIndexOf(Entry item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (index == 0) index = Count - 1;
			return Count == 0
						? -1
						: Array.LastIndexOf(_items, item, index, count);
		}

		public bool Contains(Entry item) { return IndexOf(item, 0, -1) > -1; }

		public Entry Find(Predicate<Entry> match)
		{
			for (int i = 0; i < Count; i++)
			{
				if (!match(_items[i])) continue;
				return _items[i];
			}
			return default(Entry);
		}

		public Entry FindLast(Predicate<Entry> match)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				Entry item = _items[i];
				if (!match(item)) continue;
				return item;
			}
			return default(Entry);
		}

		public IEnumerable<Entry> FindAll([NotNull] Predicate<Entry> match)
		{
			for (int i = 0; i < Count; i++)
			{
				Entry item = _items[i];
				if (!match(item)) continue;
				yield return item;
			}
		}

		public int FindIndex(Predicate<Entry> match) { return FindIndex(0, Count, match); }
		public int FindIndex(int startIndex, Predicate<Entry> match) { return FindIndex(startIndex, -1, match); }
		public int FindIndex(int startIndex, int count, [NotNull] Predicate<Entry> match)
		{
			Count.ValidateRange(startIndex, ref count);
			return Array.FindIndex(_items, startIndex, count, match);
		}

		public int FindLastIndex(Predicate<Entry> match) { return FindLastIndex(0, -1, match); }
		public int FindLastIndex(int startIndex, Predicate<Entry> match) { return FindLastIndex(startIndex, -1, match); }
		public int FindLastIndex(int startIndex, int count, Predicate<Entry> match)
		{
			Count.ValidateRange(startIndex, ref count);
			return Array.FindLastIndex(_items, startIndex, count, match);
		}

		public void CopyTo([NotNull] Entry[] array) { CopyTo(array, 0); }
		public void CopyTo([NotNull] Entry[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
		public void CopyTo([NotNull] Entry[] array, int arrayIndex, int count)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, ref count);
			if (count == 0) return;
			Count.ValidateRange(arrayIndex, ref count);
			Array.Copy(_items, 0, array, arrayIndex, count);
		}

		[NotNull]
		public Entry[] ToArray()
		{
			Entry[] array = new Entry[Count];
			Array.Copy(_items, 0, array, 0, Count);
			return array;
		}

		public IEnumerable<Entry> GetRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);

			int last = index + count;

			for (int i = index; i < last; i++)
				yield return _items[i];
		}

		private void Insert(int index, Entry item, bool add)
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
	}

	[Serializable]
	[DebuggerDisplay("{Value}")]
	public class Entry
	{
		internal Entry(T value)
		{
			Value = value;
		}

		public T Value { get; set; }

		/// <inheritdoc />
		[NotNull]
		public override string ToString() { return Convert.ToString(Value); }

		[NotNull]
		public string ToString(int level) { return $"{Value} :L{level}"; }

		public void Swap([NotNull] Entry other)
		{
			(other.Value, Value) = (Value, other.Value);
		}

		public static implicit operator T([NotNull] Entry node) { return node.Value; }
	}

	[Serializable]
	[DebuggerDisplay("Count = {Count}")]
	[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
	public class NodeCollection : IReadOnlyList<Node>
	{
		[Serializable]
		private struct CollectionEnumerator : IEnumerator<Node>, IEnumerator
		{
			[NonSerialized]
			private readonly NodeCollection _collection;
			private readonly int _version;

			private int _index;
			private Node _current;

			internal CollectionEnumerator([NotNull] NodeCollection collection)
			{
				_collection = collection;
				_version = collection._tree._version;
				_index = 0;
				_current = default(Node);
			}

			public Node Current
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
				_current = default(Node);
				return false;
			}

			void IEnumerator.Reset()
			{
				if (_version != _collection._tree._version) throw new VersionChangedException();
				_index = 0;
				_current = default(Node);
			}
		}

		private readonly BTree<T> _tree;

		private Node[] _items;

		/// <inheritdoc />
		internal NodeCollection(BTree<T> tree)
			: this(tree, 0)
		{
		}

		internal NodeCollection(BTree<T> tree, int capacity)
		{
			if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
			_tree = tree;
			_items = new Node[capacity];
		}

		public int Capacity
		{
			get => _items.Length;
			internal set
			{
				if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
				if (value == _items.Length) return;

				if (value > 0)
				{
					Node[] newItems = new Node[value];
					if (Count > 0) Array.Copy(_items, 0, newItems, 0, Count);
					_items = newItems;
				}
				else
				{
					_items = Array.Empty<Node>();
				}
			}
		}

		[field: ContractPublicPropertyName("Count")]
		public int Count { get; private set; }

		public Node this[int index]
		{
			get => _items[index];
			set => Insert(index, value, false);
		}

		internal void Insert(int index, Node item) { Insert(index, item, true); }

		internal void Add(Node item) { Insert(Count, item, true); }

		internal void RemoveAt(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (index < Count - 1) Array.Copy(_items, index + 1, _items, index, Count - (index + 1));
			_items[Count - 1] = default(Node);
			Count--;
			_tree.Count--;
			_tree._version++;
		}

		internal bool Remove(Node item)
		{
			int index = IndexOf(item);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		internal void Clear()
		{
			if (Count == 0) return;
			Array.Clear(_items, 0, Count);
			Count = 0;
			_tree._version++;
		}

		internal void AddRange([NotNull] IEnumerable<Node> enumerable) { InsertRange(Count, enumerable); }

		internal void InsertRange(int index, [NotNull] IEnumerable<Node> enumerable)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));

			int count;

			switch (enumerable)
			{
				case IReadOnlyCollection<Node> readOnlyCollection:
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
						foreach (Node item in readOnlyCollection)
							_items[index++] = item;
					}

					Count += count;
					_tree._version++;
					break;
				}
				case ICollection<Node> collection:
				{
					count = collection.Count;
					if (count == 0) return;
					EnsureCapacity(Count + count);
					if (index < Count) Array.Copy(_items, index, _items, index + count, Count - index);
					collection.CopyTo(_items, index);
					Count += count;
					_tree._version++;
					break;
				}
				default:
				{
					count = enumerable.FastCount();
					if (count > 0) EnsureCapacity(Count + count);

					using (IEnumerator<Node> en = enumerable.GetEnumerator())
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

		internal void RemoveRange(int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (count == 0) return;
			if (index < Count) Array.Copy(_items, index + count, _items, index, Count - index);
			Array.Clear(_items, Count - count, count);
			Count -= count;
			_tree._version++;
		}

		public IEnumerator<Node> GetEnumerator() { return new CollectionEnumerator(this); }

		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		public int BinarySearch([NotNull] Node item) { return BinarySearch(0, Count, item, null); }
		public int BinarySearch([NotNull] Node item, IComparer<Node> comparer) { return BinarySearch(0, Count, item, comparer); }
		public int BinarySearch(int index, int count, [NotNull] Node item, IComparer<Node> comparer)
		{
			Count.ValidateRange(index, ref count);
			return Array.BinarySearch(_items, index, count, item, comparer);
		}

		public int IndexOf(Node item) { return IndexOf(item, 0, -1); }
		public int IndexOf(Node item, int index) { return IndexOf(item, index, -1); }
		public int IndexOf(Node item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			return Array.IndexOf(_items, item, index, count);
		}

		public int LastIndexOf(Node item) { return LastIndexOf(item, 0, -1); }
		public int LastIndexOf(Node item, int index) { return LastIndexOf(item, index, -1); }
		public int LastIndexOf(Node item, int index, int count)
		{
			Count.ValidateRange(index, ref count);
			if (index == 0) index = Count - 1;
			return Count == 0
						? -1
						: Array.LastIndexOf(_items, item, index, count);
		}

		public bool Contains(Node item) { return IndexOf(item, 0, -1) > -1; }

		public Node Find([NotNull] Predicate<Node> match)
		{
			for (int i = 0; i < Count; i++)
			{
				if (!match(_items[i])) continue;
				return _items[i];
			}
			return default(Node);
		}

		public Node FindLast([NotNull] Predicate<Node> match)
		{
			for (int i = Count - 1; i >= 0; i--)
			{
				Node item = _items[i];
				if (!match(item)) continue;
				return item;
			}
			return default(Node);
		}

		public IEnumerable<Node> FindAll([NotNull] Predicate<Node> match)
		{
			for (int i = 0; i < Count; i++)
			{
				Node item = _items[i];
				if (!match(item)) continue;
				yield return item;
			}
		}

		public int FindIndex([NotNull] Predicate<Node> match) { return FindIndex(0, Count, match); }
		public int FindIndex(int startIndex, [NotNull] Predicate<Node> match) { return FindIndex(startIndex, -1, match); }
		public int FindIndex(int startIndex, int count, [NotNull] Predicate<Node> match)
		{
			Count.ValidateRange(startIndex, ref count);
			return Array.FindIndex(_items, startIndex, count, match);
		}

		public int FindLastIndex([NotNull] Predicate<Node> match) { return FindLastIndex(0, -1, match); }
		public int FindLastIndex(int startIndex, [NotNull] Predicate<Node> match) { return FindLastIndex(startIndex, -1, match); }
		public int FindLastIndex(int startIndex, int count, [NotNull] Predicate<Node> match)
		{
			Count.ValidateRange(startIndex, ref count);
			return Array.FindLastIndex(_items, startIndex, count, match);
		}

		public void CopyTo([NotNull] Node[] array) { CopyTo(array, 0); }
		public void CopyTo([NotNull] Node[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
		public void CopyTo([NotNull] Node[] array, int arrayIndex, int count)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, ref count);
			if (count == 0) return;
			Count.ValidateRange(arrayIndex, ref count);
			Array.Copy(_items, 0, array, arrayIndex, count);
		}

		[NotNull]
		public Node[] ToArray()
		{
			Node[] array = new Node[Count];
			Array.Copy(_items, 0, array, 0, Count);
			return array;
		}

		public IEnumerable<Node> GetRange(int index, int count)
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

		private void Insert(int index, Node item, bool add)
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
	}

	[Serializable]
	private struct LevelOrderEnumerator : IEnumerableEnumerator<T>
	{
		internal LevelOrderEnumerator([NotNull] Node node)
		{

		}
	}

	[Serializable]
	private struct PreOrderEnumerator : IEnumerableEnumerator<T>
	{

	}

	[Serializable]
	private struct InOrderEnumerator : IEnumerableEnumerator<T>
	{

	}

	[Serializable]
	private struct PostOrderEnumerator : IEnumerableEnumerator<T>
	{

	}

	[Serializable]
	private struct LevelOrderNodeEnumerator : IEnumerableEnumerator<Entry>
	{

	}

	[Serializable]
	private struct PreOrderNodeEnumerator : IEnumerableEnumerator<Entry>
	{

	}

	[Serializable]
	private struct InOrderNodeEnumerator : IEnumerableEnumerator<Entry>
	{

	}

	[Serializable]
	private struct PostOrderNodeEnumerator : IEnumerableEnumerator<Entry>
	{

	}

	private int _version;

	private Node _root;

	[NonSerialized]
	private object _syncRoot;

	public BTree(int degree)
		: this(degree, null)
	{
	}

	public BTree(int degree, IGenericComparer<T> comparer)
	{
		if (degree < BTree.MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
		Degree = degree;
		Comparer = comparer ?? GenericComparer<T>.Default;
		Height = 1;
	}

	public IGenericComparer<T> Comparer { get; }

	public Node Root
	{
		get => _root;
		protected set => _root = value;
	}

	public int Degree { get; }

	public int Height { get; internal set; }

	public int Count { get; internal set; }

	/// <inheritdoc />
	public bool IsReadOnly => false;

	bool ICollection.IsSynchronized => false;

	object ICollection.SyncRoot
	{
		get
		{
			if (_syncRoot == null) Interlocked.CompareExchange<object>(ref _syncRoot, new object(), null);
			return _syncRoot;
		}
	}

	/// <inheritdoc />
	public IEnumerator<T> GetEnumerator() { return Enumerate(); }

	/// <inheritdoc />
	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

	[NotNull]
	public IEnumerableEnumerator<T> Enumerate(Node root, TreeTraverseMethod method, bool rightToLeft)
	{
		return _root?.Enumerate(method, rightToLeft) ?? EnumerableEnumerator.Empty<T>();
	}

	#region Enumerate overloads
	[NotNull]
	public IEnumerableEnumerator<T> Enumerate() { return Enumerate(TreeTraverseMethod.InOrder, false); }

	[NotNull]
	public IEnumerableEnumerator<T> Enumerate(TreeTraverseMethod method) { return Enumerate(method, false); }

	[NotNull]
	public IEnumerableEnumerator<T> Enumerate(bool rightToLeft) { return Enumerate(TreeTraverseMethod.InOrder, rightToLeft); }
	#endregion

	[NotNull]
	public IEnumerableEnumerator<Entry> EnumerateEntries(Node root, TreeTraverseMethod method, bool rightToLeft) { return _root?.EnumerateNodes(method, rightToLeft) ?? EnumerableEnumerator.Empty<Entry>(); }

	#region EnumerateEntries overloads
	[NotNull]
	public IEnumerableEnumerator<Entry> EnumerateEntries() { return EnumerateEntries(TreeTraverseMethod.InOrder, false); }

	[NotNull]
	public IEnumerableEnumerator<Entry> EnumerateEntries(TreeTraverseMethod method) { return EnumerateEntries(method, false); }

	[NotNull]
	public IEnumerableEnumerator<Entry> EnumerateEntries(bool rightToLeft) { return EnumerateEntries(TreeTraverseMethod.InOrder, rightToLeft); }
	#endregion

	public void Iterate(Node root, TreeTraverseMethod method, bool rightToLeft, Action<T> visitCallback) { _root?.Iterate(method, rightToLeft, visitCallback); }

	#region Iterate overloads - visitCallback action
	public void Iterate(Action<T> visitCallback) { Iterate(TreeTraverseMethod.InOrder, false, visitCallback); }

	public void Iterate(TreeTraverseMethod method, Action<T> visitCallback) { Iterate(method, false, visitCallback); }

	public void Iterate(bool rightToLeft, Action<T> visitCallback) { Iterate(TreeTraverseMethod.InOrder, rightToLeft, visitCallback); }
	#endregion

	public void IterateEntries(Node root, TreeTraverseMethod method, bool rightToLeft, Action<Entry> visitCallback) { _root?.IterateEntries(method, rightToLeft, visitCallback); }

	#region IterateEntries overloads - visitCallback action
	public void IterateEntries(Action<Entry> visitCallback) { IterateEntries(TreeTraverseMethod.InOrder, false, visitCallback); }

	public void IterateEntries(TreeTraverseMethod method, Action<Entry> visitCallback) { IterateEntries(method, false, visitCallback); }

	public void IterateEntries(bool rightToLeft, Action<Entry> visitCallback) { IterateEntries(TreeTraverseMethod.InOrder, rightToLeft, visitCallback); }
	#endregion

	public void Iterate(Node root, TreeTraverseMethod method, bool rightToLeft, Func<T, bool> visitCallback) { _root?.Iterate(method, rightToLeft, visitCallback); }

	#region Iterate overloads - visitCallback function
	public void Iterate(Func<T, bool> visitCallback) { Iterate(TreeTraverseMethod.InOrder, false, visitCallback); }

	public void Iterate(TreeTraverseMethod method, Func<T, bool> visitCallback) { Iterate(method, false, visitCallback); }

	public void Iterate(bool rightToLeft, Func<T, bool> visitCallback) { Iterate(TreeTraverseMethod.InOrder, rightToLeft, visitCallback); }
	#endregion

	public void IterateEntries(Node root, TreeTraverseMethod method, bool rightToLeft, Func<Entry, bool> visitCallback) { _root?.IterateEntries(method, rightToLeft, visitCallback); }

	#region IterateEntries overloads - visitCallback function
	public void IterateEntries(Func<Entry, bool> visitCallback) { IterateEntries(TreeTraverseMethod.InOrder, false, visitCallback); }

	public void IterateEntries(TreeTraverseMethod method, Func<Entry, bool> visitCallback) { IterateEntries(method, false, visitCallback); }

	public void IterateEntries(bool rightToLeft, Func<Entry, bool> visitCallback) { IterateEntries(TreeTraverseMethod.InOrder, rightToLeft, visitCallback); }
	#endregion

	/// <inheritdoc />
	public void Add(T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		Root ??= MakeNode();

		if (!Root.IsFull)
		{
			Add(Root, item);
			return;
		}

		Node oldRoot = Root;
		Root = MakeNode();
		Root.Children ??= new NodeCollection(this);
		Root.Children.Add(oldRoot);
		Split(Root, 0, oldRoot);
		Add(Root, item);
		Height++;
	}

	/// <inheritdoc />
	public bool Remove(T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		if (Root == null || !Remove(Root, item)) return false;
		if (Root.Count > 0 || Root.IsLeaf) return true;
		Root = Root.Children[0];
		Height--;
		return true;
	}

	/// <inheritdoc />
	public void Clear()
	{
		_root = null;
		Count = 0;
		_version++;
	}

	/// <inheritdoc />
	public bool Contains(T item)
	{
		if (item == null) throw new ArgumentNullException(nameof(item));
		if (Root == null) return false;

		Queue<Node> queue = new Queue<Node>();
		queue.Enqueue(Root);

		while (queue.Count > 0)
		{
			Node node = queue.Dequeue();
			int position = node.Count(e => Comparer.Compare(item, e.Value) > 0);
			if (position < node.Count && Comparer.Equals(item, node[position].Value)) return true;
			if (position >= node.Children.Count) continue;
			queue.Enqueue(node.Children[position]);
		}

		return false;
	}

	public void CopyTo([NotNull] T[] array) { CopyTo(array, 0); }
	public void CopyTo(T[] array, int arrayIndex) { CopyTo(array, arrayIndex, -1); }
	public void CopyTo([NotNull] T[] array, int arrayIndex, int count)
	{
		array.Length.ValidateRange(arrayIndex, ref count);
		if (count == 0) return;
		Count.ValidateRange(arrayIndex, ref count);
		if (_root == null) return;

		while (count > 0)
		{
			// todo
			?? count--;
		}
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
				objects[arrayIndex++] = item;
		}
		catch (ArrayTypeMismatchException)
		{
			throw new ArgumentException("Invalid array type", nameof(array));
		}
	}

	private void Add([NotNull] Node node, [NotNull] T item)
	{
		int position = node.Count == 0
							? 0
							: node.FindLastIndex(e => Comparer.Compare(item, e.Value) >= 0) + 1;

		while (!node.IsLeaf)
		{
			Node child = node.Children[position];

			if (child.IsFull)
			{
				Split(node, position, child);
				if (Comparer.Compare(item, node[position].Value) > 0) position++;
				node = node.Children[position];
			}

			position = node.Count == 0
							? 0
							: node.FindLastIndex(e => Comparer.Compare(item, e.Value) >= 0) + 1;
		}

		node.Insert(position, MakeEntry(item));
	}

	private bool Remove([NotNull] Node block, [NotNull] T item)
	{
		return RemoveItem(block, item, Comparer);

		static bool RemoveItem(Node block, T item, IGenericComparer<T> comparer)
		{
			if (block.Count == 0) return false;

			int position = block.Count(e => comparer.Compare(item, e.Value) > 0);
			return position < block.Count && comparer.Equals(item, block[position].Value)
						? RemoveFromBlock(block, item, position, comparer)
						: !block.IsLeaf && RemoveFromSubTree(block, item, position, comparer);
		}

		static bool RemoveFromSubTree(Node root, T item, int subtreeIndexInBlock, IGenericComparer<T> comparer)
		{
			Node block = root.Children[subtreeIndexInBlock];
			if (!block.HasMinimumEntries) return RemoveItem(block, item, comparer);

			/*
			 * Removing any item from the block will break the btree property, So this block must have
			 * at least 'degree' of nodes by moving an item from a sibling block or merging nodes.
			 */
			int leftIndex = subtreeIndexInBlock - 1;
			Node leftSibling = subtreeIndexInBlock > 0
									? root.Children[leftIndex]
									: null;
			int rightIndex = subtreeIndexInBlock + 1;
			Node rightSibling = subtreeIndexInBlock < root.Children.Count - 1
									? root.Children[rightIndex]
									: null;

			if (leftSibling != null && leftSibling.Count > root.Degree - 1)
			{
				/*
				 * left sibling has a node to spare, so move one node from it into the parent's block
				 * and one node from parent into this block.
				 */
				block.Insert(0, root[subtreeIndexInBlock]);
				root[subtreeIndexInBlock] = leftSibling[leftSibling.Count - 1];
				leftSibling.RemoveAt(leftSibling.Count - 1);
				if (leftSibling.IsLeaf) return RemoveItem(block, item, comparer);
				block.Children.Insert(0, leftSibling.Children[leftSibling.Children.Count - 1]);
				leftSibling.Children.RemoveAt(leftSibling.Children.Count - 1);
			}
			else if (rightSibling != null && rightSibling.Count > root.Degree - 1)
			{
				/*
				 * right sibling has a node to spare, so move one node from it into the parent's block
				 * and one node from parent into this block.
				 */
				block.Add(root[subtreeIndexInBlock]);
				root[subtreeIndexInBlock] = rightSibling[0];
				rightSibling.RemoveAt(0);
				if (rightSibling.IsLeaf) return RemoveItem(block, item, comparer);
				block.Children.Add(rightSibling.Children[0]);
				rightSibling.Children.RemoveAt(0);
			}
			else if (leftSibling != null)
			{
				// this block merges left sibling into this block
				block.Insert(0, root[subtreeIndexInBlock]);
				block.InsertRange(0, leftSibling);
				if (!leftSibling.IsLeaf) block.Children.InsertRange(0, leftSibling.Children);
				root.RemoveAt(leftIndex);
				root.RemoveAt(subtreeIndexInBlock);
			}
			else
			{
				Debug.Assert(rightSibling != null, "Node should have at least one sibling.");
				block.Add(root[subtreeIndexInBlock]);
				block.AddRange(rightSibling);
				if (!rightSibling.IsLeaf) block.Children.AddRange(rightSibling.Children);
				root.Children.RemoveAt(rightIndex);
				root.RemoveAt(subtreeIndexInBlock);
			}

			/*
			 * At this point, block has at least 'degree' of nodes. This guarantees that if any node needs to be
			 * removed from it in order to guarantee BTree's property.
			 */
			return RemoveItem(block, item, comparer);
		}

		static bool RemoveFromBlock(Node block, T item, int position, IGenericComparer<T> comparer)
		{
			if (block.IsLeaf)
			{
				// just remove it and move on. BTree property is maintained.
				block.RemoveAt(position);
				return true;
			}

			Node predecessor = block.Children[position];

			if (predecessor.Count > block.Degree)
			{
				block[position] = DeletePredecessor(predecessor);
				return true;
			}

			Node successor = block.Children[position + 1];

			if (successor.Count >= block.Degree)
			{
				block[position] = DeleteSuccessor(predecessor);
				return true;
			}

			predecessor.Add(block[position]);
			predecessor.AddRange(successor);
			predecessor.Children.AddRange(successor.Children);
			block.RemoveAt(position);
			block.Children.RemoveAt(position + 1);
			return RemoveItem(predecessor, item, comparer);
		}

		static Entry DeletePredecessor(Node block)
		{
			while (!block.IsLeaf)
			{
				block = block.Children[block.Children.Count - 1];
			}

			Entry node = block[block.Count - 1];
			block.RemoveAt(block.Count - 1);
			return node;
		}

		static Entry DeleteSuccessor(Node block)
		{
			while (!block.IsLeaf)
			{
				block = block.Children[0];
			}

			Entry node = block[0];
			block.RemoveAt(0);
			return node;
		}
	}

	private void Split([NotNull] Node parent, int index, [NotNull] Node node)
	{
		if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));

		Node newBlock = MakeNode();
		parent.Insert(index, node[Degree - 1]);
		parent.Children.Insert(index + 1, newBlock);
		newBlock.AddRange(node.GetRange(Degree, Degree - 1));
		node.RemoveRange(Degree - 1, Degree);
		if (node.IsLeaf) return;
		newBlock.Children.AddRange(node.Children.GetRange(Degree, Degree));
		node.Children.RemoveRange(Degree, Degree);
	}

	[NotNull]
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private NodeCollection MakeCollection() { return new NodeCollection(this); }

	[NotNull]
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private Node MakeNode() { return new Node(this, Degree); }

	[NotNull]
	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	private Entry MakeEntry([NotNull] T value) { return new Entry(value); }
}

public static class BTree
{
	public const int MINIMUM_DEGREE = 2;

	[Serializable]
	private class SynchronizedCollection<T> : ICollection<T>
	{
		private readonly BTree<T> _tree;

		private object _root;

		internal SynchronizedCollection(BTree<T> tree)
		{
			_tree = tree;
			_root = ((ICollection)tree).SyncRoot;
		}

		/// <inheritdoc />
		public int Count
		{
			get
			{
				lock (_root)
				{
					return _tree.Count;
				}
			}
		}

		/// <inheritdoc />
		public bool IsReadOnly
		{
			get
			{
				lock (_root)
				{
					return _tree.IsReadOnly;
				}
			}
		}

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator()
		{
			lock (_root)
			{
				return _tree.GetEnumerator();
			}
		}

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void Add(T item)
		{
			lock (_tree)
			{
				_tree.Add(item);
			}
		}

		/// <inheritdoc />
		public bool Remove(T item)
		{
			lock (_tree)
			{
				return _tree.Remove(item);
			}
		}

		/// <inheritdoc />
		public void Clear()
		{
			lock (_root)
			{
				_tree.Clear();
			}
		}

		/// <inheritdoc />
		public bool Contains(T item)
		{
			lock (_root)
			{
				return _tree.Contains(item);
			}
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex)
		{
			lock (_root)
			{
				_tree.CopyTo(array, arrayIndex);
			}
		}
	}

	public static int MinimumEntries(int degree)
	{
		if (degree < MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree));
		return degree - 1;
	}

	public static int MaximumEntries(int degree)
	{
		if (degree < MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree));
		return 2 * degree - 1;
	}

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int FastMinimumEntries(int degree) { return degree - 1; }

	[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
	public static int FastMaximumEntries(int degree) { return 2 * degree - 1; }

	[NotNull]
	public static ICollection<T> Synchronized<T>([NotNull] BTree<T> tree) { return new SynchronizedCollection<T>(tree); }
}
