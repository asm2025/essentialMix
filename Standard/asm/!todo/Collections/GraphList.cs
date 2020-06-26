using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using asm.Exceptions.Collections;
using JetBrains.Annotations;

namespace asm.Collections
{
	/// <summary>
	/// <see href="https://en.wikipedia.org/wiki/Graph_(abstract_data_type)">Graph (abstract data type)</see> using the <see cref="IDictionary{TKey,TValue}">adjacency list</see> representation.
	/// </summary>
	/// <typeparam name="TVertex">The vertex type. Must inherit from <see cref="GraphVertex{TVertex,T}"/></typeparam>
	/// <typeparam name="TEdge">The edge type. Must inherit from <see cref="GraphEdge{TVertex, TEdge,T}"/></typeparam>
	/// <typeparam name="T">The element type of the tree</typeparam>
	// Udemy - Code With Mosh - Data Structures & Algorithms - Part 2
	// https://www.researchgate.net/publication/2349751_Design_And_Implementation_Of_A_Generic_Graph_Container_In_Java
	// https://www.lri.fr/~filliatr/ftp/publis/ocamlgraph-tfp-8.pdf
	// https://citeseerx.ist.psu.edu/viewdoc/download?doi=10.1.1.30.1944&rep=rep1&type=pdf
	// https://gist.github.com/kevinmorio/f7102c5094aa748503f9
	[DebuggerDisplay("Label = '{Label}', Count = {Count}")]
	[Serializable]
	public abstract class GraphList<TVertex, TEdge, T> : ICollection<T>, ICollection, IReadOnlyCollection<T>
		where TVertex : GraphVertex<TVertex, T>
		where TEdge : GraphEdge<TVertex, TEdge, T>
	{
		private struct BreadthFirstEnumerator : IGraphEnumeratorImpl<T>
		{
			private readonly GraphList<TVertex, TEdge, T> _graph;
			private readonly int _version;
			private readonly TVertex _root;
			private readonly Queue<TVertex> _queue;
			private readonly HashSet<T> _visited;

			private TVertex _current;
			private bool _started;
			private bool _done;

			internal BreadthFirstEnumerator([NotNull] GraphList<TVertex, TEdge, T> graph, TVertex root)
			{
				_graph = graph;
				_version = _graph._version;
				_root = root;
				_current = null;
				_started = false;
				_done = _graph.Count == 0 || _root == null;
				_visited = _done ? null : new HashSet<T>(graph.Comparer);
				_queue = _done ? null : new Queue<TVertex>();
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
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_queue.Enqueue(_root);
				}

				// visit the next queued vertex
				do
				{
					_current = _queue.Count > 0
									? _queue.Dequeue()
									: null;
				}
				while (_current != null && _visited.Contains(_current.Value));

				if (_current == null)
				{
					_done = true;
					return false;
				}

				_visited.Add(_current.Value);

				// Queue the next vertices
				if (_graph.Edges.TryGetValue(_current.Value, out KeyedDictionary<T, TEdge> edges))
				{
					foreach (TEdge edge in edges.Values) 
						_queue.Enqueue(edge.To);
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_queue.Clear();
				_visited.Clear();
				_done = _graph.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		private struct DepthFirstEnumerator : IGraphEnumeratorImpl<T>
		{
			private readonly GraphList<TVertex, TEdge, T> _graph;
			private readonly int _version;
			private readonly TVertex _root;
			private readonly Stack<TVertex> _stack;
			private readonly HashSet<T> _visited;

			private TVertex _current;
			private bool _started;
			private bool _done;

			internal DepthFirstEnumerator([NotNull] GraphList<TVertex, TEdge, T> graph, TVertex root)
			{
				_graph = graph;
				_version = _graph._version;
				_root = root;
				_current = null;
				_started = false;
				_done = _graph.Count == 0 || _root == null;
				_visited = _done ? null : new HashSet<T>(graph.Comparer);
				_stack = _done ? null : new Stack<TVertex>();
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
				if (_version != _graph._version) throw new VersionChangedException();
				if (_done) return false;

				if (!_started)
				{
					_started = true;
					// Start at the root
					_stack.Push(_root);
				}

				// visit the next queued vertex
				do
				{
					_current = _stack.Count > 0
									? _stack.Pop()
									: null;
				}
				while (_current != null && _visited.Contains(_current.Value));

				if (_current == null)
				{
					_done = true;
					return false;
				}

				_visited.Add(_current.Value);
				// Queue the next vertices
				if (_graph.Edges.TryGetValue(_current.Value, out KeyedDictionary<T, TEdge> edges))
				{
					foreach (TEdge edge in edges.Values)
						_stack.Push(edge.To);
				}

				return true;
			}

			void IEnumerator.Reset()
			{
				if (_version != _graph._version) throw new VersionChangedException();
				_current = null;
				_started = false;
				_stack.Clear();
				_visited.Clear();
				_done = _graph.Count == 0 || _root == null;
			}

			/// <inheritdoc />
			public void Dispose() { }
		}

		/*
		 * todo: implement iterators to do the following:
		 * 1. Walk: traversing a graph where vertex and edges can be repeated.
		 *		a. open walk: when starting and ending vertices are different.
		 *		b. closed walk when starting and ending vertices are the same.
		 * 2. Trail: an open walk in which no edge is repeated.
		 * 3. Circuit: a closed walk in which no edge is repeated.
		 * 4. Path: a trail in which neither vertices nor edges are repeated.
		 * 5. Cycle: traversing a graph where neither vertices nor edges are
		 * repeated and starting and ending vertices are the same.
		 */

		[NotNull]
		private readonly ICollection _collectionRef;

		protected internal int _version;

		/// <inheritdoc />
		protected GraphList()
			: this((IEqualityComparer<T>)null)
		{
		}

		protected GraphList(IEqualityComparer<T> comparer)
		{
			Comparer = comparer ?? EqualityComparer<T>.Default;
			Vertices = new KeyedDictionary<T, TVertex>(e => e.Value, Comparer);
			Edges = new Dictionary<T, KeyedDictionary<T, TEdge>>(Comparer);
			_collectionRef = Vertices;
		}

		/// <inheritdoc />
		protected GraphList([NotNull] IEnumerable<T> collection)
			: this(collection, null)
		{
		}

		/// <inheritdoc />
		protected GraphList([NotNull] IEnumerable<T> collection, IEqualityComparer<T> comparer)
			: this(comparer)
		{
			Add(collection);
		}

		public string Label { get; set; }

		/// <summary>
		/// <inheritdoc cref="ICollection{T}" />
		/// <para>Graph's count is also its order.</para>
		/// </summary>
		public int Count => Vertices.Count;
		
		[NotNull]
		public IEqualityComparer<T> Comparer { get; }

		/// <inheritdoc />
		bool ICollection<T>.IsReadOnly => Vertices.IsReadOnly;

		/// <inheritdoc />
		bool ICollection.IsSynchronized => _collectionRef.IsSynchronized;

		/// <inheritdoc />
		object ICollection.SyncRoot => _collectionRef.SyncRoot;

		[NotNull]
		protected internal KeyedDictionary<T, TVertex> Vertices { get; }

		[NotNull]
		protected internal IDictionary<T, KeyedDictionary<T, TEdge>> Edges { get; }

		/// <inheritdoc />
		public IEnumerator<T> GetEnumerator() { return Vertices.Keys.GetEnumerator(); }

		/// <inheritdoc />
		IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

		/// <summary>
		/// Enumerate vertices' values in a semi recursive approach
		/// </summary>
		/// <param name="from">The starting vertex's value</param>
		/// <param name="method">The traverse method</param>
		/// <returns></returns>
		[NotNull]
		public IGraphEnumeratorImpl<T> Enumerate([NotNull] T from, GraphTraverseMethod method)
		{
			if (!Vertices.TryGetValue(from, out TVertex root)) throw new KeyNotFoundException();

			return method switch
			{
				GraphTraverseMethod.BreadthFirst => new BreadthFirstEnumerator(this, root),
				GraphTraverseMethod.DepthFirst => new DepthFirstEnumerator(this, root),
				_ => throw new ArgumentOutOfRangeException(nameof(method), method, null)
			};
		}

		/// <inheritdoc />
		public void Add(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (Vertices.ContainsKey(value)) throw new DuplicateKeyException();
			Vertices.Add(NewVertex(value));
		}

		public void Add([NotNull] IEnumerable<T> collection)
		{
			foreach (T value in collection) 
				Add(value);
		}

		public abstract void AddEdge([NotNull] T from, [NotNull] T to);

		/// <inheritdoc />
		public bool Remove(T value)
		{
			if (ReferenceEquals(value, null)) throw new ArgumentNullException(nameof(value));
			if (!Vertices.RemoveByKey(value)) return false;
			RemoveAllEdges(value);
			return true;
		}

		public abstract void RemoveEdge([NotNull] T from, [NotNull] T to);

		public void RemoveEdges([NotNull] T value)
		{
			Edges.Remove(value);
		}

		public void RemoveAllEdges([NotNull] T value)
		{
			Edges.Remove(value);
			if (Edges.Count == 0) return;

			List<T> empty = new List<T>();

			foreach (KeyValuePair<T, KeyedDictionary<T, TEdge>> pair in Edges)
			{
				pair.Value.RemoveByKey(value);
				if (pair.Value.Count == 0) empty.Add(pair.Key);
			}

			if (empty.Count == 0) return;

			foreach (T key in empty)
			{
				Edges.Remove(key);
			}
		}

		public void ClearEdges() { Edges.Clear(); }

		/// <inheritdoc />
		public void Clear()
		{
			Vertices.Clear();
			Edges.Clear();
		}

		/// <inheritdoc />
		public bool Contains(T value) { return !ReferenceEquals(value, null) && Vertices.ContainsKey(value); }

		public bool ContainsEdge([NotNull] T from, [NotNull] T to)
		{
			return Edges.Count > 0 && Edges.TryGetValue(from, out KeyedDictionary<T, TEdge> edges) && edges.ContainsKey(to);
		}

		public bool ContainsEdge([NotNull] T value)
		{
			return Edges.Count > 0 && Edges.ContainsKey(value);
		}

		public int Degree([NotNull] T value)
		{
			return Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges)
						? edges.Count
						: 0;
		}

		public virtual int GetSize()
		{
			int sum = 0;

			foreach (KeyedDictionary<T, TEdge> e in Edges.Values) 
				sum += e.Count;

			return sum;
		}

		public ICollection<T> FindCycle(bool ignoreLoop = false) { return FindCycle(Edges.Keys, ignoreLoop); }
		protected ICollection<T> FindCycle([NotNull] ICollection<T> vertices, bool ignoreLoop = false)
		{
			if (vertices.Count == 0) return null;
			
			Stack<T> stack = new Stack<T>();
			Queue<T> cycle = new Queue<T>();
			HashSet<T> visiting = new HashSet<T>(Comparer);
			HashSet<T> visited = new HashSet<T>(Comparer);
			int version = _version;

			foreach (T vertex in vertices)
			{
				if (version != _version) throw new VersionChangedException();
				stack.Push(vertex);

				while (stack.Count > 0)
				{
					T value = stack.Pop();
					if (visited.Contains(value)) continue;
					visiting.Add(value);
					cycle.Enqueue(value);

					if (Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges))
					{
						foreach (TEdge edge in edges)
						{
							if (visited.Contains(edge.To.Value) || ignoreLoop && IsLoop(value, edge)) continue;

							// cycle detected
							if (visiting.Contains(edge.To.Value))
							{
								cycle.Enqueue(edge.To.Value);

								while (!Comparer.Equals(cycle.Peek(), edge.To.Value))
									cycle.Dequeue();

								return cycle.ToArray();
							}

							stack.Push(edge.To.Value);
						}

						continue;
					}

					visiting.Remove(value);
					cycle.Dequeue();
					visited.Add(value);
				}
			}

			return null;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public bool IsLoop([NotNull] T value, [NotNull] TEdge edge)
		{
			return Comparer.Equals(value, edge.To.Value);
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public bool IsInternal([NotNull] T value)
		{
			return Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges) && edges.Count > 1;
		}

		[MethodImpl(MethodImplOptions.ForwardRef | MethodImplOptions.AggressiveInlining)]
		public bool IsExternal([NotNull] T value)
		{
			return Edges.TryGetValue(value, out KeyedDictionary<T, TEdge> edges) && edges.Count < 2;
		}

		/// <summary>
		/// Gets the value with the maximum number of connections
		/// </summary>
		[NotNull]
		public IEnumerable<T> Top(int count = 1)
		{
			return count < 0
						? throw new ArgumentOutOfRangeException(nameof(count))
						: count == 0
							? Enumerable.Empty<T>()
							: Edges.OrderByDescending(e => e.Value.Count)
									.Take(count)
									.Select(e => e.Key);
		}

		/// <summary>
		/// Gets the value with the minimum number of connections
		/// </summary>
		[NotNull]
		public IEnumerable<T> Bottom(int count = 1)
		{
			return count < 0
						? throw new ArgumentOutOfRangeException(nameof(count))
						: count == 0
							? Enumerable.Empty<T>()
							: Edges.OrderBy(e => e.Value.Count)
									.Take(count)
									.Select(e => e.Key);
		}

		/// <inheritdoc />
		public void CopyTo(T[] array, int arrayIndex) { Vertices.Keys.CopyTo(array, arrayIndex); }

		/// <inheritdoc />
		public void CopyTo(Array array, int index) { ((ICollection)Vertices.Keys).CopyTo(array, index); }

		[NotNull]
		protected abstract TVertex NewVertex(T value);

		[NotNull]
		protected abstract TEdge NewEdge(T value);

		[NotNull]
		protected KeyedDictionary<T, TEdge> NewEdgesContainer() { return new KeyedDictionary<T, TEdge>(e => e.To.Value, Comparer); }
	}

	public static class GraphExtension
	{
		public static void WriteTo<TVertex, TEdge, T>([NotNull] this GraphList<TVertex, TEdge, T> thisValue, [NotNull] TextWriter writer)
			where TVertex : GraphVertex<TVertex, T>
			where TEdge : GraphEdge<TVertex, TEdge, T>
		{
			if (thisValue.Edges.Count == 0) return;

			IDictionary<T, KeyedDictionary<T, TEdge>> allEdges = thisValue.Edges;

			foreach (TVertex vertex in thisValue.Vertices.Values)
			{
				if (!allEdges.TryGetValue(vertex.Value, out KeyedDictionary<T, TEdge> vertexEdges)) continue;
				writer.WriteLine($"{vertex}->[{string.Join(", ", vertexEdges.Values)}]");
			}
		}
	}
}