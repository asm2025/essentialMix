using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using essentialMix.Exceptions.Collections;
using JetBrains.Annotations;

namespace essentialMix.Collections
{
	[Serializable]
	public abstract class SiblingsHeap<TNode, T> : Heap<TNode, T>
		where TNode : class, ISiblingNode<TNode, T>
	{
		private struct BreadthFirstEnumerator : IEnumerableEnumerator<T>
		{
			private readonly SiblingsHeap<TNode, T> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] SiblingsHeap<TNode, T> heap, TNode root)
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
			private readonly SiblingsHeap<TNode, T> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] SiblingsHeap<TNode, T> heap, TNode root)
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

		public virtual bool Equals(SiblingsHeap<TNode, T> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() 
				|| Count != other.Count 
				|| !Comparer.Equals(other.Comparer)) return false;
			if (Count == 0) return true;

			using (IEnumerator<T> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<T> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();

					while (thisMoved && otherMoved)
					{
						if (Comparer.Compare(thisEnumerator.Current, otherEnumerator.Current) != 0) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
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

	[Serializable]
	public abstract class SiblingsHeap<TNode, TKey, TValue> : Heap<TNode, TKey, TValue>
		where TNode : class, ISiblingNode<TNode, TKey, TValue>
	{
		private struct BreadthFirstEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly SiblingsHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Queue<TNode> _queue;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] SiblingsHeap<TNode, TKey, TValue> heap, TNode root)
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
			public TValue Current
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

		private struct DepthFirstEnumerator : IEnumerableEnumerator<TValue>
		{
			private readonly SiblingsHeap<TNode, TKey, TValue> _heap;
			private readonly int _version;
			private readonly TNode _root;
			private readonly Stack<TNode> _stack;

			private TNode _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] SiblingsHeap<TNode, TKey, TValue> heap, TNode root)
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
			public TValue Current
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
		protected SiblingsHeap()
			: this((IComparer<TKey>)null)
		{
		}

		protected SiblingsHeap(IComparer<TKey> comparer)
			: base(comparer)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<TValue> enumerable)
			: this(enumerable, null)
		{
		}

		protected SiblingsHeap([NotNull] IEnumerable<TValue> enumerable, IComparer<TKey> comparer)
			: base(enumerable, comparer)
		{
		}

		/// <inheritdoc />
		public sealed override IEnumerableEnumerator<TValue> Enumerate(TNode root, BreadthDepthTraversal method)
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

		public virtual bool Equals(SiblingsHeap<TNode, TKey, TValue> other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			if (GetType() != other.GetType() 
				|| Count != other.Count 
				|| !ValueComparer.Equals(other.ValueComparer)) return false;
			if (Count == 0) return true;

			using (IEnumerator<TValue> thisEnumerator = GetEnumerator())
			{
				using (IEnumerator<TValue> otherEnumerator = other.GetEnumerator())
				{
					bool thisMoved = thisEnumerator.MoveNext();
					bool otherMoved = otherEnumerator.MoveNext();

					while (thisMoved && otherMoved)
					{
						if (!ValueComparer.Equals(thisEnumerator.Current, otherEnumerator.Current)) return false;
						thisMoved = thisEnumerator.MoveNext();
						otherMoved = otherEnumerator.MoveNext();
					}

					if (thisMoved ^ otherMoved) return false;
				}
			}

			return true;
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

	public static class SiblingsHeapExtension
	{
		public static void WriteTo<TNode, T>([NotNull] this SiblingsHeap<TNode, T> thisValue, [NotNull] TextWriter writer)
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

		public static void WriteTo<TNode, TKey, TValue>([NotNull] this SiblingsHeap<TNode, TKey, TValue> thisValue, [NotNull] TextWriter writer)
			where TNode : class, ISiblingNode<TNode, TKey, TValue>
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