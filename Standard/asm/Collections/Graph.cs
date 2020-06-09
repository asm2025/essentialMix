using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	// https://www.researchgate.net/publication/2349751_Design_And_Implementation_Of_A_Generic_Graph_Container_In_Java
	// https://www.lri.fr/~filliatr/ftp/publis/ocamlgraph-tfp-8.pdf
	// https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.30.1944&rep=rep1&type=pdf
	// https://gist.github.com/kevinmorio/f7102c5094aa748503f9
	[DebuggerDisplay("{Label} Count = {Count}")]
	[Serializable]
	public abstract class Graph<TNode, TEdge, T> : ICollection<T>, ICollection
		where TNode : GraphNode<TNode, T>
		where TEdge : GraphEdge<TNode, TEdge, T>
	{
		///// <summary>
		///// a semi recursive approach to traverse the graph
		///// </summary>
		//internal sealed class Enumerator : IEnumerator<T>, IEnumerator, IEnumerable<T>, IEnumerable, IDisposable
		//{
		//	private readonly Graph<TNode, TEdge, T> _graph;
		//	private readonly int _version;
		//	private readonly TNode _root;
		//	private readonly DynamicQueue<TNode> _queueOrStack;
		//	private readonly HashSet<T> _visited;
		//	private readonly Func<bool> _moveNext;

		//	private TNode _current;
		//	private bool _started;
		//	private bool _done;

		//	internal Enumerator([NotNull] Graph<TNode, TEdge, T> graph, [NotNull] TNode root, GraphTraverseMethod method, HorizontalFlow flow)
		//	{
		//		_graph = graph;
		//		_version = _graph._version;
		//		_root = root;
		//		_visited = new HashSet<T>(graph.Comparer);

		//		switch (method)
		//		{
		//			case GraphTraverseMethod.BreadthFirst:
		//				_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.FIFO);
		//				_moveNext = flow switch
		//				{
		//					HorizontalFlow.LeftToRight => BreadthFirstLR,
		//					HorizontalFlow.RightToLeft => BreadthFirstRL,
		//					_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
		//				};
		//				break;
		//			case GraphTraverseMethod.DepthFirst:
		//				_queueOrStack = new DynamicQueue<TNode>(DequeuePriority.LIFO);
		//				_moveNext = flow switch
		//				{
		//					HorizontalFlow.LeftToRight => DepthFirstLR,
		//					HorizontalFlow.RightToLeft => DepthFirstRL,
		//					_ => throw new ArgumentOutOfRangeException(nameof(flow), flow, null)
		//				};
		//				break;
		//			default:
		//				throw new ArgumentOutOfRangeException(nameof(method), method, null);
		//		}
		//	}

		//	/// <inheritdoc />
		//	[NotNull]
		//	public T Current
		//	{
		//		get
		//		{
		//			if (_current == null) throw new InvalidOperationException();
		//			return _current.Value;
		//		}
		//	}

		//	/// <inheritdoc />
		//	[NotNull]
		//	object IEnumerator.Current => Current;

		//	/// <inheritdoc />
		//	public IEnumerator<T> GetEnumerator()
		//	{
		//		IEnumerator enumerator = this;
		//		enumerator.Reset();
		//		return this;
		//	}

		//	/// <inheritdoc />
		//	IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		//	public bool MoveNext() { return _moveNext(); }

		//	void IEnumerator.Reset()
		//	{
		//		_current = null;
		//		_started = _done = false;
		//		_queueOrStack.Clear();
		//		_visited.Clear();
		//	}

		//	/// <inheritdoc />
		//	public void Dispose()
		//	{
		//		_current = null;
		//		_queueOrStack.Clear();
		//	}

		//	private bool BreadthFirstLR()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Left-Right (Queue)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		_current = _queueOrStack.Count > 0
		//						? _queueOrStack.Dequeue()
		//						: null;

		//		if (_current == null)
		//		{
		//			_done = true;
		//			return false;
		//		}

		//		// Queue the next nodes
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		return true;
		//	}

		//	private bool BreadthFirstRL()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Right-Left (Queue)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		_current = _queueOrStack.Count > 0
		//						? _queueOrStack.Dequeue()
		//						: null;

		//		if (_current == null)
		//		{
		//			_done = true;
		//			return false;
		//		}

		//		// Queue the next nodes
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		return true;
		//	}

		//	private bool DepthFirstLR()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Left-Right (Stack)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		do
		//		{
		//			_current = _queueOrStack.Count > 0
		//							? _queueOrStack.Dequeue()
		//							: null;
		//			if (_current != null) continue;
		//			_done = true;
		//			return false;
		//		}
		//		while (_visited.Contains(_current.Value));

		//		_visited.Add(_current.Value);

		//		// Queue the next nodes
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		return true;
		//	}

		//	private bool DepthFirstRL()
		//	{
		//		if (_version != _graph._version) throw new VersionChangedException();

		//		// Root-Right-Left (Stack)
		//		if (_done) return false;

		//		if (!_started)
		//		{
		//			_started = true;
		//			// Start at the root
		//			_queueOrStack.Enqueue(_root);
		//		}

		//		// visit the next queued node
		//		_current = _queueOrStack.Count > 0
		//						? _queueOrStack.Dequeue()
		//						: null;

		//		if (_current == null)
		//		{
		//			_done = true;
		//			return false;
		//		}

		//		// Queue the next nodes
		//		if (_current.Left != null) _queueOrStack.Enqueue(_current.Left);
		//		if (_current.Right != null) _queueOrStack.Enqueue(_current.Right);
		//		return true;
		//	}
		//}

		[NotNull]
		private readonly ICollection _collectionRef;

		protected internal int _version;

		/// <inheritdoc />
		protected Graph()
			: this((IEqualityComparer<T>)null)
		{
		}

		protected Graph(IEqualityComparer<T> comparer)
		{
			Comparer = comparer ?? EqualityComparer<T>.Default;
			Nodes = new KeyedDictionary<T, TNode>(e => e.Value, Comparer);
			Edges = new Dictionary<T, KeyedDictionary<T, TEdge>>(Comparer);
			_collectionRef = Nodes;
		}

		/// <inheritdoc />
		protected Graph([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected Graph([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(comparer)
		{
			Add(collection);
		}

		public string Label { get; set; }

		public int Order => Nodes.Count;

		/// <inheritdoc cref="ICollection{T}" />
		public int Count => Order;

		public abstract int Size { get; }

		/// <inheritdoc />
		public bool IsReadOnly => Nodes.IsReadOnly;
		
		[NotNull]
		public IEqualityComparer<T> Comparer { get; }

		/// <inheritdoc />
		public bool IsSynchronized => _collectionRef.IsSynchronized;

		/// <inheritdoc />
		public object SyncRoot => _collectionRef.SyncRoot;

		[NotNull]
		protected internal KeyedDictionary<T, TNode> Nodes { get; }

		[NotNull]
		protected internal IDictionary<T, KeyedDictionary<T, TEdge>> Edges { get; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Nodes.Keys.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <inheritdoc />
		public void Add(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (Nodes.ContainsKey(value)) throw new DuplicateKeyException();
			Nodes.Add(NewNode(value));
		}

		public void Add([NotNull] IEnumerable<T> collection)
		{
			foreach (T value in collection)
			{
				Add(value);
			}
		}

		public abstract void AddEdge(T from, T to);

		/// <inheritdoc />
		public bool Remove(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (!Nodes.RemoveByKey(value)) return false;
			RemoveAllEdges(value);
			return true;
		}

		public abstract void RemoveEdge([NotNull] T from, [NotNull] T to);
		
		public abstract void RemoveEdges([NotNull] T from);
		
		public abstract void RemoveAllEdges([NotNull] T value);

		public void ClearEdges() { Edges.Clear(); }

		/// <inheritdoc />
		public void Clear()
		{
			Nodes.Clear();
			Edges.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T value) { return !ReferenceEquals(value, null) && Nodes.ContainsKey(value); }

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Nodes.Keys.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { ((ICollection)Nodes.Keys).CopyTo(array, index); }

		[NotNull]
		protected abstract TNode NewNode(T value);

		[NotNull]
		protected abstract TEdge NewEdge(T value);

		[NotNull]
		protected KeyedDictionary<T, TEdge> NewEdges() { return new KeyedDictionary<T, TEdge>(e => e.To.Value, Comparer); }
	}

	public static class GraphExtension
	{
		public static void WriteTo<TNode, TEdge, T>([NotNull] this Graph<TNode, TEdge, T> thisValue, [NotNull] TextWriter writer)
			where TNode : GraphNode<TNode, T>
			where TEdge : GraphEdge<TNode, TEdge, T>
		{
			if (thisValue.Edges.Count == 0) return;

			foreach (T value in thisValue)
			{
				if (!thisValue.Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges)
					|| !thisValue.Nodes.TryGetValue(value, out TNode node)) continue;

				int i = 0;
				writer.Write($"{node}->[");

				foreach (TEdge edge in edges.Values)
				{
					if (i++ > 0) writer.Write(", ");
					writer.Write(edge.To);
				}

				writer.WriteLine(']');
			}
		}
	}
}