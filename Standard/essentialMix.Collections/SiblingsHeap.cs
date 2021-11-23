using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	/// <summary>
	/// <inheritdoc />
	/// <para>
	/// This heap type and its subclasses use a <see cref="ISiblingNode{TNode, T}">sibling tree node</see>
	/// </para>
	/// </summary>
	/// <typeparam name="TNode">The node type.</typeparam>
	/// <typeparam name="T">The element type of the heap</typeparam>
	public abstract class SiblingsHeapBase<TNode, T> : LinkedHeapBase<TNode, T>
		where TNode : class, ISiblingNode<TNode, T>
	{
		private struct BreadthFirstEnumerator : IEnumerableEnumerator<T>
		{
			private readonly SiblingsHeapBase<TNode, T> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] SiblingsHeapBase<TNode, T> heap, TNode root)
			{
				_heap = heap;
				_version = _heap._version;
				_root = root;
				_queue = new Queue<TNode>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			[NotNull]
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			[NotNull]
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
			{
				IEnumerator enumerator = this;
				enumerator.Reset();
				return this;
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			public bool MoveNext()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				// Head-Sibling-Child (Queue)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queue.Enqueue(_root);
				}

				// visit the next queued node
				_current = _queue.Count > 0
								? _queue.Dequeue()
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Child != null) _queue.Enqueue(_current.Child);
				if (_current.Sibling != null) _queue.Enqueue(_current.Sibling);
				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_queue.Clear();
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct DepthFirstEnumerator : IEnumerableEnumerator<T>
		{
			private readonly SiblingsHeapBase<TNode, T> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] SiblingsHeapBase<TNode, T> heap, TNode root)
			{
				_heap = heap;
				_version = _heap._version;
				_root = root;
				_stack = new Stack<TNode>();
				_current = null;
				_started = false;
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			[NotNull]
			public T Current
			{
				get
				{
					if (!_started || _current == null) throw new InvalidOperationException();
					return _current.Value;
				}
			}

			/// <inheritdoc />
			[NotNull]
			object IEnumerator.Current => Current;

			/// <inheritdoc />
			public IEnumerator<T> GetEnumerator()
			{
				IEnumerator enumerator = this;
				enumerator.Reset();
				return this;
			}

			/// <inheritdoc />
			IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

			public bool MoveNext()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				// Head-Child-Sibling (Stack)
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_stack.Push(_root);
				}

				// visit the next queued node
				_current = _stack.Count > 0
								? _stack.Pop()
								: null;

				if (_current == null)
				{
					_done = true;
					return false;
				}

				// Queue the next nodes
				if (_current.Child != null) _stack.Push(_current.Child);
				if (_current.Sibling != null) _stack.Push(_current.Sibling);
				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _heap._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_done = _heap.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}
		
		/// <inheritdoc />
		protected SiblingsHeapBase()
			: this((IComparer<T>)null)
		{
		}

		protected SiblingsHeapBase(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected SiblingsHeapBase([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected SiblingsHeapBase([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override IEnumerableEnumerator<T> Enumerate(TNode root, BreadthDepthTraversal method)
		{
			return method switch
			{
				BreadthDepthTraversal.BreadthFirst => new BreadthFirstEnumerator(this, root),
				BreadthDepthTraversal.DepthFirst => new DepthFirstEnumerator(this, root),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		/// <inheritdoc />
		public sealed override void Iterate(TNode root, BreadthDepthTraversal method, Action<TNode> visitCallback)
		{
			if (Count == 0 || root == null) return;

			switch (method)
			{
				case BreadthDepthTraversal.BreadthFirst:
					BreadthFirst(root, visitCallback);
					break;
				case BreadthDepthTraversal.DepthFirst:
					DepthFirst(root, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}
		}

		/// <inheritdoc />
		public sealed override void Iterate(TNode root, BreadthDepthTraversal method, Func<TNode, bool> visitCallback)
		{
			if (Count == 0 || root == null) return;

			switch (method)
			{
				case BreadthDepthTraversal.BreadthFirst:
					BreadthFirst(root, visitCallback);
					break;
				case BreadthDepthTraversal.DepthFirst:
					DepthFirst(root, visitCallback);
					break;
				default:
					throw new ArgumentOutOfRangeException(nameof(method), method, null);
			}
		}

		#region Iterator Traversal for Action<TNode>
		private void BreadthFirst([NotNull] TNode root, [NotNull] Action<TNode> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				queue.Enqueue(root);

				while (queue.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = queue.Dequeue();
					visitCallback(current);
					if (current.Child == null) continue;

					// Queue the next nodes
					queue.Enqueue(current.Child);
					if (current.Child.Sibling == null) continue;

					foreach (TNode sibling in current.Child.Siblings())
						queue.Enqueue(sibling);
				}

				root = root.Sibling;
			}
		}

		private void DepthFirst([NotNull] TNode root, [NotNull] Action<TNode> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Stack)
			Stack<TNode> stack = new Stack<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				stack.Push(root);

				while (stack.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = stack.Pop();
					visitCallback(current);
					if (current.Child == null) continue;

					// Queue the next nodes
					if (current.Child.Sibling != null) stack.Push(current.Child.Sibling);

					foreach (TNode child in current.Child.Children())
						stack.Push(child);
				}

				root = root.Sibling;
			}
		}
		#endregion

		#region Iterator Traversal for Func<TNode, bool>
		private void BreadthFirst([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			int version = _version;
			// Head-Sibling-Child (Queue)
			Queue<TNode> queue = new Queue<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				queue.Enqueue(root);

				while (queue.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = queue.Dequeue();
					if (!visitCallback(current)) return;
					if (current.Child == null) continue;

					// Queue the next nodes
					queue.Enqueue(current.Child);
					if (current.Child.Sibling == null) continue;

					foreach (TNode sibling in current.Child.Siblings())
						queue.Enqueue(sibling);
				}

				root = root.Sibling;
			}
		}

		private void DepthFirst([NotNull] TNode root, [NotNull] Func<TNode, bool> visitCallback)
		{
			int version = _version;
			// Head-Child-Sibling (Stack)
			Stack<TNode> stack = new Stack<TNode>();

			while (root != null)
			{
				if (version != _version) throw new VersionChangedException();
				// Start at the root
				stack.Push(root);

				while (stack.Count > 0)
				{
					if (version != _version) throw new VersionChangedException();

					// visit the next queued node
					TNode current = stack.Pop();
					if (!visitCallback(current)) return;
					if (current.Child == null) continue;

					// Queue the next nodes
					if (current.Child.Sibling != null) stack.Push(current.Child.Sibling);

					foreach (TNode child in current.Child.Children())
						stack.Push(child);
				}

				root = root.Sibling;
			}
		}
		#endregion
	}
	
	/// <inheritdoc cref="SiblingsHeapBase{TNode,T}" />
	/// <typeparam name="TNode">The node type.</typeparam>
	/// <typeparam name="T">The element type of the heap</typeparam>
	[Serializable]
	public abstract class SiblingsHeap<TNode, T> : SiblingsHeapBase<TNode, T>, IBinaryHeap<TNode, T>
		where TNode : class, ISiblingNode<TNode, T>
	{
		/// <inheritdoc />
		protected SiblingsHeap()
			: this((IComparer<T>)null)
		{
		}

		protected SiblingsHeap(IComparer<T> comparer)
			: base(comparer)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<T> enumerable)
			: this(enumerable, null)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<T> enumerable, IComparer<T> comparer)
			: base(comparer)
		{
			Add(enumerable);
		}

		/// <inheritdoc />
		public abstract void DecreaseKey(TNode node, T newValue);
	}

	/// <summary>
	/// <inheritdoc cref="SiblingsHeapBase{TNode,T}" />
	/// <para>
	/// This heap type and its subclasses use a <see cref="ISiblingNode{TNode, TKey, TValue}">sibling tree node</see>
	/// </para>
	/// </summary>
	/// <typeparam name="TNode">The node type.</typeparam>
	/// <typeparam name="TKey">The element key type of the heap</typeparam>
	/// <typeparam name="TValue">The element type of the heap</typeparam>
	[Serializable]
	public abstract class SiblingsHeap<TNode, TKey, TValue> : SiblingsHeapBase<TNode, TValue>, IBinaryHeap<TNode, TKey, TValue>
		where TNode : class, ISiblingNode<TNode, TKey, TValue>
	{
		/// <inheritdoc />
		protected SiblingsHeap([NotNull] Func<TValue, TKey> getKeyForItem)
			: this(getKeyForItem, null, null)
		{
		}

		/// <inheritdoc />
		protected SiblingsHeap([NotNull] Func<TValue, TKey> getKeyForItem, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: base(comparer)
		{
			KeyComparer = keyComparer ?? Comparer<TKey>.Default;
			GetKeyForItem = getKeyForItem;
		}

		/// <inheritdoc />
		protected SiblingsHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable)
			: this(getKeyForItem, enumerable, null, null)
		{
		}

		/// <inheritdoc />
		protected SiblingsHeap([NotNull] Func<TValue, TKey> getKeyForItem, [NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> keyComparer, IComparer<TValue> comparer)
			: this(getKeyForItem, keyComparer, comparer)
		{
			Add(enumerable);
		}

		public IComparer<TKey> KeyComparer { get; }

		[NotNull]
		protected Func<TValue, TKey> GetKeyForItem { get; }

		protected abstract int Compare([NotNull] TKey x, [NotNull] TKey y);

		/// <inheritdoc />
		public abstract void DecreaseKey(TNode node, TKey newKey);

		/// <inheritdoc />
		public TNode FindByKey(TKey key)
		{
			TNode node = null;
			IEqualityComparer<TKey> comparer = EqualityComparer<TKey>.Default;
			Iterate(e =>
			{
				if (comparer.Equals(e.Key, key)) node = e;
				return node == null;
			});
			return node;
		}
	}

	public static class SiblingsHeapExtension
	{
		public static void WriteTo<TNode, T>([NotNull] this SiblingsHeapBase<TNode, T> thisValue, [NotNull] TextWriter writer)
			where TNode : class, ISiblingNode<TNode, T>
		{
			if (thisValue.Head == null) return;

			StringBuilder indent = new StringBuilder();
			Stack<(TNode Node, int Level)> stack = new Stack<(TNode Node, int Level)>(1);
			stack.Push((thisValue.Head, 0));

			while (stack.Count > 0)
			{
				(TNode node, int level) = stack.Pop();
				int n = Constants.INDENT * level;

				if (indent.Length > n) indent.Length = n;
				else if (indent.Length < n) indent.Append(' ', n - indent.Length);

				writer.Write(indent);

				if (node == null)
				{
					writer.WriteLine(Constants.NULL);
					continue;
				}

				writer.WriteLine(node.ToString(level));
				if (node.Sibling != null) stack.Push((node.Sibling, level));
				if (node.Child != null) stack.Push((node.Child, level + 1));
			}
		}
	}
}