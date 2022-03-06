using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using essentialMix.Collections.DebugView;
using essentialMix.Exceptions.Collections;
using essentialMix.Extensions;
using JetBrains.Annotations;

namespace essentialMix.Collections;

/// <inheritdoc cref="ITreeBlock{TBlock,TNode,T}" />
[Serializable]
[DebuggerDisplay("{Degree}, Count = {Count}")]
[DebuggerTypeProxy(typeof(Dbg_CollectionDebugView<>))]
public abstract class BTreeBlockBase<TBlock, TNode, T> : BTreeCollection<TBlock, TNode, T, TNode>, ITreeBlockBase<TBlock, TNode, T>, IReadOnlyList<TNode>, IList
	where TBlock : BTreeBlockBase<TBlock, TNode, T>
	where TNode : class, ITreeNode<TNode, T>
{
	[Serializable]
	private class SynchronizedBlock : IList<TNode>
	{
		private readonly BTreeBlockBase<TBlock, TNode, T> _block;

		private object _root;

		internal SynchronizedBlock(BTreeBlockBase<TBlock, TNode, T> block)
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
					return ((ICollection<T>)_block).IsReadOnly;
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
				return ((IEnumerable<TNode>)_block).GetEnumerator();
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
		private readonly BTreeBlockBase<TBlock, TNode, T> _block;
		private readonly int _version;

		private int _index;
		private TNode _current;

		internal Enumerator([NotNull] BTreeBlockBase<TBlock, TNode, T> block)
		{
			_block = block;
			_version = block._tree._version;
			_index = 0;
			_current = null;
		}

		public TNode Current
		{
			get
			{
				if (!_index.InRange(0, _block.Count)) throw new InvalidOperationException();
				return _current;
			}
		}

		object IEnumerator.Current => Current;

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			if (_version == _block._tree._version && _index < _block.Count)
			{
				_current = _block._items[_index];
				_index++;
				return true;
			}
			return MoveNextRare();
		}

		private bool MoveNextRare()
		{
			if (_version != _block._tree._version) throw new VersionChangedException();
			_index = _block.Count + 1;
			_current = null;
			return false;
		}

		void IEnumerator.Reset()
		{
			if (_version != _block._tree._version) throw new VersionChangedException();
			_index = 0;
			_current = null;
		}
	}

	private readonly int _minEntries;
	private readonly int _maxEntries;

	private BTreeBase<TBlock, TNode, T> _tree;
	private BTreeCollection<TBlock, TNode, T, TBlock> _children;

	protected BTreeBlockBase([NotNull] BTreeBase<TBlock, TNode, T> tree, int degree)
		: base(tree, degree)
	{
		if (degree < BTree.MINIMUM_DEGREE) throw new ArgumentOutOfRangeException(nameof(degree), $"{GetType()}'s degree must be at least 2.");
		_tree = tree;
		_minEntries = BTree.FastMinimumEntries(degree);
		_maxEntries = BTree.FastMaximumEntries(degree);
	}

	/// <inheritdoc />
	public BTreeCollection<TBlock, TNode, T, TBlock> Children
	{
		get => _children;
		set => _children = value;
	}

	/// <inheritdoc />
	public int Degree => _items.Length;

	/// <inheritdoc />
	public bool IsLeaf => _children == null || _children.Count == 0;

	/// <inheritdoc />
	public bool IsEmpty => Count == 0;

	/// <inheritdoc />
	public bool IsFull => Count >= _maxEntries;

	/// <inheritdoc />
	public bool HasMinimumEntries => Count >= _minEntries;

	/// <inheritdoc />
	public void EnsureChildren() { _children ??= new BTreeCollection<TBlock, TNode, T>(_tree); }
}

/// <inheritdoc cref="BTreeBlockBase{TBlock, TNode, T}" />
[Serializable]
public sealed class BTreeBlock<T> : BTreeBlockBase<BTreeBlock<T>, BTreeNode<T>, T>, ITreeBlock<BTreeBlock<T>, BTreeNode<T>, T>
{
	/// <inheritdoc />
	internal BTreeBlock(BTreeBase<BTreeBlock<T>, BTreeNode<T>, T> tree, int degree)
		: base(tree, degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<T> MakeNode(T value) { return new BTreeNode<T>(value); }
}

/// <inheritdoc cref="BTreeBlockBase{TBlock, TNode,T}" />
[Serializable]
public sealed class BTreeBlock<TKey, TValue> : BTreeBlockBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue>, ITreeBlock<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TKey, TValue>
{
	/// <inheritdoc />
	internal BTreeBlock(BTreeBase<BTreeBlock<TKey, TValue>, BTreeNode<TKey, TValue>, TValue> tree, int degree)
		: base(tree, degree)
	{
	}

	/// <inheritdoc />
	public BTreeNode<TKey, TValue> MakeNode(TKey key, TValue value) { return new BTreeNode<TKey, TValue>(key, value); }
}
