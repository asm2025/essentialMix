using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using asm.Collections.DebugView;
using asm.Exceptions;
using asm.Exceptions.Collections;
using asm.Extensions;
using JetBrains.Annotations;

namespace asm.Collections
{

	/// <summary>
	/// <see href="https://algs4.cs.princeton.edu/44sp/IndexMinPQ.java.html">IndexMin</see> priority queue.
	/// This implementation is based on Robert Sedgewick and Kevin Wayne IndexMinPQ from their book: Algorithms, 4th Edition
	/// <a href="https://algs4.cs.princeton.edu/24pq">Section 2.4</a>
	/// <para>
	/// This implementation uses a binary heap along with an array to associate keys with integers in the given range.
	/// </para>
	/// </summary>
	/// <typeparam name="TNode">The node type. This is just for abstraction purposes and shouldn't be dealt with directly.</typeparam>
	/// <typeparam name="TKey">The key assigned to the element. It should have its value from the value at first but changing
	/// this later will not affect the value itself, except for primitive value types. Changing the key will of course affect the
	/// priority of the item.</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	[DebuggerDisplay("Count = {Count}")]
	//[DebuggerTypeProxy(typeof(Dbg_IndexMinDebugView<,,>))]
	[Serializable]
	public abstract class IndexMin<TNode, TKey, TValue> : IKeyedHeap<TNode, TKey, TValue>, IReadOnlyCollection<TValue>, ICollection
		where TNode : KeyedBinaryNode<TNode, TKey, TValue>
	{
		[Serializable]
		public struct Enumerator : IEnumerableEnumerator<TValue>
		{
			[NonSerialized]
			private readonly IndexMin<TNode, TKey, TValue> _indexMin;
			private readonly int _version;

			private int _index;
			private bool _done;

			internal Enumerator([NotNull] IndexMin<TNode, TKey, TValue> indexMin)
			{
				_indexMin = indexMin;
				_version = indexMin._version;
				_index = -1;
				Current = default(TValue);
				_done = indexMin.Count == 0;
			}

			public TValue Current { get; private set; }

			object IEnumerator.Current
			{
				get
				{
					if (_index.InRangeRx(0, _indexMin.Count)) throw new InvalidOperationException();
					return Current;
				}
			}

			public void Dispose()
			{
			}

			/// <inheritdoc />
			public IEnumerator<TValue> GetEnumerator()
			{
				IEnumerator enumerator = this;
				enumerator.Reset();
				return this;
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			public bool MoveNext()
			{
				if (_version != _indexMin._version) throw new VersionChangedException();
				if (_done) return false;

				_index = _indexMin.NextValidIndex(_index);

				if (_index == _indexMin.Count)
				{
					Current = default(TValue);
					_done = true;
					return false;
				}

				Current = _indexMin._nodes[_index].Value;
				_index++;
				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _indexMin._version) throw new VersionChangedException();
				_index = -1;
				Current = default(TValue);
				_done = _indexMin.Count == 0;
			}
		}

		protected internal int _version;

		private int[] _pq; // binary heap using 1-based indexing
		private int[] _qp; // inverse of pq - qp[pq[i]] = pq[qp[i]] = i
		private TNode[] _nodes; // nodes[i].Key = priority of i
		private object _syncRoot;

		/// <inheritdoc />
		protected IndexMin() 
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected IndexMin(int capacity)
			: this(capacity, null)
		{
		}

		/// <inheritdoc />
		protected IndexMin(IComparer<TKey> comparer)
			: this(0, comparer)
		{
		}

		protected IndexMin(int capacity, IComparer<TKey> comparer)
		{
			if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
			Comparer = comparer ?? Comparer<TKey>.Default;
			EnsureCapacity(capacity);
		}

		/// <inheritdoc />
		protected IndexMin([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		/// <inheritdoc />
		protected IndexMin([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(0, comparer)
		{
			Add(enumerable);
		}

		public int Capacity
		{
			get => _nodes.Length;
			set
			{
				if (value < Count) throw new ArgumentOutOfRangeException(nameof(value));
				if (_nodes != null && value == _nodes.Length - 1) return;

				if (value == 0)
				{
					_nodes = Array.Empty<TNode>();
					_pq = Array.Empty<int>();
					_qp = Array.Empty<int>();
				}
				else
				{
					value++;
					TNode[] nodes = new TNode[value];
					int[] pq = new int[value];
					int[] qp = new int[value];

					if (Count > 0)
					{
						Array.Copy(_nodes!, 0, nodes, 0, Count);
						Array.Copy(_pq, 0, pq, 0, Count);
						Array.Copy(_qp, 0, qp, 0, Count);
					}

					qp.FastInitialize(-1, Count);
					_nodes = nodes;
					_pq = pq;
					_qp = qp;
				}

				_version++;
			}
		}

		/// <inheritdoc />
		public IComparer<TKey> Comparer { get; }

		[NotNull]
		protected EqualityComparer<TValue> ValueComparer { get; } = EqualityComparer<TValue>.Default;

		/// <inheritdoc cref="ICollection{TValue}" />
		public int Count { get; protected set; }

		/// <inheritdoc />
		bool ICollection<TValue>.IsReadOnly => false;

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

		/// <inheritdoc />
		public IEnumerator<TValue> GetEnumerator() { return new Enumerator(this); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Iterate over nodes with a callback action
		/// </summary>
		/// <param name="visitCallback">callback action to handle the node</param>
		public void Iterate([NotNull] Action<TNode> visitCallback)
		{
			if (Count == 0) return;
			
			int version = _version;
			int index = -1;

			while ((index = NextValidIndex(index + 1)) < Count)
			{
				if (version != _version) throw new VersionChangedException();
				visitCallback(_nodes[index]);
			}
		}

		/// <summary>
		/// Iterate over nodes with a callback function
		/// </summary>
		/// <param name="visitCallback">callback function to handle the node that can cancel the loop</param>
		public void Iterate([NotNull] Func<TNode, bool> visitCallback)
		{
			if (Count == 0) return;
			
			int version = _version;
			int index = -1;

			while ((index = NextValidIndex(index + 1)) < Count)
			{
				if (version != _version) throw new VersionChangedException();
				if (!visitCallback(_nodes[index])) return;
			}
		}

		/// <inheritdoc />
		public abstract TNode MakeNode(TValue value);

		public virtual bool Equals(IndexMin<TNode, TKey, TValue> other)
		{
			if (other is null) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() || Count != other.Count || !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			for (int i = 0; i < Count; i++)
			{
				if (Compare(_nodes[i], other._nodes[i]) == 0) continue;
				return false;
			}

			return true;
		}

		public bool ContainsIndex(int index)
		{
			if (!index.InRangeRx(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			return _qp[index] != -1;
		}

		/// <inheritdoc />
		public bool Contains(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			return IndexOf(value) > -1;
		}

		public bool Exists([NotNull] Predicate<TValue> match)
		{
			if (Count == 0) return false;

			bool exists = false;
			Iterate(e =>
			{
				exists = match(e.Value);
				return !exists; // continue search
			});
			return exists;
		}

		/// <summary>
		/// Finds the node's index with the specified value
		/// </summary>
		/// <param name="value">The value to search for</param>
		/// <returns>The found node's index or -1 if no match is found</returns>
		public int IndexOf([NotNull] TValue value)
		{
			if (Count == 0) return -1;

			int version = _version;
			int index = -1;

			while ((index = NextValidIndex(index + 1)) < Count)
			{
				if (version != _version) throw new VersionChangedException();
				if (ValueComparer.Equals(_nodes[index].Value, value)) return index;
			}

			return -1;
		}

		public int IndexOfByKey([NotNull] TKey key)
		{
			if (Count == 0) return -1;

			int version = _version;
			int index = -1;

			while ((index = NextValidIndex(index + 1)) < Count)
			{
				if (version != _version) throw new VersionChangedException();
				if (Comparer.IsEqual(_nodes[index].Key, key)) return index;
			}

			return -1;
		}

		/// <inheritdoc />
		public TNode Find(TValue value)
		{
			int index = IndexOf(value);
			return index < 0
						? null
						: _nodes[index];
		}

		/// <inheritdoc />
		public TNode FindByKey(TKey key)
		{
			int index = IndexOfByKey(key);
			return index < 0
						? null
						: _nodes[index];
		}

		/// <inheritdoc />
		public void Add(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			Insert(Count, MakeNode(value));
		}

		/// <inheritdoc />
		public TNode Add(TNode node)
		{
			return Insert(Count, node);
		}

		public void Add(IEnumerable<TValue> values)
		{
			foreach (TValue value in values) 
				Add(value);
		}

		[NotNull]
		public TNode Insert(int index, [NotNull] TValue value)
		{
			return Insert(index, MakeNode(value));
		}

		[NotNull]
		public TNode Insert(int index, [NotNull] TNode node)
		{
			if (!index.InRange(0, Count)) throw new ArgumentOutOfRangeException(nameof(index));
			if (_qp[index] != -1) throw new ArgumentException("index is already in the priority queue.", nameof(index));
			if (Count == _nodes.Length - 1) EnsureCapacity(Count + 1);
			Count++;
			_qp[index] = Count;
			_pq[Count] = index;
			_nodes[index] = node;
			Swim(Count - 1);
			return node;
		}

		public void RemoveAt(int index)
		{
			if (!ContainsIndex(index)) throw new ArgumentException("Index is not in the priority queue.", nameof(index));
			int i = _qp[index];
			Swap(i, Count--);
			Swim(i);
			Sink(i);
			_nodes[index] = null;
			_qp[index] = -1;
			_version++;
		}

		/// <inheritdoc />
		public bool Remove(TValue value)
		{
			if (value == null) throw new ArgumentNullException(nameof(value));
			if (Count == 0) return false;
			int index = IndexOf(value);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		/// <inheritdoc />
		public bool Remove(TNode node)
		{
			if (Count == 0) return false;
			int index = Array.IndexOf(_nodes, node, 0, Count);
			if (index < 0) return false;
			RemoveAt(index);
			return true;
		}

		/// <inheritdoc />
		public void Clear()
		{
			if (Count == 0) return;
			Array.Clear(_nodes, 0, Count);
			Array.Clear(_pq, 0, Count);
			_qp.FastInitialize(-1);
			Count = 0;
			_version++;
		}

		/// <inheritdoc />
		public void DecreaseKey(TNode node, TKey newKey)
		{
			if (Count == 0) throw new InvalidOperationException("Heap is empty.");
			int index = Array.IndexOf(_nodes, node, 0, Count);
			if (index < 0) throw new NotFoundException();
			DecreaseKey(index, newKey);
		}

		public void DecreaseKey(int index, [NotNull] TKey newKey)
		{
			if (!ContainsIndex(index)) throw new ArgumentException("Index is not in the priority queue.", nameof(index));
			if (Compare(_nodes[index].Key, newKey) < 0) throw new InvalidOperationException("Invalid new key.");
			_nodes[index].Key = newKey;
			Swim(index);
		}

		public void ChangeKey(int index, [NotNull] TKey newKey)
		{
			if (!ContainsIndex(index)) throw new ArgumentException("Index is not in the priority queue.", nameof(index));
			_nodes[index].Key = newKey;
			Swim(_qp[index]);
			Sink(_qp[index]);
		}

		public int Index()
		{
			if (Count == 0) throw new InvalidOperationException("Heap is empty.");
			return _pq[0];
		}

		[NotNull]
		public TKey Key()
		{
			return _nodes[Index()].Key;
		}

		[NotNull]
		public TKey Key(int index)
		{
			if (!ContainsIndex(index)) throw new ArgumentException("Index is not in the priority queue.", nameof(index));
			return _nodes[Index()].Key;
		}

		/// <inheritdoc />
		public TValue Value()
		{
			return _nodes[Index()].Value;
		}

		/// <inheritdoc />
		TValue IHeap<TValue>.ExtractValue() { return ExtractValue().Value; }

		/// <inheritdoc />
		public TNode ExtractValue()
		{
			if (Count == 0) throw new InvalidOperationException("Heap is empty.");
			int index = _pq[0];
			TNode node = _nodes[index];
			Swap(0, Count--);
			Sink(0);
			Debug.Assert(index == _pq[Count + 1], $"Shit...! index = {index}, _pq[Count + 1] = {_pq[Count + 1]}");
			_qp[index] = -1; // actually delete it
			_nodes[index] = null; // and get rid of its reference for garbage collection
			_pq[Count + 1] = -1; // not needed
			_version++;
			return node;
		}

		/// <inheritdoc />
		public TValue ElementAt(int k)
		{
			if (k < 1 || Count < k) throw new ArgumentOutOfRangeException(nameof(k));

			for (int i = 1; i < k; i++) 
				ExtractValue();

			return Value();
		}

		public void TrimExcess()
		{
			int threshold = (int)(_nodes.Length * 0.9);
			if (Count >= threshold) return;
			Capacity = Count;
		}

		public IEnumerable<TOutput> ConvertAll<TOutput>([NotNull] Converter<TValue, TOutput> converter)
		{
			if (Count == 0) yield break;
			
			int version = _version;
			int index = -1;

			while ((index = NextValidIndex(index + 1)) < Count)
			{
				if (version != _version) throw new VersionChangedException();
				yield return converter(_nodes[index].Value);
			}
		}

		/// <inheritdoc />
		public void CopyTo(TValue[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(e => array[arrayIndex++] = e.Value);
		}

		/// <inheritdoc />
		void ICollection.CopyTo(Array array, int index)
		{
			if (array.Rank != 1) throw new RankException();
			if (array.GetLowerBound(0) != 0) throw new ArgumentException("Invalid array lower bound.", nameof(array));
			if (Count == 0) return;

			if (array is TValue[] tArray)
			{
				CopyTo(tArray, index);
				return;
			}

			/*
			* Catch the obvious case assignment will fail.
			* We can find all possible problems by doing the check though.
			* For example, if the element type of the Array is derived from T,
			* we can't figure out if we can successfully copy the element beforehand.
			*/
			array.Length.ValidateRange(index, Count);

			Type targetType = array.GetType().GetElementType() ?? throw new TypeAccessException();
			Type sourceType = typeof(TValue);
			if (!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) throw new ArgumentException("Invalid array type", nameof(array));
			if (!(array is object[] objects)) throw new ArgumentException("Invalid array type", nameof(array));
			Iterate(e => objects[index++] = e.Value);
		}

		internal void CopyTo(TNode[] array, int arrayIndex)
		{
			if (Count == 0) return;
			array.Length.ValidateRange(arrayIndex, Count);
			Iterate(e => array[arrayIndex++] = e);
		}

		protected void EnsureCapacity(int min)
		{
			if (_nodes != null && _nodes.Length - 1 >= min) return;
			Capacity = (_nodes == null || _nodes.Length == 0 ? Constants.DEFAULT_CAPACITY : _nodes.Length * 2).NotBelow(min);
		}

		protected int NextValidIndex(int index)
		{
			while (index < Count && (index < 0 || _qp[index] == -1)) 
				index++;

			return index;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		protected void Swap(int x, int y)
		{
			int tmp = _pq[x];
			_pq[x] = _pq[y];
			_pq[y] = tmp;
			_qp[_pq[x]] = x;
			_qp[_pq[y]] = y;
		}

		protected void Swim(int index)
		{
			bool changed = false;

			while (index > -1 && Compare(_nodes[index / 2], _nodes[index]) > 0)
			{
				Swap(index, index / 2);
				index /= 2;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		protected void Sink(int index)
		{
			bool changed = false;

			while (2 * index <= Count)
			{
				int j = 2 * index;
				if (j < Count && Compare(_nodes[j], _nodes[j + 1]) > 0) j++;
				if (Compare(_nodes[index], _nodes[j]) < 1) break;
				Swap(index, j);
				index = j;
				changed = true;
			}

			if (!changed) return;
			_version++;
		}

		protected abstract int Compare(TNode x, TNode y);
		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);
	}

	[DebuggerTypeProxy(typeof(IndexMin<,>.DebugView))]
	[Serializable]
	public abstract class IndexMin<TKey, TValue> : IndexMin<KeyedBinaryNode<TKey, TValue>, TKey, TValue>
	{
		internal sealed class DebugView : Dbg_IndexMinDebugView<KeyedBinaryNode<TKey, TValue>, TKey, TValue>
		{
			public DebugView([NotNull] IndexMin<TKey, TValue> heap)
				: base(heap)
			{
			}
		}

		[NotNull]
		protected Func<TValue, TKey> _getKeyForItem;

		/// <inheritdoc />
		protected IndexMin([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, 0, null)
		{
		}

		/// <inheritdoc />
		protected IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, int capacity)
			: this(getKeyForItem, capacity, null)
		{
		}

		protected IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> comparer)
			: this(getKeyForItem, 0, comparer)
		{
		}

		protected IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, int capacity, IComparer<TKey> comparer)
			: base(capacity, comparer)
		{
			_getKeyForItem = getKeyForItem;
		}

		protected IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null)
		{
		}

		protected IndexMin([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: this(getKeyForItem, 0, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public override KeyedBinaryNode<TKey, TValue> MakeNode(TValue value) { return new KeyedBinaryNode<TKey, TValue>(_getKeyForItem(value), value); }
	}

	[DebuggerTypeProxy(typeof(IndexMin<>.DebugView))]
	[Serializable]
	public abstract class IndexMin<T> : IndexMin<KeyedBinaryNode<T>, T, T>
	{
		internal sealed class DebugView : Dbg_IndexMinDebugView<KeyedBinaryNode<T>, T, T>
		{
			public DebugView([NotNull] IndexMin<T> heap)
				: base(heap)
			{
			}
		}

		/// <inheritdoc />
		protected IndexMin()
			: this(0, null)
		{
		}

		/// <inheritdoc />
		protected IndexMin(int capacity)
			: this(capacity, null)
		{
		}

		protected IndexMin(IComparer<T> comparer)
			: this(0, comparer)
		{
		}

		protected IndexMin(int capacity, IComparer<T> comparer)
			: base(capacity, comparer)
		{
		}

		protected IndexMin([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected IndexMin([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: this(0, comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public override KeyedBinaryNode<T> MakeNode(T value) { return new KeyedBinaryNode<T>(value); }
	}
}